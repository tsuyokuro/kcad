using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        private Bitmap mImage;
        private PlotterController mController = null;

        private bool firstSizeChange = true;

        ContextMenuEx mPolyLineContextMenu;
        ToolStripMenuItem mMnItemClosePolyLines;
        ToolStripMenuItem mMnItemEndPolyLines;

        ContextMenuEx mRectContextMenu;
        ToolStripMenuItem mMnItemQuitRect;

        ContextMenuEx mCurrentContextMenu = null;


        MyMessageHandler mMessageHandler;

        private DrawContext mDrawContext = new DrawContextGDI();

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
            base.SizeChanged += onSizeChanged;

            mMessageHandler = new MyMessageHandler(this, 100);

            mMessageHandler.start();

            mDrawContext.SetupTools(DrawTools.ToolsType.DARK);

            base.MouseMove += mouseMove;
            base.MouseDown += mouseDown;
            base.MouseUp += mouseUp;

            base.MouseWheel += new System.Windows.Forms.MouseEventHandler(mouseWheel);

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
            if (mImage != null)
            {
                mImage.Dispose();
            }

            base.Dispose(disposing);
        }

        override protected void OnPaintBackground(PaintEventArgs pevent)
        {
        }

        private void onSizeChanged(object sender, System.EventArgs e)
        {
            if (mImage != null)
            {
                mImage.Dispose();
            }

            if (Width > 0 && Height > 0)
            {
                mImage = new Bitmap(Width, Height);

                mDrawContext.SetViewSize(Width, Height);

                if (firstSizeChange)
                {
                    CadVector org = default(CadVector);
                    org.x = Width / 2;
                    org.y = Height / 2;

                    mDrawContext.ViewOrg = org;

                    firstSizeChange = false;
                }

                DrawContext dc = StartDraw();
                mController.Clear(dc);
                mController.Draw(dc);
                EndDraw();
            }
        }

        public DrawContext StartDraw()
        {
            mDrawContext.StartDraw(mImage);
            return mDrawContext;
        }

        public void EndDraw()
        {
            mDrawContext.EndDraw();
            Image = mImage;
        }

        private void mouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            /*
            DrawContext g = StartDraw();
            mController.Mouse.MouseMove(g, e.X, e.Y);
            EndDraw();
            */

            mMessageHandler.RemoveAll(MyMessageHandler.MOUSE_MOVE);

            MessageHandler.Message msg = mMessageHandler.ObtainMessage();

            msg.What = MyMessageHandler.MOUSE_MOVE;
            msg.Arg1 = e.X;
            msg.Arg2 = e.Y;

            mMessageHandler.SendMessage(msg, 1);
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

            DrawContext dc = StartDraw();

            mController.Mouse.MouseDown(dc, e.Button, e.X, e.Y);

            EndDraw();
        }

        private void mouseUp(Object sender, MouseEventArgs e)
        {
            DrawContext g = StartDraw();

            mController.Mouse.MouseUp(g, e.Button, e.X, e.Y);

            EndDraw();
        }

        private void mouseWheel(object sender, MouseEventArgs e)
        {
            DrawContext g = StartDraw();

            mController.Mouse.MouseWheel(g, e.X, e.Y, e.Delta);

            EndDraw();
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
                RedrawAll();
            }
            else if (sender == mMnItemEndPolyLines || sender == mMnItemQuitRect)
            {
                mController.EndCreateFigureState();
                RedrawAll();
            }
        }

        public void RedrawAll()
        {
            DrawContext dc = StartDraw();
            mController.Clear(dc);
            mController.DrawAll(dc);
            EndDraw();
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
            }

            public void handleMouseMove(int x, int y)
            {
                mPlotterView.Invoke(new Action(() =>
                {
                    DrawContext g = mPlotterView.StartDraw();

                    mPlotterView.mController.Mouse.MouseMove(g, x, y);

                    mPlotterView.EndDraw();
                }));
            }
        }
    }
}
