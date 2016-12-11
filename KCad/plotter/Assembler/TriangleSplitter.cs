using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public class TriangleSplitter
    {
        public static List<CadFigure> split(CadFigure fig)
        {
            CadPoint p0 = default(CadPoint);

            CadFigure tfig = new CadFigure();

            tfig.copyPoints(fig);

            var triangles = new List<CadFigure>();

            int i1 = -1;

            int state = 0;

            CadFigure triangle;

            i1 = CadUtil.findMaxDistantPointIndex(p0, tfig.PointList);

            if (i1 == -1)
            {
                return triangles;
            }

            triangle = getTriangleWithCenterPoint(tfig, i1);

            CadPoint tp0 = triangle.PointList[0];
            CadPoint tp1 = triangle.PointList[1];
            CadPoint tp2 = triangle.PointList[2];

            double dir = CadMath.crossProduct2D(tp1, tp0, tp2);
            double currentDir = 0;

            while (tfig.PointCount > 3)
            {
                if (state == 0)
                {
                    i1 = CadUtil.findMaxDistantPointIndex(p0, tfig.PointList);
                    if (i1 == -1)
                    {
                        return triangles;
                    }
                }

                triangle = getTriangleWithCenterPoint(tfig, i1);

                tp0 = triangle.PointList[0];
                tp1 = triangle.PointList[1];
                tp2 = triangle.PointList[2];

                currentDir = CadMath.crossProduct2D(tp1, tp0, tp2);

                bool hasIn = listContainsPointInTriangle(tfig.PointList, triangle);
                if (!hasIn && (Math.Sign(dir) == Math.Sign(currentDir)))
                {
                    triangles.Add(triangle);
                    tfig.removePointAt(i1);
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
                    if (i1 >= tfig.PointCount)
                    {
                        break;
                    }
                }
            }

            if (tfig.PointCount == 3)
            {
                triangle = new CadFigure(CadFigure.Types.POLY_LINES);

                triangle.copyPoints(tfig);
                triangle.Closed = true;

                triangles.Add(triangle);
            }

            return triangles;
        }

        private static CadFigure getTriangleWithCenterPoint(CadFigure fig, int cpIndex)
        {
            int i1 = cpIndex;
            int endi = fig.PointCount - 1;

            int i0 = i1 - 1;
            int i2 = i1 + 1;

            if (i0 < 0) { i0 = endi; }
            if (i2 > endi) { i2 = 0; }

            var triangle = new CadFigure(CadFigure.Types.POLY_LINES);

            CadPoint tp0 = fig.PointList[i0];
            CadPoint tp1 = fig.PointList[i1];
            CadPoint tp2 = fig.PointList[i2];

            triangle.addPoint(tp0);
            triangle.addPoint(tp1);
            triangle.addPoint(tp2);

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
