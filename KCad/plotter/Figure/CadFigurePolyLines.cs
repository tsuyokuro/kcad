using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using static Plotter.CadFigure;
using CadDataTypes;

namespace Plotter
{
    public class CadFigurePolyLines : CadFigure
    {
        protected bool RestrictionByNormal = false;

        public CadFigurePolyLines()
        {
            Type = Types.POLY_LINES;
        }

        public override void StartCreate(DrawContext dc)
        {
        }

        public override void EndCreate(DrawContext dc)
        {
        }

        public override void DrawTemp(DrawContext dc, CadVector tp, DrawPen pen)
        {
        }

        public override void AddPointInCreating(DrawContext dc, CadVector p)
        {
        }


        #region Point Move
        public override void MoveSelectedPointsFromStored(DrawContext dc, CadVector delta)
        {
            //base.MoveSelectedPoints(dc, delta);

            if (Locked) return;

            CadVector d;


            if (!IsSelectedAll() && mPointList.Count > 2 && RestrictionByNormal)
            {
                CadVector vdir = (CadVector)dc.ViewDir;

                CadVector a = delta;
                CadVector b = delta + vdir;

                d = CadUtil.CrossPlane(a, b, StoreList[0], Normal);

                if (!d.Valid)
                {
                    CadVector nvNormal = CadMath.Normal(Normal, vdir);

                    double ip = CadMath.InnerProduct(nvNormal, delta);

                    d = nvNormal * ip;
                }
            }
            else
            {
                d = delta;
            }

            FigUtil.MoveSelectedPointsFromStored(this, dc, d);

            mChildList.ForEach(c =>
            {
                c.MoveSelectedPointsFromStored(dc, delta);
            });
        }

        public override void MoveAllPoints(CadVector delta)
        {
            if (Locked) return;

            FigUtil.MoveAllPoints(this, delta);
        }
        #endregion


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

        public override void Draw(DrawContext dc, DrawPen pen)
        {
            bool drawed = false;

            if (!drawed)
            {
                DrawLines(dc, pen);
            }
        }

        public override void DrawSelected(DrawContext dc, DrawPen pen)
        {
            DrawSelectedLines(dc, pen);
        }

        public override void DrawSeg(DrawContext dc, DrawPen pen, int idxA, int idxB)
        {
            CadVector a = PointList[idxA];
            CadVector b = PointList[idxB];

            dc.Drawing.DrawLine(pen, a, b);
        }

        public override void InvertDir()
        {
            mPointList.Reverse();
            Normal = -Normal;
        }

        protected void DrawLines(DrawContext dc, DrawPen pen)
        {
            VectorList pl = mPointList;
            int start = 0;
            int cnt = mPointList.Count;

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
                    dc.Drawing.DrawHighlightPoint(a, dc.GetPen(DrawTools.PEN_POINT_HIGHLIGHT));
                }

                return;
            }

            PolyLineExpander.ForEachPoints(mPointList, start + 1, cnt - 1, 8, action);
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
            return PolyLineExpander.GetExpandList(mPointList, start, cnt, curveSplitNum);
        }

        private void DrawSelectedLines(DrawContext dc, DrawPen pen)
        {
            int i;
            int num = PointList.Count;

            for (i = 0; i < num; i++)
            {
                CadVector p = PointList[i];

                if (!p.Selected) continue;

                dc.Drawing.DrawSelectedPoint(p, dc.GetPen(DrawTools.PEN_SELECT_POINT));


                if (p.IsHandle)
                {
                    int idx = i + 1;

                    if (idx < PointCount)
                    {
                        CadVector np = GetPointAt(idx);
                        if (!np.IsHandle)
                        {
                            dc.Drawing.DrawLine(dc.GetPen(DrawTools.PEN_MATCH_SEG), p, np);
                            dc.Drawing.DrawSelectedPoint(np, dc.GetPen(DrawTools.PEN_SELECT_POINT));
                        }
                    }

                    idx = i - 1;

                    if (idx >= 0)
                    {
                        CadVector np = GetPointAt(idx);
                        if (!np.IsHandle)
                        {
                            dc.Drawing.DrawLine(dc.GetPen(DrawTools.PEN_MATCH_SEG), p, np);
                            dc.Drawing.DrawSelectedPoint(np, dc.GetPen(DrawTools.PEN_SELECT_POINT));
                        }
                    }
                }
                else
                {
                    int idx = i + 1;

                    if (idx < PointCount)
                    {
                        CadVector np = GetPointAt(idx);
                        if (np.IsHandle)
                        {
                            dc.Drawing.DrawLine(dc.GetPen(DrawTools.PEN_MATCH_SEG), p, np);
                            dc.Drawing.DrawSelectedPoint(np, dc.GetPen(DrawTools.PEN_SELECT_POINT));
                        }
                    }

                    idx = i - 1;

                    if (idx >= 0)
                    {
                        CadVector np = GetPointAt(idx);
                        if (np.IsHandle)
                        {
                            dc.Drawing.DrawLine(dc.GetPen(DrawTools.PEN_MATCH_SEG), p, np);
                            dc.Drawing.DrawSelectedPoint(np, dc.GetPen(DrawTools.PEN_SELECT_POINT));
                        }
                    }
                }
            }
        }

        public override void SetPointAt(int index, CadVector pt)
        {
            mPointList[index] = pt;
        }

       public override void EndEdit()
        {
            base.EndEdit();
            RecalcNormal();
            //例外ハンドリングテスト用
            //CadVector v = mPointList[100];
        }

        public override Centroid GetCentroid()
        {
            if (PointList.Count == 0)
            {
                return default(Centroid);
            }

            if (PointList.Count == 1)
            {
                return GetPointCentroid();
            }

            if (PointList.Count < 3)
            {
                return GetSegCentroid();
            }

            return GetPointListCentroid();
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

        private Centroid GetPointListCentroid()
        {
            Centroid ret = default(Centroid);

            List<CadFigure> triangles = TriangleSplitter.Split(this);

            ret = CadUtil.TriangleListCentroid(triangles);

            return ret;
        }

        private Centroid GetPointCentroid()
        {
            Centroid ret = default(Centroid);

            ret.Point = PointList[0];
            ret.Area = 0;

            return ret;
        }

        private Centroid GetSegCentroid()
        {
            Centroid ret = default(Centroid);

            CadVector d = PointList[1] - PointList[0];

            d /= 2.0;

            ret.Point = PointList[0] + d;
            ret.Area = 0;

            return ret;
        }
    }
}