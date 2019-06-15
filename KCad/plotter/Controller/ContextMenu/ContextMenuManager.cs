using System.Collections.Generic;

namespace Plotter.Controller
{
    public class ContextMenuManager
    {
        MenuInfo mContextMenuInfo = new MenuInfo();

        PlotterController mController;


        public ContextMenuManager(PlotterController controller)
        {
            mController = controller;
        }

        public void RequestContextMenu(double x, double y)
        {
            mContextMenuInfo.Items.Clear();


            if (mController.FigureCreator != null)
            {
                switch (mController.CreatingFigType)
                {
                    case CadFigure.Types.POLY_LINES:
                        if (mController.FigureCreator.Figure.PointCount > 2)
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

                bool hasSelect = mController.HasSelect();
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
                mController.Observer.RequestContextMenu(mController, mContextMenuInfo, (int)x, (int)y);
            }
        }

        private bool SegSelected()
        {
            if (mController.LastSelSegment == null)
            {
                return false;
            }

            MarkSegment seg = mController.LastSelSegment.Value;

            CadFigure fig = mController.DB.GetFigure(seg.FigureID);

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
                    mController.CloseFigure();
                    break;

                case MenuInfo.Commands.CREATING_FIGURE_END:
                    mController.EndCreateFigure();

                    break;
                case MenuInfo.Commands.CREATING_FIGURE_QUIT:
                    mController.EndCreateFigure();
                    break;

                case MenuInfo.Commands.COPY:
                    mController.Copy();
                    break;

                case MenuInfo.Commands.PASTE:
                    mController.Paste();
                    break;

                case MenuInfo.Commands.INSERT_POINT:
                    mController.InsPoint();
                    break;
            }
        }
    }
}