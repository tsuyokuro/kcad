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

        public override void DrawTemp(DrawContext dc, CadVector tp, int pen)
        {
        }

        public override void AddPointInCreating(DrawContext dc, CadVector p)
        {
        }


        #region Point Move
        public override void MoveSelectedPoints(DrawContext dc, CadVector delta)
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

            Util.MoveSelectedPoint(this, dc, d);

            mChildList.ForEach(c =>
            {
                c.MoveSelectedPoints(dc, delta);
            });
        }

        public override void MoveAllPoints(DrawContext dc, CadVector delta)
        {
            if (Locked) return;

            Util.MoveAllPoints(this, dc, delta);
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

        public override void Draw(DrawContext dc, int pen)
        {
            bool drawed = false;
/*
            if (IsLoop && SettingsHolder.Settings.FillFace)
            {
                drawed = DrawFaces(dc, pen);
            }
*/
            if (!drawed)
            {
                DrawLines(dc, pen);
            }
        }

        public override void DrawSelected(DrawContext dc, int pen)
        {
            DrawSelectedLines(dc, pen);
        }

        public override void DrawSeg(DrawContext dc, int pen, int idxA, int idxB)
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

        protected bool DrawFaces(DrawContext dc, int pen)
        {
            VectorList vl;
            int srcCnt = mPointList.Count;

            if (srcCnt < 2)
            {
                return false;
            }

            vl = GetPointsPart(0, srcCnt, 32);

            if (!CadUtil.IsConvex(vl))
            {
                return false;
            }

            bool outline = SettingsHolder.Settings.DrawFaceOutline;


            dc.Drawing.DrawFace(pen, vl, Normal, outline);

            // Debug 法線表示
            //dc.Drawing.DrawArrow(pen, vl[0], vl[0] + Normal * 10, ArrowTypes.PLUS, ArrowPos.END, 4, 2);

            return true;


            //if (Thickness == 0)
            //{
            //    return true;
            //}

            //if (Normal.IsZero())
            //{
            //    Normal = CadUtil.RepresentativeNormal(mPointList);
            //}

            //CadVector tv = Normal * Thickness;

            //tv *= -1;

            //VectorList vl2 = new VectorList(vl);

            //for (int i=0; i< vl2.Count; i++)
            //{
            //    vl2[i] += tv;
            //}

            //dc.Drawing.DrawFace(pen, vl2, -Normal, outline);

            //VectorList side = new VectorList(4);

            //side.Add(default(CadVector));
            //side.Add(default(CadVector));
            //side.Add(default(CadVector));
            //side.Add(default(CadVector));

            //CadVector n;

            //for (int i = 0; i < vl.Count-1; i++)
            //{
            //    side[0] = vl[i];
            //    side[1] = vl[i+1];
            //    side[2] = vl2[i+1];
            //    side[3] = vl2[i];

            //    n = CadMath.Normal(side[2],side[0],side[1]);

            //    dc.Drawing.DrawFace(pen, side, n, outline);
            //}

            //int e = vl.Count - 1;

            //side[3] = vl[e];
            //side[2] = vl[0];
            //side[1] = vl2[0];
            //side[0] = vl2[e];

            //n = CadMath.Normal(side[2], side[0], side[1]);

            //dc.Drawing.DrawFace(pen, side, n, outline);

            //return true;
        }

        protected void DrawLines(DrawContext dc, int pen)
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

            //CadVector tv = Normal * Thickness;

            //tv *= -1;

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

            PolyLineExpander.ForEachPoints(mPointList, start + 1, cnt - 1, 8, action);
            void action(CadVector v)
            {
                dc.Drawing.DrawLine(pen, a, v);

                //if (Thickness != 0)
                //{
                //    dc.Drawing.DrawLine(pen, a + tv, v + tv);
                //    dc.Drawing.DrawLine(pen, a, a + tv);
                //}
                a = v;
            }

            if (IsLoop)
            {
                dc.Drawing.DrawLine(pen, a, pl[start]);
                //if (Thickness != 0)
                //{
                //    dc.Drawing.DrawLine(pen, a + tv, pl[start] + tv);
                //    dc.Drawing.DrawLine(pen, a, a + tv);
                //}
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

        private void DrawSelectedLines(DrawContext dc, int pen)
        {
            int i;
            int num = PointList.Count;

            for (i = 0; i < num; i++)
            {
                CadVector p = PointList[i];

                if (!p.Selected) continue;

                dc.Drawing.DrawSelectedPoint(p);


                if (p.IsHandle)
                {
                    int idx = i + 1;

                    if (idx < PointCount)
                    {
                        CadVector np = GetPointAt(idx);
                        if (!np.IsHandle)
                        {
                            dc.Drawing.DrawLine(DrawTools.PEN_MATCH_SEG, p, np);
                            dc.Drawing.DrawSelectedPoint(np);
                        }
                    }

                    idx = i - 1;

                    if (idx >= 0)
                    {
                        CadVector np = GetPointAt(idx);
                        if (!np.IsHandle)
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
                        if (np.IsHandle)
                        {
                            dc.Drawing.DrawLine(DrawTools.PEN_MATCH_SEG, p, np);
                            dc.Drawing.DrawSelectedPoint(np);
                        }
                    }

                    idx = i - 1;

                    if (idx >= 0)
                    {
                        CadVector np = GetPointAt(idx);
                        if (np.IsHandle)
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

        public override DiffData EndEditWithDiff()
        {
            DiffData diff = base.EndEditWithDiff();
            RecalcNormal();

            //例外ハンドリングテスト用
            //CadVector v = mPointList[100];

            return diff;
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