using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Windows.Forms;

namespace Plotter
{
    class PlotterViewGL : GLControl, IPlotterView
    {
        private DrawContext mDrawContext = new DrawContextGL();

        private PlotterController mController = null;

        public DrawContext DrawContext
        {
            get
            {
                return mDrawContext;
            }
        }

        public PaperPageSize PageSize
        {
            get
            {
                return mDrawContext.PageSize;
            }
        }

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

        public DrawContext startDraw()
        {
            throw new NotImplementedException();
        }

        public void endDraw()
        {
            throw new NotImplementedException();
        }

        public void SetController(PlotterController controller)
        {
            mController = controller;
        }
    }
}
