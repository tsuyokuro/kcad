using System;
using System.Collections.Generic;

namespace Plotter
{
    public class TriangleSplitter
    {
        public static List<CadFigure> split(CadFigure fig)
        {
            CadPoint p0 = default(CadPoint);

            var triangles = new List<CadFigure>();

            int i1 = -1;

            int state = 0;

            CadFigure triangle;

            IReadOnlyList<CadPoint> orgList = fig.GetPoints(64);

            List<CadPoint> pointList = new List<CadPoint>(orgList);

            i1 = CadUtil.findMaxDistantPointIndex(p0, pointList);

            if (i1 == -1)
            {
                return triangles;
            }

            triangle = getTriangleWithCenterPoint(pointList, i1);

            CadPoint tp0 = triangle.PointList[0];
            CadPoint tp1 = triangle.PointList[1];
            CadPoint tp2 = triangle.PointList[2];

            double dir = CadMath.crossProduct2D(tp1, tp0, tp2);
            double currentDir = 0;

            while (pointList.Count > 3)
            {
                if (state == 0)
                {
                    i1 = CadUtil.findMaxDistantPointIndex(p0, pointList);
                    if (i1 == -1)
                    {
                        return triangles;
                    }
                }

                triangle = getTriangleWithCenterPoint(pointList, i1);

                tp0 = triangle.PointList[0];
                tp1 = triangle.PointList[1];
                tp2 = triangle.PointList[2];

                currentDir = CadMath.crossProduct2D(tp1, tp0, tp2);

                bool hasIn = listContainsPointInTriangle(pointList, triangle);
                if (!hasIn && (Math.Sign(dir) == Math.Sign(currentDir)))
                {
                    triangles.Add(triangle);
                    pointList.RemoveAt(i1);
                    state = 0;
                    continue;
                }

                if (state == 0)
                {
                    state = 1;
                    i1 = 0;
                }
                else if (state == 1)
                {
                    i1++;
                    if (i1 >= pointList.Count)
                    {
                        break;
                    }
                }
            }

            if (pointList.Count == 3)
            {
                triangle = new CadFigure(CadFigure.Types.POLY_LINES);

                triangle.AddPoints(pointList,0,3);
                triangle.Closed = true;

                triangles.Add(triangle);
            }

            return triangles;
        }

        private static CadFigure getTriangleWithCenterPoint(IReadOnlyList<CadPoint> pointList, int cpIndex)
        {
            int i1 = cpIndex;
            int endi = pointList.Count - 1;

            int i0 = i1 - 1;
            int i2 = i1 + 1;

            if (i0 < 0) { i0 = endi; }
            if (i2 > endi) { i2 = 0; }

            var triangle = new CadFigure(CadFigure.Types.POLY_LINES);

            CadPoint tp0 = pointList[i0];
            CadPoint tp1 = pointList[i1];
            CadPoint tp2 = pointList[i2];

            triangle.AddPoint(tp0);
            triangle.AddPoint(tp1);
            triangle.AddPoint(tp2);

            triangle.Closed = true;

            return triangle;
        }

        private static bool listContainsPointInTriangle(IReadOnlyList<CadPoint> check, CadFigure triangle)
        {
            var tps = triangle.PointList;

            foreach (CadPoint cp in check)
            {
                if (
                    cp.coordEquals(tps[0]) ||
                    cp.coordEquals(tps[1]) ||
                    cp.coordEquals(tps[2])
                    )
                {
                    continue;
                }

                bool ret = CadUtil.isPointInTriangle3D(cp, triangle.PointList);
                if (ret)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
