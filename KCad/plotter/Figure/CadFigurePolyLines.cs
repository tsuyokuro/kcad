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
            public override States GetState(CadFigure fig)
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

            public override void RemoveSelected(CadFigure fig)
            {
                fig.mPointList.RemoveAll(a => a.Selected);

                if (fig.PointCount < 2)
                {
                    fig.mPointList.Clear();
                }
            }

            public override void AddPointInCreating(CadFigure fig, DrawContext dc, CadVector p)
            {
                fig.mPointList.Add(p);
            }

            public override void AddPoint(CadFigure fig, CadVector p)
            {
                fig.mPointList.Add(p);
            }

            public override void Draw(CadFigure fig, DrawContext dc, int pen)
            {
                if (fig.Closed && CadUtil.IsConvex(fig.PointList))
                {
                    dc.Drawing.DrawFace(pen, GetPoints(fig, 32), fig.Normal, true);
                }
                else
                {
                    drawLines(fig, dc, pen);
                }
            }

            public override void DrawSelected(CadFigure fig, DrawContext dc, int pen)
            {
                drawSelected_Lines(fig, dc, pen);
            }

            public override void DrawSeg(CadFigure fig, DrawContext dc, int pen, int idxA, int idxB)
            {
                CadVector a = fig.PointList[idxA];
                CadVector b = fig.PointList[idxB];

                dc.Drawing.DrawLine(pen, a, b);
            }

            protected void drawLines(CadFigure fig, DrawContext dc, int pen)
            {
                IReadOnlyList<CadVector> pl = fig.PointList;

                if (pl.Count <= 0)
                {
                    return;
                }

                CadVector a;
                CadVector b;

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
                        if (pl[i + 1].Type == CadVector.Types.HANDLE &&
                            pl[i + 2].Type == CadVector.Types.HANDLE)
                        {
                            dc.Drawing.DrawBezier(pen,
                                pl[i], pl[i + 1], pl[i + 2], pl[i + 3]);

                            i += 3;
                            a = pl[i];
                            continue;
                        }
                        else if (pl[i + 1].Type == CadVector.Types.HANDLE &&
                            pl[i + 2].Type == CadVector.Types.STD)
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
                        if (pl[i + 1].Type == CadVector.Types.HANDLE &&
                                                pl[i + 2].Type == CadVector.Types.STD)
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

            public override IReadOnlyList<CadVector> GetPoints(CadFigure fig, int curveSplitNum)
            {
                List<CadVector> ret = new List<CadVector>();

                IReadOnlyList<CadVector> pl = fig.PointList;

                if (pl.Count <= 0)
                {
                    return ret;
                }

                int i = 0;

                for (; i < pl.Count;)
                {
                    if (i + 3 < pl.Count)
                    {
                        if (pl[i + 1].Type == CadVector.Types.HANDLE &&
                            pl[i + 2].Type == CadVector.Types.HANDLE)
                        {
                            CadUtil.BezierPoints(pl[i], pl[i + 1], pl[i + 2], pl[i + 3], curveSplitNum, ret);

                            i += 4;
                            continue;
                        }
                        else if (pl[i + 1].Type == CadVector.Types.HANDLE &&
                            pl[i + 2].Type == CadVector.Types.STD)
                        {
                            CadUtil.BezierPoints(pl[i], pl[i + 1], pl[i + 2], curveSplitNum, ret);

                            i += 3;
                            continue;
                        }
                    }

                    if (i + 2 < pl.Count)
                    {
                        if (pl[i + 1].Type == CadVector.Types.HANDLE &&
                                                pl[i + 2].Type == CadVector.Types.STD)
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
                    CadVector p = fig.PointList[i];

                    if (!p.Selected) continue;

                    dc.Drawing.DrawSelectedPoint(p);


                    if (p.Type == CadVector.Types.HANDLE)
                    {
                        int idx = i + 1;

                        if (idx < fig.PointCount)
                        {
                            CadVector np = fig.GetPointAt(idx);
                            if (np.Type != CadVector.Types.HANDLE)
                            {
                                dc.Drawing.DrawLine(DrawTools.PEN_MATCH_SEG, p, np);
                                dc.Drawing.DrawSelectedPoint(np);
                            }
                        }

                        idx = i - 1;

                        if (idx >= 0)
                        {
                            CadVector np = fig.GetPointAt(idx);
                            if (np.Type != CadVector.Types.HANDLE)
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
                            CadVector np = fig.GetPointAt(idx);
                            if (np.Type == CadVector.Types.HANDLE)
                            {
                                dc.Drawing.DrawLine(DrawTools.PEN_MATCH_SEG, p, np);
                                dc.Drawing.DrawSelectedPoint(np);
                            }
                        }

                        idx = i - 1;

                        if (idx >= 0)
                        {
                            CadVector np = fig.GetPointAt(idx);
                            if (np.Type == CadVector.Types.HANDLE)
                            {
                                dc.Drawing.DrawLine(DrawTools.PEN_MATCH_SEG, p, np);
                                dc.Drawing.DrawSelectedPoint(np);
                            }
                        }
                    }
                }
            }

            public override void DrawTemp(CadFigure fig, DrawContext dc, CadVector tp, int pen)
            {
                if (fig.PointCount == 0)
                {
                    return;
                }

                CadVector lastPt = fig.PointList[fig.PointCount - 1];

                dc.Drawing.DrawLine(pen, lastPt, tp);

                //dc.Drawing.DrawArrow(pen, lastPt, tp, ArrowTypes.CROSS, ArrowPos.START_END, 6, 3);

            }

            public override void SetPointAt(CadFigure fig, int index, CadVector pt)
            {
                fig.mPointList[index] = pt;
            }

            public override void StartCreate(CadFigure fig, DrawContext dc)
            {
                // NOP
            }

            public override CadFigure.Types EndCreate(CadFigure fig, DrawContext dc)
            {
                if (fig.PointList.Count > 2)
                {
                    //Vector3d normal = CadUtil.RepresentativeNormal(fig.PointList);
                    //double t = Vector3d.Dot(normal, dc.ViewDir);

                    fig.Normal = CadVector.Create(dc.ViewDir);
                    fig.Normal *= -1;
                }
                return fig.Type;
            }

            public override void EndEdit(CadFigure fig)
            {
                RecalcNormal(fig);
            }

            public override Centroid GetCentroid(CadFigure fig)
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

            public override void RecalcNormal(CadFigure fig)
            {
                if (fig.PointList.Count == 0)
                {
                    return;
                }

                CadVector prevNormal = fig.Normal;

                CadVector normal = CadVector.Create(CadUtil.RepresentativeNormal(fig.PointList));

                if (CadMath.InnerProduct(prevNormal, normal) < 0)
                {
                    normal *= -1;
                }

                fig.Normal = normal;
            }

            private Centroid getPointListCentroid(CadFigure fig)
            {
                Centroid ret = default(Centroid);

                List<CadFigure> triangles = TriangleSplitter.Split(fig);

                ret = CadUtil.TriangleListCentroid(triangles);

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

                CadVector d = fig.PointList[1] - fig.PointList[0];

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