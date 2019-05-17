using System;
using System.Collections.Generic;
using System.Drawing;
using CadDataTypes;

namespace Plotter
{
    public class CadFigureCircle : CadFigure
    {
        public override int SegmentCount
        {
            get => 0;
        }

        public override void InvertDir()
        {
            Normal = -Normal;
        }

        public CadFigureCircle()
        {
            Type = Types.CIRCLE;
        }

        public override void AddPointInCreating(DrawContext dc, CadVertex p)
        {
            mPointList.Add(p);
        }

        public override void AddPoint(CadVertex p)
        {
            mPointList.Add(p);
        }

        public override void SetPointAt(int index, CadVertex pt)
        {
            mPointList[index] = pt;
        }

        public override void RemoveSelected()
        {
            mPointList.Clear();
        }

        public override void Draw(DrawContext dc, DrawPen pen)
        {
            drawCircle(dc, pen);
            //drawDisk(dc, pen);
        }

        public override void DrawSeg(DrawContext dc, DrawPen pen, int idxA, int idxB)
        {
            //drawCircle(dc, pen);
        }

        public override void DrawSelected(DrawContext dc, DrawPen pen)
        {
            drawSelected_Circle(dc, pen);
        }

        public override void DrawTemp(DrawContext dc, CadVertex tp, DrawPen pen)
        {
            if (PointList.Count <= 0)
            {
                return;
            }

            CadVertex cp = PointList[0];

            CadVertex a = tp;
            CadVertex b = getRP(dc, cp, tp, true);

            CadVertex c = -(a - cp) + cp;
            CadVertex d = -(b - cp) + cp;


            CircleExpander.ForEachSegs(cp, a, b, 32,
                (CadVertex p0, CadVertex p1) =>
                {
                    dc.Drawing.DrawLine(pen, p0, p1);
                });


            dc.Drawing.DrawLine(pen, cp, a);
            dc.Drawing.DrawLine(pen, cp, b);
            dc.Drawing.DrawLine(pen, cp, c);
            dc.Drawing.DrawLine(pen, cp, d);
        }

        private void drawCircle(DrawContext dc, DrawPen pen)
        {
            if (PointList.Count == 0)
            {
                return;
            }

            if (PointList.Count == 1)
            {
                dc.Drawing.DrawCross(pen, PointList[0], 2);
                if (PointList[0].Selected) dc.Drawing.DrawSelectedPoint(PointList[0], dc.GetPen(DrawTools.PEN_SELECT_POINT));
                return;
            }

            CadVertex normal = CadMath.Normal(PointList[0], PointList[2], PointList[1]);

            CircleExpander.ForEachSegs(PointList[0], PointList[1], PointList[2], 32, (p0, p1) =>
            {
                dc.Drawing.DrawLine(pen, p0, p1);
            });

            double size = dc.DevSizeToWoldSize(4);
            dc.Drawing.DrawCross(pen, PointList[0], size);
        }

        private void drawDisk(DrawContext dc, DrawPen pen)
        {
            if (PointList.Count == 0)
            {
                return;
            }

            if (PointList.Count < 3)
            {
                dc.Drawing.DrawCross(pen, PointList[0], 2);

                if (PointList[0].Selected)
                {
                    dc.Drawing.DrawSelectedPoint(
                        PointList[0], dc.GetPen(DrawTools.PEN_SELECT_POINT));
                }
                return;
            }

            bool outline = SettingsHolder.Settings.DrawMeshEdge;

            VertexList vl = CircleExpander.GetExpandList(PointList[0], PointList[1], PointList[2], 48);

            CadVertex normal = CadMath.Normal(PointList[0], PointList[2], PointList[1]);

            dc.Drawing.DrawFace(pen, vl, normal, outline);

            dc.Drawing.DrawLine(pen, mPointList[1], mPointList[3]);
            dc.Drawing.DrawLine(pen, mPointList[2], mPointList[4]);
        }

        private void drawSelected_Circle(DrawContext dc, DrawPen pen)
        {
            for (int i=0; i<PointList.Count; i++)
            {
                if (PointList[i].Selected)
                {
                    dc.Drawing.DrawSelectedPoint(
                        PointList[i], dc.GetPen(DrawTools.PEN_SELECT_POINT));
                }

            }
        }

