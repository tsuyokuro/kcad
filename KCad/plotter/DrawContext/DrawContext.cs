using OpenTK;
using System.Drawing;

using static System.Math;

namespace Plotter
{
    public class DrawContext
    {
        CadObjectDB DB
        {
            set; get;
        }

        // 用紙サイズ
        public PaperPageSize PageSize = new PaperPageSize();

        // 画素/Milli
        // 1ミリあたりの画素数
        public double UnitPerMilli = 1;

        // 1inchは何ミリ?
        public const double MILLI_PER_INCH = 25.4;

        // Screen 座標系の原点 
        public CadPoint mViewOrg;


        protected Vector3d Eye = Vector3d.UnitZ;
        protected Vector3d LookAt = Vector3d.Zero;
        protected Vector3d UpVector = Vector3d.UnitY;

        protected double ProjectionNear = 10.0f;
        protected double ProjectionFar = 10000.0f;


        // 投影スクリーンの向き
        protected Vector3d mViewDir = default(Vector3d);

        public virtual Vector3d ViewDir
        {
            get
            {
                return mViewDir;
            }
        }

        // ワールド座標系から視点座標系への変換行列
        protected UMatrix4 mViewMatrix = new UMatrix4();

        public UMatrix4 ViewMatrix
        {
            get { return mViewMatrix; }
        }

        // 視点座標系からワールド座標系への変換行列
        protected UMatrix4 mViewMatrixInv = new UMatrix4();

        public UMatrix4 ViewMatrixInv
        {
            get { return mViewMatrixInv; }
        }


        // 視点座標系から投影座標系への変換行列
        protected UMatrix4 mProjectionMatrix = new UMatrix4();

        public UMatrix4 ProjectionMatrix
        {
            get { return mProjectionMatrix; }
        }


        // 投影座標系から視点座標系への変換行列
        protected UMatrix4 mProjectionMatrixInv = new UMatrix4();

        public UMatrix4 ProjectionMatrixInv
        {
            get { return mProjectionMatrixInv; }
        }


        // 座標系の原点がView座標上で何処にあるかを示す
        public CadPoint ViewOrg
        {
            set
            {
                mViewOrg = value;
                mViewOrg.dump(DebugOut.Std);
            }

            get
            {
                return mViewOrg;
            }
        }

        public double mViewWidth = 1;
        public double mViewHeight = 1;

        // Screenのサイズ
        public double ViewWidth
        {
            get
            {
                return mViewWidth;
            }
        }

        public double ViewHeight
        {
            get
            {
                return mViewHeight;
            }
        }

        public double WoldScale = 1.0;
        public double DeviceScaleX = 1.0;
        public double DeviceScaleY = -1.0;

        protected DrawTools Tools = new DrawTools();

        public DrawingBase Drawing;


        public virtual void SetupTools(DrawTools.ToolsType type)
        {
            Tools.Setup(type);
        }

        public virtual void setViewSize(double w, double h)
        {
            mViewWidth = w;
            mViewHeight = h;
        }

        public virtual void StartDraw()
        {
        }

        public virtual void StartDraw(Bitmap image)
        {
        }

        public virtual void EndDraw()
        {
        }


        // set dots per milli.
        public virtual void SetUnitPerMilli(double upm)
        {
            UnitPerMilli = upm;
        }

        // Calc inch units per milli.
        public virtual void SetUnitPerInch(double unit)
        {
            UnitPerMilli = unit / MILLI_PER_INCH;
        }

        public virtual CadPoint CadPointToUnitPoint(CadPoint pt)
        {
            return default(CadPoint);
        }

        public virtual CadPoint UnitPointToCadPoint(CadPoint pt)
        {
            return default(CadPoint);
        }

        public virtual double UnitToMilli(double d)
        {
            return d / UnitPerMilli;
        }

        public virtual double MilliToUnit(double d)
        {
            return d * UnitPerMilli;
        }

        public virtual void SetMatrix(UMatrix4 viewMatrix, UMatrix4 projMatrix)
        {
            mViewMatrix = viewMatrix;
            mViewMatrixInv = UMatrix4.Invert(viewMatrix);

            mProjectionMatrix = projMatrix;
            mProjectionMatrixInv = UMatrix4.Invert(projMatrix);
            RecalcCamera();
        }

        public virtual void SetViewMatrix(UMatrix4 viewMatrix)
        {
            mViewMatrix = viewMatrix;
            mViewMatrixInv = UMatrix4.Invert(viewMatrix);
            RecalcCamera();
        }

        public virtual void RecalcCamera()
        {
            CadPoint p0 = CadPoint.Create(0, 0, 0);
            CadPoint p1 = CadPoint.Create(0, 0, 1);

            CadPoint cp0 = UnitPointToCadPoint(p0);
            CadPoint cp1 = UnitPointToCadPoint(p1);

            Vector3d ret = cp0.vector - cp1.vector;
            ret.Normalize();

            mViewDir = ret;
            LookAt = mViewDir;
            Eye = Vector3d.Zero;
        }

        public virtual void RecalcViewDirFromCameraDirection()
        {
            Vector3d ret = LookAt - Eye;
            ret.Normalize();
            mViewDir = ret;
        }

        public virtual void SetCamera(Vector3d eye, Vector3d lookAt, Vector3d upVector)
        {
            Eye = eye;
            LookAt = lookAt;
            UpVector = upVector;

            mViewMatrix.GLMatrix = Matrix4d.LookAt(Eye, LookAt, UpVector);

            mViewMatrixInv.GLMatrix = Matrix4d.Invert(mViewMatrix.GLMatrix);

            RecalcViewDirFromCameraDirection();
        }

        public virtual void dump(DebugOut dout)
        {
            dout.println("ViewOrg");
            ViewOrg.dump(dout);

            dout.println("View Width=" + mViewWidth.ToString() + " Height=" + mViewHeight.ToString());

            CadPoint t = CadPoint.Create(mViewDir);

            dout.println("GazeVector");
            t.dump(dout);
        }
    }
}
