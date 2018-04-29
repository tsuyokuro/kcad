using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public enum TargetCoord : uint
    {
        X = 1,
        Y = 2,
        Z = 4,
    }

    public static class CadConst
    {
        public const double MaxValue = double.MaxValue;
        public const double MinValue = double.MinValue;
    }
}
