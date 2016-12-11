using System;
using System.Collections.Generic;
using System.Drawing;

namespace Plotter
{
    public partial class CadFigure
    {
        [Serializable]
        public class CadFigurePolyLines : CadFigureBehavior
        {
            // Do not have data member.

            public override States getState(CadFigure fig)
            {
                if (fig.PointList.Count < 2)
                {
                    return States.NOT_ENOUGH;
                }
                else if (fig.PointList.Count == 2)
                {
                    return States.ENOUGH;
                }
                else if (fig.PointList.Count > 2)
                {
                    return States.CONTINUE;
                }

                return States.NONE;
            }

            public CadFigurePolyLines()
            {
            }

            public override void removeSelected(CadFigure fig)
            {
                fig.mPointList.RemoveAll(a => a.Selected);

                if (fig.PointCount < 2)
                {
                    fig.mPointList.Clear();
                }
            }

            public override void addPoint(CadFigure fig, CadPoint p)
            {
                fig.mPointList.Add(p);
            }

            public override void draw(CadFigure fig, DrawContext dc, Pen pen)
            {
                drawLines(fig, dc, pen);
            }

            public override void drawSelected(CadFigure fig, DrawContext dc, Pen pen)
            {
                drawSelected_Lines(fig, dc, pen);
            }

            public override void drawSeg(CadFigure fig, DrawContext dc, Pen pen, int idxA, int idxB)
            {
                CadPoint a = fig.PointList[idxA];
                CadPoint b = fig.PointList[idxB];

                Drawer.drawLine(dc, pen, a, b);
            }

            protected void drawLines(CadFigure fig, DrawContext dc, Pen pen)
            {
                IReadOnlyList<CadPoint> pl = fig.PointList;

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

                if (fig.Closed)
                {
                    b = pl[0];
                    Drawer.drawLine(dc, pen, a, b);
                }
            }

            public override IReadOnlyList<CadPoint> getPoints(CadFigure fig, int curveSplitNum)
            {
                List<CadPoint> ret = new List<CadPoint>();

                IReadOnlyList<CadPoint> pl = fig.PointList;

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

            private void drawSelected_Lines(CadFigure fig, DrawContext dc, Pen pen)
            {
                int i;
                int num = fig.PointList.Count;

                for (i = 0; i < num; i++)
                {
                    CadPoint p = fig.PointList[i];

                    if (!p.Selected) continue;

                    Drawer.drawSelectedPoint(dc, p);


                    if (p.Type == CadPoint.Types.HANDLE)
                    {
                        int idx = i + 1;

                        if (idx < fig.PointCount)
                        {
                            CadPoint np = fig.getPointAt(idx);
                            if (np.Type != CadPoint.Types.HANDLE)
                            {
                                Drawer.drawLine(dc, dc.Tools.MatchSegPen, p, np);
                                Drawer.drawSelectedPoint(dc, np);
                            }
                        }

                        idx = i - 1;

                        if (idx >= 0)
                        {
                            CadPoint np = fig.getPointAt(idx);
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

                        if (idx < fig.PointCount)
                        {
                            CadPoint np = fig.getPointAt(idx);
                            if (np.Type == CadPoint.Types.HANDLE)
                            {
                                Drawer.drawLine(dc, dc.Tools.MatchSegPen, p, np);
                                Drawer.drawSelectedPoint(dc, np);
                            }
                        }

                        idx = i - 1;

                        if (idx >= 0)
                        {
                            CadPoint np = fig.getPointAt(idx);
                            if (np.Type == CadPoint.Types.HANDLE)
                            {
                                Drawer.drawLine(dc, dc.Tools.MatchSegPen, p, np);
                                Drawer.drawSelectedPoint(dc, np);
                            }
                        }
                    }
                }
            }

            public override void drawTemp(CadFigure fig, DrawContext dc, CadPoint tp, Pen pen)
            {
                if (fig.PointCount == 0)
                {
                    return;
                }

                CadPoint lastPt = fig.PointList[fig.PointCount - 1];

                Drawer.drawLine(dc, pen, lastPt, tp);
            }

            public override void setPointAt(CadFigure fig, int index, CadPoint pt)
            {
                fig.mPointList[index] = pt;
            }

            public override void startCreate(CadFigure fig)
            {
                // NOP
            }

            public override CadFigure.Types endCreate(CadFigure fig)
            {
                return fig.Type;
            }
        }
    }
}