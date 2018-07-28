﻿#define MOUSE_THREAD

using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Resources;
using CadDataTypes;
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

        ContextMenuEx mPolyLineContextMenu;
        ToolStripMenuItem mMnItemClosePolyLines;
        ToolStripMenuItem mMnItemEndPolyLines;

        ContextMenuEx mRectContextMenu;
        ToolStripMenuItem mMnItemQuitRect;

        ContextMenuEx mCurrentContextMenu = null;


        MyMessageHandler mMessageHandler;

        private DrawContextGDI mDrawContext = new DrawContextGDI();

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
                return mDrawContext.PageSize.clone();
            }
        }

        public Control FromsControl
        {
            get
            {
                return (Control)this;
            }
        }

        public PlotterView()
        {
            this.DoubleBuffered = false;

            base.SizeChanged += onSizeChanged;

            mMessageHandler = new MyMessageHandler(this, 100);

            mMessageHandler.start();

            mDrawContext.SetupTools(DrawTools.ToolsType.DARK);

            mDrawContext.OnPush = OnPushDraw;

            base.MouseMove += mouseMove;
            base.MouseDown += mouseDown;
            base.MouseUp += mouseUp;

            base.MouseWheel += mouseWheel;

            StreamResourceInfo si = System.Windows.Application.GetResourceStream(
                new Uri("/KCad;component/Resources/mini_cross.cur", UriKind.Relative));

            Cursor cc = new Cursor(si.Stream);

            base.Cursor = cc;

            // Context menue for creating polyline
            {
                mPolyLineContextMenu = new ContextMenuEx();
                mPolyLineContextMenu.AutoClose = false;

                mMnItemClosePolyLines = new ToolStripMenuItem();
                mMnItemClosePolyLines.Text = "Close";
                mMnItemClosePolyLines.Click += cm_Click;

                mMnItemEndPolyLines = new ToolStripMenuItem();
                mMnItemEndPolyLines.Text = "End";
                mMnItemEndPolyLines.Click += cm_Click;

                mPolyLineContextMenu.Items.Add(mMnItemClosePolyLines);
                mPolyLineContextMenu.Items.Add(mMnItemEndPolyLines);
            }

            // Context menue for creating rect
            {
                mRectContextMenu = new ContextMenuEx();
                mRectContextMenu.AutoClose = false;

                mMnItemQuitRect = new ToolStripMenuItem();
                mMnItemQuitRect.Text = "Quit";
                mMnItemQuitRect.Click += cm_Click;

                mRectContextMenu.Items.Add(mMnItemQuitRect);
            }
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

        public void ShowContextMenu(PlotterController sender, PlotterController.StateInfo state, int x, int y)
        {
            if (state.CreatingFigureType != CadFigure.Types.NONE)
            {
                switch (state.CreatingFigureType)
                {
                    case CadFigure.Types.POLY_LINES:
                        if (state.CreatingFigurePointCnt > 2)
                        {
                            mMnItemClosePolyLines.Enabled = true;
                        }
                        else
                        {
                            mMnItemClosePolyLines.Enabled = false;
                        }

                        mCurrentContextMenu = mPolyLineContextMenu;
                        mCurrentContextMenu.Show(this, new Point(x, y));

                        break;

                    case CadFigure.Types.RECT:
                        mCurrentContextMenu = mRectContextMenu;
                        mCurrentContextMenu.Show(this, new Point(x, y));
                        break;
                }
            }
        }

        private void cm_Click(object sender, System.EventArgs e)
        {
            if (sender == mMnItemClosePolyLines)
            {
                mController.CloseFigure();
                Redraw();
            }
            else if (sender == mMnItemEndPolyLines || sender == mMnItemQuitRect)
            {
                mController.EndCreateFigureState();
                Redraw();
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
                mController.RequestContextMenu -= ShowContextMenu;
            }

            mController = controller;

            if (controller != null)
            {
                mController.RequestContextMenu += ShowContextMenu;
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
                mPlotterView.Invoke(new Action(() =>
                {
                    mPlotterView.mController.Mouse.MouseMove(mPlotterView.mDrawContext, x, y);
                    mPlotterView.Redraw();
                }));
            }

            public void handleMouseWheel(MouseEventArgs e)
            {
                mPlotterView.Invoke(new Action(() =>
                {
                    mPlotterView.mController.Mouse.MouseWheel(mPlotterView.mDrawContext, e.X, e.Y, e.Delta);
                    mPlotterView.Redraw();
                }));
            }
        }
    }
}
