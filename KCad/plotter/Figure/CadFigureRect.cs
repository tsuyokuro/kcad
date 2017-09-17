using System;
using System.Drawing;

namespace Plotter
{
    public partial class CadFigure
    {
        [Serializable]
        public class CadFigureRect : CadFigurePolyLines
        {
            public override States GetState(CadFigure fig)
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

            public override void AddPointInCreating(CadFigure fig, DrawContext dc, CadVector p)
            {
                if (fig.mPointList.Count == 0)
                {
                    fig.mPointList.Add(p);
                }
                else
                {
                    // 左回りになるように設定

                    CadVector pp0 = dc.CadPointToUnitPoint(fig.PointList[0]);
                    CadVector pp2 = dc.CadPointToUnitPoint(p);

                    CadVector pp1 = pp0;
                    pp1.y = pp2.y;

                    CadVector pp3 = pp0;
                    pp3.x = pp2.x;

                    fig.mPointList.Add(dc.UnitPointToCadPoint(pp1));
                    fig.mPointList.Add(dc.UnitPointToCadPoint(pp2));
                    fig.mPointList.Add(dc.UnitPointToCadPoint(pp3));

                    fig.IsLoop = true;
                }
            }

            public override void DrawTemp(CadFigure fig, DrawContext dc, CadVector tp, int pen)
            {
                if (fig.PointList.Count <= 0)
                {
                    return;
                }

                dc.Drawing.DrawRect(pen, fig.PointList[0], tp);
            }

            public override CadFigure.Types EndCreate(CadFigure fig, DrawContext dc)
            {
                fig.Normal = CadVector.Create(dc.ViewDir);
                fig.Normal *= -1;

                fig.Type = Types.POLY_LINES;
                return fig.Type;
            }
        }
    }
}