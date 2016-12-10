using System;
using System.Drawing;

namespace Plotter
{
    public partial class CadFigure
    {
        [Serializable]
        public class CadFigureCircle : CadFigureBehavior
        {
            // Do not have data member.

            public override States getState(CadFigure fig)
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

            public CadFigureCircle()
            {
            }

            public override void addPoint(CadFigure fig, CadPoint p)
            {
                p.Type = CadPoint.Types.BREAK;
                fig.PointList.Add(p);
            }

            public override void setPointAt(CadFigure fig, int index, CadPoint pt)
            {
                pt.Type = CadPoint.Types.BREAK;
                fig.PointList[index] = pt;
            }

            public override void removeSelected(CadFigure fig)
            {
                fig.PointList.RemoveAll(a => a.Selected);

                if (fig.PointCount < 2)
                {
                    fig.PointList.Clear();
                }
            }

            public override void draw(CadFigure fig, DrawContext dc, Pen pen)
            {
                drawCircle(fig, dc, pen);
            }

            public override void drawSeg(CadFigure fig, DrawContext dc, Pen pen, int idxA, int idxB)
            {
                drawCircle(fig, dc, pen);
            }

            public override void drawSelected(CadFigure fig, DrawContext dc, Pen pen)
            {
                drawSelected_Circle(fig, dc, pen);
            }

            public override void drawTemp(CadFigure fig, DrawContext dc, CadPoint tp, Pen pen)
            {
                if (fig.PointList.Count <= 0)
                {
                    return;
                }

                CadPoint cp = fig.PointList[0];


                Drawer.drawCircle(dc, pen, cp, tp);
            }

            private void drawCircle(CadFigure fig, DrawContext dc, Pen pen)
            {
                if (fig.PointList.Count == 0)
                {
                    return;
                }

                if (fig.PointList.Count == 1)
                {
                    Drawer.drawCross(dc, pen, fig.PointList[0], 2);
                    if (fig.PointList[0].Selected) Drawer.drawSelectedPoint(dc, fig.PointList[0]);
                    return;
                }

                Drawer.drawCircle(dc, pen, fig.PointList[0], fig.PointList[1]);
            }

            private void drawSelected_Circle(CadFigure fig, DrawContext dc, Pen pen)
            {
                if (fig.PointList[0].Selected) Drawer.drawSelectedPoint(dc, fig.PointList[0]);
                if (fig.PointList[1].Selected) Drawer.drawSelectedPoint(dc, fig.PointList[1]);
            }

            public override void startCreate(CadFigure fig)
            {
                // NOP
            }

            public override Types endCreate(CadFigure fig)
            {
                return fig.Type;
            }

            public override void moveSelectedPoint(CadFigure fig, CadPoint delta)
            {
                CadPoint cp = fig.StoreList[0];
                CadPoint rp = fig.StoreList[1];

                if (cp.Selected)
                {
                    fig.PointList[0] = cp + delta;
                    fig.PointList[1] = rp + delta;
                    return;
                }

                fig.PointList[1] = rp + delta;
            }
        }
    }
}