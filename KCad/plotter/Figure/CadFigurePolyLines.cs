using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using static Plotter.CadFigure;

namespace Plotter
{
    public partial class CadFigure
    {
        public class PolyLinesCreateBehavior : CreateBehavior
        {
            public override void AddPointInCreating(CadFigure fig, DrawContext dc, CadVector p)
            {
                fig.mPointList.Add(p);
            }

            public override void DrawTemp(CadFigure fig, DrawContext dc, CadVector tp, int pen)
            {
                if (fig.PointCount == 0)
                {
                    return;
                }

                CadVector lastPt = fig.PointList[fig.PointCount - 1];

                dc.Drawing.DrawLine(pen, lastPt, tp);
            }

            public override Types EndCreate(CadFigure fig, DrawContext dc)
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

            public override CreateStates GetCreateState(CadFigure fig)
            {
                if (fig.PointList.Count < 2)
                {
                    return CreateStates.NOT_ENOUGH;
                }
                else if (fig.PointList.Count == 2)
                {
                    return CreateStates.ENOUGH;
                }
                else if (fig.PointList.Count > 2)
                {
                    return CreateStates.CONTINUE;
                }

                return CreateStates.NONE;
            }

            public override void StartCreate(CadFigure fig, DrawContext dc)
            {
                // NOP
            }
        }

        public class RectCreateBehavior : CreateBehavior
        {
            public override void AddPointInCreating(CadFigure fig, DrawContext dc, CadVector p)
            {
                if (fig.mPointList.Count == 0)
                {
                    fig.mPointList.Add(p);
                }
                else
                {
                    // 左回りになるように設定

                    CadVector pp0 = dc.CadPointToUnitPoint(fig.PointList[0]);
                    CadVector pp2 = dc.CadPointToUnitPoint(p);

                    CadVector pp1 = pp0;
                    pp1.y = pp2.y;

                    CadVector pp3 = pp0;
                    pp3.x = pp2.x;

                    fig.mPointList.Add(dc.UnitPointToCadPoint(pp1));
                    fig.mPointList.Add(dc.UnitPointToCadPoint(pp2));
                    fig.mPointList.Add(dc.UnitPointToCadPoint(pp3));

                    fig.IsLoop = true;
                }
            }

            public override void DrawTemp(CadFigure fig, DrawContext dc, CadVector tp, int pen)
            {
                if (fig.PointList.Count <= 0)
                {
                    return;
                }

                dc.Drawing.DrawRect(pen, fig.PointList[0], tp);
            }

            public override Types EndCreate(CadFigure fig, DrawContext dc)
            {
                fig.Normal = CadVector.Create(dc.ViewDir);
                fig.Normal *= -1;

                fig.Type = Types.POLY_LINES;
                return fig.Type;
            }

            public override CreateStates GetCreateState(CadFigure fig)
            {
                DebugOut.Std.print("RectCreateBehavior#GetCreateState");

                if (fig.PointList.Count < 1)
                {
                    return CreateStates.NOT_ENOUGH;
                }
                else if (fig.PointList.Count < 4)
                {
                    return CreateStates.WAIT_LAST_POINT;
                }

                return CreateStates.FULL;
            }

            public override void StartCreate(CadFigure fig, DrawContext dc)
            {
                // NOP
            }
        }

        public class LineCreateBehavior : PolyLinesCreateBehavior
        {
            public override CreateStates GetCreateState(CadFigure fig)
            {
                if (fig.PointList.Count < 1)
                {
                    return CreateStates.NOT_ENOUGH;
                }
                else if (fig.PointList.Count < 2)
                {
                    return CreateStates.WAIT_LAST_POINT;
                }

                return CreateStates.FULL;
            }

            public override CadFigure.Types EndCreate(CadFigure fig, DrawContext dc)
            {
                fig.Type = Types.POLY_LINES;
                return fig.Type;
            }
        }
    }

    public class CadFigurePolyLines : CadFigure
    {
        protected CreateBehavior mCreateBehavior;

        public override CreateStates CreateState
        {
            get
            {
                CreateStates st = mCreateBehavior.GetCreateState(this);
                return st;
            }
        }

        public CadFigurePolyLines(CreateBehavior createBehavior)
        {
            Type = Types.POLY_LINES;
            mCreateBehavior = createBehavior;
        }


        public override void StartCreate(DrawContext dc)
        {
            mCreateBehavior.StartCreate(this, dc);
        }

        public override Types EndCreate(DrawContext dc)
        {
            return mCreateBehavior.EndCreate(this, dc);
        }

        public override void DrawTemp(DrawContext dc, CadVector tp, int pen)
        {
            mCreateBehavior.DrawTemp(this, dc, tp, pen);
        }

        public override void AddPointInCreating(DrawContext dc, CadVector p)
        {
            mCreateBehavior.AddPointInCreating(this, dc, p);
        }



        public override int PointCount
        {
            get
            {
                return mPointList.Count;
            }
        }

        public override void RemoveSelected()
        {
            mPointList.RemoveAll(a => a.Selected);

            if (PointCount < 2)
            {
                mPointList.Clear();
            }
        }

        public override void AddPoint(CadVector p)
        {
            mPointList.Add(p);
        }

        public override void Draw(DrawContext dc, int pen)
        {
            bool drawed = false;

            if (IsLoop)
            {
                drawed = DrawFaces(dc, pen);
            }

            if (!drawed)
            {
                DrawLines(dc, pen);
            }
        }

