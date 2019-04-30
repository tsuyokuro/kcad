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

        public override void AddPointInCreating(DrawContext dc, CadVector p)
        {
            mPointList.Add(p);
        }

        public override void AddPoint(CadVector p)
        {
            mPointList.Add(p);
        }

        public override void SetPointAt(int index, CadVector pt)
        {
            mPointList[index] = pt;
        }

        public override void RemoveSelected()
        {
            mPointList.Clear();
        }

        public override void Draw(DrawContext dc, int pen)
        {
            drawCircle(dc, pen);
            //drawDisk(dc, pen);
        }

        public override void DrawSeg(DrawContext dc, int pen, int idxA, int idxB)
        {
            //drawCircle(dc, pen);
        }

        public override void DrawSelected(DrawContext dc, int pen)
        {
            drawSelected_Circle(dc, pen);
        }

        public override void DrawTemp(DrawContext dc, CadVector tp, int pen)
        {
            if (PointList.Count <= 0)
            {
                return;
            }

            CadVector cp = PointList[0];

            CadVector a = tp;
            CadVector b = getRP(dc, cp, tp, true);

            CadVector c = -(a - cp) + cp;
            CadVector d = -(b - cp) + cp;


            CircleExpander.ForEachSegs(cp, a, b, 32,
                (CadVector p0, CadVector p1) =>
                {
                    dc.Drawing.DrawLine(pen, p0, p1);
                });


            dc.Drawing.DrawLine(pen, cp, a);
            dc.Drawing.DrawLine(pen, cp, b);
            dc.Drawing.DrawLine(pen, cp, c);
            dc.Drawing.DrawLine(pen, cp, d);
        }

        private void drawCircle(DrawContext dc, int pen)
        {
            if (PointList.Count == 0)
            {
                return;
            }

            if (PointList.Count == 1)
            {
                dc.Drawing.DrawCross(pen, PointList[0], 2);
                if (PointList[0].Selected) dc.Drawing.DrawSelectedPoint(PointList[0]);
                return;
            }

            CadVector normal = CadMath.Normal(PointList[0], PointList[2], PointList[1]);

            CircleExpander.ForEachSegs(PointList[0], PointList[1], PointList[2], 32, (p0, p1) =>
            {
                dc.Drawing.DrawLine(pen, p0, p1);
            });

            double size = dc.DevSizeToWoldSize(4);
            dc.Drawing.DrawCross(pen, PointList[0], size);
        }

        private void drawDisk(DrawContext dc, int pen)
        {
            if (PointList.Count == 0)
            {
                return;
            }

            if (PointList.Count < 3)
            {
                dc.Drawing.DrawCross(pen, PointList[0], 2);
                if (PointList[0].Selected) dc.Drawing.DrawSelectedPoint(PointList[0]);
                return;
            }

            bool outline = SettingsHolder.Settings.DrawMeshEdge;

            VectorList vl = CircleExpander.GetExpandList(PointList[0], PointList[1], PointList[2], 48);

            CadVector normal = CadMath.Normal(PointList[0], PointList[2], PointList[1]);

            dc.Drawing.DrawFace(pen, vl, normal, outline);

            dc.Drawing.DrawLine(pen, mPointList[1], mPointList[3]);
            dc.Drawing.DrawLine(pen, mPointList[2], mPointList[4]);
        }

        private void drawSelected_Circle(DrawContext dc, int pen)
        {
            for (int i=0; i<PointList.Count; i++)
            {
                if (PointList[i].Selected)
                {
                    dc.Drawing.DrawSelectedPoint(PointList[i]);
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

            CadVector cp = mPointList[0];

            CadVector a = mPointList[1];

            CadVector b = getRP(dc, cp, a, true);

            AddPoint(b);

            CadVector c = -(a - cp) + cp;
            CadVector d = -(b - cp) + cp;

            AddPoint(c);

            AddPoint(d);

            return;
        }

        public override void MoveSelectedPointsFromStored(DrawContext dc, CadVector delta)
        {
            CadVector cp = StoreList[0];

            if (cp.Selected)
            {
                mPointList[0] = cp + delta;
                mPointList[1] = StoreList[1] + delta;
                mPointList[2] = StoreList[2] + delta;
                mPointList[3] = StoreList[3] + delta;
                mPointList[4] = StoreList[4] + delta;
                return;
            }

            Span<CadVector> vt = stackalloc CadVector[4];

            vt[0] = StoreList[1] - cp;
            vt[1] = StoreList[2] - cp;
            vt[2] = StoreList[3] - cp;
            vt[3] = StoreList[4] - cp;

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

            CadVector normal = CadMath.CrossProduct(vt[ai], vt[bi]);
            normal = normal.UnitVector();

            vt[ai] += delta;

            CadVector uva = vt[ai].UnitVector();
            CadVector uvb = vt[bi].UnitVector();

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

            vt[0].Selected = false;
            vt[1].Selected = false;
            vt[2].Selected = false;
            vt[3].Selected = false;

            vt[ai].Selected = true;


            mPointList[1] = vt[0] + cp;
            mPointList[2] = vt[1] + cp;
            mPointList[3] = vt[2] + cp;
            mPointList[4] = vt[3] + cp;
        }

        public override Centroid GetCentroid()
        {
            Centroid ret = default(Centroid);

            CadVector cp = StoreList[0];
            CadVector rp = StoreList[1];

            CadVector d = rp - cp;

            double r = d.Norm();

            ret.Point = cp;
            ret.Area = r * r * Math.PI;

            return ret;
        }

        private CadVector getRP(DrawContext dc, CadVector cp, CadVector p, bool isA)
        {
            if (p.Equals(cp))
            {
                return cp;
            }


            CadVector r = CadMath.CrossProduct(p - cp, (CadVector)(dc.ViewDir));

            r = r.UnitVector();

            r = r * (p - cp).Norm() + cp;

            return r;
        }

        public override CadSegment GetSegmentAt(int n)
        {
            return new CadSegment(CadVector.InvalidValue, CadVector.InvalidValue);
        }

        public override FigureSegment GetFigSegmentAt(int n)
        {
            return new FigureSegment(null, -1, -1, -1);
        }
    }
}