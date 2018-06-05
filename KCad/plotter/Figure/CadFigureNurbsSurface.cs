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
        public NURBSSurface Nurbs;

        private VectorList NurbsPointList;

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

        public override void DrawTemp(DrawContext dc, CadVector tp, int pen)
        {
        }

        public override void AddPointInCreating(DrawContext dc, CadVector p)
        {
        }


        public override void MoveSelectedPoints(DrawContext dc, CadVector delta)
        {
            base.MoveSelectedPoints(dc, delta);

            NeedsEval = true;
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
            int deg,
            int ucnt, int vcnt,
            VectorList vl,
            int uDivCnt, int vDivCnt,
            bool uedge=true, bool vedge=true,
            bool uclose=false, bool vclose=false)
        {
            mPointList = vl;

            Nurbs = new NURBSSurface(deg, ucnt, vcnt, uDivCnt, vDivCnt, uedge, vedge, uclose, vclose);

            NurbsPointList = new VectorList(Nurbs.UOutCnt * Nurbs.VOutCnt);
        }

        public override void Draw(DrawContext dc, int pen)
        {
            if (PointList.Count < 2)
            {
                return;
            }

            DrawSurfaces(dc, pen);

            DrawControlPoints(dc, DrawTools.PEN_NURBS_CTRL_LINE);
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

        private void DrawSurfaces(DrawContext dc, int pen)
        {
            if (NeedsEval)
            {
                NurbsPointList.Clear();
                Nurbs.CtrlPoints = mPointList;
                Nurbs.Eval(NurbsPointList);
                NeedsEval = false;
            }

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

        public override MpGeometricData GeometricDataToMp()
        {
            MpNurbsSurfaceGeometricData geo = new MpNurbsSurfaceGeometricData();
            geo.PointList = MpUtil.VectortListToMp(PointList);
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

            mPointList = MpUtil.VectortListFromMp(g.PointList);

            Nurbs = g.Nurbs.Restore();

            Nurbs.CtrlPoints = mPointList;

            NurbsPointList = new VectorList(Nurbs.UOutCnt * Nurbs.VOutCnt);

            NeedsEval = true;
        }


        public override JObject GeometricDataToJson()
        {
            JArray pointArray = CadJson.ToJson.VectorListToJson(PointList);

            JObject jvdata = new JObject();
            jvdata.Add(CadJson.VECTOR.POINT_LIST, pointArray);
            jvdata.Add("Nurbs", BSplineJson.NURBSSurfaceToJson(Nurbs));

            return jvdata;
        }

        public override void GeometricDataFromJson(JObject jvdata, CadJson.VersionCode version)
        {
            JArray jarray = (JArray)jvdata[CadJson.VECTOR.POINT_LIST];

            VectorList vl = CadJson.FromJson.VectorListFromJson(jarray, version);
            mPointList = vl;

            Nurbs = BSplineJson.NURBSSurfaceFromJson((JObject)jvdata["Nurbs"]);

            Nurbs.CtrlPoints = mPointList;

            NurbsPointList = new VectorList(Nurbs.UOutCnt * Nurbs.VOutCnt);
        }
    }
}