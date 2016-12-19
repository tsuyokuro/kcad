using System;
using System.Drawing;

namespace Plotter
{
    public partial class CadFigure
    {
        [Serializable]
        public class CadFigureRect : CadFigurePolyLines
        {
            // Do not have data member.

            public override States getState(CadFigure fig)
            {
                if (fig.PointList.Count < 1)
                {
                    return States.NOT_ENOUGH;
                }
                else if (fig.PointList.Count < 4)
                {
                    return States.WAIT_LAST_POINT;
                }

                return States.FULL;
            }

            public CadFigureRect()
            {
            }

            public override void addPointInCreating(CadFigure fig, DrawContext dc, CadPoint p)
            {
                if (fig.mPointList.Count == 0)
                {
                    fig.mPointList.Add(p);
                }
                else
                {
                    CadPoint pp0 = dc.pointToPixelPoint(fig.PointList[0]);
                    CadPoint pp2 = dc.pointToPixelPoint(p);

                    CadPoint pp1 = pp0;
                    pp1.x = pp2.x;

                    CadPoint pp3 = pp0;
                    pp3.y = pp2.y;

                    fig.mPointList.Add(dc.pixelPointToCadPoint(pp1));
                    fig.mPointList.Add(dc.pixelPointToCadPoint(pp2));
                    fig.mPointList.Add(dc.pixelPointToCadPoint(pp3));

                    fig.Closed = true;
                }
            }

            public override void drawTemp(CadFigure fig, DrawContext dc, CadPoint tp, Pen pen)
            {
                if (fig.PointList.Count <= 0)
                {
                    return;
                }

                Drawer.drawRect(dc, pen, fig.PointList[0], tp);
            }

            public override CadFigure.Types endCreate(CadFigure fig)
            {
                fig.Type = Types.POLY_LINES;
                return fig.Type;
            }
        }
    }
}