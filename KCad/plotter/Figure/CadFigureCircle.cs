using System;
using System.Collections.Generic;
using System.Drawing;

namespace Plotter
{
    public class CadFigureCircle : CadFigure
    {
        public override CreateStates CreateState
        {
            get
            {
                CreateStates st = GetCreateState();
                return st;
            }
        }

        public override int SegmentCount
        {
            get
            {
                return 0;
            }
        }

        private CreateStates GetCreateState()
        {
            if (PointList.Count < 1)
            {
                return CreateStates.NOT_ENOUGH;
            }
            else if (PointList.Count < 2)
            {
                return CreateStates.WAIT_LAST_POINT;
            }

            return CreateStates.FULL;
        }

        public CadFigureCircle()
        {
            Type = Types.CIRCLE;
        }

        public override void AddPointInCreating(DrawContext dc, CadVector p)
        {
            p.Type = CadVector.Types.BREAK;
            mPointList.Add(p);
        }

        public override void AddPoint(CadVector p)
        {
            p.Type = CadVector.Types.BREAK;
            mPointList.Add(p);
        }

        public override void SetPointAt(int index, CadVector pt)
        {
            pt.Type = CadVector.Types.BREAK;
            mPointList[index] = pt;
        }

        public override void RemoveSelected()
        {
            mPointList.RemoveAll(a => a.Selected);

            if (PointCount < 2)
            {
                mPointList.Clear();
            }
        }

        public override void Draw(DrawContext dc, int pen)
        {
            drawCircle(dc, pen);
        }

        public override void DrawSeg(DrawContext dc, int pen, int idxA, int idxB)
        {
            drawCircle(dc, pen);
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
            CadVector b = getRP(dc, cp, tp, true);


            dc.Drawing.DrawCircle(pen, cp, tp, b);

            dc.Drawing.DrawLine(pen, cp, tp);
            dc.Drawing.DrawLine(pen, cp, b);
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

            dc.Drawing.DrawCircle(pen, PointList[0], PointList[1], PointList[2]);
            //dc.Drawing.DrawLine(pen, PointList[0], PointList[1]);
            //dc.Drawing.DrawLine(pen, PointList[0], PointList[2]);
        }

        private void drawSelected_Circle(DrawContext dc, int pen)
        {
            if (PointList[0].Selected) dc.Drawing.DrawSelectedPoint(PointList[0]);
            if (PointList[1].Selected) dc.Drawing.DrawSelectedPoint(PointList[1]);
            if (PointList[2].Selected) dc.Drawing.DrawSelectedPoint(PointList[2]);
        }

        public override void StartCreate(DrawContext dc)
        {
            // NOP
        }

        public override Types EndCreate(DrawContext dc)
        {
            if (PointCount < 2)
            {
                return Types.NONE;
            }

            CadVector b = getRP(dc, mPointList[0], mPointList[1], true);

            AddPoint(b);

            return Type;
        }

        public override void MoveSelectedPoints(DrawContext dc, CadVector delta)
        {
            CadVector cp = StoreList[0];
            CadVector a = StoreList[1];
            CadVector b = StoreList[2];

            if (cp.Selected)
            {
                mPointList[0] = cp + delta;
                mPointList[1] = a + delta;
                mPointList[2] = b + delta;
                return;
            }

            CadVector va = a - cp;
            CadVector vb = b - cp;

            if (va.Norm() < 0.01)
            {
                return;
            }

            CadVector normal = CadMath.CrossProduct(va, vb);
            normal = normal.UnitVector();

            if (a.Selected)
            {
                va += delta;

                CadVector uva = va.UnitVector();
                CadVector uvb = vb.UnitVector();

                if (!uva.CoordEqualsThreshold(uvb))
                {
                    normal = CadMath.CrossProduct(va, vb);

                    if (normal.IsZero())
                    {
                        return;
                    }

                    normal = normal.UnitVector();

                }

                CadQuaternion q = CadQuaternion.RotateQuaternion(normal, -Math.PI / 2.0);
                CadQuaternion r = q.Conjugate();

                CadQuaternion qp = CadQuaternion.FromPoint(va);
                qp = r * qp;
                qp = qp * q;

                vb = qp.ToPoint();

                mPointList[1] = va + cp;
                mPointList[2] = vb + cp;
            }
            else if (b.Selected)
            {
                vb += delta;

                CadVector uva = va.UnitVector();
                CadVector uvb = vb.UnitVector();

                if (!uva.CoordEqualsThreshold(uvb))
                {
                    normal = CadMath.CrossProduct(va, vb);
                    normal = normal.UnitVector();

                }

                CadQuaternion q = CadQuaternion.RotateQuaternion(normal, Math.PI / 2.0);
                CadQuaternion r = q.Conjugate();

                CadQuaternion qp = CadQuaternion.FromPoint(vb);
                qp = r * qp;
                qp = qp * q;

                va = qp.ToPoint();

                mPointList[1] = va + cp;
                mPointList[2] = vb + cp;
            }
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
            ret.SplitList = new List<CadFigure>();
            ret.SplitList.Add(this);

            return ret;
        }

        private CadVector getRP(DrawContext dc, CadVector cp, CadVector p, bool isA)
        {
            CadVector scp = dc.CadPointToUnitPoint(cp);
            CadVector sbasep = dc.CadPointToUnitPoint(p);

            CadVector t = sbasep - scp;
            CadVector r = t;

            if (isA)
            {
                r.x = -t.y;
                r.y = t.x;
            }
            else
            {
                r.x = t.y;
                r.y = -t.x;
            }

            r += scp;

            r = dc.UnitPointToCadPoint(r);

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