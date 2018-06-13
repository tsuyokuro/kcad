using CadDataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter.Controller
{
    public class GideLineManager
    {
        public bool Enabled = false;

        public VectorList GideVectors = new VectorList(2);

        public void Add(CadVector v)
        {
            GideVectors.Add(v);
        }

        public void Clear()
        {
            GideVectors.Clear();
            Enabled = false;
        }

        public CadVector GetOnGideLine(CadVector sp, CadVector p)
        {
            double min = Double.MaxValue;

            CrossInfo match = default(CrossInfo);

            match.IsCross = false;

            for (int i = 0; i < GideVectors.Count; i++)
            {
                CrossInfo ci = CadUtil.PerpendicularCrossLine(sp, sp + GideVectors[i], p);

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
