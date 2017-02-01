using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Windows.Forms;

namespace Plotter
{
    class PlotterViewGL : GLControl
    {
        private DrawContext mDrawContext = new DrawContextGL();

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
            mDrawContext.setViewSize(Size.Width, Size.Height);
        }
    }
}
