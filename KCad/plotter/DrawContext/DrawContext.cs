using OpenTK;
using System;
using CadDataTypes;
using System.Drawing;

namespace Plotter
{
    public abstract class DrawContext : IDisposable
    {
        protected Action<DrawContext> mPushDraw;

        public enum ProjectionType
        {
            Orthographic,
            Perspective,
        }

        public Action<DrawContext> PushDraw
        {
            set => mPushDraw = value;
            get => mPushDraw;
        }

        // 画素/Milli
        // 1ミリあたりの画素数
        protected double mUnitPerMilli = 1;
        public virtual double UnitPerMilli
        {
            set => mUnitPerMilli = value;
            get => mUnitPerMilli;
        }

        // 視点
        public const double STD_EYE_DIST = 250.0;
        protected Vector3d mEye = Vector3d.UnitZ * STD_EYE_DIST;
        public Vector3d Eye => mEye;

        // 注視点
        protected Vector3d mLookAt = Vector3d.Zero;
        public Vector3d LookAt => mLookAt;

        // 投影面までの距離
        protected double mProjectionNear = 0.1;
        public double ProjectionNear => mProjectionNear;

        // 視野空間の遠方側クリップ面までの距離
        protected double mProjectionFar = 2000.0;
        public double ProjectionFar => mProjectionFar;

        // 画角 大きければ広角レンズ、小さければ望遠レンズ
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
        public ref Matrix4d ViewMatrixRef => ref mViewMatrix.Matrix;

        // 視点座標系からワールド座標系への変換行列
        protected UMatrix4 mViewMatrixInv = new UMatrix4();
        public UMatrix4 ViewMatrixInv => mViewMatrixInv;

        // 視点座標系から投影座標系への変換行列
        public UMatrix4 mProjectionMatrix = new UMatrix4();
        public UMatrix4 ProjectionMatrix => mProjectionMatrix;
        public ref Matrix4d ProjectionMatrixRef => ref mProjectionMatrix.Matrix;

        // 投影座標系から視点座標系への変換行列
        protected UMatrix4 mProjectionMatrixInv = new UMatrix4();
        public UMatrix4 ProjectionMatrixInv => mProjectionMatrixInv;

        protected double mProjectionW = 1.0;
        public double ProjectionW => mProjectionW;

        protected double mProjectionZ = 0;
        public double ProjectionZ => mProjectionZ;

        // Screen 座標系の原点 
        // 座標系の原点がView座標上で何処にあるかを示す
        protected CadVector mViewOrg;
        public virtual CadVector ViewOrg
        {
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

        public virtual void Active()
        {

        }

        public virtual void Deactive()
        {

        }

        public virtual void SetViewOrg(CadVector org)
        {
            mViewOrg = org;
        }

        public virtual void CopyProjectionMetrics(DrawContext dc)
        {
            mUnitPerMilli = dc.mUnitPerMilli;
            mProjectionNear = dc.mProjectionNear;
            mProjectionFar = dc.mProjectionFar;
            mFovY = dc.mFovY;
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
            mPushDraw?.Invoke(this);
        }

        public virtual CadVector WorldPointToDevPoint(CadVector pt)
        {
            CadVector p = WorldVectorToDevVector(pt);
            p = p + mViewOrg;
            return p;
        }

        public virtual CadVector DevPointToWorldPoint(CadVector pt)
        {
            pt = pt - mViewOrg;
            return DevVectorToWorldVector(pt);
        }

        public virtual CadVector WorldVectorToDevVector(CadVector pt)
        {
            pt *= WorldScale;

            Vector4d wv = (Vector4d)pt;

            wv.W = 1.0f;

            Vector4d sv = wv * mViewMatrix;
            Vector4d pv = sv * mProjectionMatrix;

            Vector4d dv;

            dv.X = pv.X / pv.W;
            dv.Y = pv.Y / pv.W;
            dv.Z = pv.Z / pv.W;
            dv.W = pv.W;

            dv.X = dv.X * DeviceScaleX;
            dv.Y = dv.Y * DeviceScaleY;
            dv.Z = 0;

            return CadVector.Create(dv);
        }

        public virtual CadVector DevVectorToWorldVector(CadVector pt)
        {
            pt.x = pt.x / DeviceScaleX;
            pt.y = pt.y / DeviceScaleY;

            Vector4d wv;

            wv.W = mProjectionW;
            wv.Z = mProjectionZ;

            wv.X = pt.x * wv.W;
            wv.Y = pt.y * wv.W;

            wv = wv * mProjectionMatrixInv;
            wv = wv * mViewMatrixInv;

            wv /= WorldScale;

            return CadVector.Create(wv);
        }

        public virtual void CalcViewDir()
        {
            Vector3d ret = mLookAt - mEye;
            ret.Normalize();
            mViewDir = ret;
        }

        public abstract void CalcProjectionMatrix();

        public virtual void CalcProjectionZW()
        {
            Vector4d wv = Vector4d.Zero;
            wv.W = 1.0f;
            wv.Z = -((mEye - mLookAt).Length);

            Vector4d pv = wv * mProjectionMatrix;

            mProjectionW = pv.W;
            mProjectionZ = pv.Z;
        }

        public virtual void CalcViewMatrix()
        {
            mViewMatrix = Matrix4d.LookAt(mEye, mLookAt, mUpVector);
            mViewMatrixInv = mViewMatrix.Invert();
        }

        public virtual void CopyCamera(DrawContext dc)
        {
            SetCamera(dc.mEye, dc.mLookAt, dc.mUpVector);
        }

        public virtual void CopyProjectionMatrix(DrawContext dc)
        {
            mProjectionMatrix = dc.mProjectionMatrix;
            mProjectionMatrixInv = dc.mProjectionMatrixInv;
        }

        public virtual void CopyVewMatrix(DrawContext dc)
        {
            mViewMatrix = dc.mViewMatrix;
            mViewMatrixInv = dc.mViewMatrixInv;
        }

        public virtual void SetCamera(Vector3d eye, Vector3d lookAt, Vector3d upVector)
        {
            mEye = eye;
            mLookAt = lookAt;
            mUpVector = upVector;

            CalcViewMatrix();
            CalcViewDir();
            CalcProjectionZW();
        }

        public virtual double DevSizeToWoldSize(double s)
        {
            CadVector size = DevVectorToWorldVector(CadVector.UnitX * s);
            return size.Norm();
        }

        public virtual DrawContext CreatePrinterContext(CadSize2D pageSize, CadSize2D deviceSize)
        {
            return null;
        }

        public virtual void dump()
        {
            ViewOrg.dump("ViewOrg");

            DOut.pl("View Width=" + mViewWidth.ToString() + " Height=" + mViewHeight.ToString());

            CadVector t = CadVector.Create(mViewDir);
            t.dump("ViewDir");

            DOut.pl("ViewMatrix");
            mViewMatrix.dump();

            DOut.pl("ProjectionMatrix");
            mProjectionMatrix.dump();

            DOut.pl($"ProjectionW={mProjectionW}");
            DOut.pl($"ProjectionZ={mProjectionZ}");
        }

        public abstract void Dispose();

        public abstract DrawPen GetPen(int idx);
        public abstract DrawBrush GetBrush(int idx);
    }
}
