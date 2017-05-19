using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public struct RulerInfo
    {
        public bool IsValid;
        public CadPoint CrossPoint;
        public double Distance;

        public CadRuler Ruler;
    }


    public struct CadRuler
    {
        public bool IsValid;
        public CadPoint P0;
        public CadPoint P1;
        public CadPoint V;

        public RulerInfo Capture(DrawContext dc, CadPoint p, double range)
        {
            RulerInfo ret = default(RulerInfo);
            CrossInfo ci = default(CrossInfo);

            ci = CadUtil.getPerpCrossLine(P0, P1, p);

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

        public void Draw(DrawContext dc, int pen)
        {
            CadPoint pt = (V * 1000) + P0;

            dc.Drawing.DrawLine(pen, P0, pt);
        }

        public static CadRuler Create(CadPoint p0, CadPoint p1)
        {
            CadRuler ret = default(CadRuler);

            ret.P0 = p0;
            ret.P1 = p1;

            ret.V = (p1 - p0).UnitVector();

            return ret;
        }
    }

    public class CadRulerSet
    {
        private CadRuler[] Ruler = new CadRuler[2];
        private int RCount = 0;
        private int MatchIndex = -1;

        public void Set(
                        IReadOnlyList<CadPoint> list,
                        int pointIndex,
                        CadPoint cp)
        {
            if (list.Count < 2)
            {
                RCount = 0;
                return;
            }

            if (pointIndex == list.Count - 1)
            {
                Ruler[0] = CadRuler.Create(list[pointIndex - 1], list[pointIndex]);
                RCount = 1;
                return;
            }
            else if (pointIndex == 0)
            {
                Ruler[0] = CadRuler.Create(list[1], list[0]);
                RCount = 1;
                return;
            }

            Ruler[0] = CadRuler.Create(list[pointIndex - 1], list[pointIndex]);
            Ruler[1] = CadRuler.Create(list[pointIndex + 1], list[pointIndex]);

            RCount = 2;
        }

        public void Draw(DrawContext dc)
        {
            if (MatchIndex == -1)
            {
                return;
            }

            Ruler[MatchIndex].Draw(dc, DrawTools.PEN_GRID);
        }

        public RulerInfo Capture(DrawContext dc, CadPoint p, double rangePixel)
        {
            RulerInfo match = default(RulerInfo);
            RulerInfo ri = default(RulerInfo);

            double range = dc.UnitToMilli(12);

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
    }
}
