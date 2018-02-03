﻿using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Plotter
{
    public partial class CadFigure
    {
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
                VectorList vl1;
                int srcCnt = fig.mPointList.Count;

                if (srcCnt < 2)
                {
                    return false;
                }

                vl1 = GetPointsPart(fig, 0, srcCnt, 32);

                if (CadUtil.IsConvex(vl1))
                {
                    dc.Drawing.DrawFace(pen, vl1, fig.Normal, true);
                    return true;
                }

                return false;
            }

            protected void DrawLines(CadFigure fig, DrawContext dc, int pen)
            {
                int cnt = fig.mPointList.Count;
                drawLinesPart(fig, dc, 0, cnt, pen);
            }

            protected void drawLinesPart(CadFigure fig, DrawContext dc, int start, int cnt, int pen)
            {
                VectorList pl = fig.PointList;

                if (cnt <= 0)
                {
                    return;
                }

                if (fig.Normal.IsZero())
                {
                    fig.Normal = CadUtil.RepresentativeNormal(fig.PointList);
                }

                CadVector a;

                a = pl[start];

                if (cnt == 1)
                {
                    dc.Drawing.DrawCross(pen, a, 2);
                    if (a.Selected)
                    {
                        dc.Drawing.DrawHighlightPoint(a);
                    }

                    return;
                }

                pl.ForEachExpandPoints(start+1, cnt-1, 8, action);
                void action(CadVector v)
                {
                    dc.Drawing.DrawLine(pen, a, v);
                    a = v;
                }

                if (fig.IsLoop)
                {
                    dc.Drawing.DrawLine(pen, a, pl[start]);
                }
            }

            public override VectorList GetPoints(CadFigure fig, int curveSplitNum)
            {
                return GetPointsPart(fig, 0, fig.mPointList.Count, curveSplitNum);
            }

            private VectorList GetPointsPart(CadFigure fig, int start, int cnt, int curveSplitNum)
            {
                return fig.mPointList.GetExpandList(start, cnt, curveSplitNum);
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