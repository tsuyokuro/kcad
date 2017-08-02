using System;
using System.Collections.Generic;

namespace Plotter
{
    public partial class CadFigure
    {

        [Serializable]
        public class CadFigureLine : CadFigurePolyLines
        {
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

            public override void AddPoint(CadFigure fig, CadVector p)
            {
                fig.mPointList.Add(p);
            }

            public override CadFigure.Types EndCreate(CadFigure fig, DrawContext dc)
            {
                fig.Type = Types.POLY_LINES;
                return fig.Type;
            }

            public override Centroid GetCentroid(CadFigure fig)
            {
                Centroid ret = default(Centroid);

                CadVector t = fig.PointList[1] - fig.PointList[0];

                ret.Point = (t / 2) + fig.PointList[0];

                ret.Area = 0;

                ret.SplitList = new List<CadFigure>();
                ret.SplitList.Add(fig);

                return ret;
            }
        }
    }
}