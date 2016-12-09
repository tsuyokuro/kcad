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
    public class CadFigureRect : CadFigurePolyLines
    {
        public override States State
        {
            get
            {
                if (Fig.PointList.Count < 1)
                {
                    return States.NOT_ENOUGH;
                }
                else if (Fig.PointList.Count < 4)
                {
                    return States.WAIT_LAST_POINT;
                }
                return States.FULL;
            }
        }

        public CadFigureRect(CadFigure fig) : base(fig)
        {
        }

        public override void addPoint(CadPoint p)
        {
            if (Fig.PointList.Count == 0)
            {
                Fig.PointList.Add(p);
            }
            else
            {
                CadPoint p0 = Fig.PointList[0];
                CadPoint p2 = p;

                CadPoint p1 = p0;
                p1.x = p2.x;

                CadPoint p3 = p0;
                p3.y = p2.y;

                Fig.PointList.Add(p1);
                Fig.PointList.Add(p2);
                Fig.PointList.Add(p3);

                Fig.Closed = true;
            }
        }

        public override void drawTemp(DrawContext dc, CadPoint tp, Pen pen)
        {
            if (Fig.PointList.Count <= 0)
            {
                return;
            }

            Drawer.drawRect(dc, pen, Fig.PointList[0], tp);
        }

        public override CadFigure.Types endCreate()
        {
            Fig.Type = Types.POLY_LINES;
            return Fig.Type;
        }
    }
}
