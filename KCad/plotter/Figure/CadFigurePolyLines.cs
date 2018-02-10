using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using static Plotter.CadFigure;

namespace Plotter
{
    public class CadFigurePolyLines : CadFigure
    {
        #region Create behavior
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

        public override void EndCreate(DrawContext dc)
        {
            mCreateBehavior.EndCreate(this, dc);
        }

        public override void DrawTemp(DrawContext dc, CadVector tp, int pen)
        {
            mCreateBehavior.DrawTemp(this, dc, tp, pen);
        }

        public override void AddPointInCreating(DrawContext dc, CadVector p)
        {
            mCreateBehavior.AddPointInCreating(this, dc, p);
        }
        #endregion

        #region Point Move
        public override void MoveSelectedPoints(DrawContext dc, CadVector delta)
        {
            //base.MoveSelectedPoints(dc, delta);

            if (Locked) return;

            CadVector d;


            if (!IsSelectedAll())
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

                    //DebugOut.Std.println("nvNormal:" + nvNormal.SimpleString());
                    //DebugOut.Std.println("para d:" + d.SimpleString());
                }

                //DebugOut.Std.println("vdir:" + vdir.SimpleString());
                //DebugOut.Std.println("n:" + Normal.SimpleString());
                //DebugOut.Std.println("delta:" + delta.SimpleString());
                //DebugOut.Std.println("a:" + a.SimpleString());
                //DebugOut.Std.println("b:" + b.SimpleString());
                //DebugOut.Std.println("d:" + d.SimpleString());
            }
            else
            {
                d = delta;
            }

            Log.d("MoveSelectedPoints d=" + d.SimpleString());

            Util.MoveSelectedPoint(this, dc, d);

            mChildList.ForEach(c =>
            {
                c.MoveSelectedPoints(dc, delta);
            });


            /*
            if (Locked) return;
            
            Util.MoveSelectedPoint(this, dc, delta);

            mChildList.ForEach(c =>
            {
                c.MoveSelectedPoints(dc, delta);
            });
            */
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

            bool outline = dc.DrawFaceOutline;


            dc.Drawing.DrawFace(pen, vl, Normal, outline);

            if (Thickness == 0)
            {
                return true;
            }

            if (Normal.IsZero())
            {
                Normal = CadUtil.RepresentativeNormal(mPointList);
            }

            CadVector tv = Normal * Thickness;

            tv *= -1;

            VectorList vl2 = new VectorList(vl);

            for (int i=0; i< vl2.Count; i++)
            {
                vl2[i] += tv;
            }

            dc.Drawing.DrawFace(pen, vl2, (Normal * -1), outline);

            VectorList side = new VectorList(4);

            side.Add(default(CadVector));
            side.Add(default(CadVector));
            side.Add(default(CadVector));
            side.Add(default(CadVector));

            CadVector n;

            for (int i = 0; i < vl.Count-1; i++)
            {
                side[0] = vl[i];
                side[1] = vl[i+1];
                side[2] = vl2[i+1];
                side[3] = vl2[i];

                n = CadMath.Normal(side[2],side[0],side[1]);

                dc.Drawing.DrawFace(pen, side, n, outline);
            }

            int e = vl.Count - 1;

            side[3] = vl[e];
            side[2] = vl[0];
            side[1] = vl2[0];
            side[0] = vl2[e];

            n = CadMath.Normal(side[2], side[0], side[1]);

            dc.Drawing.DrawFace(pen, side, n, outline);

            return true;
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

            CadVector tv = Normal * Thickness;

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

            PolyLineExpander.ForEachExpandPoints(mPointList, start + 1, cnt - 1, 8, action);
            void action(CadVector v)
            {
                dc.Drawing.DrawLine(pen, a, v);
                dc.Drawing.DrawLine(pen, a+tv, v+tv);
                dc.Drawing.DrawLine(pen, a, a+tv);
                a = v;
            }

            if (IsLoop)
            {
                dc.Drawing.DrawLine(pen, a, pl[start]);
                dc.Drawing.DrawLine(pen, a+tv, pl[start]+tv);
                dc.Drawing.DrawLine(pen, a, a + tv);
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