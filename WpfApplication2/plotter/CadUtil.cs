using System;
using System.Collections.Generic;

namespace Plotter
{
    public struct CrossInfo
    {
        public bool isCross;
        public CadPoint CrossPoint;
    }

    public struct Centroid
    {
        public double Weight;
        public CadPoint Point;
    }

    public class CadUtil
    {
        // 三角形の面積 3D対応
        public static double getTriangleArea(List<CadPoint> triangle)
        {
            CadPoint v1 = triangle[0] - triangle[1];
            CadPoint v2 = triangle[2] - triangle[1];

            CadPoint cp = CadMath.crossProduct3D(v1, v2);

            double area = cp.length();

            return area;
        }

        // 三角形の重心を求める
        public static CadPoint getTriangleCentroid(List<CadPoint> triangle)
        {
            CadPoint gp = default(CadPoint);

            gp.x = (triangle[0].x + triangle[1].x + triangle[2].x) / 3.0;
            gp.y = (triangle[0].y + triangle[1].y + triangle[2].y) / 3.0;
            gp.z = (triangle[0].z + triangle[1].z + triangle[2].z) / 3.0;

            return gp;
        }

        // 三角形群の重心を求める
        public static Centroid getTriangleListCentroid(List<CadFigure> triangles)
        {
            Centroid c0;
            Centroid c1;
            Centroid ct;

            int i = 1;

            c0.Weight= getTriangleArea(triangles[0].PointList);
            c0.Point = getTriangleCentroid(triangles[0].PointList);

            for (; i < triangles.Count; i++)
            {
                c1.Weight = getTriangleArea(triangles[i].PointList);
                c1.Point = getTriangleCentroid(triangles[i].PointList);

                ct = getCentroid(c0, c1);

                c0 = ct;
            }

            return c0;
        }


        // 二つの重心情報から重心を求める
        public static Centroid getCentroid(Centroid c0, Centroid c1)
        {
            CadPoint gpt = default(CadPoint);

            double ratio = c1.Weight / (c0.Weight + c1.Weight);

            gpt.x = (c1.Point.x - c0.Point.x) * ratio + c0.Point.x;
            gpt.y = (c1.Point.y - c0.Point.y) * ratio + c0.Point.y;
            gpt.z = (c1.Point.z - c0.Point.z) * ratio + c0.Point.z;

            Centroid ret;

            ret.Weight = c0.Weight + c1.Weight;
            ret.Point = gpt;

            return ret;
        }

        /*
        public static Centroid getTriangleListCentroid(List<CadFigure> triangles)
        {
            CadPoint gp0 = default(CadPoint);
            CadPoint gp1 = default(CadPoint);
            CadPoint gpt = default(CadPoint);

            double w0 = 0;
            double w1 = 0;

            int i = 1;

            w0 = getTriangleArea(triangles[0].PointList);
            gp0 = getTriangleCentroid(triangles[0].PointList);

            for (; i < triangles.Count; i++)
            {
                w1 = getTriangleArea(triangles[i].PointList);
                gp1 = getTriangleCentroid(triangles[i].PointList);

                double ratio = w1 / (w0 + w1);

                gpt.x = (gp1.x - gp0.x) * ratio + gp0.x;
                gpt.y = (gp1.y - gp0.y) * ratio + gp0.y;
                gpt.z = (gp1.z - gp0.z) * ratio + gp0.z;

                gp0 = gpt;
                w0 = w0 + w1;
            }

            Centroid ret = default(Centroid);

            ret.Weight = w0;
            ret.Point = gp0;

            return ret;
        }
        */




        public static bool isPointInTriangle(CadPoint p, List<CadPoint> triangle)
        {
            if (triangle.Count < 3)
            {
                return false;
            }

            double c1 = CadMath.crossProduct2D(p, triangle[0], triangle[1]);
            double c2 = CadMath.crossProduct2D(p, triangle[1], triangle[2]);
            double c3 = CadMath.crossProduct2D(p, triangle[2], triangle[0]);


            // When all corossProduct result's sign are same, Point is in triangle
            if ((c1 > 0 && c2 > 0 && c3 > 0) || (c1 < 0 && c2 < 0 && c3 < 0))
            {
                return true;
            }

            return false;
        }

        public static int findMaxDistantPointIndex(CadPoint p0, List<CadPoint> points)
        {
            int ret = -1;
            int i;

            CadPoint t;

            double maxd = 0;

            for (i = 0; i < points.Count; i++)
            {
                CadPoint fp = points[i];

                t = fp - p0;
                double d = t.length();

                if (d > maxd)
                {
                    maxd = d;
                    ret = i;
                }
            }

            return ret;
        }

