using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Windows.Forms;
using CadDataTypes;
using Plotter.Controller;

namespace Plotter
{
    class PlotterViewGL : GLControl, IPlotterView
    {
        private DrawContextGL mDrawContext = null;

        private PlotterController mController = null;

        CadVector PrevMousePos = default(CadVector);

        MouseButtons DownButton = MouseButtons.None;

        public DrawContext DrawContext
        {
            get
            {
                return mDrawContext;
            }
        }

        public Control FromsControl
        {
            get
            {
                return this;
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
            v.MakeCurrent();
            return v;
        }

        private PlotterViewGL(GraphicsMode mode) : base(mode)
        {
            mDrawContext = new DrawContextGL(this);

            Load += OnLoad;
            Resize += OnResize;
            Paint += OnPaint;
            MouseMove += OnMouseMove;
            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
            MouseWheel += OnMouseWheel;

            mDrawContext.OnPush = OnPushDraw;
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            DownButton = MouseButtons.None;
            mController.Mouse.MouseUp(mDrawContext, e.Button, e.X, e.Y);

            Redraw();
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            PrevMousePos.Set(e.X, e.Y, 0);
            DownButton = e.Button;

            if (DownButton != MouseButtons.Middle)
            {
                mController.Mouse.MouseDown(mDrawContext, e.Button, e.X, e.Y);

            }
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            if (CadKeyboard.IsCtrlKeyDown())
            {
                if (e.Delta > 0)
                {
                    mDrawContext.MoveForwardEyePoint(3);
                }
                else if (e.Delta < 0)
                {
                    mDrawContext.MoveForwardEyePoint(-3);
                }

                Redraw();
            }
        }

        private void OnLoad(object sender, EventArgs e)
        {
            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.DepthTest);
            SwapBuffers();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (DownButton == MouseButtons.Middle)
            {
                CadVector t = CadVector.Create(e.X, e.Y, 0);

                Vector2 prev = default(Vector2);

                prev.X = (float)PrevMousePos.x;
                prev.Y = (float)PrevMousePos.y;

                Vector2 current = default(Vector2);

                current.X = (float)t.x;
                current.Y = (float)t.y;

                mDrawContext.RotateEyePoint(prev, current);

                Redraw();

                PrevMousePos = t;
            }
            // TODO とりあえずDragできない様にしときます
            else if (DownButton == MouseButtons.None)
            {
                //DrawContext dc = StartDraw();

                mController.Mouse.MouseMove(mDrawContext, e.X, e.Y);

                Redraw();
            }
        }

        public void Redraw()
        {
            //MakeCurrent();
            mController.Redraw(mController.CurrentDC);
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            if (mController != null)
            {
                Redraw();
            }
        }

        private void OnResize(object sender, EventArgs e)
        {
            mDrawContext.SetViewSize(Size.Width, Size.Height);

            if (mController != null)
            {
                Redraw();
            }
        }

        public void SetController(PlotterController controller)
        {
            mController = controller;
        }

        public void OnPushDraw(DrawContext dc)
        {
            if (dc == mDrawContext)
            {
                SwapBuffers();
            }
        } 
    }
}
