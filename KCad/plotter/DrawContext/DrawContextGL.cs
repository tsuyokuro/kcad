using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using CadDataTypes;
using System.Windows.Forms;

namespace Plotter
{
    class DrawContextGL : DrawContext
    {
        protected Control ViewCtrl;

        Vector4 LightPosition;
        Color4 LightAmbient;    // 環境光
        Color4 LightDiffuse;    // 拡散光
        Color4 LightSpecular;   // 鏡面反射光

        Color4 MaterialAmbient;
        Color4 MaterialDiffuse;
        Color4 MaterialSpecular;
        Color4 MaterialShininess;

        public bool LightingEnable = true;

        public enum ProjectionType
        {
            TELESCOPE,
            STANDERD,
            WIDE_ANGLE,
        }

        public DrawContextGL()
        {
            Init(null);
        }

        public DrawContextGL(Control control)
        {
            Init(control);
        }

        public void Init(Control control)
        {
            ViewCtrl = control;

            WorldScale = 1.0f;
            Tools.Setup(DrawTools.ToolsType.DARK_GL);

            mDrawing = new DrawingGL(this);

            InitCamera(ProjectionType.STANDERD);

            mViewMatrix.GLMatrix = Matrix4d.LookAt(mEye, mLookAt, mUpVector);
            mViewMatrixInv.GLMatrix = Matrix4d.Invert(mViewMatrix.GLMatrix);

            /*
            LightPosition = new Vector4(150f, 150f, 150f, 0.0f);
            LightAmbient = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
            LightDiffuse = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
            LightSpecular = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
            */

            LightPosition = new Vector4(100.0f, 500f, 150.0f, 0.0f);
            LightAmbient = new Color4(0.5f, 0.5f, 0.5f, 1.0f);
            LightDiffuse = new Color4(0.7f, 0.7f, 0.7f, 1.0f);
            LightSpecular = new Color4(1.0f, 1.0f, 1.0f, 1.0f);

            MaterialAmbient = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
            MaterialDiffuse = new Color4(0.7f, 0.7f, 0.7f, 1.0f);
            MaterialSpecular = new Color4(0.0f, 0.0f, 0.0f, 1.0f);
            MaterialShininess = new Color4(0.1f, 0.1f, 0.1f, 1.0f);

            RecalcViewDirFromCameraDirection();
        }

        public void InitCamera(ProjectionType type)
        {
            double ez = 250;
            double near = 0.1;
            double far = 2000;

            mLookAt = Vector3d.Zero;
            mUpVector = Vector3d.UnitY;

            mProjectionNear = near;
            mProjectionFar = far;

            mEye = Vector3d.Zero;

            // FovY 視野角を指定
            // 初期カメラ位置を調整
            switch (type)
            {
                case ProjectionType.TELESCOPE:
                    // 望遠
                    mEye.Z = ez;
                    mFovY = Math.PI / 4;
                    break;

                case ProjectionType.STANDERD:
                    // 標準
                    mEye.Z = ez;
                    mFovY = Math.PI / 3;
                    break;

                case ProjectionType.WIDE_ANGLE:
                    // 広角
                    mEye.Z = ez * 0.75;
                    mFovY = Math.PI / 2;
                    break;
            }
        }

        public GLPen Pen(int id)
        {
            return Tools.glpen(id);
        }

        public Color4 Color(int id)
        {
            return Tools.glcolor(id);
        }

        public override void SetupTools(DrawTools.ToolsType type)
        {
            if (DrawTools.IsTypeForGL(type))
            {
                Tools.Setup(type);
            }
        }

