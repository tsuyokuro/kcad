using OpenTK;
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
    public class CadFigureNurbsLine : CadFigure
    {
        public NurbsLine Nurbs;

        private VectorList NurbsPointList;

        public CadFigureNurbsLine()
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


        #region Point Move
        public override void MoveSelectedPoints(DrawContext dc, CadVector delta)
        {
            base.MoveSelectedPoints(dc, delta);
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

        public void Setup(int deg, int divCnt, bool edge = true, bool close=false)
        {
            Nurbs = new NurbsLine(deg, mPointList.Count, divCnt, edge, close);
            Nurbs.CtrlPoints = mPointList;
            Nurbs.SetupDefaultCtrlOrder();

            NurbsPointList = new VectorList(Nurbs.OutCnt);
        }

        public override void Draw(DrawContext dc, int pen)
        {
            if (PointList.Count < 2)
            {
                return;
            }

            CadVector c;
            CadVector n;

            c = PointList[0];

            for (int i=1; i<PointList.Count; i++)
            {
                n = PointList[i];
                dc.Drawing.DrawLine(DrawTools.PEN_NURBS_CTRL_LINE, c, n);

                c = n;
            }

            NurbsPointList.Clear();

            Nurbs.Eval(NurbsPointList);

            if (NurbsPointList.Count<2)
            {
                return;
            }

            c = NurbsPointList[0];

            for (int i=1; i< NurbsPointList.Count; i++)
            {
                n = NurbsPointList[i];
                dc.Drawing.DrawLine(pen, c, n);

                c = n;
            }
        }

        public override void InvertDir()
        {
            mPointList.Reverse();
            Normal = -Normal;
        }

        public override void SetPointAt(int index, CadVector pt)
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
            MpNurbsLineGeometricData geo = new MpNurbsLineGeometricData();
            geo.Nurbs = MpNurbsLine.Create(Nurbs);
            return geo;
        }

        public override void GeometricDataFromMp(MpGeometricData geo)
        {
            if (!(geo is MpNurbsLineGeometricData))
            {
                return;
            }

            MpNurbsLineGeometricData g = (MpNurbsLineGeometricData)geo;

            Nurbs = g.Nurbs.Restore();

            mPointList = Nurbs.CtrlPoints;

            NurbsPointList = new VectorList(Nurbs.OutCnt);
        }


        public override JObject GeometricDataToJson()
        {
            JObject jvdata = new JObject();
            jvdata.Add("Nurbs", BSplineJson.NURBSLineToJson(Nurbs));

            return jvdata;
        }

        public override void GeometricDataFromJson(JObject jvdata)
        {
            Nurbs = BSplineJson.NURBSLineFromJson((JObject)jvdata["Nurbs"]);

            mPointList = Nurbs.CtrlPoints;

            NurbsPointList = new VectorList(Nurbs.OutCnt);
        }
    }
}