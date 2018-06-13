﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadDataTypes;
using Plotter.Controller;

namespace Plotter
{
    public class SpPointSearcher
    {
        enum Types
        {
            NONE,
            POINT,
            CENTER_POINT,
            CROSS_POINT,
        }

        Types Type = Types.NONE;

        struct PointItem
        {
            public CadLayer Layer;
            public CadFigure Fig;
            public int PointIndex;
            public double Distance;
            public CadVector ScrPoint;
        }

        struct CenterItem
        {
            public CadLayer Layer;
            public CadFigure Fig;
            public int SegIndex;
            public double Distance;
            public CadVector ScrPoint;
        }

        struct SegmentItem
        {
            public CadLayer Layer;
            public CadFigure Fig;
            public int SegIndex;
            public CadSegment ScrSegment;
        }

        struct CrossItem
        {
            public SegmentItem Seg0;
            public SegmentItem Seg1;
            public CadVector ScrPoint;
            public double Distance;
        }


        private PlotterController Controller;

        private DrawContext DC;

        PointItem Point = default(PointItem);

        CenterItem Center = default(CenterItem);

        CrossItem Cross = default(CrossItem);

        List<SegmentItem> SegList = new List<SegmentItem>();

        double MinDist;

        public CadVector TargetPoint = CadVector.InvalidValue;

        public double Range = 64;

        public CadVector Search(PlotterController controller, CadVector p)
        {
            Controller = controller;
            DC = Controller.CurrentDC;

            TargetPoint = p;

            MinDist = CadConst.MaxValue;

            SegList.Clear();

            Controller.DB.WalkEditable(CheckFig);

            CheckCross();

            if (Type == Types.POINT)
            {
                return Point.ScrPoint;
            }
            else if (Type == Types.CENTER_POINT)
            {
                return Center.ScrPoint;
            }
            else if (Type == Types.CROSS_POINT)
            {
                return Cross.ScrPoint;
            }

            return CadVector.InvalidValue;
        }

        void CheckFig(CadLayer layer, CadFigure fig)
        {
            int n = fig.PointCount;
            for (int i = 0; i < n; i++)
            {
                CadVector p = fig.PointList[i];

                if (p.Selected)
                {
                    continue;
                }

                p = DC.CadPointToUnitPoint(p);

                CadVector d = p - TargetPoint;

                double dist = d.Norm2D();

                if (dist > Range)
                {
                    continue;
                }

                if (dist < MinDist)
                {
                    Point.Layer = layer;
                    Point.Fig = fig;
                    Point.PointIndex = i;
                    Point.Distance = dist;
                    Point.ScrPoint = p;

                    Type = Types.POINT;
                    MinDist = dist;
                }
            }

            // 範囲内の線分リスト作成
            // ついでに中点のチェックも行う
            n = fig.SegmentCount;

            for (int i = 0; i < n; i++)
            {
                CadSegment seg = fig.GetSegmentAt(i);

                if (seg.P0.Selected || seg.P1.Selected)
                {
                    continue;
                }

                CadVector p0 = DC.CadPointToUnitPoint(seg.P0);
                CadVector p1 = DC.CadPointToUnitPoint(seg.P1);

                CadVector pc = (p1 - p0) / 2 + p0;

                double dist = (pc - TargetPoint).Norm2D();

                if (dist < Range && dist < MinDist)
                {
                    Center.Layer = layer;
                    Center.Fig = fig;
                    Center.SegIndex = i;
                    Center.Distance = dist;
                    Center.ScrPoint = pc;

                    Type = Types.CENTER_POINT;
                    MinDist = dist;
                }

                double d = CadUtil.DistancePointToSeg(p0, p1, TargetPoint);

                if (d > Range)
                {
                    continue;
                }

                SegmentItem segItem = new SegmentItem();

                segItem.Layer = layer;
                segItem.Fig = fig;
                segItem.SegIndex = i;
                segItem.ScrSegment = new CadSegment(p0, p1);

                SegList.Add(segItem);
            }
        }

        private void CheckCross()
        {
            int i = 0;
            for (; i < SegList.Count; i++)
            {
                SegmentItem seg0 = SegList[i];

                int j = i + 1;
                for (; j < SegList.Count; j++)
                {
                    SegmentItem seg1 = SegList[j];

                    if (!CheckCrossSegSegScr(seg0, seg1))
                    {
                        continue;
                    }

                    CadVector cv = CrossLineScr(seg0, seg1);

                    if (cv.Invalid)
                    {
                        continue;
                    }

                    CadVector dv = cv - TargetPoint;

                    double dist = dv.Norm2D();

                    if (dist > Range)
                    {
                        continue;
                    }

                    if (dist < MinDist)
                    {
                        Cross.Distance = dist;
                        Cross.Seg0 = seg0;
                        Cross.Seg1 = seg1;
                        Cross.ScrPoint = cv;

                        Type = Types.CROSS_POINT;
                        MinDist = dist;
                    }
                }
            }
        }

        private bool CheckCrossSegSegScr(SegmentItem seg0, SegmentItem seg1)
        {
            return CadUtil.CheckCrossSegSeg2D(
                seg0.ScrSegment.P0,
                seg0.ScrSegment.P1,
                seg1.ScrSegment.P0,
                seg1.ScrSegment.P1);

        }

        private CadVector CrossLineScr(SegmentItem seg0, SegmentItem seg1)
        {
            return CadUtil.CrossLine2D(
                seg0.ScrSegment.P0,
                seg0.ScrSegment.P1,
                seg1.ScrSegment.P0,
                seg1.ScrSegment.P1);
        }
    }
}
