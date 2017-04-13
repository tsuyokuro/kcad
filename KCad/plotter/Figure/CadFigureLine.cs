using System;
using System.Collections.Generic;

namespace Plotter
{
    public partial class CadFigure
    {

        [Serializable]
        public class CadFigureLine : CadFigurePolyLines
        {
            // Do not have data member.

            public override States GetState(CadFigure fig)
            {
                if (fig.PointList.Count < 1)
                {
                    return States.NOT_ENOUGH;
                }
                else if (fig.PointList.Count < 2)
                {
                    return States.WAIT_LAST_POINT;
                }

                return States.FULL;
            }

            public CadFigureLine()
            {
            }

            public override void AddPoint(CadFigure fig, CadPoint p)
            {
                fig.mPointList.Add(p);
            }

            public override CadFigure.Types EndCreate(CadFigure fig, DrawContext dc)
            {
                fig.Type = Types.POLY_LINES;
                return fig.Type;
            }
        }
    }
}