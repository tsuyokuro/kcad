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
                bool drawed = false;

                if (fig.IsLoop)
                {
                    drawed = DrawFaces(fig, dc, pen);
                }

                if (!drawed)
                {
                    DrawLines(fig, dc, pen);
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

            protected bool DrawFaces(CadFigure fig, DrawContext dc, int pen)
            {
                List<CadVector> vl1;
                int srcCnt = fig.mPointList.Count;

                if (srcCnt < 2)
                {
                    return false;
                }

                if (fig.Thickness == 0)
                {
                     vl1 = GetPointsPart(fig, 0, srcCnt, 32);

                    if (CadUtil.IsConvex(vl1))
                    {
                        dc.Drawing.DrawFace(pen, vl1, fig.Normal, true);
                        return true;
                    }

                    return false;
                }

                srcCnt /= 2;

                vl1 = GetPointsPart(fig, 0, srcCnt, 32);
                List<CadVector> vl2 = GetPointsPart(fig, srcCnt, srcCnt, 32);

                if (CadUtil.IsConvex(vl1))
                {
                    dc.Drawing.DrawFace(pen, vl1, fig.Normal, true);
                    dc.Drawing.DrawFace(pen, vl2, fig.Normal, true);
                }
                else
                {
                    return false;
                }

                List<CadVector> sd = new List<CadVector>();

                sd.Add(CadVector.Zero);
                sd.Add(CadVector.Zero);
                sd.Add(CadVector.Zero);
                sd.Add(CadVector.Zero);

                int i = 0;

                CadVector n;
                int cnt = vl1.Count;

                for (; i<cnt-1; i++)
                {
                    sd[0] = vl1[i];
                    sd[1] = vl1[i + 1];
                    sd[2] = vl2[i + 1];
                    sd[3] = vl2[i];

                    n = CadMath.Normal(sd[0], sd[1], sd[2]);
                    dc.Drawing.DrawFace(pen, sd, n, true);
                }

                sd[0] = vl1[cnt-1];
                sd[1] = vl1[0];
                sd[2] = vl2[0];
                sd[3] = vl2[cnt-1];

                n = CadMath.Normal(sd[0], sd[1], sd[2]);
                dc.Drawing.DrawFace(pen, sd, n, true);

                return true;
            }

            protected void DrawLines(CadFigure fig, DrawContext dc, int pen)
            {
                int cnt = fig.mPointList.Count;

                if (fig.Thickness != 0)
                {
                    cnt /= 2;
                }

                drawLinesPart(fig, dc, 0, cnt, pen);

                if (fig.Thickness == 0)
                {
                    return;
                }

                drawLinesPart(fig, dc, cnt, cnt, pen);

                int i = 0;
                int ti = i + cnt;

                for (;i<cnt;i++, ti++)
                {
                    CadVector a = fig.mPointList[i];
                    CadVector b = fig.mPointList[ti];

                    if (a.Type == CadVector.Types.HANDLE)
                    {
                        continue;
                    }

                    dc.Drawing.DrawLine(pen, a, b);
                }
            }

            protected void drawLinesPart(CadFigure fig, DrawContext dc, int start, int cnt, int pen)
            {
                IReadOnlyList<CadVector> pl = fig.PointList;

                if (cnt <= 0)
                {
                    return;
                }

                if (fig.Normal.IsZero())
                {
                    fig.Normal = CadUtil.RepresentativeNormal(fig.PointList);
                }

                //CadVector tv = (fig.Normal * -1) * fig.Thickness;

                CadVector a;
                CadVector b;

                int i = start;
                a = pl[i];

                // If the count of point is 1, draw + mark.  
                if (cnt == 1)
                {
                    dc.Drawing.DrawCross(pen, a, 2);
                    if (a.Selected)
                    {
                        dc.Drawing.DrawHighlightPoint(a);
                    }

                    return;
                }

                int end = start + cnt - 1;

                for (; true;)
                {
                    if (i + 3 <= end)
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

                    if (i + 2 <= end)
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

                    if (i + 1 <= end)
                    {
                        b = pl[i + 1];
                        dc.Drawing.DrawLine(pen, a, b);

                        /*
                        if (fig.Thickness != 0)
                        {
                            dc.Drawing.DrawLine(pen, a + tv, b + tv);
                            dc.Drawing.DrawLine(pen, a, a + tv);
                        }
                        */
                        a = b;
                        i++;

                        continue;
                    }

                    break;
                }

                if (fig.IsLoop)
                {
                    b = pl[start];
                    dc.Drawing.DrawLine(pen, a, b);

                    /*
                    if (fig.Thickness != 0)
                    {
                        dc.Drawing.DrawLine(pen, a + tv, b + tv);
                        dc.Drawing.DrawLine(pen, a, a + tv);
                    }
                    */
                }
            }

            public override List<CadVector> GetPoints(CadFigure fig, int curveSplitNum)
            {
                return GetPointsPart(fig, 0, fig.mPointList.Count, curveSplitNum);
            }

            private List<CadVector> GetPointsPart(CadFigure fig, int start, int cnt, int curveSplitNum)
            {
                List<CadVector> ret = new List<CadVector>();

                IReadOnlyList<CadVector> pl = fig.PointList;

                if (cnt <= 0)
                {
                    return ret;
                }

                int i = start;
                int end = start + cnt - 1;

                for (; i <= end;)
                {
                    if (i + 3 <= end)
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

                    if (i + 2 <= end)
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
                else if (fig.PointList.Count < 2)
                {
                    return Types.NONE;
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

                CadVector normal = CadUtil.RepresentativeNormal(fig.PointList);

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