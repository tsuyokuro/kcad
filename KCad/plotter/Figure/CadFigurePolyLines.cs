using OpenTK;
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

            public override void addPointInCreating(CadFigure fig, DrawContext dc, CadPoint p)
            {
                fig.mPointList.Add(p);
            }

            public override void addPoint(CadFigure fig, CadPoint p)
            {
                fig.mPointList.Add(p);
            }

            public override void draw(CadFigure fig, DrawContext dc, int pen)
            {
                if (fig.Closed)
                {
                    dc.Drawing.DrawFace(pen, getPoints(fig, 32));
                }
                else
                {
                    drawLines(fig, dc, pen);
                }
            }

            public override void drawSelected(CadFigure fig, DrawContext dc, int pen)
            {
                drawSelected_Lines(fig, dc, pen);
            }

            public override void drawSeg(CadFigure fig, DrawContext dc, int pen, int idxA, int idxB)
            {
                CadPoint a = fig.PointList[idxA];
                CadPoint b = fig.PointList[idxB];

                dc.Drawing.DrawLine(pen, a, b);
            }

            protected void drawLines(CadFigure fig, DrawContext dc, int pen)
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
                    dc.Drawing.DrawCross(pen, a, 2);
                    if (a.Selected)
                    {
                        dc.Drawing.DrawHighlightPoint(a);
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
                            dc.Drawing.DrawBezier(pen,
                                pl[i], pl[i + 1], pl[i + 2], pl[i + 3]);

                            i += 3;
                            a = pl[i];
                            continue;
                        }
                        else if (pl[i + 1].Type == CadPoint.Types.HANDLE &&
                            pl[i + 2].Type == CadPoint.Types.STD)
                        {
                            dc.Drawing.DrawBezier(pen,
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
                            dc.Drawing.DrawBezier(pen,
                                pl[i], pl[i + 1], pl[i + 2]);

                            i += 2;
                            a = pl[i];
                            continue;
                        }
                    }

                    if (i + 1 < pl.Count)
                    {
                        b = pl[i + 1];
                        dc.Drawing.DrawLine(pen, a, b);

                        a = b;
                        i++;

                        continue;
                    }

                    break;
                }

                if (fig.Closed)
                {
                    b = pl[0];
                    dc.Drawing.DrawLine(pen, a, b);
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

            private void drawSelected_Lines(CadFigure fig, DrawContext dc, int pen)
            {
                int i;
                int num = fig.PointList.Count;

                for (i = 0; i < num; i++)
                {
                    CadPoint p = fig.PointList[i];

                    if (!p.Selected) continue;

                    dc.Drawing.DrawSelectedPoint(p);


                    if (p.Type == CadPoint.Types.HANDLE)
                    {
                        int idx = i + 1;

                        if (idx < fig.PointCount)
                        {
                            CadPoint np = fig.getPointAt(idx);
                            if (np.Type != CadPoint.Types.HANDLE)
                            {
                                dc.Drawing.DrawLine(DrawTools.PEN_MATCH_SEG, p, np);
                                dc.Drawing.DrawSelectedPoint(np);
                            }
                        }

                        idx = i - 1;

                        if (idx >= 0)
                        {
                            CadPoint np = fig.getPointAt(idx);
                            if (np.Type != CadPoint.Types.HANDLE)
                            {
                                dc.Drawing.DrawLine(DrawTools.PEN_MATCH_SEG, p, np);
                                dc.Drawing.DrawSelectedPoint(np);
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
                                dc.Drawing.DrawLine(DrawTools.PEN_MATCH_SEG, p, np);
                                dc.Drawing.DrawSelectedPoint(np);
                            }
                        }

                        idx = i - 1;

                        if (idx >= 0)
                        {
                            CadPoint np = fig.getPointAt(idx);
                            if (np.Type == CadPoint.Types.HANDLE)
                            {
                                dc.Drawing.DrawLine(DrawTools.PEN_MATCH_SEG, p, np);
                                dc.Drawing.DrawSelectedPoint(np);
                            }
                        }
                    }
                }
            }

            public override void drawTemp(CadFigure fig, DrawContext dc, CadPoint tp, int pen)
            {
                if (fig.PointCount == 0)
                {
                    return;
                }

                CadPoint lastPt = fig.PointList[fig.PointCount - 1];

                dc.Drawing.DrawLine(pen, lastPt, tp);

                //dc.Drawing.DrawArrow(pen, lastPt, tp, ArrowTypes.CROSS, ArrowPos.START_END, 6, 3);

            }

            public override void setPointAt(CadFigure fig, int index, CadPoint pt)
            {
                fig.mPointList[index] = pt;
            }

            public override void startCreate(CadFigure fig, DrawContext dc)
            {
                // NOP
            }

            public override CadFigure.Types endCreate(CadFigure fig, DrawContext dc)
            {
                if (fig.PointList.Count > 2)
                {
                    Vector3d normal = CadUtil.RepresentNormal(fig.PointList);

                    double t = Vector3d.Dot(normal, dc.ViewDir);

                    DebugOut.Std.println("PolyLine endCreate t=" + t.ToString());


                    if (t < 0)
                    {
                        fig.mPointList.Reverse();
                    }
                }
                return fig.Type;
            }

            public override Centroid getCentroid(CadFigure fig)
            {
                if (fig.PointList.Count == 0)
                {
                    return default(Centroid);
                }

                if (fig.PointList.Count == 1)
                {
                    return getPointCentroid(fig);
                }

                if (fig.PointList.Count < 3)
                {
                    return getSegCentroid(fig);
                }

                return getPointListCentroid(fig);
            }

            private Centroid getPointListCentroid(CadFigure fig)
            {
                Centroid ret = default(Centroid);

                List<CadFigure> triangles = TriangleSplitter.split(fig);

                ret = CadUtil.getTriangleListCentroid(triangles);

                return ret;
            }

            private Centroid getPointCentroid(CadFigure fig)
            {
                Centroid ret = default(Centroid);

                ret.Point = fig.PointList[0];
                ret.Area = 0;

                ret.SplitList = new List<CadFigure>();
                ret.SplitList.Add(fig);

                return ret;
            }

            private Centroid getSegCentroid(CadFigure fig)
            {
                Centroid ret = default(Centroid);

                CadPoint d = fig.PointList[1] - fig.PointList[0];

                d /= 2.0;

                ret.Point = fig.PointList[0] + d;
                ret.Area = 0;
                ret.SplitList = new List<CadFigure>();
                ret.SplitList.Add(fig);

                return ret;
            }
        }
    }
}