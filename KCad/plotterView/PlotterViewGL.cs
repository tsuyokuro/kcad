using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Windows.Forms;

namespace Plotter
{
    class PlotterViewGL : GLControl
    {
        Matrix4 Projection;
        Matrix4 ModelView;

        Vector3 Eye = default(Vector3);
        Vector3 LookAt = default(Vector3);
        Vector3 UpVector = default(Vector3);

        public static PlotterViewGL Create()
        {
            GraphicsMode mode = GraphicsMode.Default;
            return Create(mode);
        }

        public static PlotterViewGL Create(GraphicsMode mode)
        {
            PlotterViewGL v = new PlotterViewGL(mode);
            return v;
        }

        private PlotterViewGL(GraphicsMode mode) : base(mode)
        {
            Load += onLoad;
            Resize += onResize;
            Paint += onPaint;
            MouseMove += onMouseMove;
            SwapBuffers();
        }

        private void onLoad(object sender, EventArgs e)
        {
            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.DepthTest);
            SwapBuffers();
        }

        private void onMouseMove(object sender, MouseEventArgs e)
        {
        }

        private void onPaint(object sender, PaintEventArgs e)
        {
            SwapBuffers();
        }

        private void onResize(object sender, EventArgs e)
        {
        }
    }
}
