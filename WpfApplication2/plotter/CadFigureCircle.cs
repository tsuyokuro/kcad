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
    public class CadFigureCircle : CadFigureBehavior
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

        public CadFigureCircle(CadFigure fig) : base(fig)
        {
        }

        public override void addPoint(CadPoint p)
        {
            p.Type = CadPoint.Types.BREAK;
            Fig.PointList.Add(p);
        }

        public override void setPointAt(int index, CadPoint pt)
        {
            pt.Type = CadPoint.Types.BREAK;
            Fig.PointList[index] = pt;
        }

        public override void removeSelected()
        {
            Fig.PointList.RemoveAll(a => a.Selected);

            if (Fig.PointCount < 2)
            {
                Fig.PointList.Clear();
            }
        }

        public override void draw(DrawContext dc, Pen pen)
        {
            drawCircle(dc, pen);
        }

        public override void drawSeg(DrawContext dc, Pen pen, int idxA, int idxB)
        {
            drawCircle(dc, pen);
        }

        public override void drawSelected(DrawContext dc, Pen pen)
        {
            drawSelected_Circle(dc, pen);
        }

        public override void drawTemp(DrawContext dc, CadPoint tp, Pen pen)
        {
            if (Fig.PointList.Count <= 0)
            {
                return;
            }

            CadPoint cp = Fig.PointList[0];


            Drawer.drawCircle(dc, pen, cp, tp);
        }



        private void drawCircle(DrawContext dc, Pen pen)
        {
            if (Fig.PointList.Count == 0)
            {
                return;
            }

            if (Fig.PointList.Count == 1)
            {
                Drawer.drawCross(dc, pen, Fig.PointList[0], 2);
                if (Fig.PointList[0].Selected) Drawer.drawSelectedPoint(dc, Fig.PointList[0]);
                return;
            }

            Drawer.drawCircle(dc, pen, Fig.PointList[0], Fig.PointList[1]);
        }

        private void drawSelected_Circle(DrawContext dc, Pen pen)
        {
            if (Fig.PointList[0].Selected) Drawer.drawSelectedPoint(dc, Fig.PointList[0]);
            if (Fig.PointList[1].Selected) Drawer.drawSelectedPoint(dc, Fig.PointList[1]);
        }

        public override void startCreate()
        {
            // NOP
        }

        public override Types endCreate()
        {
            return Fig.Type;
        }

        public override void moveSelectedPoint(CadPoint delta)
        {
            CadPoint cp = Fig.StoreList[0];
            CadPoint rp = Fig.StoreList[1];

            if (cp.Selected)
            {
                Fig.PointList[0] = cp + delta;
                Fig.PointList[1] = rp + delta;
                return;
            }

            Fig.PointList[1] = rp + delta;
        }
    }
}