        public static double distancePtoSeg2D(CadPoint a, CadPoint b, CadPoint p)
        {
            double t;

            CadPoint ab = b - a;
            CadPoint ap = p - a;

            t = CadMath.innrProduct2D(ab, ap);

            if (t < 0)
            {
                return vectAbs2D(ap);
            }

            CadPoint ba = a - b;
            CadPoint bp = p - b;

            t = CadMath.innrProduct2D(ba, bp);

            if (t < 0)
            {
                return vectAbs2D(bp);
            }

            double d = Math.Abs(CadMath.crossProduct2D(ab, ap));
            double abl = vectAbs2D(ab);

            return d / abl;
        }

        public static CrossInfo getNormCross2D(CadPoint a, CadPoint b, CadPoint p)
        {
            CrossInfo ret = default(CrossInfo);

            double t1;

            CadPoint ab = b - a;
            CadPoint ap = p - a;

            t1 = CadMath.innrProduct2D(ab, ap);

            if (t1 < 0)
            {
                return ret;
            }

            double t2;

            CadPoint ba = a - b;
            CadPoint bp = p - b;

            t2 = CadMath.innrProduct2D(ba, bp);

            if (t2 < 0)
            {
                return ret;
            }

            double abl = vectAbs2D(ab);
            double abl2 = abl * abl;

            ret.isCross = true;
            ret.CrossPoint.x = ab.x * t1 / abl2 + a.x;
            ret.CrossPoint.y = ab.y * t1 / abl2 + a.y;

            return ret;
        }

        public static double vectAbs2D(CadPoint v)
        {
            return Math.Sqrt(v.x * v.x + v.y * v.y);
        }

        public static double lineAbs2D(CadPoint a, CadPoint b)
        {
            double dx = b.x - a.x;
            double dy = b.y - a.y;

            return Math.Sqrt((dx * dx) + (dy * dy));
        }

        public static double vectAbs(CadPoint v)
        {
            return v.length();
        }

        public static double lineAbs(CadPoint a, CadPoint b)
        {
            CadPoint v = b - a;

            return v.length();
        }

        public static void movePoints(List<CadPoint> list, CadPoint delta)
        {
            for (int i = 0; i < list.Count; i++)
            {
                CadPoint op = list[i];
                list[i] = op + delta;
            }
        }

        public static CadRect getContainsRect(List<CadPoint> list)
        {
            CadRect rect = default(CadRect);

            double minx = Double.MaxValue;
            double miny = Double.MaxValue;
            double minz = Double.MaxValue;

            double maxx = Double.MinValue;
            double maxy = Double.MinValue;
            double maxz = Double.MinValue;

            foreach (CadPoint p in list)
            {
                minx = Math.Min(minx, p.x);
                miny = Math.Min(miny, p.y);
                minz = Math.Min(minz, p.z);

                maxx = Math.Max(maxx, p.x);
                maxy = Math.Max(maxy, p.y);
                maxz = Math.Max(maxz, p.z);
            }

            rect.p0 = default(CadPoint);
            rect.p1 = default(CadPoint);

            rect.p0.x = minx;
            rect.p0.y = miny;
            rect.p0.z = minz;

            rect.p1.x = maxx;
            rect.p1.y = maxy;
            rect.p1.z = maxz;

            return rect;
        }

        public static CadRect getContainsRect(List<CadFigure> list)
        {
            CadRect rect = default(CadRect);
            CadRect fr;

            double minx = Double.MaxValue;
            double miny = Double.MaxValue;
            double minz = Double.MaxValue;

            double maxx = Double.MinValue;
            double maxy = Double.MinValue;
            double maxz = Double.MinValue;

            foreach (CadFigure fig in list)
            {
                fr = fig.getContainsRect();

                minx = Math.Min(minx, fr.p0.x);
                miny = Math.Min(miny, fr.p0.y);
                minz = Math.Min(minz, fr.p0.z);

                maxx = Math.Max(maxx, fr.p0.x);
                maxy = Math.Max(maxy, fr.p0.y);
                maxz = Math.Max(maxz, fr.p0.z);

                minx = Math.Min(minx, fr.p1.x);
                miny = Math.Min(miny, fr.p1.y);
                minz = Math.Min(minz, fr.p1.z);

                maxx = Math.Max(maxx, fr.p1.x);
                maxy = Math.Max(maxy, fr.p1.y);
                maxz = Math.Max(maxz, fr.p1.z);
            }

            rect.p0 = default(CadPoint);
            rect.p1 = default(CadPoint);

            rect.p0.x = minx;
            rect.p0.y = miny;
            rect.p0.z = minz;

            rect.p1.x = maxx;
            rect.p1.y = maxy;
            rect.p1.z = maxz;

            return rect;
        }
    }
}
