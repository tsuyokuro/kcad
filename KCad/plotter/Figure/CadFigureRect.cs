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
                    // 左回りになるように設定

                    CadPoint pp0 = dc.CadPointToUnitPoint(fig.PointList[0]);
                    CadPoint pp2 = dc.CadPointToUnitPoint(p);

                    CadPoint pp1 = pp0;
                    pp1.y = pp2.y;

                    CadPoint pp3 = pp0;
                    pp3.x = pp2.x;

                    fig.mPointList.Add(dc.UnitPointToCadPoint(pp1));
                    fig.mPointList.Add(dc.UnitPointToCadPoint(pp2));
                    fig.mPointList.Add(dc.UnitPointToCadPoint(pp3));

                    fig.Closed = true;
                }
            }

            public override void drawTemp(CadFigure fig, DrawContext dc, CadPoint tp, int pen)
            {
                if (fig.PointList.Count <= 0)
                {
                    return;
                }

                dc.Drawing.DrawRect(pen, fig.PointList[0], tp);
            }

            public override CadFigure.Types endCreate(CadFigure fig, DrawContext dc)
            {
                fig.Normal = CadPoint.Create(dc.ViewDir);
                fig.Normal *= -1;

                fig.Type = Types.POLY_LINES;
                return fig.Type;
            }
        }
    }
}