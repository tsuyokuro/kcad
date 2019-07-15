#define LOG_DEBUG

using System;
using OpenTK;

namespace Plotter.Controller
{
    public partial class PlotterController
    {
        public struct SnapInfo
        {
            public enum SanpTypes
            {
                NONE,
                POINT_MATCH,
            }

            public CadCursor Cursor;
            public Vector3d SnapPoint;
            public double Distance;

            public SanpTypes SnapType;

            public SnapInfo(CadCursor cursor, Vector3d snapPoint, double dist = Double.MaxValue)
            {
                Cursor = cursor;
                SnapPoint = snapPoint;
                Distance = dist;
                SnapType = SanpTypes.NONE;
            }
        }
    }
}
