using System;
using System.Collections.Generic;
using System.Drawing;

namespace Plotter
{
    public partial class CadFigure
    {
        [Serializable]
        public class CadFigureCircle : CadFigureBehavior
        {
            // Do not have data member.

            public override States getState(CadFigure fig)
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

            public override void addPointInCreating(CadFigure fig, DrawContext dc, CadPoint p)
            {
                p.Type = CadPoint.Types.BREAK;
                fig.mPointList.Add(p);
            }

            public override void addPoint(CadFigure fig, CadPoint p)
            {
                p.Type = CadPoint.Types.BREAK;
                fig.mPointList.Add(p);
            }

            public override void setPointAt(CadFigure fig, int index, CadPoint pt)
            {
                pt.Type = CadPoint.Types.BREAK;
                fig.mPointList[index] = pt;
            }

            public override void removeSelected(CadFigure fig)
            {
                fig.mPointList.RemoveAll(a => a.Selected);

                if (fig.PointCount < 2)
                {
                    fig.mPointList.Clear();
                }
            }

            public override void draw(CadFigure fig, DrawContext dc, int pen)
            {
                drawCircle(fig, dc, pen);
            }

            public override void drawSeg(CadFigure fig, DrawContext dc, int pen, int idxA, int idxB)
            {
                drawCircle(fig, dc, pen);
            }

            public override void drawSelected(CadFigure fig, DrawContext dc, int pen)
            {
                drawSelected_Circle(fig, dc, pen);
            }

            public override void drawTemp(CadFigure fig, DrawContext dc, CadPoint tp, int pen)
            {
                if (fig.PointList.Count <= 0)
                {
                    return;
                }

                CadPoint cp = fig.PointList[0];
                CadPoint b = getRP(dc, cp, tp, true);


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

            public override void startCreate(CadFigure fig, DrawContext dc)
            {
                // NOP
            }

            public override Types endCreate(CadFigure fig, DrawContext dc)
            {
                CadPoint b = getRP(dc, fig.mPointList[0], fig.mPointList[1], true);

                fig.addPoint(b);

                return fig.Type;
            }

            public override void moveSelectedPoint(CadFigure fig, DrawContext dc, CadPoint delta)
            {
                CadPoint cp = fig.StoreList[0];
                CadPoint a = fig.StoreList[1];
                CadPoint b = fig.StoreList[2];

                if (cp.Selected)
                {
                    fig.mPointList[0] = cp + delta;
                    fig.mPointList[1] = a + delta;
                    fig.mPointList[2] = b + delta;
                    return;
                }

                CadPoint va = a - cp;
                CadPoint vb = b - cp;

                if (va.Norm() < 0.01)
                {
                    return;
                }

                CadPoint normal = CadMath.crossProduct3D(va, vb);
                normal = normal.UnitVector();

                if (a.Selected)
                {
                    va += delta;

                    CadPoint uva = va.UnitVector();
                    CadPoint uvb = vb.UnitVector();

                    if (!uva.coordEqualsR(uvb))
                    {
                        normal = CadMath.crossProduct3D(va, vb);
                        normal = normal.UnitVector();

                    }

                    CadQuaternion q = CadQuaternion.RotateQuaternion(-Math.PI / 2.0, normal);
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

                    CadPoint uva = va.UnitVector();
                    CadPoint uvb = vb.UnitVector();

                    if (!uva.coordEqualsR(uvb))
                    {
                        normal = CadMath.crossProduct3D(va, vb);
                        normal = normal.UnitVector();

                    }

                    CadQuaternion q = CadQuaternion.RotateQuaternion(Math.PI / 2.0, normal);
                    CadQuaternion r = q.Conjugate();

                    CadQuaternion qp = CadQuaternion.FromPoint(vb);
                    qp = r * qp;
                    qp = qp * q;

                    va = qp.ToPoint();

                    fig.mPointList[1] = va + cp;
                    fig.mPointList[2] = vb + cp;
                }
            }

            public override Centroid getCentroid(CadFigure fig)
            {
                Centroid ret = default(Centroid);

                CadPoint cp = fig.StoreList[0];
                CadPoint rp = fig.StoreList[1];

                CadPoint d = rp - cp;

                double r = d.Norm();

                ret.Point = cp;
                ret.Area = r * r * Math.PI;
                ret.SplitList = new List<CadFigure>();
                ret.SplitList.Add(fig);

                return ret;
            }

            private CadPoint getRP(DrawContext dc, CadPoint cp, CadPoint p, bool isA)
            {
                CadPoint scp = dc.CadPointToUnitPoint(cp);
                CadPoint sbasep = dc.CadPointToUnitPoint(p);

                CadPoint t = sbasep - scp;
                CadPoint r = t;

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
        }
    }
}