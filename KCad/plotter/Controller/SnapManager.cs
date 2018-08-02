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

        public double PointSnapRange;

        public double LineSnapRange;

        public void CleanMatches()
        {
            mPointSearcher.CleanMatches();
            mSegSearcher.Clean();

            mDist = Double.MaxValue;
        }

        


    }
}
