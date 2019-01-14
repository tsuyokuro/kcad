using System.Collections.Generic;

namespace Plotter.Controller
{
    public partial class PlotterController
    {
        MenuInfo mContextMenuInfo = new MenuInfo();

        private void RequestContextMenu(double x, double y)
        {
            mContextMenuInfo.Items.Clear();


            if (FigureCreator != null)
            {
                switch (CreatingFigType)
                {
                    case CadFigure.Types.POLY_LINES:
                        if (FigureCreator.Figure.PointCount > 2)
                        {
                            mContextMenuInfo.Items.Add(MenuInfo.CreatingFigureClose);
                        }

                        mContextMenuInfo.Items.Add(MenuInfo.CreatingFigureEnd);

                        break;

                    case CadFigure.Types.RECT:
                        mContextMenuInfo.Items.Add(MenuInfo.CreatingFigureQuit);
                        break;
                }
            }
            else
            {
                if (SegSelected())
                {
                    mContextMenuInfo.Items.Add(MenuInfo.InsertPoint);
                }

                bool hasSelect = HasSelect();
                bool hasCopyData = PlotterClipboard.HasCopyData();

                if (hasSelect)
                {
                    mContextMenuInfo.Items.Add(MenuInfo.Copy);
                }

                if (hasCopyData)
                {
                    mContextMenuInfo.Items.Add(MenuInfo.Paste);
                }
            }

            if (mContextMenuInfo.Items.Count > 0)
            {
                Observer.RequestContextMenu(this, mContextMenuInfo, (int)x, (int)y);
            }
        }

        private bool SegSelected()
        {
            if (LastSelSegment == null)
            {
                return false;
            }

            MarkSegment seg = LastSelSegment.Value;

            CadFigure fig = DB.GetFigure(seg.FigureID);

            if (fig == null)
            {
                return false;
            }

            if (fig.Type != CadFigure.Types.POLY_LINES)
            {
                return false;
            }

            bool handle = false;

            handle |= fig.GetPointAt(seg.PtIndexA).IsHandle;
            handle |= fig.GetPointAt(seg.PtIndexB).IsHandle;

            if (handle)
            {
                return false;
            }

            return true;
        }


        public void ContextMenuEvent(MenuInfo.Item menuItem)
        {
            MenuInfo.Commands cmd = menuItem.Command;

            switch (cmd)
            {
                case MenuInfo.Commands.CREATING_FIGURE_CLOSE:
                    CloseFigure();
                    break;

                case MenuInfo.Commands.CREATING_FIGURE_END:
                    EndCreateFigureState();

                    break;
                case MenuInfo.Commands.CREATING_FIGURE_QUIT:
                    EndCreateFigureState();
                    break;

                case MenuInfo.Commands.COPY:
                    Copy();
                    break;

                case MenuInfo.Commands.PASTE:
                    Paste();
                    break;

                case MenuInfo.Commands.INSERT_POINT:
                    InsPoint();
                    break;
            }
        }
    }
}