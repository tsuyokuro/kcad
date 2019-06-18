#define LOG_DEBUG

using System;
using OpenTK;

namespace Plotter.Controller
{
    public partial class PlotterController
    {
        public struct SnapInfo
        {
            public CadCursor Cursor;
            public Vector3d SnapPoint;
            public double Distance;

            public SnapInfo(CadCursor cursor, Vector3d snapPoint, double dist = Double.MaxValue)
            {
                Cursor = cursor;
                SnapPoint = snapPoint;
                Distance = dist;
            }
        }
    }
}
