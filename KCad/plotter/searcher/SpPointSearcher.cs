using System.Collections.Generic;
using CadDataTypes;
using Plotter.Controller;

namespace Plotter
{
    public class SpPointSearcher
    {
        public class Result
        {
            public double Dist = double.MaxValue;
            public CadVector ScrPoint;
            public CadVector WoldPoint;
            public CadFigure Fig = null;

            public Result(CadVector sp, CadVector wp, double dist)
            {
                ScrPoint = sp;
                WoldPoint = wp;
                Dist = dist;
            }
        }

        List<Result> ResultList = new List<Result>();

        struct SegmentItem
        {
            public CadLayer Layer;
            public CadFigure Fig;
            public int SegIndex;
            public CadSegment ScrSegment;
        }

        private PlotterController Controller;

        private DrawContext DC;

        List<SegmentItem> SegList = new List<SegmentItem>();

        public CadVector TargetPoint = CadVector.InvalidValue;

        public double Range = 128;

        public SpPointSearcher(PlotterController controller)
        {
            Controller = controller;
            DC = Controller.CurrentDC;
        }

        public List<Result> Search(CadVector p, double range)
        {
            TargetPoint = p;
            Range = range;

            ResultList.Clear();

            SegList.Clear();

            CheckZeroPoint();

            Controller.DB.WalkEditable(CheckFig);

            CheckCross();

            ResultList.Sort((a, b) =>
            {
                return (int)(a.Dist * 1000 - b.Dist * 1000);
            });

            DOut.pl($"ResultList.Count:{ResultList.Count}");

            return ResultList;
        }

        void CheckZeroPoint()
        {
            CadVector p = DC.WorldPointToDevPoint(CadVector.Zero);

            CadVector d = p - TargetPoint;

            double dist = d.Norm2D();

            if (dist > Range)
            {
                return;
            }

            Result res = new Result(p, CadVector.Zero, dist);
            ResultList.Add(res);
        }

        void CheckFig(CadLayer layer, CadFigure fig)
        {
            int n = fig.PointCount;
            for (int i = 0; i < n; i++)
            {
                CadVector cp = fig.PointList[i];

                //if (cp.Selected)
                //{
                //    continue;
                //}

                CadVector p = DC.WorldPointToDevPoint(cp);

                CadVector d = p - TargetPoint;

                double dist = d.Norm2D();

                if (dist > Range)
                {
                    continue;
                }

                Result res = new Result(p, cp, dist);
                res.Fig = fig;
                ResultList.Add(res);
            }

            // 範囲内の線分リスト作成
            // ついでに中点のチェックも行う
            n = fig.SegmentCount;

            for (int i = 0; i < n; i++)
            {
                CadSegment seg = fig.GetSegmentAt(i);

                //if (seg.P0.Selected || seg.P1.Selected)
                //{
                //    continue;
                //}

                //CadVector cpc = (seg.P1 - seg.P0) / 2 + seg.P0;

                CadVector pw = (seg.P1 - seg.P0) / 2 + seg.P0;
                CadVector ps = DC.WorldPointToDevPoint(pw);

                double dist = (ps - TargetPoint).Norm2D();

                if (dist <= Range)
                {
                    Result res = new Result(ps, pw, dist);
                    res.Fig = fig;
                    ResultList.Add(res);
                }

                CadVector p0 = DC.WorldPointToDevPoint(seg.P0);
                CadVector p1 = DC.WorldPointToDevPoint(seg.P1);

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

                    Result res = new Result(cv, CadVector.InvalidValue, dist);
                    ResultList.Add(res);
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
