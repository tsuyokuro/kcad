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

        public DrawContextGL()
        {
            Tools.Setup(DrawTools.ToolsType.DARK_GL);

            Drawing = new DrawingGL(this);

            Eye = Vector3.Zero;
            Eye.X = 4f;
            Eye.Y = 4f;
            Eye.Z = 4f;

            LookAt = Vector3.Zero;
            UpVector = Vector3.UnitY;

            ViewMatrix.GLMatrix = Matrix4.LookAt(Eye, LookAt, UpVector);
        }

        public override void startDraw()
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

        public override void endDraw()
        {
        }

        public override void setViewSize(double w, double h)
        {
            mViewWidth = w;
            mViewHeight = h;

            GL.Viewport(0, 0, (int)mViewWidth, (int)mViewHeight);

            float aspect = (float)(w / h);
            float fovy = (float)Math.PI / 2.0f; // Yの傾き

            ProjectionMatrix.GLMatrix = Matrix4.CreatePerspectiveFieldOfView(
                                            fovy,
                                            aspect,
                                            1.0f,       // near
                                            6400.0f     // far
                                            );
        }
    }
}
