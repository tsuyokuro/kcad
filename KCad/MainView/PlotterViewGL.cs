using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Windows.Forms;
using CadDataTypes;
using Plotter.Controller;
using System.Drawing;
using System.Windows.Resources;

namespace Plotter
{
    class PlotterViewGL : GLControl, IPlotterView
    {
        private DrawContextGL mDrawContext = null;

        private PlotterController mController = null;

        CadVertex PrevMousePos = default(CadVertex);

        MouseButtons DownButton = MouseButtons.None;

        ContextMenuEx mCurrentContextMenu = null;
        ContextMenuEx mContextMenu = null;

        private Cursor PointCursor;

        public DrawContext DrawContext
        {
            get
            {
                return mDrawContext;
            }
        }

        public Control FormsControl
        {
            get
            {
                return this;
            }
        }

        private DrawContextGLOrtho mDrawContextOrtho;

        private DrawContextGLPers mDrawContextPers;

        private bool mEnablePerse = false;

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
            SetupContextMenu();

            Load += OnLoad;
            SizeChanged += OnResize;
            Paint += OnPaint;
            MouseMove += OnMouseMove;
            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
            MouseWheel += OnMouseWheel;

            SetupCursor();
        }

        protected void SetupCursor()
        {
            StreamResourceInfo si = System.Windows.Application.GetResourceStream(
                new Uri("/KCad;component/Resources/mini_cross.cur", UriKind.Relative));

            PointCursor = new Cursor(si.Stream);

            base.Cursor = PointCursor;
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            DownButton = MouseButtons.None;
            mController.Mouse.MouseUp(mDrawContext, e.Button, e.X, e.Y);

            Redraw();
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (mCurrentContextMenu != null)
            {
                if (mCurrentContextMenu.Visible)
                {
                    mCurrentContextMenu.Close();
                    return;
                }
            }

            if (mDrawContext is DrawContextGLOrtho)
            {
                mController.Mouse.MouseDown(mDrawContext, e.Button, e.X, e.Y);
            }
            else
            {
                PrevMousePos.Set(e.X, e.Y, 0);
                DownButton = e.Button;

                if (DownButton != MouseButtons.Middle)
                {
                    mController.Mouse.MouseDown(mDrawContext, e.Button, e.X, e.Y);
                }
            }
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            if (mDrawContext is DrawContextGLOrtho)
            {
                mController.Mouse.MouseWheel(mDrawContext, e.X, e.Y, e.Delta);
                Redraw();
            }
            else
            {
                DrawContextGLPers dc = mDrawContext as DrawContextGLPers;

                if (CadKeyboard.IsCtrlKeyDown())
                {
                    if (e.Delta > 0)
                    {
                        dc.MoveForwardEyePoint(3);
                    }
                    else if (e.Delta < 0)
                    {
                        dc.MoveForwardEyePoint(-3);
                    }

                    Redraw();
                }
            }
        }

        private void OnLoad(object sender, EventArgs e)
        {
            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.DepthTest);

            mDrawContextOrtho = new DrawContextGLOrtho(this);
            mDrawContextPers = new DrawContextGLPers(this);

            mDrawContext = mDrawContextOrtho;

            mDrawContextOrtho.PushDraw = OnPushDraw;
            mDrawContextPers.PushDraw = OnPushDraw;

            SwapBuffers();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (mDrawContext is DrawContextGLOrtho)
            {
                mController.Mouse.MouseMove(mDrawContext, e.X, e.Y);
                Redraw();
            }
            else
            {
                DrawContextGLPers dc = mDrawContext as DrawContextGLPers;

                if (DownButton == MouseButtons.Middle)
                {
                    CadVertex t = CadVertex.Create(e.X, e.Y, 0);

                    Vector2 prev = default(Vector2);

                    prev.X = (float)PrevMousePos.x;
                    prev.Y = (float)PrevMousePos.y;

                    Vector2 current = default(Vector2);

                    current.X = (float)t.x;
                    current.Y = (float)t.y;

                    dc.RotateEyePoint(prev, current);

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

        private int sizeChangeCnt = 0;

        private void OnResize(object sender, EventArgs e)
        {
            if (mDrawContext == null)
            {
                return;
            }

            if (sizeChangeCnt == 2)
            {
                CadVertex org = default(CadVertex);
                org.x = Width / 2;
                org.y = Height / 2;

                mDrawContext.SetViewOrg(org);

                mController.SetCursorWoldPos(CadVertex.Zero);
            }

            sizeChangeCnt++;

            mDrawContext.SetViewSize(Size.Width, Size.Height);

            if (mController != null)
            {
                Redraw();
            }
        }

        public void EnablePerse(bool enable)
        {
            if (enable)
            {
                if (mDrawContext != mDrawContextPers)
                {
                    mDrawContext = mDrawContextPers;
                }
            }
            else
            {
                if (mDrawContext != mDrawContextOrtho)
                {
                    mDrawContext = mDrawContextOrtho;
                }
            }

            if (mDrawContext == null)
            {
                return;
            }

            mDrawContext.SetViewSize(Size.Width, Size.Height);
        }

        public void SetController(PlotterController controller)
        {
            if (mController != null)
            {
                mController.Observer.RequestContextMenu -= ShowContextMenu;
            }

            mController = controller;

            if (controller != null)
            {
                mController.Observer.RequestContextMenu += ShowContextMenu;
            }
        }

        public void OnPushDraw(DrawContext dc)
        {
            if (dc == mDrawContext)
            {
                SwapBuffers();
            }
        }

        public void CursorLocked(bool locked)
        {
            // NOP
        }

        public void ChangeMouseCursor(PlotterObserver.MouseCursorType cursorType)
        {
            // NOP
        }

        private void SetupContextMenu()
        {
            mContextMenu = new ContextMenuEx();

            mContextMenu.StateChanged = (s) =>
            {
                if (s == ContextMenuEx.State.OPENED)
                {
                    base.Cursor = Cursors.Arrow;
                }
                else if (s == ContextMenuEx.State.CLOSED)
                {
                    base.Cursor = PointCursor;
                }
            };
        }

        private void ContextMenueClick(object sender, System.EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;

            MenuInfo.Item infoItem = item.Tag as MenuInfo.Item;

            if (infoItem != null)
            {
                mController.ContextMenuEvent(infoItem);
            }
        }

        public void ShowContextMenu(PlotterController sender, MenuInfo menuInfo, int x, int y)
        {
            ThreadUtil.RunOnMainThread(() => {
                ShowContextMenuProc(sender, menuInfo, x, y);
            }, true);
        }

        private void ShowContextMenuProc(PlotterController sender, MenuInfo menuInfo, int x, int y)
        {
            mContextMenu.Items.Clear();

            foreach (MenuInfo.Item item in menuInfo.Items)
            {
                ToolStripMenuItem m = new ToolStripMenuItem(item.DefaultText);
                m.Tag = item;
                m.Click += ContextMenueClick;

                mContextMenu.Items.Add(m);
            }

            mCurrentContextMenu = mContextMenu;
            mCurrentContextMenu.Show(this, new Point(x, y));
        }
    }
}
