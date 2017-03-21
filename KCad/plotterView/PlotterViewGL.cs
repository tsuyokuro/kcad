using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Windows.Forms;

namespace Plotter
{
    class PlotterViewGL : GLControl, IPlotterView
    {
        private DrawContextGL mDrawContext = new DrawContextGL();

        private PlotterController mController = null;

        CadPoint PrevMousePos = default(CadPoint);

        MouseButtons DownButton = MouseButtons.None;




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

        public System.Windows.Forms.Control FromsControl
        {
            get
            {
                return (System.Windows.Forms.Control)this;
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
            MouseDown += onMouseDown;
            MouseUp += onMouseUp;
            //SwapBuffers();
        }

        private void onMouseUp(object sender, MouseEventArgs e)
        {
            DownButton = MouseButtons.None;

            startDraw();

            mController.Mouse.up(mDrawContext, e.Button, e.X, e.Y);

            endDraw();
        }

        private void onMouseDown(object sender, MouseEventArgs e)
        {
            PrevMousePos.set(e.X, e.Y, 0);
            DownButton = e.Button;

            if (DownButton != MouseButtons.Middle)
            {
                startDraw();

                mController.Mouse.down(mDrawContext, e.Button, e.X, e.Y);

                endDraw();
            }
        }

        private void onLoad(object sender, EventArgs e)
        {
            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.DepthTest);
            SwapBuffers();
        }

        private void onMouseMove(object sender, MouseEventArgs e)
        {
            if (DownButton == MouseButtons.Middle)
            {
                CadPoint t = CadPoint.Create(e.X, e.Y, 0);

                Vector2 prev = default(Vector2);

                prev.X = (float)PrevMousePos.x;
                prev.Y = (float)PrevMousePos.y;

                Vector2 current = default(Vector2);

                current.X = (float)t.x;
                current.Y = (float)t.y;

                mDrawContext.RotateEyePoint(prev, current);

                startDraw();
                mController.Clear(mDrawContext);
                mController.Draw(mDrawContext);
                endDraw();

                PrevMousePos = t;
            }
        }

        private void onPaint(object sender, PaintEventArgs e)
        {
            //SwapBuffers();
        }

        private void onResize(object sender, EventArgs e)
        {
            mDrawContext.SetViewSize(Size.Width, Size.Height);

            if (mController != null)
            {
                DrawContext dc = startDraw();
                mController.Clear(dc);
                mController.Draw(dc);
                endDraw();
            }
        }

        public DrawContext startDraw()
        {
            MakeCurrent();
            mDrawContext.StartDraw();
            return mDrawContext;
        }

        public void endDraw()
        {
            mDrawContext.EndDraw();
            SwapBuffers();
        }

        public void SetController(PlotterController controller)
        {
            mController = controller;
        }
    }
}
