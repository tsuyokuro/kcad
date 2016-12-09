using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            public override void addPoint(CadFigure fig, CadPoint p)
            {
                if (fig.PointList.Count == 0)
                {
                    fig.PointList.Add(p);
                }
                else
                {
                    CadPoint p0 = fig.PointList[0];
                    CadPoint p2 = p;

                    CadPoint p1 = p0;
                    p1.x = p2.x;

                    CadPoint p3 = p0;
                    p3.y = p2.y;

                    fig.PointList.Add(p1);
                    fig.PointList.Add(p2);
                    fig.PointList.Add(p3);

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