using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Plotter
{
    class DrawContextGL : DrawContext
    {
        public Vector3 Eye = default(Vector3);
        public Vector3 LookAt = default(Vector3);
        public Vector3 UpVector = default(Vector3);

        public Vector3 GazeVector = default(Vector3); 

        float ProjectionNear = 10.0f;
        float ProjectionFar = 10000.0f;


        Vector4 lightPosition;
        Color4 lightAmbient;
        Color4 lightDiffuse;
        Color4 lightSpecular;

        Color4 materialAmbient;
        Color4 materialDiffuse;
        Color4 materialSpecular;
        float materialShininess;

        public bool LightingEnable = false;

        public DrawContextGL()
        {
            WoldScale = 0.2f;
            Tools.Setup(DrawTools.ToolsType.DARK_GL);

            Drawing = new DrawingGL(this);

            Eye = Vector3.Zero;
            Eye.X = 20f;
            Eye.Y = 20f;
            Eye.Z = 20f;

            LookAt = Vector3.Zero;
            UpVector = Vector3.UnitY;

            ViewMatrix.GLMatrix = Matrix4.LookAt(Eye, LookAt, UpVector);

            RecalcGazeVector();


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
            GL.LoadMatrix(ref ViewMatrix.GLMatrix);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref ProjectionMatrix.GLMatrix);
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
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);

            //法線の正規化
            GL.Enable(EnableCap.Normalize);

            GL.Light(LightName.Light0, LightParameter.Position, lightPosition);
            GL.Light(LightName.Light0, LightParameter.Ambient, lightAmbient);
            GL.Light(LightName.Light0, LightParameter.Diffuse, lightDiffuse);
            GL.Light(LightName.Light0, LightParameter.Specular, lightSpecular);

            GL.Material(MaterialFace.Front, MaterialParameter.Ambient, materialAmbient);
            GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, materialDiffuse);
            GL.Material(MaterialFace.Front, MaterialParameter.Specular, materialSpecular);
            GL.Material(MaterialFace.Front, MaterialParameter.Shininess, materialShininess);
        }

        public override void setViewSize(double w, double h)
        {
            Console.WriteLine("DrawContextGL setViewSize w=" + w.ToString() + " h=" + h.ToString());

            mViewWidth = w;
            mViewHeight = h;

            GL.Viewport(0, 0, (int)mViewWidth, (int)mViewHeight);

            float aspect = (float)(mViewWidth / mViewHeight);
            float fovy = (float)Math.PI / 2.0f; // Yの傾き

            ProjectionMatrix.GLMatrix = Matrix4.CreatePerspectiveFieldOfView(
                                            fovy,
                                            aspect,
                                            ProjectionNear,
                                            ProjectionFar
                                            );
        }

        public void RotateEyePoint(Vector2 prev, Vector2 current)
        {
            Vector2 d = current - prev;

            double ry = (-d.X / 10.0) * (Math.PI / 20);
            double rx = (-d.Y / 10.0) * (Math.PI / 20);

            Matrix4 my = Matrix4.CreateRotationY((float)ry);

            Eye = Vector3.TransformPosition(Eye, my);

            Vector3 t = Eye - LookAt;

            Vector3 axis = t;

            axis.X = t.Z;
            axis.Z = -t.X;
            axis.Y = 0;


            Matrix4 mx = Matrix4.CreateFromAxisAngle(axis, (float)rx);


            Eye = Vector3.TransformPosition(Eye, mx);

            ViewMatrix.GLMatrix = Matrix4.LookAt(Eye, LookAt, UpVector);

            RecalcGazeVector();
        }

        private void RecalcGazeVector()
        {
            GazeVector = LookAt - Eye;
            GazeVector.Normalize();
        }
    }
}
