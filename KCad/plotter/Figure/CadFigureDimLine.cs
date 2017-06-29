using System;
using System.Collections.Generic;

namespace Plotter
{
    public partial class CadFigure
    {

        [Serializable]
        public class CadFigureDimLine : CadFigureBehavior
        {
            // Do not have data member.

            public override States GetState(CadFigure fig)
            {
                if (fig.PointList.Count < 2)
                {
                    return States.NOT_ENOUGH;
                }
                else if (fig.PointList.Count < 3)
                {
                    return States.WAIT_LAST_POINT;
                }

                return States.FULL;
            }

            public CadFigureDimLine()
            {
            }

            public override void AddPoint(CadFigure fig, CadPoint p)
            {
                fig.mPointList.Add(p);
            }

            public override Centroid GetCentroid(CadFigure fig)
            {
                Centroid ret = default(Centroid);

                ret.IsInvalid = true;

                return ret;
            }

            public override void AddPointInCreating(CadFigure fig, DrawContext dc, CadPoint p)
            {
                fig.PointList.Add(p);
            }

            public override void SetPointAt(CadFigure fig, int index, CadPoint pt)
            {
            }

            public override void RemoveSelected(CadFigure fig)
            {
            }

            public override void Draw(CadFigure fig, DrawContext dc, int pen)
            {
                DrawDim(fig, dc, fig.PointList[0], fig.PointList[1], fig.PointList[2], pen);
            }

            public override void DrawSeg(CadFigure fig, DrawContext dc, int pen, int idxA, int idxB)
            {
            }

            public override void DrawSelected(CadFigure fig, DrawContext dc, int pen)
            {
                Draw(fig, dc, pen);

                foreach (CadPoint p in fig.PointList)
                {
                    if (p.Selected)
                    {
                        dc.Drawing.DrawSelectedPoint(p);
                    }
                }
            }

            public override void DrawTemp(CadFigure fig, DrawContext dc, CadPoint tp, int pen)
            {
                int cnt = fig.PointList.Count;

                if (cnt < 1) return;

                if (cnt == 1)
                {
                    DrawDim(fig, dc, fig.PointList[0], tp, tp, pen);
                    return;
                }

                DrawDim(fig, dc, fig.PointList[0], fig.PointList[1], tp, pen);
            }

            public override void StartCreate(CadFigure fig, DrawContext dc)
            {
            }

            public override CadFigure.Types EndCreate(CadFigure fig, DrawContext dc)
            {
                CadSegment seg = GetDiffSeg(fig.PointList[0], fig.PointList[1], fig.PointList[2]);

                fig.PointList[2] = fig.PointList[2].setVector(seg.P1.vector);
                fig.PointList.Add(seg.P0);

                return fig.Type;
            }

            public override void MoveSelectedPoint(CadFigure fig, DrawContext dc, CadPoint delta)
            {
                if (fig.PointList[2].Selected || fig.PointList[3].Selected)
                {
                    CadSegment seg = GetDiffSeg(fig.PointList[0], fig.PointList[1],
                        fig.StoreList[2] + delta);

                    fig.PointList[2] = fig.PointList[2].setVector(seg.P1.vector);
                    fig.PointList[3] = fig.PointList[3].setVector(seg.P0.vector);
                }
            }

            public override void EndEdit(CadFigure fig)
            {
                CadSegment seg = GetDiffSeg(fig.PointList[0], fig.PointList[1], fig.PointList[2]);

                fig.PointList[2] = fig.PointList[2].setVector(seg.P1.vector);
                fig.PointList[3] = fig.PointList[3].setVector(seg.P0.vector);
            }

            private CadSegment GetDiffSeg(CadPoint a, CadPoint b, CadPoint p)
            {
                CadSegment seg = default(CadSegment);

                seg.P0 = a;
                seg.P1 = b;

                CrossInfo ci = CadUtil.PerpendicularCrossLine(a, b, p);

                if (ci.IsCross)
                {
                    CadPoint nv = p - ci.CrossPoint;

                    seg.P0 += nv;
                    seg.P1 += nv;
                }

                return seg;
            }


            private void DrawDim(
                                CadFigure fig,
                                DrawContext dc,
                                CadPoint a,
                                CadPoint b,
                                CadPoint p,
                                int pen)
            {
                CadSegment seg = GetDiffSeg(a, b, p);

                dc.Drawing.DrawLine(pen, a, seg.P0);
                dc.Drawing.DrawLine(pen, b, seg.P1);

                CadPoint cp = CadUtil.CenterPoint(seg.P0, seg.P1);

                dc.Drawing.DrawArrow(pen, cp, seg.P0, ArrowTypes.CROSS, ArrowPos.END, 4, 2);
                dc.Drawing.DrawArrow(pen, cp, seg.P1, ArrowTypes.CROSS, ArrowPos.END, 4, 2);
            }
        }
    }
}