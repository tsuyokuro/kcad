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

        bool IsMouseDown = false;

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
            SwapBuffers();
        }

        private void onMouseUp(object sender, MouseEventArgs e)
        {
            IsMouseDown = false;
        }

        private void onMouseDown(object sender, MouseEventArgs e)
        {
            PrevMousePos.set(e.X, e.Y, 0);
            IsMouseDown = true;
        }

        private void onLoad(object sender, EventArgs e)
        {
            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.DepthTest);
            SwapBuffers();
        }

        private void onMouseMove(object sender, MouseEventArgs e)
        {
            if (IsMouseDown)
            {
                //CadPoint t = CadPoint.Create(e.X, e.Y, 0);
                //CadPoint d = t - PrevMousePos;

                //double ry = (-d.x / 10.0) * (Math.PI / 20);
                //double rx = (d.y / 10.0) * (Math.PI / 20);

                //mDrawContext.RotateEyePoint(rx, ry, 0);

                //startDraw();
                //mController.draw(mDrawContext);
                //endDraw();

                //PrevMousePos = t;


                CadPoint t = CadPoint.Create(e.X, e.Y, 0);

                Vector2 prev = default(Vector2);

                prev.X = (float)PrevMousePos.x;
                prev.Y = (float)PrevMousePos.y;

                Vector2 current = default(Vector2);

                current.X = (float)t.x;
                current.Y = (float)t.y;

                mDrawContext.RotateEyePoint(prev, current);

                startDraw();
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
            mDrawContext.setViewSize(Size.Width, Size.Height);

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
