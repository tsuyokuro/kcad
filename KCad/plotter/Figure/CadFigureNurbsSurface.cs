﻿using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using static Plotter.CadFigure;
using CadDataTypes;
using SplineCurve;
using Plotter.Serializer;
using Newtonsoft.Json.Linq;

namespace Plotter
{
    public class CadFigureNurbsSurface : CadFigure
    {
        public NurbsSurface Nurbs;

        private VertexList NurbsPointList;

        private bool NeedsEval = true;

        public CadFigureNurbsSurface()
        {
            Type = Types.NURBS_SURFACE;
        }

        public override void StartCreate(DrawContext dc)
        {
        }

        public override void EndCreate(DrawContext dc)
        {
        }

        public override void DrawTemp(DrawContext dc, CadVertex tp, DrawPen pen)
        {
        }

        public override void AddPointInCreating(DrawContext dc, CadVertex p)
        {
        }


        public override void MoveSelectedPointsFromStored(DrawContext dc, CadVertex delta)
        {
            base.MoveSelectedPointsFromStored(dc, delta);

            NeedsEval = true;
        }

        public override void MoveAllPoints(CadVertex delta)
        {
            if (Locked) return;

            FigUtil.MoveAllPoints(this, delta);
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

        public override void AddPoint(CadVertex p)
        {
            mPointList.Add(p);
        }

        public void Setup(
            int deg,
            int ucnt, int vcnt,
            VertexList vl,
            int[] ctrlOrder,
            int uDivCnt, int vDivCnt,
            bool uedge=true, bool vedge=true,
            bool uclose=false, bool vclose=false)
        {
            Nurbs = new NurbsSurface(deg, ucnt, vcnt, uDivCnt, vDivCnt, uedge, vedge, uclose, vclose);

            Nurbs.CtrlPoints = vl;

            mPointList = vl;

            if (ctrlOrder == null)
            {
                Nurbs.SetupDefaultCtrlOrder();
            }
            else
            {
                Nurbs.CtrlOrder = ctrlOrder;
            }

            NurbsPointList = new VertexList(Nurbs.UOutCnt * Nurbs.VOutCnt);
        }

        public override void Draw(DrawContext dc, DrawPen pen)
        {
            if (PointList.Count < 2)
            {
                return;
            }

            DrawSurfaces(dc, pen);

            DrawControlPoints(dc,
                dc.GetPen(DrawTools.PEN_NURBS_CTRL_LINE));
        }

        public override void DrawSelected(DrawContext dc, DrawPen pen)
        {
            for (int i=0; i<mPointList.Count; i++)
            {
                ref CadVertex p0 = ref mPointList.Ref(i);

                if (p0.Selected)
                {
                    dc.Drawing.DrawSelectedPoint(p0, dc.GetPen(DrawTools.PEN_SELECT_POINT));
                }
            }
        }

        private void DrawControlPoints(DrawContext dc, DrawPen pen)
        {
            CadVertex p0;
            CadVertex p1;

            int ucnt = Nurbs.UCtrlDataCnt;
            int vcnt = Nurbs.VCtrlDataCnt;

            p0 = mPointList[0];

            for (int u = 1; u < ucnt; u++)
            {
                p1 = mPointList[u];
                dc.Drawing.DrawLine(pen, p0, p1);
                p0 = p1;
            }

            p0 = mPointList[0];

            for (int v = 1; v < vcnt; v++)
            {
                p1 = mPointList[ucnt * v];
                dc.Drawing.DrawLine(pen, p0, p1);
                p0 = p1;
            }

            for (int v = 1; v < vcnt; v++)
            {
                for (int u = 1; u < ucnt; u++)
                {
                    p0 = mPointList[ucnt * v + u - 1];
                    p1 = mPointList[ucnt * v + u];

                    dc.Drawing.DrawLine(pen, p0, p1);

                    p0 = mPointList[ucnt * (v - 1) + u];
                    p1 = mPointList[ucnt * v + u];

                    dc.Drawing.DrawLine(pen, p0, p1);
                }
            }
        }

        private void DrawSurfaces(DrawContext dc, DrawPen pen)
        {
            if (NeedsEval)
            {
                NurbsPointList.Clear();
                Nurbs.Eval(NurbsPointList);
                NeedsEval = false;
            }

            int ucnt = Nurbs.UOutCnt;
            int vcnt = Nurbs.VOutCnt;


            CadVertex p0;
            CadVertex p1;

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

        public override void SetPointAt(int index, CadVertex pt)
        {
            mPointList[index] = pt;
        }

        public override void EndEdit()
        {
            base.EndEdit();
            RecalcNormal();
        }

        public override MpGeometricData GeometricDataToMp()
        {
            MpNurbsSurfaceGeometricData geo = new MpNurbsSurfaceGeometricData();
            geo.Nurbs = MpNurbsSurface.Create(Nurbs);
            return geo;
        }

        public override void GeometricDataFromMp(MpGeometricData geo)
        {
            if (!(geo is MpNurbsSurfaceGeometricData))
            {
                return;
            }

            MpNurbsSurfaceGeometricData g = (MpNurbsSurfaceGeometricData)geo;

            Nurbs = g.Nurbs.Restore();

            mPointList = Nurbs.CtrlPoints;

            NurbsPointList = new VertexList(Nurbs.UOutCnt * Nurbs.VOutCnt);

            NeedsEval = true;
        }


        public override JObject GeometricDataToJson()
        {
            JObject jvdata = new JObject();
            jvdata.Add("Nurbs", BSplineJson.NURBSSurfaceToJson(Nurbs));

            return jvdata;
        }

        public override void GeometricDataFromJson(JObject jvdata)
        {
            Nurbs = BSplineJson.NURBSSurfaceFromJson((JObject)jvdata["Nurbs"]);

            mPointList = Nurbs.CtrlPoints;

            NurbsPointList = new VertexList(Nurbs.UOutCnt * Nurbs.VOutCnt);
        }
    }
}