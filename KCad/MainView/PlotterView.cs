#define MOUSE_THREAD

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Resources;
using CadDataTypes;
using KCad;
using Plotter.Controller;

namespace Plotter
{
    public partial class PlotterView : PictureBox, IPlotterView
    {
        private PlotterController mController = null;

        private bool firstSizeChange = true;

        ContextMenuEx mCurrentContextMenu = null;
        ContextMenuEx mContextMenu = null;

        MyEventSequencer mEventSequencer;

        private DrawContextGDI mDrawContext = null;

        private Cursor PointCursor;

        public DrawContext DrawContext
        {
            get => mDrawContext;
        }

        public Control FormsControl
        {
            get => this;
        }

        public PlotterView()
        {
            mDrawContext = new DrawContextGDI(this);

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

            DoubleBuffered = false;

            SizeChanged += onSizeChanged;

            mEventSequencer = new MyEventSequencer(this, 100);

            mEventSequencer.Start();

            mDrawContext.SetupTools(DrawTools.ToolsType.DARK);

            mDrawContext.PushDraw = PushDraw;

            MouseMove += OnMouseMove;
            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
            MouseWheel += OnMouseWheel;

            StreamResourceInfo si = System.Windows.Application.GetResourceStream(
                new Uri("/KCad;component/Resources/dot.cur", UriKind.Relative));

            PointCursor = new Cursor(si.Stream);

            base.Cursor = PointCursor;
        }

        protected override void Dispose(bool disposing)
        {
            mDrawContext.Dispose();

            base.Dispose(disposing);
        }

        override protected void OnPaintBackground(PaintEventArgs pevent)
        {
            mController.Redraw();
        }

        private void onSizeChanged(object sender, System.EventArgs e)
        {
            if (Width > 0 && Height > 0)
            {
                mDrawContext.SetViewSize(Width, Height);

                if (firstSizeChange)
                {
                    CadVector org = default(CadVector);
                    org.x = Width / 2;
                    org.y = Height / 2;

                    mDrawContext.ViewOrg = org;

                    //DOut.pl($"{GetType().Name} onSizeChanged firstChange {Width}, {Height}");

                    mController.SetCursorWoldPos(CadVector.Zero);

                    firstSizeChange = false;
                }

                Redraw();
            }
        }

        public void PushDraw(DrawContext dc)
        {
            if (dc == mDrawContext)
            {
                //Image = mDrawContext.Image;
                mDrawContext.Refresh();
            }
        }

        private void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
#if MOUSE_THREAD
            // Mouse eventを別スレッドで処理
            // 未処理のEventは破棄
            mEventSequencer.RemoveAll(MyEventSequencer.MOUSE_MOVE);

            EventSequencer.Event evt = mEventSequencer.ObtainEvent();

            evt.What = MyEventSequencer.MOUSE_MOVE;
            evt.Arg1 = e.X;
            evt.Arg2 = e.Y;

            mEventSequencer.Post(evt);
#else
            // Mouse eventを直接処理
            mController.Mouse.MouseMove(mDrawContext, e.X, e.Y);
            Redraw();
#endif
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
#if MOUSE_THREAD
            mEventSequencer.RemoveAll(MyEventSequencer.MOUSE_WHEEL);

            EventSequencer.Event evt = mEventSequencer.ObtainEvent();

            evt.What = MyEventSequencer.MOUSE_WHEEL;
            evt.Obj = e;

            mEventSequencer.Post(evt);
#else
            // 直接描画
            mController.Mouse.MouseWheel(mDrawContext, e.X, e.Y, e.Delta);
            Redraw();
#endif
        }

        private void OnMouseDown(Object sender, MouseEventArgs e)
        {
            if (mCurrentContextMenu != null)
            {
                if (mCurrentContextMenu.Visible)
                {
                    mCurrentContextMenu.Close();
                    return;
                }
            }

#if MOUSE_THREAD
            mEventSequencer.RemoveAll(MyEventSequencer.MOUSE_DOWN);

            EventSequencer.Event evt = mEventSequencer.ObtainEvent();

            evt.What = MyEventSequencer.MOUSE_DOWN;
            evt.Obj = e;

            mEventSequencer.Post(evt);
#else
            mController.Mouse.MouseDown(mDrawContext, e.Button, e.X, e.Y);
            Redraw();
#endif
        }

