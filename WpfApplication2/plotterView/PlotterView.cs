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

        private bool firstSizeChange = true;

        ContextMenuEx mPolyLineContextMenu;
        ToolStripMenuItem mMnItemClosePolyLines;
        ToolStripMenuItem mMnItemEndPolyLines;

        ContextMenuEx mRectContextMenu;
        ToolStripMenuItem mMnItemQuitRect;

        ContextMenuEx mCurrentContextMenu = null;

        public PlotterController.Interaction Interact = 
            new PlotterController.Interaction();

        public PlotterController.Interaction InteractCtrl =
            new PlotterController.Interaction();

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

            InteractCtrl.print = MessageOut;

            mController.Interact = InteractCtrl;
        }

        private void MessageOut(String s)
        {
            Interact.print(s);
        }

        private void onSizeChanged(object sender, System.EventArgs e)
        {
            if (Width > 0 && Height > 0)
            {
                mImage = new Bitmap(Width, Height);

                mDrawContext.ViewWidth = Width;
                mDrawContext.ViewHeight = Height;

                if (firstSizeChange)
                {
                    CadPixelPoint org = default(CadPixelPoint);
                    org.x = Width / 2;
                    org.y = Height / 2;

                    mDrawContext.ViewOrg = org;

                    firstSizeChange = false;
                }

                DrawContext dc = startDraw();
                Controller.clear(dc);
                Controller.draw(dc);
                endDraw(dc);
            }
        }

        private DrawContext startDraw(bool log = true)
        {
            Graphics g = Graphics.FromImage(mImage);
            mDrawContext.graphics = g;
            return mDrawContext;
        }

        private void endDraw(DrawContext dc)
        {
            mDrawContext.disposeGraphics();
            Image = mImage;
        }

        private void mouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            DrawContext g = startDraw(false);

            mController.Mouse.pointerMoved(g, e.X, e.Y);

            endDraw(g);
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

            endDraw(dc);
        }

        private void mouseUp(Object sender, MouseEventArgs e)
        {
            DrawContext g = startDraw();

            mController.Mouse.up(g, e.Button, e.X, e.Y);

            endDraw(g);
        }

        private void mouseWheel(object sender, MouseEventArgs e)
        {
            DrawContext g = startDraw();

            mController.Mouse.wheel(g, e.X, e.Y, e.Delta);

            endDraw(g);
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

        public void startCreateFigure(CadFigure.Types type)
        {
            mController.startCreateFigure(type);
        }

        public void endCreateFigure()
        {
            DrawContext dc = startDraw();

            mController.endCreateFigure(dc);

            endDraw(dc);
        }

        public void print(Graphics g)
        {
            DrawContext dc = new DrawContext();

            dc.graphics = g;
            dc.Tools.setupPrinterSet();
            dc.PageSize = mDrawContext.PageSize.clone();

            // Default printers's unit is 1/100 inch
            dc.setUnitPerInch(100.0, 100.0);

            CadPixelPoint org = default(CadPixelPoint);

            org.x = dc.PageSize.widthInch / 2.0 * 100;
            org.y = dc.PageSize.heightInch / 2.0 * 100;

            dc.ViewOrg = org;

            mController.print(dc);
        }

        public void undo()
        {
            DrawContext dc = startDraw();
            mController.undo(dc);
            endDraw(dc);
        }

        public void redo()
        {
            DrawContext dc = startDraw();
            mController.redo(dc);
            endDraw(dc);
        }

        public void remove()
        {
            DrawContext dc = startDraw();
            mController.remove(dc);
            endDraw(dc);
        }

        public void separateFigure()
        {
            DrawContext dc = startDraw();
            mController.separateFigures(dc);
            endDraw(dc);
        }

        public void bondFigure()
        {
            DrawContext g = startDraw();
            mController.bondFigures(g);
            endDraw(g);
        }

        public void toBezier()
        {
            DrawContext dc = startDraw();
            mController.toBezier(dc);
            endDraw(dc);
        }

        public void cutSegment()
        {
            DrawContext dc = startDraw();
            mController.cutSegment(dc);
            endDraw(dc);
        }

        public void addCenterPoint()
        {
            DrawContext dc = startDraw();
            mController.addCenterPoint(dc);
            endDraw(dc);
        }

        public void Copy()
        {
            DrawContext dc = startDraw();
            mController.Copy(dc);
            endDraw(dc);
        }

        public void Paste()
        {
            DrawContext dc = startDraw();
            mController.Paste(dc);
            endDraw(dc);
        }

        public void onKeyUp(object sender, KeyEventArgs e)
        {
        }

        public void onKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.Z:
                        undo();
                        break;

                    case Keys.Y:
                        redo();
                        break;

                    case Keys.C:
                    case Keys.Insert:
                        Copy();
                        break;

                    case Keys.V:
                        Paste();
                        break;
                }
            }
            else if (e.Shift)
            {
                switch (e.KeyCode)
                {
                    case Keys.Insert:
                        Paste();
                        break;
                }
            }
            else
            {
                switch (e.KeyCode)
                {
                    case Keys.Delete:
                        remove();
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
                endDraw(g);
            }
            else if (sender == mMnItemEndPolyLines || sender == mMnItemQuitRect)
            {
                DrawContext g = startDraw();
                mController.endCreateFigureState(g);
                endDraw(g);

            }
        }

        public void command(String s)
        {
            mController.command(s);

            DrawContext g = startDraw();

            mController.draw(g);

            endDraw(g);
        }


        public void debugCommand(string s)
        {
            DrawContext g = startDraw();

            mController.debugCommand(g, s);

            endDraw(g);
        }

        public void SaveFile(String fname)
        {
            mController.SaveToJsonFile(fname);
        }

        public void LoadFile(String fname)
        {
            mController.LoadFromJsonFile(fname);

            DrawContext g = startDraw();

            mController.clear(g);

            mController.draw(g);

            endDraw(g);
        }
    }
}
