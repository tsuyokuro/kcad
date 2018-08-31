using CadDataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public class HighlightPointListItem
    {
        public CadVector Point;
        public int Pen;

        public HighlightPointListItem(CadVector p, int pen = DrawTools.PEN_POINT_HIGHTLITE)
        {
            Point = p;
            Pen = pen;
        }
    }

    class SnapManager
    {
        private PointSearcher mPointSearcher = new PointSearcher();
        private SegSearcher mSegSearcher = new SegSearcher();

        private double mDist;

        private CadCursor CrossCursor;

        public CadVector SnapPointScrn;

        public CadVector SnapPoint;

        private CadVector RawPoint;

        private double LineSnapRange;

        private List<HighlightPointListItem> HighlightPointList = new List<HighlightPointListItem>();

        private void Clean()
        {
            mPointSearcher.Clean();
            mSegSearcher.Clean();

            mDist = Double.MaxValue;
        }

        public void SetPointRange(DrawContext dc, double range)
        {
            mPointSearcher.SetRangePixel(dc, range);
        }

        public void SetSegRange(DrawContext dc, double range)
        {
            mSegSearcher.SetRangePixel(dc, range);
        }

        public void SetLineSnapRange(double range)
        {
            LineSnapRange = range;
        }


        public void SetTarget(CadCursor cursor)
        {
            mDist = Double.MaxValue;

            CrossCursor = cursor;

            mPointSearcher.SetTargetPoint(cursor);
            mSegSearcher.SetTargetPoint(cursor);
        }

        public void Check(DrawContext dc, CadVector p)
        {
            mPointSearcher.Check(dc, p);
        }

        public void Check(DrawContext dc, VectorList vl)
        {
            mPointSearcher.Check(dc, vl);
        }

        public void CheckAllLayer(DrawContext dc, CadObjectDB db)
        {
            mPointSearcher.SearchAllLayer(dc, db);
            mSegSearcher.SearchAllLayer(dc, db);
        }

        private void EvalPointSearcher(DrawContext dc)
        {
            MarkPoint mxy = mPointSearcher.GetXYMatch();
            MarkPoint mx = mPointSearcher.GetXMatch();
            MarkPoint my = mPointSearcher.GetYMatch();

            CadVector tp = default(CadVector);

            if (mx.IsValid)
            {
                HighlightPointList.Add(new HighlightPointListItem(mx.Point));

                tp = dc.CadPointToUnitPoint(mx.Point);

                CadVector distanceX = CrossCursor.DistanceX(tp);

                CrossCursor.Pos += distanceX;

                SnapPointScrn = CrossCursor.Pos;

                SnapPoint = dc.UnitPointToCadPoint(SnapPointScrn);
            }

            if (my.IsValid)
            {
                HighlightPointList.Add(new HighlightPointListItem(my.Point));

                tp = dc.CadPointToUnitPoint(my.Point);

                CadVector distanceY = CrossCursor.DistanceY(tp);

                CrossCursor.Pos += distanceY;

                SnapPointScrn = CrossCursor.Pos;

                SnapPoint = dc.UnitPointToCadPoint(SnapPointScrn);
            }

            if (mxy.IsValid)
            {
                HighlightPointList.Add(new HighlightPointListItem(mxy.Point, DrawTools.PEN_POINT_HIGHTLITE2));
            }

            mDist = Math.Min(mDist, mPointSearcher.Distance());
        }

        private void EvalSegSearcher(DrawContext dc)
        {
            double dist = mDist;

            mSegSearcher.SetRangePixel(dc, Math.Min(LineSnapRange, dist - CadMath.Epsilon));

            MarkSeg markSeg = mSegSearcher.GetMatch();

            if (mSegSearcher.IsMatch)
            {
                if (markSeg.Distance < dist)
                {
                    CadVector center = markSeg.CenterPoint;

                    CadVector t = dc.CadPointToUnitPoint(center);

                    if ((t - CrossCursor.Pos).Norm() < LineSnapRange)
                    {
                        HighlightPointList.Add(new HighlightPointListItem(center));

                        SnapPoint = center;
                        SnapPointScrn = t;
                        SnapPointScrn.z = 0;
                    }
                    else
                    {
                        SnapPoint = markSeg.CrossPoint;
                        SnapPointScrn = markSeg.CrossPointScrn;
                        SnapPointScrn.z = 0;
                    }
                }

                mDist = Math.Min(mDist, markSeg.Distance);
            }
        }

        public void Eval(DrawContext dc)
        {
            EvalPointSearcher(dc);
            EvalSegSearcher(dc);
        }
    }
}