        private void OnMouseUp(Object sender, MouseEventArgs e)
        {
#if MOUSE_THREAD
            mEventSequencer.RemoveAll(MyEventSequencer.MOUSE_UP);

            EventSequencer.Event evt = mEventSequencer.ObtainEvent();

            evt.What = MyEventSequencer.MOUSE_UP;
            evt.Obj = e;

            mEventSequencer.Post(evt);
#else
            mController.Mouse.MouseUp(mDrawContext, e.Button, e.X, e.Y);
            Redraw();
#endif
        }

        public void ShowContextMenu(PlotterController sender, MenuInfo menuInfo, int x, int y)
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

        private void ContextMenueClick(object sender, System.EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;

            MenuInfo.Item infoItem = item.Tag as MenuInfo.Item;

            if (infoItem != null)
            {
                mController.ContextMenuEvent(infoItem);
            }
        }

        public void Redraw()
        {
            mController.Redraw(mController.CurrentDC);
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

        class MyEventSequencer : EventSequencer
        {
            public const int MOUSE_MOVE = 1;
            public const int MOUSE_WHEEL = 2;
            public const int MOUSE_DOWN = 3;
            public const int MOUSE_UP = 4;

            private PlotterView mPlotterView;

            public MyEventSequencer(PlotterView view, int queueSize) : base(queueSize)
            {
                mPlotterView = view;
            }

            public override void HandleEvent(Event msg)
            {
                if (msg.What == MOUSE_MOVE)
                {
                    int x = msg.Arg1;
                    int y = msg.Arg2;

                    handleMouseMove(x, y);
                }
                else if (msg.What == MOUSE_WHEEL)
                {
                    MouseEventArgs e = (MouseEventArgs)msg.Obj;
                    handleMouseWheel(e);
                }
                else if (msg.What == MOUSE_DOWN)
                {
                    MouseEventArgs e = (MouseEventArgs)msg.Obj;
                    handleMouseDown(e);
                }
                else if (msg.What == MOUSE_UP)
                {
                    MouseEventArgs e = (MouseEventArgs)msg.Obj;
                    handleMouseUp(e);
                }

            }

            public void handleMouseMove(int x, int y)
            {
                Exception exp = null;

                ThreadUtil.RunOnMainThread(() =>
                {
                    try
                    {
                        mPlotterView.mController.Mouse.MouseMove(mPlotterView.mDrawContext, x, y);
                        mPlotterView.Redraw();
                    }
                    catch (Exception ex)
                    {
                        exp = ex;
                    }
                }, true);

                if (exp != null)
                {
                    App.ThrowException(exp);
                }
            }

            public void handleMouseWheel(MouseEventArgs e)
            {
                Exception exp = null;

                ThreadUtil.RunOnMainThread(() =>
                {
                    try
                    {
                        mPlotterView.mController.Mouse.MouseWheel(mPlotterView.mDrawContext, e.X, e.Y, e.Delta);
                        mPlotterView.Redraw();
                    }
                    catch (Exception ex)
                    {
                        exp = ex;
                    }

                }, true);

                if (exp != null)
                {
                    App.ThrowException(exp);
                }
            }

            public void handleMouseDown(MouseEventArgs e)
            {
                Exception exp = null;

                ThreadUtil.RunOnMainThread(() =>
                {
                    try
                    {
                        mPlotterView.mController.Mouse.MouseDown(mPlotterView.mDrawContext, e.Button, e.X, e.Y);
                        mPlotterView.Redraw();
                    }
                    catch (Exception ex)
                    {
                        exp = ex;
                    }

                }, true);

                if (exp != null)
                {
                    App.ThrowException(exp);
                }
            }

            public void handleMouseUp(MouseEventArgs e)
            {
                Exception exp = null;

                ThreadUtil.RunOnMainThread(() =>
                {
                    try
                    {
                        mPlotterView.mController.Mouse.MouseUp(mPlotterView.mDrawContext, e.Button, e.X, e.Y);
                        mPlotterView.Redraw();
                    }
                    catch (Exception ex)
                    {
                        exp = ex;
                    }

                }, true);

                if (exp != null)
                {
                    App.ThrowException(exp);
                }
            }
        }
    }
}
