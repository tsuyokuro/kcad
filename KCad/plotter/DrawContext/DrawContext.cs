using OpenTK;
using System;
using CadDataTypes;

namespace Plotter
{
    public abstract class DrawContext : IDisposable
    {
        protected Action<DrawContext> mOnPush;

        public Action<DrawContext> OnPush
        {
            set => mOnPush = value;
            get => mOnPush;
        }

        // 画素/Milli
        // 1ミリあたりの画素数
        protected double mUnitPerMilli = 1;
        public double UnitPerMilli
        {
            set => mUnitPerMilli = value;
            get => mUnitPerMilli;
        }

        public const double STD_EYE_DIST = 250.0;

        // 視点
        protected Vector3d mEye = Vector3d.UnitZ * STD_EYE_DIST;
        Vector3d Eye => mEye;

        // 注視点
        protected Vector3d mLookAt = Vector3d.Zero;
        Vector3d LookAt => mLookAt;

        // 投影面までの距離
        protected double mProjectionNear = 0.1;
        public double ProjectionNear => mProjectionNear;

        // 視野空間の遠方側クリップ面までの距離
        protected double mProjectionFar = 2000.0;
        public double ProjectionFar => mProjectionFar;

        // 視野角　大きければ広角レンズ、小さければ望遠レンズ
        protected double mFovY = Math.PI / 4;
        public double FovY => mFovY;

        // 上を示す Vector
        protected Vector3d mUpVector = Vector3d.UnitY;
        public Vector3d UpVector => mUpVector;

        // 投影スクリーンの向き
        protected Vector3d mViewDir = default(Vector3d);
        public virtual Vector3d ViewDir => mViewDir;

        // ワールド座標系から視点座標系への変換(ビュー変換)行列
        protected UMatrix4 mViewMatrix = new UMatrix4();
        public UMatrix4 ViewMatrix => mViewMatrix;

        // 視点座標系からワールド座標系への変換行列
        protected UMatrix4 mViewMatrixInv = new UMatrix4();
        public UMatrix4 ViewMatrixInv => mViewMatrixInv;

        // 視点座標系から投影座標系への変換行列
        protected UMatrix4 mProjectionMatrix = new UMatrix4();
        public UMatrix4 ProjectionMatrix => mProjectionMatrix;

        // 投影座標系から視点座標系への変換行列
        protected UMatrix4 mProjectionMatrixInv = new UMatrix4();
        public UMatrix4 ProjectionMatrixInv => mProjectionMatrixInv;

        protected double mProjectionW = 1.0;
        protected double mProjectionZ = 0;

        // Screen 座標系の原点 
        // 座標系の原点がView座標上で何処にあるかを示す
        protected CadVector mViewOrg;
        public CadVector ViewOrg
        {
            set => mViewOrg = value;
            get => mViewOrg;
        }

        public double mViewWidth = 32;
        public double mViewHeight = 32;

        // Screenのサイズ
        public double ViewWidth => mViewWidth;
        public double ViewHeight => mViewHeight;

        // 縮尺
        public double WorldScale = 1.0;

        // 画面に描画する際の係数
        public double DeviceScaleX = 1.0;
        public double DeviceScaleY = -1.0;

        public DrawTools Tools = new DrawTools();

        protected IDrawing mDrawing;
        public IDrawing Drawing => mDrawing;

        public virtual void CopyFrom(DrawContext dc)
        {
            CopyMetrics(dc);
            SetViewSize(dc.mViewWidth, dc.mViewHeight);
        }

        public virtual void CopyMetrics(DrawContext dc)
        {
            //PageSize = dc.PageSize.clone();
            mUnitPerMilli = dc.mUnitPerMilli;
            mEye = dc.mEye;
            mLookAt = dc.mLookAt;
            mUpVector = dc.mUpVector;
            mProjectionNear = dc.mProjectionNear;
            mProjectionFar = dc.mProjectionFar;
            mFovY = dc.mFovY;
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
            mOnPush?.Invoke(this);
        }

        public abstract CadVector WorldPointToDevPoint(CadVector pt);

        public abstract CadVector DevPointToWorldPoint(CadVector pt);

        public abstract CadVector WorldVectorToDevVector(CadVector pt);

        public abstract CadVector DevVectorToWorldVector(CadVector pt);

        public virtual double UnitToMilli(double d)
        {
            return d / mUnitPerMilli;
        }

        public virtual double MilliToUnit(double d)
        {
            return d * mUnitPerMilli;
        }

        public virtual void RecalcViewDirFromCameraDirection()
        {
            Vector3d ret = mLookAt - mEye;
            ret.Normalize();
            mViewDir = ret;
        }

        public virtual void CopyCamera(DrawContext dc)
        {
            SetCamera(dc.mEye, dc.mLookAt, dc.mUpVector);
            mProjectionMatrix = dc.mProjectionMatrix;
            mProjectionMatrixInv = dc.mProjectionMatrixInv;
        }

        public virtual void SetCamera(Vector3d eye, Vector3d lookAt, Vector3d upVector)
        {
            mEye = eye;
            mLookAt = lookAt;
            mUpVector = upVector;

            mViewMatrix.GLMatrix = Matrix4d.LookAt(mEye, mLookAt, mUpVector);

            mViewMatrixInv.GLMatrix = Matrix4d.Invert(mViewMatrix.GLMatrix);

            RecalcViewDirFromCameraDirection();
        }

        public virtual void dump(string prefix)
        {
            ViewOrg.dump("ViewOrg");

            DOut.pl("View Width=" + mViewWidth.ToString() + " Height=" + mViewHeight.ToString());

            CadVector t = CadVector.Create(mViewDir);
            t.dump("ViewDir");
        }

        public abstract void Dispose();
    }
}
