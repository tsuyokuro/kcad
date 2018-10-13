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
                if (HasSelect())
                {
                    mContextMenuInfo.Items.Add(MenuInfo.Copy);
                }

                if (HasCopyData())
                {
                    mContextMenuInfo.Items.Add(MenuInfo.Paste);
                }
            }

            if (mContextMenuInfo.Items.Count > 0)
            {
                Observer.RequestContextMenu(this, mContextMenuInfo, (int)x, (int)y);
            }
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
            }
        }
    }
}