        public override void StartCreate(DrawContext dc)
        {
            // NOP
        }

        public override void EndCreate(DrawContext dc)
        {
            if (PointCount < 2)
            {
                return;
            }

            CadVertex cp = mPointList[0];

            CadVertex a = mPointList[1];

            CadVertex b = getRP(dc, cp, a, true);

            AddPoint(b);

            CadVertex c = -(a - cp) + cp;
            CadVertex d = -(b - cp) + cp;

            AddPoint(c);

            AddPoint(d);

            return;
        }

        public override void MoveSelectedPointsFromStored(DrawContext dc, CadVertex delta)
        {
            CadVertex cp = StoreList[0];

            if (cp.Selected)
            {
                mPointList[0] = cp + delta;
                mPointList[1] = StoreList[1] + delta;
                mPointList[2] = StoreList[2] + delta;
                mPointList[3] = StoreList[3] + delta;
                mPointList[4] = StoreList[4] + delta;
                return;
            }

            CadVectorArray4 vt = default;

            vt[0] = StoreList[1] - cp;
            vt[1] = StoreList[2] - cp;
            vt[2] = StoreList[3] - cp;
            vt[3] = StoreList[4] - cp;
            vt.Length = 4;

            if (vt[0].Norm() < 0.01)
            {
                return;
            }

            int ai = -1;

            for (int i = 0; i < 4; i++)
            {
                if (StoreList[i+1].Selected)
                {
                    ai = i;
                    break;
                }
            }

            if (ai < 0)
            {
                return;
            }

            int bi = (ai + 1) % 4;
            int ci = (ai + 2) % 4;
            int di = (ai + 3) % 4;

            CadVertex normal = CadMath.CrossProduct(vt[ai], vt[bi]);
            normal = normal.UnitVector();

            vt[ai] += delta;

            CadVertex uva = vt[ai].UnitVector();
            CadVertex uvb = vt[bi].UnitVector();

            if (!uva.EqualsThreshold(uvb))
            {
                normal = CadMath.CrossProduct(vt[ai], vt[bi]);

                if (normal.IsZero())
                {
                    return;
                }

                normal = normal.UnitVector();

            }

            CadQuaternion q = CadQuaternion.RotateQuaternion(normal, Math.PI / 2.0);
            CadQuaternion r = q.Conjugate();

            CadQuaternion qp = CadQuaternion.FromPoint(vt[ai]);
            qp = r * qp;
            qp = qp * q;

            vt[bi] = qp.ToPoint();

            vt[ci] = -vt[ai];
            vt[di] = -vt[bi];

            CadVertex tmp;

            for (int i=0; i<vt.Length; i++)
            {
                tmp = vt[i];
                tmp.Selected = false;
                vt[i] = tmp;
            }

            tmp = vt[ai];
            tmp.Selected = true;
            vt[ai] = tmp;

            mPointList[1] = vt[0] + cp;
            mPointList[2] = vt[1] + cp;
            mPointList[3] = vt[2] + cp;
            mPointList[4] = vt[3] + cp;
        }

        public override Centroid GetCentroid()
        {
            Centroid ret = default(Centroid);

            CadVertex cp = StoreList[0];
            CadVertex rp = StoreList[1];

            CadVertex d = rp - cp;

            double r = d.Norm();

            ret.Point = cp;
            ret.Area = r * r * Math.PI;

            return ret;
        }

        private CadVertex getRP(DrawContext dc, CadVertex cp, CadVertex p, bool isA)
        {
            if (p.Equals(cp))
            {
                return cp;
            }


            CadVertex r = CadMath.CrossProduct(p - cp, (CadVertex)(dc.ViewDir));

            r = r.UnitVector();

            r = r * (p - cp).Norm() + cp;

            return r;
        }

        public override CadSegment GetSegmentAt(int n)
        {
            return new CadSegment(CadVertex.InvalidValue, CadVertex.InvalidValue);
        }

        public override FigureSegment GetFigSegmentAt(int n)
        {
            return new FigureSegment(null, -1, -1, -1);
        }
    }
}