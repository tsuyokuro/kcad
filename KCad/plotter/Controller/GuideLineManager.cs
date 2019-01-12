using CadDataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter.Controller
{
    public class GuideLineManager
    {
        public bool Enabled = false;

        public VectorList GuideVectors = new VectorList(2);

        public void Add(CadVector v)
        {
            GuideVectors.Add(v);
        }

        public void Clear()
        {
            GuideVectors.Clear();
            Enabled = false;
        }

        public CadVector GetOnGuideLine(CadVector sp, CadVector p)
        {
            double min = Double.MaxValue;

            CrossInfo match = default(CrossInfo);

            match.IsCross = false;

            for (int i = 0; i < GuideVectors.Count; i++)
            {
                CrossInfo ci = CadUtil.PerpendicularCrossLine(sp, sp + GuideVectors[i], p);

                if (!ci.IsCross)
                {
                    continue;
                }

                double dist = (ci.CrossPoint - p).Norm();

                if (dist < min)
                {
                    match = ci;
                    min = dist;
                }
            }

            if (!match.IsCross)
            {
                return p;
            }

            return match.CrossPoint;
        }
    }
}
