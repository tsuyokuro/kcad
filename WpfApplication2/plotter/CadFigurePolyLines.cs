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
    public class CadFigurePolyLines : CadFigureBehavior
    {
        public override States State
        {
            get
            {
                if (Fig.PointList.Count < 2)
                {
                    return States.NOT_ENOUGH;
                }
                else if (Fig.PointList.Count == 2)
                {
                    return States.ENOUGH;
                }
                else if (Fig.PointList.Count > 2)
                {
                    return States.CONTINUE;
                }

                return States.NONE;
            }
        }

        public CadFigurePolyLines(CadFigure fig) : base(fig)
        {
        }

        public override void removeSelected()
        {
            Fig.PointList.RemoveAll(a => a.Selected);

            if (Fig.PointCount < 2)
            {
                Fig.PointList.Clear();
            }
        }

        public override void addPoint(CadPoint p)
        {
            Fig.PointList.Add(p);
        }

        public override void draw(DrawContext dc, Pen pen)
        {
            drawLines(dc, pen);
        }

        public override void drawSelected(DrawContext dc, Pen pen)
        {
            drawSelected_Lines(dc, pen);
        }

        public override void drawSeg(DrawContext dc, Pen pen, int idxA, int idxB)
        {
            CadPoint a = Fig.PointList[idxA];
            CadPoint b = Fig.PointList[idxB];

            Drawer.drawLine(dc, pen, a, b);
        }

        protected void drawLines(DrawContext dc, Pen pen)
        {
            List<CadPoint> pl = Fig.PointList;

            if (pl.Count <= 0)
            {
                return;
            }

            CadPoint a;
            CadPoint b;

            int i = 0;
            a = pl[i];

            // If the count of point is 1, draw + mark.  
            if (pl.Count == 1)
            {
                Drawer.drawCross(dc, pen, a, 2);
                if (a.Selected)
                {
                    Drawer.drawHighlitePoint(dc, a);
                }

                return;
            }

            for (; true;)
            {
                if (i + 3 < pl.Count)
                {
                    if (pl[i + 1].Type == CadPoint.Types.HANDLE &&
                        pl[i + 2].Type == CadPoint.Types.HANDLE)
                    {
                        Drawer.drawBezier(dc, pen,
                            pl[i], pl[i + 1], pl[i + 2], pl[i + 3]);

                        i += 3;
                        a = pl[i];
                        continue;
                    }
                    else if (pl[i + 1].Type == CadPoint.Types.HANDLE &&
                        pl[i + 2].Type == CadPoint.Types.STD)
                    {
                        Drawer.drawBezier(dc, pen,
                            pl[i], pl[i + 1], pl[i + 2]);

                        i += 2;
                        a = pl[i];
                        continue;
                    }
                }

                if (i + 2 < pl.Count)
                {
                    if (pl[i + 1].Type == CadPoint.Types.HANDLE &&
                                            pl[i + 2].Type == CadPoint.Types.STD)
                    {
                        Drawer.drawBezier(dc, pen,
                            pl[i], pl[i + 1], pl[i + 2]);

                        i += 2;
                        a = pl[i];
                        continue;
                    }
                }

                if (i + 1 < pl.Count)
                {
                    b = pl[i + 1];
                    Drawer.drawLine(dc, pen, a, b);

                    a = b;
                    i++;

                    continue;
                }

                break;
            }

            if (Fig.Closed)
            {
                b = pl[0];
                Drawer.drawLine(dc, pen, a, b);
            }
        }

        public override IReadOnlyList<CadPoint> getPoints(int curveSplitNum)
        {
            List<CadPoint> ret = new List<CadPoint>();

            List<CadPoint> pl = Fig.PointList;

            if (pl.Count <= 0)
            {
                return ret;
            }

            int i = 0;

            for (; i < pl.Count;)
            {
                if (i + 3 < pl.Count)
                {
                    if (pl[i + 1].Type == CadPoint.Types.HANDLE &&
                        pl[i + 2].Type == CadPoint.Types.HANDLE)
                    {
                        CadUtil.BezierPoints(pl[i], pl[i + 1], pl[i + 2], pl[i + 3], curveSplitNum, ret);

                        i += 4;
                        continue;
                    }
                    else if (pl[i + 1].Type == CadPoint.Types.HANDLE &&
                        pl[i + 2].Type == CadPoint.Types.STD)
                    {
                        CadUtil.BezierPoints(pl[i], pl[i + 1], pl[i + 2], curveSplitNum, ret);

                        i += 3;
                        continue;
                    }
                }

                if (i + 2 < pl.Count)
                {
                    if (pl[i + 1].Type == CadPoint.Types.HANDLE &&
                                            pl[i + 2].Type == CadPoint.Types.STD)
                    {
                        CadUtil.BezierPoints(pl[i], pl[i + 1], pl[i + 2], curveSplitNum, ret);

                        i += 3;
                        continue;
                    }
                }

                ret.Add(pl[i]);
                i++;
            }

            return ret;
        }

        private void drawSelected_Lines(DrawContext dc, Pen pen)
        {
            int i;
            int num = Fig.PointList.Count;

            for (i = 0; i < num; i++)
            {
                CadPoint p = Fig.PointList[i];

                if (!p.Selected) continue;

                Drawer.drawSelectedPoint(dc, p);


                if (p.Type == CadPoint.Types.HANDLE)
                {
                    int idx = i + 1;

                    if (idx < Fig.PointCount)
                    {
                        CadPoint np = Fig.getPointAt(idx);
                        if (np.Type != CadPoint.Types.HANDLE)
                        {
                            Drawer.drawLine(dc, dc.Tools.MatchSegPen, p, np);
                            Drawer.drawSelectedPoint(dc, np);
                        }
                    }

                    idx = i - 1;

                    if (idx >= 0)
                    {
                        CadPoint np = Fig.getPointAt(idx);
                        if (np.Type != CadPoint.Types.HANDLE)
                        {
                            Drawer.drawLine(dc, dc.Tools.MatchSegPen, p, np);
                            Drawer.drawSelectedPoint(dc, np);
                        }
                    }
                }
                else
                {
                    int idx = i + 1;

                    if (idx < Fig.PointCount)
                    {
                        CadPoint np = Fig.getPointAt(idx);
                        if (np.Type == CadPoint.Types.HANDLE)
                        {
                            Drawer.drawLine(dc, dc.Tools.MatchSegPen, p, np);
                            Drawer.drawSelectedPoint(dc, np);
                        }
                    }

                    idx = i - 1;

                    if (idx >= 0)
                    {
                        CadPoint np = Fig.getPointAt(idx);
                        if (np.Type == CadPoint.Types.HANDLE)
                        {
                            Drawer.drawLine(dc, dc.Tools.MatchSegPen, p, np);
                            Drawer.drawSelectedPoint(dc, np);
                        }
                    }
                }
            }
        }

        public override void drawTemp(DrawContext dc, CadPoint tp, Pen pen)
        {
            if (Fig.PointCount == 0)
            {
                return;
            }

            CadPoint lastPt = Fig.PointList[Fig.PointCount - 1];

            Drawer.drawLine(dc, pen, lastPt, tp);
        }

        public override void setPointAt(int index, CadPoint pt)
        {
            Fig.PointList[index] = pt;
        }

        public override void startCreate()
        {
            // NOP
        }

        public override CadFigure.Types endCreate()
        {
            return Fig.Type;
        }
    }
}
