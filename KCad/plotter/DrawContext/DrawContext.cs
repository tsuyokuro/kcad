using OpenTK;
using System.Drawing;

using static System.Math;

namespace Plotter
{
    public class PaperPageSize
    {
        public double width;
        public double height;

        public double widthInch
        {
            set
            {
                width = value * 25.4;
            }

            get
            {
                return width / 25.4;
            }
        }

        public double heightInch
        {
            set
            {
                height = value * 25.4;
            }

            get
            {
                return height / 25.4;
            }
        }

        public PaperPageSize()
        {
            A4Land();
        }

        public void A4()
        {
            width = 210.0;
            height = 297.0;
        }

        public void A4Land()
        {
            width = 297.0;
            height = 210.0;
        }

        public PaperPageSize clone()
        {
            return (PaperPageSize)MemberwiseClone();
        }
    }

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


        public CadPoint ViewOrg
        {
            set
            {
                mViewOrg = value;
                calcViewCenter();
            }

            get
            {
                return mViewOrg;
            }
        }

        public double mViewWidth = 100;
        public double mViewHeight = 100;

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

        public CadPoint ViewCenter = default(CadPoint);

        public double WoldScale = 1.0;
        public double DeviceScaleX = 1.0;
        public double DeviceScaleY = -1.0;

        protected DrawTools Tools = new DrawTools();

        public DrawingBase Drawing;

        public void calcViewCenter()
        {
            CadPoint t = default(CadPoint);

            t.x = (mViewWidth / 2);
            t.y = (mViewHeight / 2);
            t.z = 0;

            t -= ViewOrg;

            t /= UnitPerMilli;

            ViewCenter = t;
        }

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

        public virtual CadRect GetViewRect()
        {
            CadRect rect = default(CadRect);

            CadPoint p0 = default(CadPoint);
            CadPoint p1 = default(CadPoint);

            p0.set(0, 0, 0);
            p1.set(ViewWidth, ViewHeight, 0);

            rect.p0 = UnitPointToCadPoint(p0);
            rect.p1 = UnitPointToCadPoint(p1);

            return rect;
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
            RecalcViewDir();
        }

        public virtual void SetViewMatrix(UMatrix4 viewMatrix)
        {
            mViewMatrix = viewMatrix;
            mViewMatrixInv = UMatrix4.Invert(viewMatrix);
            RecalcViewDir();
        }

        public virtual void RecalcViewDir()
        {
            CadPoint p0 = CadPoint.Create(0, 0, 0);
            CadPoint p1 = CadPoint.Create(0, 0, 1);

            CadPoint cp0 = UnitPointToCadPoint(p0);
            CadPoint cp1 = UnitPointToCadPoint(p1);

            Vector3d ret = cp0.vector - cp1.vector;
            ret.Normalize();
            mViewDir = ret;

            //CadPoint t = CadPoint.Create(mGazeVector);
            //t.dump(DebugOut.Std);
        }

        public virtual void dump(DebugOut dout)
        {
            dout.println("ViewOrg");
            ViewOrg.dump(dout);

            dout.println("ViewCenter");
            ViewCenter.dump(dout);

            dout.println("View Width=" + mViewWidth.ToString() + " Height=" + mViewHeight.ToString());

            CadPoint t = CadPoint.Create(mViewDir);

            dout.println("GazeVector");
            t.dump(dout);
        }
    }
}
