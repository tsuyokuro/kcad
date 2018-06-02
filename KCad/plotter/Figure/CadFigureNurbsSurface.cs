using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using static Plotter.CadFigure;
using CadDataTypes;
using SplineCurve;

namespace Plotter
{
    public class CadFigureNurbsSurface : CadFigure
    {
        public NURBSSerface Nurbs;

        public int UCount = 1;

        public int VCount = 0;

        private VectorList NurbsPointList;

        public CadFigureNurbsSurface()
        {
            Type = Types.NURBS_LINE;
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


        public override void MoveSelectedPoints(DrawContext dc, CadVector delta)
        {
            base.MoveSelectedPoints(dc, delta);
        }

        public override void MoveAllPoints(DrawContext dc, CadVector delta)
        {
            if (Locked) return;

            Util.MoveAllPoints(this, dc, delta);
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

        public void Setup(
            int ucnt, int vcnt,
            VectorList vl,
            int uDivCnt, int vDivCnt,
            bool uedge=true, bool vedge=true,
            bool uclose=false, bool vclose=false)
        {
            UCount = ucnt;
            VCount = vcnt;
            mPointList = vl;

            Nurbs = new NURBSSerface(3, UCount, VCount, uDivCnt, vDivCnt, uedge, vedge, uclose, vclose);

            NurbsPointList = new VectorList(Nurbs.UOutCnt * Nurbs.VOutCnt);
        }

        public override void Draw(DrawContext dc, int pen)
        {
            if (PointList.Count < 2)
            {
                return;
            }

            DrawControlPoints(dc, DrawTools.PEN_NURBS_CTRL_LINE);

            DrawSurfaces(dc, pen);
        }

        public override void DrawSelected(DrawContext dc, int pen)
        {
            for (int i=0; i<mPointList.Count; i++)
            {
                ref CadVector p0 = ref mPointList.Ref(i);

                if (p0.Selected)
                {
                    dc.Drawing.DrawSelectedPoint(p0);
                }
            }
        }

        private void DrawControlPoints(DrawContext dc, int pen)
        {
            CadVector p0;
            CadVector p1;

            p0 = mPointList[0];

            for (int u = 1; u < UCount; u++)
            {
                p1 = mPointList[u];
                dc.Drawing.DrawLine(pen, p0, p1);
                p0 = p1;
            }

            p0 = mPointList[0];

            for (int v = 1; v < VCount; v++)
            {
                p1 = mPointList[UCount * v];
                dc.Drawing.DrawLine(pen, p0, p1);
                p0 = p1;
            }

            for (int v = 1; v < VCount; v++)
            {
                for (int u = 1; u < UCount; u++)
                {
                    p0 = mPointList[UCount * v + u - 1];
                    p1 = mPointList[UCount * v + u];

                    dc.Drawing.DrawLine(pen, p0, p1);

                    p0 = mPointList[UCount * (v - 1) + u];
                    p1 = mPointList[UCount * v + u];

                    dc.Drawing.DrawLine(pen, p0, p1);
                }
            }
        }

        private void DrawSurfaces(DrawContext dc, int pen)
        {
            NurbsPointList.Clear();
            Nurbs.CtrlPoints = mPointList;
            Nurbs.Eval(NurbsPointList);

            int ucnt = Nurbs.UOutCnt;
            int vcnt = Nurbs.VOutCnt;


            CadVector p0;
            CadVector p1;

            p0 = NurbsPointList[0];

            for (int u = 1; u < ucnt; u++)
            {
                p1 = NurbsPointList[u];
                dc.Drawing.DrawLine(pen, p0, p1);
                p0 = p1;
            }

            p0 = NurbsPointList[0];

            for (int v = 1; v < vcnt; v++)
            {
                p1 = NurbsPointList[ucnt * v];
                dc.Drawing.DrawLine(pen, p0, p1);
                p0 = p1;
            }

            for (int v = 1; v < vcnt; v++)
            {
                for (int u = 1; u < ucnt; u++)
                {
                    p0 = NurbsPointList[ucnt * v + u - 1];
                    p1 = NurbsPointList[ucnt * v + u];

                    dc.Drawing.DrawLine(pen, p0, p1);

                    p0 = NurbsPointList[ucnt * (v - 1) + u];
                    p1 = NurbsPointList[ucnt * v + u];

                    dc.Drawing.DrawLine(pen, p0, p1);
                }
            }
        }


        public override void InvertDir()
        {
        }

        public override void SetPointAt(int index, CadVector pt)
        {
            mPointList[index] = pt;
        }

        public override DiffData EndEditWithDiff()
        {
            DiffData diff = base.EndEditWithDiff();
            RecalcNormal();
            return diff;
        }

        public override void EndEdit()
        {
            base.EndEdit();
            RecalcNormal();
        }
    }
}