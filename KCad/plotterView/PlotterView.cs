#define MOUSE_THREAD

using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Resources;
using CadDataTypes;
using KCad;
using Plotter.Controller;

namespace Plotter
{
    public class ContextMenuEx : ContextMenuStrip
    {
        protected override void OnItemClicked(ToolStripItemClickedEventArgs e)
        {
            Close();
            base.OnItemClicked(e);
        }
    }

    public partial class PlotterView : PictureBox, IPlotterView
    {
        private PlotterController mController = null;

        private bool firstSizeChange = true;

        ContextMenuEx mCurrentContextMenu = null;
        ContextMenuEx mContextMenu = new ContextMenuEx();

        MyMessageHandler mMessageHandler;

        private DrawContextGDI mDrawContext = null;

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

        public PlotterView()
        {
            mDrawContext = new DrawContextGDI(this);

            DoubleBuffered = false;

            SizeChanged += onSizeChanged;

            mMessageHandler = new MyMessageHandler(this, 100);

            mMessageHandler.start();

            mDrawContext.SetupTools(DrawTools.ToolsType.DARK);

            mDrawContext.OnPush = OnPushDraw;

            MouseMove += mouseMove;
            MouseDown += mouseDown;
            MouseUp += mouseUp;

            MouseWheel += mouseWheel;

            //StreamResourceInfo si = System.Windows.Application.GetResourceStream(
            //    new Uri("/KCad;component/Resources/mini_cross.cur", UriKind.Relative));

            StreamResourceInfo si = System.Windows.Application.GetResourceStream(
                new Uri("/KCad;component/Resources/dot.cur", UriKind.Relative));

            //StreamResourceInfo si = System.Windows.Application.GetResourceStream(
            //    new Uri("/KCad;component/Resources/null.cur", UriKind.Relative));

            Cursor cc = new Cursor(si.Stream);

            base.Cursor = cc;
        }

        protected override void Dispose(bool disposing)
        {
            mDrawContext.Dispose();

            base.Dispose(disposing);
        }

        override protected void OnPaintBackground(PaintEventArgs pevent)
        {
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

                    firstSizeChange = false;
                }

                Redraw();
            }
        }

        public void OnPushDraw(DrawContext dc)
        {
            if (dc == mDrawContext)
            {
                Image = mDrawContext.Image;
            }
        }

        private void mouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
#if MOUSE_THREAD
            // Mouse eventを別スレッドで処理
            // 未処理のEventは破棄
            mMessageHandler.RemoveAll(MyMessageHandler.MOUSE_MOVE);

            MessageHandler.Message msg = mMessageHandler.ObtainMessage();

            msg.What = MyMessageHandler.MOUSE_MOVE;
            msg.Arg1 = e.X;
            msg.Arg2 = e.Y;

            mMessageHandler.SendMessage(msg, 2);
#else
            // Mouse eventを直接処理
            mController.Mouse.MouseMove(mDrawContext, e.X, e.Y);
            Redraw();
#endif
        }

        private void mouseWheel(object sender, MouseEventArgs e)
        {
#if MOUSE_THREAD
            mMessageHandler.RemoveAll(MyMessageHandler.MOUSE_WHEEL);

            MessageHandler.Message msg = mMessageHandler.ObtainMessage();

            msg.What = MyMessageHandler.MOUSE_WHEEL;
            msg.Obj = e;

            mMessageHandler.SendMessage(msg, 2);
#else
            // 直接描画
            mController.Mouse.MouseWheel(mDrawContext, e.X, e.Y, e.Delta);
            Redraw();
#endif
        }

        private void mouseDown(Object sender, MouseEventArgs e)
        {
            if (mCurrentContextMenu != null)
            {
                if (mCurrentContextMenu.Visible)
                {
                    mCurrentContextMenu.Close();
                    return;
                }
            }

            mController.Mouse.MouseDown(mDrawContext, e.Button, e.X, e.Y);

            Redraw();
        }

        private void mouseUp(Object sender, MouseEventArgs e)
        {
            mController.Mouse.MouseUp(mDrawContext, e.Button, e.X, e.Y);

            Redraw();
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

        class MyMessageHandler : MessageHandler
        {
            public const int MOUSE_MOVE = 1;
            public const int MOUSE_WHEEL = 2;

            private PlotterView mPlotterView; 

            public MyMessageHandler(PlotterView view, int maxMessage) : base(maxMessage)
            {
                mPlotterView = view;
            }

            public override void HandleMessage(Message msg)
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
            }

            public void handleMouseMove(int x, int y)
            {
                Exception exp = null;

                mPlotterView.Invoke(new Action(() =>
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

                    if (exp != null)
                    {
                        App.ShowExceptionDialg(exp.ToString());
                        App.GetCurrent().Shutdown();
                    }
                }));
            }

            public void handleMouseWheel(MouseEventArgs e)
            {
                Exception exp = null;

                mPlotterView.Invoke(new Action(() =>
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

                    if (exp != null)
                    {
                        App.ShowExceptionDialg(exp.ToString());
                        App.GetCurrent().Shutdown();
                    }

                }));
            }
        }
    }
}