        public override void StartDraw()
        {
            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.DepthTest);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref mViewMatrix.GLMatrix);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref mProjectionMatrix.GLMatrix);

            SetupLight();
        }

        public override CadVector WorldPointToDevPoint(CadVector pt)
        {
            CadVector p = WorldVectorToDevVector(pt);
            p = p + mViewOrg;
            return p;
        }

        public override CadVector DevPointToWorldPoint(CadVector pt)
        {
            pt = pt - mViewOrg;
            return DevVectorToWorldVector(pt);
        }

        public override CadVector WorldVectorToDevVector(CadVector pt)
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

        /*
        public override CadVector DevVectorToWorldVector(CadVector pt)
        {
            pt.x = pt.x / DeviceScaleX;
            pt.y = pt.y / DeviceScaleY;

            Vector3d epv = pt.vector - mEye;

            Vector4d wv;

            wv.W = epv.Length;

            // mProjectionMatrixInvに掛けて wv.W=1.0 となる z を求める
            wv.Z = (1.0 - (wv.W * mProjectionMatrixInv.M44)) / mProjectionMatrixInv.M34;

            wv.X = pt.x * wv.W;
            wv.Y = pt.y * wv.W;

            wv = wv * mProjectionMatrixInv;
            wv = wv * mViewMatrixInv;

            wv /= WorldScale;

            return CadVector.Create(wv);
        }
        */

        public override CadVector DevVectorToWorldVector(CadVector pt)
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

        private void SetupLight()
        {
            if (!LightingEnable)
            {
                return;
            }

            // 裏面を描かない
            //GL.Enable(EnableCap.CullFace);
            //GL.CullFace(CullFaceMode.Back);
            //GL.FrontFace(FrontFaceDirection.Ccw);

            //法線の正規化
            //GL.Enable(EnableCap.Normalize);

            GL.Light(LightName.Light0, LightParameter.Position, LightPosition);
            GL.Light(LightName.Light0, LightParameter.Ambient, LightAmbient);
            GL.Light(LightName.Light0, LightParameter.Diffuse, LightDiffuse);
            GL.Light(LightName.Light0, LightParameter.Specular, LightSpecular);

            /*
            GL.Material(MaterialFace.Front, MaterialParameter.Ambient, MaterialAmbient);
            GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, MaterialDiffuse);
            GL.Material(MaterialFace.Front, MaterialParameter.Specular, MaterialSpecular);
            GL.Material(MaterialFace.Front, MaterialParameter.Shininess, MaterialShininess);
            */
        }

        public override void SetViewSize(double w, double h)
        {
            mViewWidth = w;
            mViewHeight = h;

            mViewOrg.x = w / 2.0;
            mViewOrg.y = h / 2.0;

            DeviceScaleX = w / 2.0;
            DeviceScaleY = -h / 2.0;

            GL.Viewport(0, 0, (int)mViewWidth, (int)mViewHeight);

            RecalcProjectionMatrix();
        }

        private void RecalcProjectionMatrix()
        {
            double aspect = mViewWidth / mViewHeight;

            mProjectionMatrix.GLMatrix = Matrix4d.CreatePerspectiveFieldOfView(
                                            mFovY,
                                            aspect,
                                            mProjectionNear,
                                            mProjectionFar
                                            );
            mProjectionMatrixInv.GLMatrix = Matrix4d.Invert(mProjectionMatrix.GLMatrix);

            //DOut.pl("DrawContextGL mProjectionMatrix");
            //mProjectionMatrix.dump();

            //DOut.pl("DrawContextGL mProjectionMatrixInv");
            //mProjectionMatrixInv.dump();


            //mProjectionW = mEye.Length - mProjectionNear;

            // mProjectionMatrixInvに掛けて wv.W=1.0 となる z を求める
            //mProjectionZ = (1.0 - (mProjectionW * mProjectionMatrixInv.M44)) / mProjectionMatrixInv.M34;


            Vector4d wv = Vector4d.Zero;
            wv.W = 1.0f;
            wv.Z = -mEye.Length;

            Vector4d pv = wv * mProjectionMatrix;

            mProjectionW = pv.W;
            mProjectionZ = pv.Z;

            DOut.pl($"DrawContextGL mProjectionW={mProjectionW} mProjectionZ={mProjectionZ}");
        }

        private void RecalcViewTransMatrix()
        {
            mViewMatrix.GLMatrix = Matrix4d.LookAt(mEye, mLookAt, mUpVector);
            mViewMatrixInv.GLMatrix = Matrix4d.Invert(mViewMatrix.GLMatrix);
        }

        public void RotateEyePoint(Vector2 prev, Vector2 current)
        {
            Vector2 d = current - prev;

            double ry = (d.X / 10.0) * (Math.PI / 20);
            double rx = (d.Y / 10.0) * (Math.PI / 20);

            CadQuaternion q;
            CadQuaternion r;
            CadQuaternion qp;

            q = CadQuaternion.RotateQuaternion(Vector3d.UnitY, ry);

            r = q.Conjugate();

            qp = CadQuaternion.FromVector(mEye);
            qp = r * qp;
            qp = qp * q;
            mEye = qp.ToVector3d();

            qp = CadQuaternion.FromVector(mUpVector);
            qp = r * qp;
            qp = qp * q;
            mUpVector = qp.ToVector3d();

            Vector3d ev = mLookAt - mEye;

            CadVector a = CadVector.Create(ev);
            CadVector b = CadVector.Create(mUpVector);

            CadVector axis = CadMath.Normal(a, b);

            if (!axis.IsZero())
            {

                q = CadQuaternion.RotateQuaternion(axis.vector, rx);

                r = q.Conjugate();

                qp = CadQuaternion.FromVector(mEye);
                qp = r * qp;
                qp = qp * q;

                mEye = qp.ToVector3d();

                qp = CadQuaternion.FromVector(mUpVector);
                qp = r * qp;
                qp = qp * q;
                mUpVector = qp.ToVector3d();
            }

            RecalcViewTransMatrix();

            RecalcViewDirFromCameraDirection();
        }

        public void MoveForwardEyePoint(double d, bool withLookAt = false)
        {
            Vector3d dv = ViewDir * d;

            if (withLookAt)
            {
                mEye += dv;
                mLookAt += dv;                
            }
            else
            {
                Vector3d eye = mEye + dv;

                Vector3d viewDir = mLookAt - eye;

                viewDir.Normalize();

                if ((ViewDir - viewDir).Length > 1.0)
                {
                    return;
                }

                mEye += dv;
            }

            RecalcViewTransMatrix();

            RecalcProjectionMatrix();
        }

        public override void Dispose()
        {
            Tools.Dispose();
        }
    }
}