using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Plotter
{
    class DrawContextGL : DrawContext
    {
        Vector4 LightPosition;
        Color4 LightAmbient;    // 環境光
        Color4 LightDiffuse;    // 拡散光
        Color4 LightSpecular;   // 鏡面反射光

        Color4 MaterialAmbient;
        Color4 MaterialDiffuse;
        Color4 MaterialSpecular;
        Color4 MaterialShininess;

        public bool LightingEnable = true;


        public DrawContextGL()
        {
            //WoldScale = 0.2f;

            WoldScale = 1.0f;
            Tools.Setup(DrawTools.ToolsType.DARK_GL);

            mDrawing = new DrawingGL(this);

            Eye = Vector3d.Zero;
            Eye.X = 0.0;
            Eye.Y = 0.0;
            Eye.Z = 200.0;

            LookAt = Vector3d.Zero;
            UpVector = Vector3d.UnitY;

            ProjectionNear = 80.0;
            ProjectionFar = 1000.0;


            mViewMatrix.GLMatrix = Matrix4d.LookAt(Eye, LookAt, UpVector);
            mViewMatrixInv.GLMatrix = Matrix4d.Invert(mViewMatrix.GLMatrix);

            /*
            LightPosition = new Vector4(150f, 150f, 150f, 0.0f);
            LightAmbient = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
            LightDiffuse = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
            LightSpecular = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
            */

            LightPosition = new Vector4(150.0f, 150f, 500.0f, 0.0f);
            LightAmbient = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
            LightDiffuse = new Color4(0.7f, 0.7f, 0.7f, 1.0f);
            LightSpecular = new Color4(1.0f, 1.0f, 1.0f, 1.0f);

            MaterialAmbient = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
            MaterialDiffuse = new Color4(0.7f, 0.7f, 0.7f, 1.0f);
            MaterialSpecular = new Color4(0.0f, 0.0f, 0.0f, 1.0f);
            MaterialShininess = new Color4(0.1f, 0.1f, 0.1f, 1.0f);
            
            RecalcViewDirFromCameraDirection();
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
            Tools.Setup(DrawTools.ToolsType.DARK_GL);
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

        public override void EndDraw()
        {
        }

        public override CadPoint CadPointToUnitPoint(CadPoint pt)
        {
            CadPoint p = CadVectorToUnitVector(pt);
            p = p + mViewOrg;
            return p;
        }

        public override CadPoint UnitPointToCadPoint(CadPoint pt)
        {
            pt = pt - mViewOrg;
            return UnitVectorToCadVector(pt);
        }

        public override CadPoint CadVectorToUnitVector(CadPoint pt)
        {
            pt *= WoldScale;

            // 透視変換用にWが必要なので、Vector4に変換
            Vector4d wv = (Vector4d)pt;

            wv.W = 1.0f;

            Vector4d sv = wv * mViewMatrix;
            Vector4d pv = sv * mProjectionMatrix;

            Vector4d dv;

            dv.X = pv.X / pv.W;
            dv.Y = pv.Y / pv.W;
            dv.Z = pv.Z / pv.W;
            dv.W = pv.W;

            dv.X = dv.X * (ViewWidth / 2.0);
            dv.Y = -dv.Y * (ViewHeight / 2.0);
            dv.Z = 0;

            return CadPoint.Create(dv);
        }

        public override CadPoint UnitVectorToCadVector(CadPoint pt)
        {
            pt.x = pt.x / (ViewWidth / 2.0);
            pt.y = -pt.y / (ViewHeight / 2.0);

            Vector3d epv = pt.vector - Eye;

            Vector4d wv;

            wv.W = epv.Length;
            wv.Z = (ProjectionMatrix.M33 * (-wv.W)) + (ProjectionMatrix.M43);

            wv.X = pt.x * wv.W;
            wv.Y = pt.y * wv.W;

            wv = wv * mProjectionMatrixInv;
            wv = wv * mViewMatrixInv;

            wv /= WoldScale;

            return CadPoint.Create(wv);
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

            GL.Viewport(0, 0, (int)mViewWidth, (int)mViewHeight);

            double aspect = mViewWidth / mViewHeight;
            double fovy = Math.PI / 2.0f; // Yの傾き

            mProjectionMatrix.GLMatrix = Matrix4d.CreatePerspectiveFieldOfView(
                                            fovy,
                                            aspect,
                                            ProjectionNear,
                                            ProjectionFar
                                            );
            mProjectionMatrixInv.GLMatrix = Matrix4d.Invert(mProjectionMatrix.GLMatrix);
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

            qp = CadQuaternion.FromVector(Eye);
            qp = r * qp;
            qp = qp * q;
            Eye = qp.ToVector3d();

            qp = CadQuaternion.FromVector(UpVector);
            qp = r * qp;
            qp = qp * q;
            UpVector = qp.ToVector3d();

            Vector3d ev = LookAt - Eye;

            CadPoint a = CadPoint.Create(ev);
            CadPoint b = CadPoint.Create(UpVector);

            CadPoint axis = CadMath.Normal(a, b);

            if (!axis.IsZero())
            {

                q = CadQuaternion.RotateQuaternion(axis.vector, rx);

                r = q.Conjugate();

                qp = CadQuaternion.FromVector(Eye);
                qp = r * qp;
                qp = qp * q;

                Eye = qp.ToVector3d();

                qp = CadQuaternion.FromVector(UpVector);
                qp = r * qp;
                qp = qp * q;
                UpVector = qp.ToVector3d();
            }

            mViewMatrix.GLMatrix = Matrix4d.LookAt(Eye, LookAt, UpVector);
            mViewMatrixInv.GLMatrix = Matrix4d.Invert(mViewMatrix.GLMatrix);

            RecalcViewDirFromCameraDirection();
        }
    }
}
