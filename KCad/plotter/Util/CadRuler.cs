using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadDataTypes;

namespace Plotter
{
    public struct RulerInfo
    {
        public bool IsValid;
        public CadVector CrossPoint;
        public double Distance;

        public CadRuler Ruler;
    }


    public struct CadRuler
    {
        public bool IsValid;
        public CadFigure Fig;
        public int Idx0;
        public int Idx1;

        public CadVector P0
        {
            get
            {
                return Fig.GetPointAt(Idx0);
            }
        }

        public CadVector P1
        {
            get
            {
                return Fig.GetPointAt(Idx1);
            }
        }

        public RulerInfo Capture(DrawContext dc, CadVector p, double range)
        {
            RulerInfo ret = default(RulerInfo);
            CrossInfo ci = default(CrossInfo);

            ci = CadUtil.PerpendicularCrossLine(P0, P1, p);

            double d = (ci.CrossPoint - p).Norm();

            if (d <= range)
            {
                ret.IsValid = true;
                ret.CrossPoint = ci.CrossPoint;
                ret.Distance = d;

                ret.Ruler = this;

                return ret;
            }

            return default(RulerInfo);
        }

        public static CadRuler Create(CadFigure fig, int idx0, int idx1)
        {
            CadRuler ret = default(CadRuler);
            ret.Fig = fig;
            ret.Idx0 = idx0;
            ret.Idx1 = idx1;

            return ret;
        }
    }

    public class CadRulerSet
    {
        private CadRuler[] Ruler = new CadRuler[10];
        private int RCount = 0;
        private int MatchIndex = -1;

        public void Set(MarkPoint mkp)
        {
            CadFigure fig = mkp.Figure;
            int pointIndex = mkp.PointIndex;

            int cnt = fig.PointList.Count;

            if (cnt < 2)
            {
                return;
            }

            if (!fig.IsLoop)
            {
                if (pointIndex == cnt - 1)
                {
                    Ruler[RCount] = CadRuler.Create(fig, pointIndex - 1, pointIndex);
                    RCount++;
                    return;
                }
                else if (pointIndex == 0)
                {
                    Ruler[RCount] = CadRuler.Create(fig, 1, 0);
                    RCount++;
                    return;
                }
            }

            int idx0;
            int idx1;

            idx0 = (pointIndex + cnt - 1) % cnt;
            idx1 = pointIndex;

            Debug.Assert(idx0 >= 0 && idx0 < cnt);

            Ruler[RCount] = CadRuler.Create(fig, idx0, idx1);
            RCount++;

            idx0 = (pointIndex + 1) % cnt;
            idx1 = pointIndex;

            Debug.Assert(idx0 >= 0 && idx0 < cnt);

            Ruler[RCount] = CadRuler.Create(fig, idx0, idx1);
            RCount++;
        }

        public void Set(MarkSegment mks, DrawContext dc)
        {
            Ruler[RCount] = CadRuler.Create(mks.Figure, mks.PtIndexA, mks.PtIndexB);
            RCount++;
        }

        public RulerInfo Capture(DrawContext dc, CadVector p, double rangePixel)
        {
            RulerInfo match = default(RulerInfo);
            RulerInfo ri = default(RulerInfo);

            double range = dc.UnitToMilli(rangePixel);

            double min = range;

            MatchIndex = -1;

            for (int i = 0; i < RCount; i++)
            {
                ri = Ruler[i].Capture(dc, p, range);

                if (ri.IsValid && ri.Distance < min)
                {
                    min = ri.Distance;
                    match = ri;
                    MatchIndex = i;
                }
            }

            return match;
        }

        public void Clear()
        {
            for (int i = 0; i < RCount; i++)
            {
                Ruler[i].IsValid = false;
            }

            RCount = 0;
        }
    }
}
