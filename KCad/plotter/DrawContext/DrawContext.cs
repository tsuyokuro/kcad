using OpenTK;
using System;
using System.Drawing;

using static System.Math;
using CadDataTypes;

namespace Plotter
{
    public abstract class DrawContext : IDisposable
    {
        Action<DrawContext> mOnPush;

        public Action<DrawContext> OnPush
        {
            set
            {
                mOnPush = value;
            }
        }

        // 用紙サイズ
        //public PaperPageSize PageSize = new PaperPageSize();

        // 画素/Milli
        // 1ミリあたりの画素数
        public double UnitPerMilli = 1;

        // 1inchは何ミリ?
        public const double MILLI_PER_INCH = 25.4;

        // 視点
        protected Vector3d Eye = Vector3d.UnitZ * 1000.0;

        // 注視点
        protected Vector3d LookAt = Vector3d.Zero;

        // 上を示す Vector
        protected Vector3d UpVector = Vector3d.UnitY;

        // 投影面までの距離
        protected double ProjectionNear = 500.0;

        // 視野空間の遠方側クリップ面までの距離
        protected double ProjectionFar = 1500.0;

        // 視野角　大きければ広角レンズ、小さければ望遠レンズ
        protected double FovY = Math.PI / 4;


        // 投影スクリーンの向き
        protected Vector3d mViewDir = default(Vector3d);

        public virtual Vector3d ViewDir
        {
            get
            {
                return mViewDir;
            }
        }

        // ワールド座標系から視点座標系への変換(ビュー変換)行列
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


        // Screen 座標系の原点 
        protected CadVector mViewOrg;

        // 座標系の原点がView座標上で何処にあるかを示す
        public CadVector ViewOrg
        {
            set
            {
                mViewOrg = value;
            }

            get
            {
                return mViewOrg;
            }
        }

        public double mViewWidth = 32;
        public double mViewHeight = 32;

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

        // 縮尺
        public double WorldScale = 1.0;

        // 画面に描画する際の係数
        public double DeviceScaleX = 1.0;
        public double DeviceScaleY = -1.0;

        public DrawTools Tools = new DrawTools();

        protected IDrawing mDrawing;

        public IDrawing Drawing
        {
            get
            {
                return mDrawing;
            }
        }

        public virtual void CopyFrom(DrawContext dc)
        {
            CopyMetrics(dc);
            SetViewSize(dc.mViewWidth, dc.mViewHeight);
        }

        public virtual void CopyMetrics(DrawContext dc)
        {
            //PageSize = dc.PageSize.clone();
            UnitPerMilli = dc.UnitPerMilli;
            Eye = dc.Eye;
            LookAt = dc.LookAt;
            UpVector = dc.UpVector;
            ProjectionNear = dc.ProjectionNear;
            ProjectionFar = dc.ProjectionFar;
            FovY = dc.FovY;
            mViewDir = dc.mViewDir;

            mViewMatrix = dc.mViewMatrix;
            mViewMatrixInv = dc.mViewMatrixInv;

            mProjectionMatrix = dc.mProjectionMatrix;
            mProjectionMatrixInv = dc.mProjectionMatrixInv;

            WorldScale = dc.WorldScale;
            DeviceScaleX = dc.DeviceScaleX;
            DeviceScaleY = dc.DeviceScaleY;

            mViewOrg = dc.mViewOrg;
        }

        public virtual void SetupTools(DrawTools.ToolsType type)
        {
            Tools.Setup(type);
        }

        public virtual void SetViewSize(double w, double h)
        {
            mViewWidth = w;
            mViewHeight = h;
        }

        public virtual void StartDraw()
        {
        }

        public virtual void EndDraw()
        {
        }

        public void Push()
        {
            if (mOnPush != null)
            {
                mOnPush(this);
            }
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

        public abstract CadVector WorldPointToDevPoint(CadVector pt);

        public abstract CadVector DevPointToWorldPoint(CadVector pt);

        public abstract CadVector WorldVectorToDevVector(CadVector pt);

        public abstract CadVector DevVectorToWorldVector(CadVector pt);

        public virtual double UnitToMilli(double d)
        {
            return d / UnitPerMilli;
        }

        public virtual double MilliToUnit(double d)
        {
            return d * UnitPerMilli;
        }

        public virtual void RecalcViewDirFromCameraDirection()
        {
            Vector3d ret = LookAt - Eye;
            ret.Normalize();
            mViewDir = ret;
        }

        public virtual void CopyCamera(DrawContext dc)
        {
            SetCamera(dc.Eye, dc.LookAt, dc.UpVector);
            mProjectionMatrix = dc.mProjectionMatrix;
            mProjectionMatrixInv = dc.mProjectionMatrixInv;
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

        public virtual void dump(string prefix)
        {
            ViewOrg.dump("ViewOrg");

            DbgOut.pln("View Width=" + mViewWidth.ToString() + " Height=" + mViewHeight.ToString());

            CadVector t = CadVector.Create(mViewDir);
            t.dump("ViewDir");
        }

        public abstract void Dispose();
    }
}
