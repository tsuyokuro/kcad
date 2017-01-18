using System;
using System.Drawing;
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

    public partial class PlotterView : PictureBox
    {
        private Bitmap mImage;
        private PlotterController mController = new PlotterController();

        private DrawContext mDrawContext = new DrawContext();

        public PlotterController Controller
        {
            get { return mController; }
        }

        public DrawContext DrawContext
        {
            get
            {
                return mDrawContext;
            }
        }

        private bool firstSizeChange = true;

        ContextMenuEx mPolyLineContextMenu;
        ToolStripMenuItem mMnItemClosePolyLines;
        ToolStripMenuItem mMnItemEndPolyLines;

        ContextMenuEx mRectContextMenu;
        ToolStripMenuItem mMnItemQuitRect;

        ContextMenuEx mCurrentContextMenu = null;

        public PaperPageSize PageSize
        {
            get
            {
                return mDrawContext.PageSize.clone();
            }
        }

        public PlotterView()
        {
            InitializeComponent();
            base.SizeChanged += onSizeChanged;

            mDrawContext.Tools.darkSet();

            BackColor = mDrawContext.Tools.BackgroundColor;

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

            mController.RequestContextMenu += ShowContextMenu;

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

                mDrawContext.setViewSize(Width, Height);

                if (firstSizeChange)
                {
                    CadPoint org = default(CadPoint);
                    org.x = Width / 2;
                    org.y = Height / 2;

                    mDrawContext.ViewOrg = org;

                    firstSizeChange = false;
                }

                DrawContext dc = startDraw();
                Controller.clear(dc);
                Controller.draw(dc);
                endDraw();
            }
        }

        public DrawContext startDraw()
        {
            mDrawContext.startDraw(mImage);
            return mDrawContext;
        }

        public void endDraw()
        {
            mDrawContext.endDraw();
            Image = mImage;
        }

        private void mouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            DrawContext g = startDraw();

            mController.Mouse.pointerMoved(g, e.X, e.Y);

            endDraw();
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

            DrawContext dc = startDraw();

            mController.Mouse.down(dc, e.Button, e.X, e.Y);

            endDraw();
        }

        private void mouseUp(Object sender, MouseEventArgs e)
        {
            DrawContext g = startDraw();

            mController.Mouse.up(g, e.Button, e.X, e.Y);

            endDraw();
        }

        private void mouseWheel(object sender, MouseEventArgs e)
        {
            DrawContext g = startDraw();

            mController.Mouse.wheel(g, e.X, e.Y, e.Delta);

            endDraw();
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
                DrawContext g = startDraw();
                mController.closeFigure(g);
                endDraw();
            }
            else if (sender == mMnItemEndPolyLines || sender == mMnItemQuitRect)
            {
                DrawContext g = startDraw();
                mController.endCreateFigureState(g);
                endDraw();

            }
        }
    }
}
