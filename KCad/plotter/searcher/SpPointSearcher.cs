using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public class SpPointSearcher
    {
        enum Types
        {
            NONE,
            POINT,
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

        CrossItem Cross = default(CrossItem);

        List<SegmentItem> SegList = new List<SegmentItem>();

        double MinDist;

        public CadVector TargetPoint = CadVector.InvalidValue;

        public double Range = 32;

        public CadVector search(PlotterController controller, CadVector p)
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
            n = fig.SegmentCount;
            for (int i = 0; i < n; i++)
            {
                CadSegment seg = fig.GetSegmentAt(i);

                CadVector p0 = DC.CadPointToUnitPoint(seg.P0);
                CadVector p1 = DC.CadPointToUnitPoint(seg.P1);

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
