using System;
using System.Collections.Generic;

namespace Plotter
{
    public class TriangleSplitter
    {
        public static List<CadFigure> split(CadFigure fig)
        {
            CadVector p0 = default(CadVector);

            var triangles = new List<CadFigure>();

            int i1 = -1;

            int state = 0;

            CadFigure triangle;

            IReadOnlyList<CadVector> orgList = fig.GetPoints(64);

            List<CadVector> pointList = new List<CadVector>(orgList);

            i1 = CadUtil.FindMaxDistantPointIndex(p0, pointList);

            if (i1 == -1)
            {
                return triangles;
            }

            triangle = getTriangleWithCenterPoint(pointList, i1);

            CadVector tp0 = triangle.PointList[0];
            CadVector tp1 = triangle.PointList[1];
            CadVector tp2 = triangle.PointList[2];

            double dir = CadMath.CrossProduct2D(tp1, tp0, tp2);
            double currentDir = 0;

            while (pointList.Count > 3)
            {
                if (state == 0)
                {
                    i1 = CadUtil.FindMaxDistantPointIndex(p0, pointList);
                    if (i1 == -1)
                    {
                        return triangles;
                    }
                }

                triangle = getTriangleWithCenterPoint(pointList, i1);

                tp0 = triangle.PointList[0];
                tp1 = triangle.PointList[1];
                tp2 = triangle.PointList[2];

                currentDir = CadMath.CrossProduct2D(tp1, tp0, tp2);

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

        private static CadFigure getTriangleWithCenterPoint(IReadOnlyList<CadVector> pointList, int cpIndex)
        {
            int i1 = cpIndex;
            int endi = pointList.Count - 1;

            int i0 = i1 - 1;
            int i2 = i1 + 1;

            if (i0 < 0) { i0 = endi; }
            if (i2 > endi) { i2 = 0; }

            var triangle = new CadFigure(CadFigure.Types.POLY_LINES);

            CadVector tp0 = pointList[i0];
            CadVector tp1 = pointList[i1];
            CadVector tp2 = pointList[i2];

            triangle.AddPoint(tp0);
            triangle.AddPoint(tp1);
            triangle.AddPoint(tp2);

            triangle.Closed = true;

            return triangle;
        }

        private static bool listContainsPointInTriangle(IReadOnlyList<CadVector> check, CadFigure triangle)
        {
            var tps = triangle.PointList;

            foreach (CadVector cp in check)
            {
                if (
                    cp.coordEquals(tps[0]) ||
                    cp.coordEquals(tps[1]) ||
                    cp.coordEquals(tps[2])
                    )
                {
                    continue;
                }

                bool ret = CadUtil.IsPointInTriangle(cp, triangle.PointList);
                if (ret)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
