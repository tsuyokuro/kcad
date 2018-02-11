using System;
using System.Collections.Generic;

namespace Plotter
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

    public class CadFigureDimLine : CadFigure
    {
        private const double ARROW_LEN = 2;
        private const double ARROW_W = 1;

        public override CreateStates CreateState
        {
            get
            {
                return GetCreateState();
            }
        }

        private CreateStates GetCreateState()
        {
            if (PointList.Count < 2)
            {
                return CreateStates.NOT_ENOUGH;
            }
            else if (PointList.Count < 3)
            {
                return CreateStates.WAIT_LAST_POINT;
            }

            return CreateStates.FULL;
        }

        public CadFigureDimLine()
        {
            Type = Types.DIMENTION_LINE;
        }

        public override void AddPoint(CadVector p)
        {
            mPointList.Add(p);
        }

        public override Centroid GetCentroid()
        {
            Centroid ret = default(Centroid);

            ret.IsInvalid = true;

            return ret;
        }

        public override void AddPointInCreating(DrawContext dc, CadVector p)
        {
            PointList.Add(p);
        }

        public override void SetPointAt(int index, CadVector pt)
        {
            mPointList[index] = pt;
        }

        public override void RemoveSelected()
        {
            mPointList.RemoveAll(a => a.Selected);

            if (PointCount < 4)
            {
                mPointList.Clear();
            }
        }

        public override void Draw(DrawContext dc, int pen)
        {
            if (pen == DrawTools.PEN_DEFAULT_FIGURE)
            {
                pen = DrawTools.PEN_DIMENTION;
            }

            DrawDim(dc, pen);
        }

        public override void DrawSeg(DrawContext dc, int pen, int idxA, int idxB)
        {
        }

        public override void DrawSelected(DrawContext dc, int pen)
        {
            Draw(dc, pen);

            foreach (CadVector p in PointList)
            {
                if (p.Selected)
                {
                    dc.Drawing.DrawSelectedPoint(p);
                }
            }
        }

        public override void DrawTemp(DrawContext dc, CadVector tp, int pen)
        {
            int cnt = PointList.Count;

            if (cnt < 1) return;

            if (cnt == 1)
            {
                DrawDim(dc, PointList[0], tp, tp, pen);
                return;
            }

            DrawDim(dc, PointList[0], PointList[1], tp, pen);
        }

        public override void StartCreate(DrawContext dc)
        {
        }

        public override void EndCreate(DrawContext dc)
        {
            if (PointList.Count < 3)
            {
                return;
            }

            CadSegment seg = CadUtil.PerpendicularSeg(PointList[0], PointList[1], PointList[2]);

            PointList[2] = PointList[2].SetVector(seg.P1.vector);
            PointList.Add(seg.P0);
        }

        public override void MoveSelectedPoints(DrawContext dc, CadVector delta)
        {
            if (PointList[0].Selected && PointList[1].Selected &&
                PointList[2].Selected && PointList[3].Selected)
            {
                PointList[0] = StoreList[0] + delta;
                PointList[1] = StoreList[1] + delta;
                PointList[2] = StoreList[2] + delta;
                PointList[3] = StoreList[3] + delta;
                return;
            }

            if (PointList[2].Selected || PointList[3].Selected)
            {
                CadVector v0 = StoreList[3] - StoreList[0];

                if (v0.IsZero())
                {
                    // 移動方向が不定の場合
                    MoveSelectedPointWithHeight(dc, delta);
                    return;
                }

                CadVector v0u = v0.UnitVector();

                double d = CadMath.InnerProduct(v0u, delta);

                CadVector vd = v0u * d;

                CadVector nv3 = StoreList[3] + vd;
                CadVector nv2 = StoreList[2] + vd;

                if (nv3.CoordEqualsThreshold(StoreList[0], 0.001) ||
                    nv2.CoordEqualsThreshold(StoreList[1], 0.001))
                {
                    return;
                }

                PointList[3] = nv3;
                PointList[2] = nv2;

                return;
            }

            if (PointList[0].Selected || PointList[1].Selected)
            {
                CadVector v0 = StoreList[0];
                CadVector v1 = StoreList[1];
                CadVector v2 = StoreList[2];
                CadVector v3 = StoreList[3];

                CadVector lv = v3 - v0;
                double h = lv.Norm();

                CadVector planeNormal = CadMath.Normal(v0, v1, v2);

                CadVector cp0 = v0;
                CadVector cp1 = v1;

                if (PointList[0].Selected)
                {
                    cp0 = CadUtil.CrossPlane(v0 + delta, v0, planeNormal);
                }

                if (PointList[1].Selected)
                {
                    cp1 = CadUtil.CrossPlane(v1 + delta, v1, planeNormal);
                }

                if (cp0.CoordEqualsThreshold(cp1, 0.001))
                {
                    return;
                }

                if (PointList[0].Selected)
                {
                    PointList[0] = PointList[0].SetVector(cp0.vector);
                }

                if (PointList[1].Selected)
                {
                    PointList[1] = PointList[1].SetVector(cp1.vector);
                }

                CadVector normal = CadMath.Normal(cp0, cp0 + planeNormal, cp1);
                CadVector d = normal * h;

                PointList[3] = PointList[3].SetVector(PointList[0] + d);
                PointList[2] = PointList[2].SetVector(PointList[1] + d);
            }
        }

        // 高さが０の場合、移動方向が定まらないので
        // 投影座標系でz=0とした座標から,List[0] - List[1]への垂線を計算して
        // そこへ移動する
        private void MoveSelectedPointWithHeight(DrawContext dc, CadVector delta)
        {
            CadSegment seg = CadUtil.PerpendicularSeg(PointList[0], PointList[1],
                StoreList[2] + delta);

            PointList[2] = PointList[2].SetVector(seg.P1.vector);
            PointList[3] = PointList[3].SetVector(seg.P0.vector);
        }

        public override DiffData EndEdit()
        {
            DiffData diff = base.EndEdit();

            if (PointList.Count == 0)
            {
                return diff;
            }

            CadSegment seg = CadUtil.PerpendicularSeg(PointList[0], PointList[1], PointList[2]);

            PointList[2] = PointList[2].SetVector(seg.P1.vector);
            PointList[3] = PointList[3].SetVector(seg.P0.vector);

            return diff;
        }

        private void DrawDim(
                            DrawContext dc,
                            CadVector a,
                            CadVector b,
                            CadVector p,
                            int pen)
        {
            CadSegment seg = CadUtil.PerpendicularSeg(a, b, p);

            dc.Drawing.DrawLine(pen, a, seg.P0);
            dc.Drawing.DrawLine(pen, b, seg.P1);

            CadVector cp = CadUtil.CenterPoint(seg.P0, seg.P1);

            dc.Drawing.DrawArrow(pen, cp, seg.P0, ArrowTypes.CROSS, ArrowPos.END, ARROW_LEN, ARROW_W);
            dc.Drawing.DrawArrow(pen, cp, seg.P1, ArrowTypes.CROSS, ArrowPos.END, ARROW_LEN, ARROW_W);
        }

        private void DrawDim(DrawContext dc, int pen)
        {
            dc.Drawing.DrawLine(pen, PointList[0], PointList[3]);
            dc.Drawing.DrawLine(pen, PointList[1], PointList[2]);

            CadVector cp = CadUtil.CenterPoint(PointList[3], PointList[2]);

            dc.Drawing.DrawArrow(pen, cp, PointList[3], ArrowTypes.CROSS, ArrowPos.END, ARROW_LEN, ARROW_W);
            dc.Drawing.DrawArrow(pen, cp, PointList[2], ArrowTypes.CROSS, ArrowPos.END, ARROW_LEN, ARROW_W);


            CadVector d = PointList[2] - PointList[3];

            double len = d.Norm();

            String s = CadUtil.ValToString(len);


            CadVector p0 = dc.CadPointToUnitPoint(PointList[3]);
            CadVector p1 = dc.CadPointToUnitPoint(PointList[2]);

            d = p1 - p0;

            len = d.Norm();

            CadVector sv = dc.Drawing.MeasureText(FontID, s);
            double sl = sv.Norm();

            CadVector sp;

            if (len > CadMath.R0Max)
            {
                double a = ((len - sl) / 2) / len;
                sp = (d * a) + p0;
            }
            else
            {
                sp = p0;
                d = CadVector.UnitX;
            }

            dc.Drawing.DrawTextScrn(FontID, BrushID, sp, d.UnitVector(), s);
        }
    }

}