        public override void DrawSelected(DrawContext dc, int pen)
        {
            drawSelected_Lines(dc, pen);
        }

        public override void DrawSeg(DrawContext dc, int pen, int idxA, int idxB)
        {
            CadVector a = PointList[idxA];
            CadVector b = PointList[idxB];

            dc.Drawing.DrawLine(pen, a, b);
        }

        protected bool DrawFaces(DrawContext dc, int pen)
        {
            VectorList vl1;
            int srcCnt = mPointList.Count;

            if (srcCnt < 2)
            {
                return false;
            }

            vl1 = GetPointsPart(0, srcCnt, 32);

            if (CadUtil.IsConvex(vl1))
            {
                dc.Drawing.DrawFace(pen, vl1, Normal, true);
                return true;
            }

            return false;
        }

        protected void DrawLines(DrawContext dc, int pen)
        {
            int cnt = mPointList.Count;
            drawLinesPart(dc, 0, cnt, pen);
        }

        protected void drawLinesPart(DrawContext dc, int start, int cnt, int pen)
        {
            VectorList pl = PointList;

            if (cnt <= 0)
            {
                return;
            }

            if (Normal.IsZero())
            {
                Normal = CadUtil.RepresentativeNormal(PointList);
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

            pl.ForEachExpandPoints(start + 1, cnt - 1, 8, action);
            void action(CadVector v)
            {
                dc.Drawing.DrawLine(pen, a, v);
                a = v;
            }

            if (IsLoop)
            {
                dc.Drawing.DrawLine(pen, a, pl[start]);
            }
        }

        public override VectorList GetPoints(int curveSplitNum)
        {
            return GetPointsPart(0, mPointList.Count, curveSplitNum);
        }

        private VectorList GetPointsPart(int start, int cnt, int curveSplitNum)
        {
            return mPointList.GetExpandList(start, cnt, curveSplitNum);
        }

        private void drawSelected_Lines(DrawContext dc, int pen)
        {
            int i;
            int num = PointList.Count;

            for (i = 0; i < num; i++)
            {
                CadVector p = PointList[i];

                if (!p.Selected) continue;

                dc.Drawing.DrawSelectedPoint(p);


                if (p.Type == CadVector.Types.HANDLE)
                {
                    int idx = i + 1;

                    if (idx < PointCount)
                    {
                        CadVector np = GetPointAt(idx);
                        if (np.Type != CadVector.Types.HANDLE)
                        {
                            dc.Drawing.DrawLine(DrawTools.PEN_MATCH_SEG, p, np);
                            dc.Drawing.DrawSelectedPoint(np);
                        }
                    }

                    idx = i - 1;

                    if (idx >= 0)
                    {
                        CadVector np = GetPointAt(idx);
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

                    if (idx < PointCount)
                    {
                        CadVector np = GetPointAt(idx);
                        if (np.Type == CadVector.Types.HANDLE)
                        {
                            dc.Drawing.DrawLine(DrawTools.PEN_MATCH_SEG, p, np);
                            dc.Drawing.DrawSelectedPoint(np);
                        }
                    }

                    idx = i - 1;

                    if (idx >= 0)
                    {
                        CadVector np = GetPointAt(idx);
                        if (np.Type == CadVector.Types.HANDLE)
                        {
                            dc.Drawing.DrawLine(DrawTools.PEN_MATCH_SEG, p, np);
                            dc.Drawing.DrawSelectedPoint(np);
                        }
                    }
                }
            }
        }

        public override void SetPointAt(int index, CadVector pt)
        {
            mPointList[index] = pt;
        }

        public override DiffData EndEdit()
        {
            DiffData diff = base.EndEdit();
            RecalcNormal();

            return diff;
        }

        public override Centroid GetCentroid()
        {
            if (PointList.Count == 0)
            {
                return default(Centroid);
            }

            if (PointList.Count == 1)
            {
                return getPointCentroid();
            }

            if (PointList.Count < 3)
            {
                return getSegCentroid();
            }

            return getPointListCentroid();
        }

        public override void RecalcNormal()
        {
            if (PointList.Count == 0)
            {
                return;
            }

            CadVector prevNormal = Normal;

            CadVector normal = CadUtil.RepresentativeNormal(PointList);

            if (CadMath.InnerProduct(prevNormal, normal) < 0)
            {
                normal *= -1;
            }

            Normal = normal;
        }

        private Centroid getPointListCentroid()
        {
            Centroid ret = default(Centroid);

            List<CadFigure> triangles = TriangleSplitter.Split(this);

            ret = CadUtil.TriangleListCentroid(triangles);

            return ret;
        }

        private Centroid getPointCentroid()
        {
            Centroid ret = default(Centroid);

            ret.Point = PointList[0];
            ret.Area = 0;

            ret.SplitList = new List<CadFigure>();
            ret.SplitList.Add(this);

            return ret;
        }

        private Centroid getSegCentroid()
        {
            Centroid ret = default(Centroid);

            CadVector d = PointList[1] - PointList[0];

            d /= 2.0;

            ret.Point = PointList[0] + d;
            ret.Area = 0;
            ret.SplitList = new List<CadFigure>();
            ret.SplitList.Add(this);

            return ret;
        }
    }
}