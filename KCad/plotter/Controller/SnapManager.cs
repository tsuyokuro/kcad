using CadDataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
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

        public List<HighlightPointListItem> HighlightPointList = new List<HighlightPointListItem>();

        private void Clean()
        {
            HighlightPointList.Clear();

            mPointSearcher.Clean();
            mSegSearcher.Clean();

            mDist = Double.MaxValue;
        }

        public void SetPointRange(DrawContext dc, double range)
        {
            mPointSearcher.SetRangePixel(dc, range);
        }

        /*
        public void SetSegRange(DrawContext dc, double range)
        {
            mSegSearcher.SetRangePixel(dc, range);
        }
        */

        public void SetLineSnapRange(double range)
        {
            LineSnapRange = range;
        }


        public void SetTarget(DrawContext dc, CadCursor cursor)
        {
            Clean();

            CrossCursor = cursor;

            SnapPointScrn = cursor.Pos;

            SnapPoint = dc.UnitPointToCadPoint(SnapPointScrn);

            mPointSearcher.SetTargetPoint(cursor);
            mSegSearcher.SetTargetPoint(cursor);
        }

        public void CheckPoint(DrawContext dc, CadVector p)
        {
            mPointSearcher.Check(dc, p);
        }

        public void CheckPoint(DrawContext dc, VectorList vl)
        {
            mPointSearcher.Check(dc, vl);
        }

        public void CheckPointAllLayer(DrawContext dc, CadObjectDB db)
        {
            mPointSearcher.SearchAllLayer(dc, db);
        }

        public void CheckSegAllLayer(DrawContext dc, CadObjectDB db)
        {
            mSegSearcher.SearchAllLayer(dc, db);
        }

        public void EvalPointSearcher(DrawContext dc)
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

                SnapPointScrn = CrossCursor.Pos + distanceX;

                SnapPoint = dc.UnitPointToCadPoint(SnapPointScrn);
            }

            if (my.IsValid)
            {
                HighlightPointList.Add(new HighlightPointListItem(my.Point));

                tp = dc.CadPointToUnitPoint(my.Point);

                CadVector distanceY = CrossCursor.DistanceY(tp);

                SnapPointScrn = CrossCursor.Pos + distanceY;

                SnapPoint = dc.UnitPointToCadPoint(SnapPointScrn);
            }

            if (mxy.IsValid)
            {
                HighlightPointList.Add(new HighlightPointListItem(mxy.Point, DrawTools.PEN_POINT_HIGHTLITE2));

                SnapPointScrn = dc.CadPointToUnitPoint(mxy.Point);
                SnapPointScrn.z = 0;

                SnapPoint = mxy.Point;
            }

            mDist = Math.Min(mDist, mPointSearcher.Distance());
        }

        public void EvalSegSearcher(DrawContext dc)
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
