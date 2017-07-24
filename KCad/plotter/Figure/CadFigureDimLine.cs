﻿using System;
using System.Collections.Generic;

namespace Plotter
{
    public partial class CadFigure
    {

        //
        // 寸法線クラス
        // 
        //   3<-------------------------->2
        //    |                          |
        //    |                          |
        //   0                            1 
        // 
        //

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
                fig.mPointList[index] = pt;
            }

            public override void RemoveSelected(CadFigure fig)
            {
                fig.mPointList.RemoveAll(a => a.Selected);

                if (fig.PointCount < 4)
                {
                    fig.mPointList.Clear();
                }
            }

            public override void Draw(CadFigure fig, DrawContext dc, int pen)
            {
                if (pen == DrawTools.PEN_DEFAULT_FIGURE)
                {
                    pen = DrawTools.PEN_DIMENTION;
                }

                DrawDim(fig, dc, pen);
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
                if (fig.PointList.Count < 3)
                {
                    return Types.NONE;
                }

                CadSegment seg = CadUtil.PerpendicularSeg(fig.PointList[0], fig.PointList[1], fig.PointList[2]);

                fig.PointList[2] = fig.PointList[2].setVector(seg.P1.vector);
                fig.PointList.Add(seg.P0);

                return fig.Type;
            }

            public override void MoveSelectedPoint(CadFigure fig, DrawContext dc, CadPoint delta)
            {
                if (fig.PointList[0].Selected && fig.PointList[1].Selected &&
                    fig.PointList[2].Selected && fig.PointList[3].Selected)
                {
                    fig.PointList[0] = fig.StoreList[0] + delta;
                    fig.PointList[1] = fig.StoreList[1] + delta;
                    fig.PointList[2] = fig.StoreList[2] + delta;
                    fig.PointList[3] = fig.StoreList[3] + delta;
                    return;
                }

                if (fig.PointList[2].Selected || fig.PointList[3].Selected)
                {
                    CadPoint v0 = fig.StoreList[3] - fig.StoreList[0];

                    if (v0.IsZero())
                    {
                        // 移動方向が不定の場合
                        MoveSelectedPointWithHeight(fig, dc, delta);
                        return;
                    }

                    CadPoint v0u = v0.UnitVector();

                    double d = CadMath.InnerProduct(v0u, delta);

                    CadPoint vd = v0u * d;

                    CadPoint nv3 = fig.StoreList[3] + vd;
                    CadPoint nv2 = fig.StoreList[2] + vd;

                    if (nv3.coordEqualsThreshold(fig.StoreList[0], 0.001) ||
                        nv2.coordEqualsThreshold(fig.StoreList[1], 0.001))
                    {
                        return;
                    }

                    fig.PointList[3] = nv3;
                    fig.PointList[2] = nv2;

                    return;
                }

                if (fig.PointList[0].Selected || fig.PointList[1].Selected)
                {
                    CadPoint v0 = fig.StoreList[0];
                    CadPoint v1 = fig.StoreList[1];
                    CadPoint v2 = fig.StoreList[2];
                    CadPoint v3 = fig.StoreList[3];

                    CadPoint lv = v3 - v0;
                    double h = lv.Norm();

                    CadPoint planeNormal = CadMath.Normal(v0, v1, v2);

                    CadPoint cp0 = v0;
                    CadPoint cp1 = v1;

                    if (fig.PointList[0].Selected)
                    {
                        cp0 = CadUtil.CrossPlane(v0 + delta, v0, planeNormal);
                    }

                    if (fig.PointList[1].Selected)
                    {
                        cp1 = CadUtil.CrossPlane(v1 + delta, v1, planeNormal);
                    }

                    if (cp0.coordEqualsThreshold(cp1, 0.001))
                    {
                        return;
                    }

                    if (fig.PointList[0].Selected)
                    {
                        fig.PointList[0]  = fig.PointList[0].setVector(cp0.vector);
                    }

                    if (fig.PointList[1].Selected)
                    {
                        fig.PointList[1] = fig.PointList[1].setVector(cp1.vector);
                    }

                    CadPoint normal = CadMath.Normal(cp0, cp0 + planeNormal, cp1);
                    CadPoint d = normal * h;

                    fig.PointList[3] = fig.PointList[3].setVector(fig.PointList[0] + d);
                    fig.PointList[2] = fig.PointList[2].setVector(fig.PointList[1] + d);
                }
            }

            // 高さが０の場合、移動方向が定まらないので
            // 投影座標系でz=0とした座標から,List[0] - List[1]への垂線を計算して
            // そこへ移動する
            private void MoveSelectedPointWithHeight(CadFigure fig, DrawContext dc, CadPoint delta)
            {
                CadSegment seg = CadUtil.PerpendicularSeg(fig.PointList[0], fig.PointList[1],
                    fig.StoreList[2] + delta);

                fig.PointList[2] = fig.PointList[2].setVector(seg.P1.vector);
                fig.PointList[3] = fig.PointList[3].setVector(seg.P0.vector);
            }

            public override void EndEdit(CadFigure fig)
            {
                if (fig.PointList.Count == 0)
                {
                    return;
                }

                CadSegment seg = CadUtil.PerpendicularSeg(fig.PointList[0], fig.PointList[1], fig.PointList[2]);

                fig.PointList[2] = fig.PointList[2].setVector(seg.P1.vector);
                fig.PointList[3] = fig.PointList[3].setVector(seg.P0.vector);
            }

            private void DrawDim(
                                CadFigure fig,
                                DrawContext dc,
                                CadPoint a,
                                CadPoint b,
                                CadPoint p,
                                int pen)
            {
                CadSegment seg = CadUtil.PerpendicularSeg(a, b, p);

                dc.Drawing.DrawLine(pen, a, seg.P0);
                dc.Drawing.DrawLine(pen, b, seg.P1);

                CadPoint cp = CadUtil.CenterPoint(seg.P0, seg.P1);

                dc.Drawing.DrawArrow(pen, cp, seg.P0, ArrowTypes.CROSS, ArrowPos.END, 4, 2);
                dc.Drawing.DrawArrow(pen, cp, seg.P1, ArrowTypes.CROSS, ArrowPos.END, 4, 2);
            }

            private void DrawDim(CadFigure fig, DrawContext dc, int pen)
            {
                dc.Drawing.DrawLine(pen, fig.PointList[0], fig.PointList[3]);
                dc.Drawing.DrawLine(pen, fig.PointList[1], fig.PointList[2]);

                CadPoint cp = CadUtil.CenterPoint(fig.PointList[3], fig.PointList[2]);

                dc.Drawing.DrawArrow(pen, cp, fig.PointList[3], ArrowTypes.CROSS, ArrowPos.END, 4, 2);
                dc.Drawing.DrawArrow(pen, cp, fig.PointList[2], ArrowTypes.CROSS, ArrowPos.END, 4, 2);
            }
        }
    }
}