using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Plotter
{
    using static CadFigure;

    [Serializable]
    public class CadFigureLine : CadFigurePolyLines
    {
        public override States State
        {
            get
            {
                if (Fig.PointList.Count < 1)
                {
                    return States.NOT_ENOUGH;
                }
                else if (Fig.PointList.Count < 2)
                {
                    return States.WAIT_LAST_POINT;
                }

                return States.FULL;
            }
        }

        public CadFigureLine(CadFigure fig) : base(fig)
        {
        }

        public override void addPoint(CadPoint p)
        {
            Fig.PointList.Add(p);
        }

        public override CadFigure.Types endCreate()
        {
            Fig.Type = Types.POLY_LINES;
            return Fig.Type;
        }
    }
}
