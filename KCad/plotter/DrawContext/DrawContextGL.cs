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
            WoldScale = 0.2f;
            Tools.Setup(DrawTools.ToolsType.DARK_GL);

            Drawing = new DrawingGL(this);

            Eye = Vector3d.Zero;
            Eye.X = 0.0;
            Eye.Y = 0.0;
            Eye.Z = 40.0;

            LookAt = Vector3d.Zero;
            UpVector = Vector3d.UnitY;

            ProjectionNear = 10.0;
            ProjectionFar = 10000.0;


            mViewMatrix.GLMatrix = Matrix4d.LookAt(Eye, LookAt, UpVector);
            mViewMatrixInv.GLMatrix = Matrix4d.Invert(mViewMatrix.GLMatrix);

            LightPosition = new Vector4(150f, 150f, 150f, 1.0f);
            LightAmbient = new Color4(0.8f, 0.8f, 0.8f, 1.0f);
            LightDiffuse = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
            LightSpecular = new Color4(0.0f, 0.0f, 0.0f, 0.0f);

            MaterialAmbient = new Color4(0.24725f, 0.1995f, 0.0225f, 1.0f);
            MaterialDiffuse = new Color4(0.75164f, 0.60648f, 0.22648f, 1.0f);
            MaterialSpecular = new Color4(0.628281f, 0.555802f, 0.366065f, 1.0f);
            MaterialShininess = new Color4(51.4f, 51.4f, 51.4f, 1.0f);
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

        public override CadPoint CadPointToUnitPoint(CadPoint pt)
        {
            pt *= WoldScale;

            // 透視変換用にWが必要なので、Vector4に変換
            Vector4d ptv = (Vector4d)pt;

            ptv.W = 1.0f;

            ptv = ptv * mViewMatrix;
            ptv = ptv * mProjectionMatrix;

            ptv.X /= ptv.W;
            ptv.Y /= ptv.W;
            ptv.Z /= ptv.W;

            ptv.X = ptv.X * (ViewWidth / 2.0);
            ptv.Y = -ptv.Y * (ViewHeight / 2.0);
            ptv.Z = 0;

            CadPoint p = CadPoint.Create(ptv);

            p = p + mViewOrg;

            return p;
        }

        public override CadPoint UnitPointToCadPoint(CadPoint pt)
        {
            Vector4d t;

            t.X = LookAt.X;
            t.Y = LookAt.Y;
            t.Z = LookAt.Z;
            t.W = 1.0f;

            t = t * mViewMatrix;
            t = t * mProjectionMatrix;


            pt = pt - mViewOrg;
            pt.x = pt.x / (ViewWidth / 2.0);
            pt.y = -pt.y / (ViewHeight / 2.0);

            Vector4d vw = (Vector4d)pt;

            vw.W = t.W;
            vw.Z = t.Z;

            vw.X *= vw.W;
            vw.Y *= vw.W;

            vw = vw * mProjectionMatrixInv;
            vw = vw * mViewMatrixInv;

            vw /= WoldScale;

            CadPoint p = CadPoint.Create(vw);

            //DebugOut.Std.println("Wold");
            //p.dump(DebugOut.Std);

            return p;
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
            GL.Enable(EnableCap.Normalize);

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
