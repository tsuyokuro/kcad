using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Plotter
{
    class DrawContextGL : DrawContext
    {
        Vector4 lightPosition;
        Color4 lightAmbient;
        Color4 lightDiffuse;
        Color4 lightSpecular;

        Color4 materialAmbient;
        Color4 materialDiffuse;
        Color4 materialSpecular;
        double materialShininess;

        public bool LightingEnable = true;


        public DrawContextGL()
        {
            WoldScale = 0.2f;
            Tools.Setup(DrawTools.ToolsType.DARK_GL);

            Drawing = new DrawingGL(this);

            Eye = Vector3d.Zero;
            Eye.X = 0.0;
            Eye.Y = 0.0;
            Eye.Z = 40.0;

            LookAt = Vector3d.Zero;
            UpVector = Vector3d.UnitY;

            mViewMatrix.GLMatrix = Matrix4d.LookAt(Eye, LookAt, UpVector);


            lightPosition = new Vector4(200.0f, 150f, 500.0f, 0.0f);
            lightAmbient = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
            lightDiffuse = new Color4(0.7f, 0.7f, 0.7f, 1.0f);
            lightSpecular = new Color4(1.0f, 1.0f, 1.0f, 1.0f);

            materialAmbient = new Color4(0.24725f, 0.1995f, 0.0225f, 1.0f);
            materialDiffuse = new Color4(0.75164f, 0.60648f, 0.22648f, 1.0f);
            materialSpecular = new Color4(0.628281f, 0.555802f, 0.366065f, 1.0f);
            materialShininess = 51.4f;
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
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.DepthTest);

            SetupLight();

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref mViewMatrix.GLMatrix);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref mProjectionMatrix.GLMatrix);
        }

        public override void EndDraw()
        {
        }

        private void SetupLight()
        {
            if (!LightingEnable)
            {
                return;
            }

            // 裏面を描かない
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.FrontFace(FrontFaceDirection.Ccw);

            //ライティングON Light0を有効化
            //GL.Enable(EnableCap.Lighting);
            //GL.Enable(EnableCap.Light0);

            //法線の正規化
            GL.Enable(EnableCap.Normalize);

            GL.Light(LightName.Light0, LightParameter.Position, lightPosition);
            GL.Light(LightName.Light0, LightParameter.Ambient, lightAmbient);
            GL.Light(LightName.Light0, LightParameter.Diffuse, lightDiffuse);
            GL.Light(LightName.Light0, LightParameter.Specular, lightSpecular);

            /*
            GL.Material(MaterialFace.Front, MaterialParameter.Ambient, materialAmbient);
            GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, materialDiffuse);
            GL.Material(MaterialFace.Front, MaterialParameter.Specular, materialSpecular);
            GL.Material(MaterialFace.Front, MaterialParameter.Shininess, materialShininess);
            */
        }

        public override void setViewSize(double w, double h)
        {
            Console.WriteLine("DrawContextGL setViewSize w=" + w.ToString() + " h=" + h.ToString());

            mViewWidth = w;
            mViewHeight = h;

            GL.Viewport(0, 0, (int)mViewWidth, (int)mViewHeight);

            double aspect = mViewWidth / mViewHeight;
            double fovy = Math.PI / 2.0f; // Yの傾き

            mProjectionMatrix.GLMatrix = Matrix4d.CreatePerspectiveFieldOfView(
                                            fovy,
                                            aspect,
                                            ProjectionNear,
                                            ProjectionFar
                                            );
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

            RecalcViewDirFromCameraDirection();
        }
    }
}
