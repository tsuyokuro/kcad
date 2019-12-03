#define MOUSE_THREAD
//#define VSYNC

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Plotter.Controller;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Resources;

namespace Plotter
{
    class PlotterViewGL : GLControl, IPlotterView
    {
        private DrawContextGL mDrawContext = null;

        private PlotterController mController = null;

        Vector3d PrevMousePos = default;

        MouseButtons DownButton = MouseButtons.None;

        ContextMenuEx mCurrentContextMenu = null;
        ContextMenuEx mContextMenu = null;

        MyEventSequencer mEventSequencer;

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

#if VSYNC
            VSync = true;
#else
            VSync = false;
#endif

            Load += OnLoad;
            SizeChanged += OnResize;
            Paint += OnPaint;
            MouseMove += OnMouseMove;
            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
            MouseWheel += OnMouseWheel;

            Disposed += OnDisposed;

            SetupCursor();

#if MOUSE_THREAD
            mEventSequencer = new MyEventSequencer(this, 100);
            mEventSequencer.Start();
#endif
        }

        private void OnDisposed(object sender, EventArgs e)
        {
            mDrawContext.Dispose();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.DepthTest);

            mDrawContextOrtho = new DrawContextGLOrtho(this);
            mDrawContextOrtho.SetupDrawing();
            mDrawContextOrtho.SetupTools(DrawTools.ToolsType.DARK);

            mDrawContextPers = new DrawContextGLPers(this);
            mDrawContextPers.SetupDrawing();
            mDrawContextPers.SetupTools(DrawTools.ToolsType.DARK);


            mDrawContext = mDrawContextOrtho;

            mDrawContextOrtho.OnPushDraw = OnPushDraw;
            mDrawContextPers.OnPushDraw = OnPushDraw;

            SwapBuffers();
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
#if MOUSE_THREAD
            int what = MyEventSequencer.MOUSE_UP;

            mEventSequencer.RemoveAll(what);

            MyEvent evt = mEventSequencer.ObtainEvent();

            evt.What = what;
            evt.EventArgs = e;

            mEventSequencer.Post(evt);
#else
            HandleMouseUp(e);
#endif
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
#if MOUSE_THREAD
            int what = MyEventSequencer.MOUSE_DOWN;

            mEventSequencer.RemoveAll(what);

            MyEvent evt = mEventSequencer.ObtainEvent();

            evt.What = what;
            evt.EventArgs = e;

            mEventSequencer.Post(evt);
#else
            HandleMouseDown(e);
#endif
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
#if MOUSE_THREAD
            int what = MyEventSequencer.MOUSE_WHEEL;

            mEventSequencer.RemoveAll(what);

            MyEvent evt = mEventSequencer.ObtainEvent();

            evt.What = what;
            evt.EventArgs = e;

            mEventSequencer.Post(evt);
#else
            HandleMouseWheel(e);
#endif
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
#if MOUSE_THREAD
            int what = MyEventSequencer.MOUSE_MOVE;

            mEventSequencer.RemoveAll(what);

            MyEvent evt = mEventSequencer.ObtainEvent();

            evt.What = what;
            evt.EventArgs = e;

            mEventSequencer.Post(evt);
#else
            HandleMouseMove(e);
#endif
        }

        public void Redraw()
        {
            ThreadUtil.RunOnMainThread(() =>
            {
                mController.Redraw(mController.CurrentDC);
            }, wait: false);
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
                Vector3d org = default;
                org.X = Width / 2;
                org.Y = Height / 2;

                mDrawContext.SetViewOrg(org);

                mController.SetCursorWoldPos(Vector3d.Zero);
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
            if (locked)
            {
                base.Cursor = Cursors.Arrow;
            }
            else
            {
                base.Cursor = PointCursor;
            }
        }

        public void ChangeMouseCursor(PlotterObserver.MouseCursorType cursorType)
        {
            switch (cursorType)
            {
                case PlotterObserver.MouseCursorType.CROSS:
                    base.Cursor = PointCursor;
                    break;
                case PlotterObserver.MouseCursorType.NORMAL_ARROW:
                    base.Cursor = Cursors.Arrow;
                    break;
                case PlotterObserver.MouseCursorType.HAND:
                    base.Cursor = Cursors.SizeAll;
                    break;
            }
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
                mController.ContextMenuMan.ContextMenuEvent(infoItem);
            }
        }

        public void ShowContextMenu(PlotterController sender, MenuInfo menuInfo, int x, int y)
        {
            ThreadUtil.RunOnMainThread(() =>
            {
                ShowContextMenuProc(sender, menuInfo, x, y);
            }, true);
        }

        private void ShowContextMenuProc(PlotterController sender, MenuInfo menuInfo, int x, int y)
        {
            mContextMenu.Items.Clear();

            foreach (MenuInfo.Item item in menuInfo.Items)
            {
                ToolStripMenuItem m = new ToolStripMenuItem(item.Text);
                m.Tag = item;
                m.Click += ContextMenueClick;

                mContextMenu.Items.Add(m);
            }

            mCurrentContextMenu = mContextMenu;
            mCurrentContextMenu.Show(this, new Point(x, y));
        }

        public void SetWorldScale(double scale)
        {
            mDrawContextPers.WorldScale = scale;
            mDrawContextOrtho.WorldScale = scale;
        }

        private void HandleMouseUp(MouseEventArgs e)
        {
            DownButton = MouseButtons.None;
            mController.Mouse.MouseUp(mDrawContext, e.Button, e.X, e.Y);

            Redraw();
        }

        private void HandleMouseDown(MouseEventArgs e)
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
                VectorExt.Set(out PrevMousePos, e.X, e.Y, 0);
                DownButton = e.Button;

                if (DownButton != MouseButtons.Middle)
                {
                    mController.Mouse.MouseDown(mDrawContext, e.Button, e.X, e.Y);
                }
            }
        }

        private void HandleMouseWheel(MouseEventArgs e)
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

        private void HandleMouseMove(MouseEventArgs e)
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
                    Vector3d t = new Vector3d(e.X, e.Y, 0);

                    Vector2 prev = default(Vector2);

                    prev.X = (float)PrevMousePos.X;
                    prev.Y = (float)PrevMousePos.Y;

                    Vector2 current = default(Vector2);

                    current.X = (float)t.X;
                    current.Y = (float)t.Y;

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

        class MyEvent : EventSequencer<MyEvent>.Event
        {
            public MouseEventArgs EventArgs;
        }

        class MyEventSequencer : EventSequencer<MyEvent>
        {
            public const int MOUSE_MOVE = 1;
            public const int MOUSE_WHEEL = 2;
            public const int MOUSE_DOWN = 3;
            public const int MOUSE_UP = 4;

            private PlotterViewGL mPlotterView;

            public MyEventSequencer(PlotterViewGL view, int queueSize) : base(queueSize)
            {
                mPlotterView = view;
            }

            public override void HandleEvent(MyEvent msg)
            {
                //DOut.pl($"HandleEvent what:{msg.What}");

                if (msg.What == MOUSE_MOVE)
                {
                    mPlotterView.HandleMouseMove(msg.EventArgs);
                }
                else if (msg.What == MOUSE_WHEEL)
                {
                    mPlotterView.HandleMouseWheel(msg.EventArgs);
                }
                else if (msg.What == MOUSE_DOWN)
                {
                    mPlotterView.HandleMouseDown(msg.EventArgs);
                }
                else if (msg.What == MOUSE_UP)
                {
                    mPlotterView.HandleMouseUp(msg.EventArgs);
                }
            }
        }
    }
}
