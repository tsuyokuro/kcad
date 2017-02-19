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
        Vector3 Eye = default(Vector3);
        Vector3 LookAt = default(Vector3);
        Vector3 UpVector = default(Vector3);

        float ProjectionNear = 10.0f;
        float ProjectionFar = 10000.0f;


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
        }
    }
}
