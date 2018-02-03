using System;
using System.Collections.Generic;
using System.Drawing;

namespace Plotter
{
    public partial class CadFigure
    {
        public class CadFigureCircle : CadFigureBehavior
        {
            public override States GetState(CadFigure fig)
            {
                if (fig.PointList.Count < 1)
                {
                    return States.NOT_ENOUGH;
                }
                else if (fig.PointList.Count < 2)
                {
                    return States.WAIT_LAST_POINT;
                }

                return States.FULL;
            }

            public CadFigureCircle()
            {
            }

            public override void AddPointInCreating(CadFigure fig, DrawContext dc, CadVector p)
            {
                p.Type = CadVector.Types.BREAK;
                fig.mPointList.Add(p);
            }

            public override void AddPoint(CadFigure fig, CadVector p)
            {
                p.Type = CadVector.Types.BREAK;
                fig.mPointList.Add(p);
            }

            public override void SetPointAt(CadFigure fig, int index, CadVector pt)
            {
                pt.Type = CadVector.Types.BREAK;
                fig.mPointList[index] = pt;
            }

            public override void RemoveSelected(CadFigure fig)
            {
                fig.mPointList.RemoveAll(a => a.Selected);

                if (fig.PointCount < 2)
                {
                    fig.mPointList.Clear();
                }
            }

            public override void Draw(CadFigure fig, DrawContext dc, int pen)
            {
                drawCircle(fig, dc, pen);
            }

            public override void DrawSeg(CadFigure fig, DrawContext dc, int pen, int idxA, int idxB)
            {
                drawCircle(fig, dc, pen);
            }

            public override void DrawSelected(CadFigure fig, DrawContext dc, int pen)
            {
                drawSelected_Circle(fig, dc, pen);
            }

            public override void DrawTemp(CadFigure fig, DrawContext dc, CadVector tp, int pen)
            {
                if (fig.PointList.Count <= 0)
                {
                    return;
                }

                CadVector cp = fig.PointList[0];
                CadVector b = getRP(dc, cp, tp, true);


                dc.Drawing.DrawCircle(pen, cp, tp, b);

                dc.Drawing.DrawLine(pen, cp, tp);
                dc.Drawing.DrawLine(pen, cp, b);
            }

            private void drawCircle(CadFigure fig, DrawContext dc, int pen)
            {
                if (fig.PointList.Count == 0)
                {
                    return;
                }

                if (fig.PointList.Count == 1)
                {
                    dc.Drawing.DrawCross(pen, fig.PointList[0], 2);
                    if (fig.PointList[0].Selected) dc.Drawing.DrawSelectedPoint(fig.PointList[0]);
                    return;
                }

                dc.Drawing.DrawCircle(pen, fig.PointList[0], fig.PointList[1], fig.PointList[2]);
                //dc.Drawing.DrawLine(pen, fig.PointList[0], fig.PointList[1]);
                //dc.Drawing.DrawLine(pen, fig.PointList[0], fig.PointList[2]);
            }

            private void drawSelected_Circle(CadFigure fig, DrawContext dc, int pen)
            {
                if (fig.PointList[0].Selected) dc.Drawing.DrawSelectedPoint(fig.PointList[0]);
                if (fig.PointList[1].Selected) dc.Drawing.DrawSelectedPoint(fig.PointList[1]);
                if (fig.PointList[2].Selected) dc.Drawing.DrawSelectedPoint(fig.PointList[2]);
            }

            public override void StartCreate(CadFigure fig, DrawContext dc)
            {
                // NOP
            }

            public override Types EndCreate(CadFigure fig, DrawContext dc)
            {
                if (fig.PointCount < 2)
                {
                    return Types.NONE;
                }

                CadVector b = getRP(dc, fig.mPointList[0], fig.mPointList[1], true);

                fig.AddPoint(b);

                return fig.Type;
            }

            public override void MoveSelectedPoint(CadFigure fig, DrawContext dc, CadVector delta)
            {
                CadVector cp = fig.StoreList[0];
                CadVector a = fig.StoreList[1];
                CadVector b = fig.StoreList[2];

                if (cp.Selected)
                {
                    fig.mPointList[0] = cp + delta;
                    fig.mPointList[1] = a + delta;
                    fig.mPointList[2] = b + delta;
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

                    fig.mPointList[1] = va + cp;
                    fig.mPointList[2] = vb + cp;
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

                    fig.mPointList[1] = va + cp;
                    fig.mPointList[2] = vb + cp;
                }
            }

            public override Centroid GetCentroid(CadFigure fig)
            {
                Centroid ret = default(Centroid);

                CadVector cp = fig.StoreList[0];
                CadVector rp = fig.StoreList[1];

                CadVector d = rp - cp;

                double r = d.Norm();

                ret.Point = cp;
                ret.Area = r * r * Math.PI;
                ret.SplitList = new List<CadFigure>();
                ret.SplitList.Add(fig);

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

            public override CadSegment GetSegmentAt(CadFigure fig, int n)
            {
                return new CadSegment(CadVector.InvalidValue, CadVector.InvalidValue);
            }

            public override FigureSegment GetFigSegmentAt(CadFigure fig, int n)
            {
                return new FigureSegment(null, -1, -1, -1);
            }

            public override int SegmentCount(CadFigure fig)
            {
                return 0;
            }
        }
    }
}