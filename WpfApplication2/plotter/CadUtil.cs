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
        public static double getTriangleArea(IReadOnlyList<CadPoint> triangle)
        {
            CadPoint v1 = triangle[0] - triangle[1];
            CadPoint v2 = triangle[2] - triangle[1];

            CadPoint cp = CadMath.crossProduct3D(v1, v2);

            double area = cp.length();

            return area;
        }

        // 三角形の重心を求める
        public static CadPoint getTriangleCentroid(IReadOnlyList<CadPoint> triangle)
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


        public static void BezierPoints(
            CadPoint p0, CadPoint p1, CadPoint p2, int s, List<CadPoint> ret)
        {
            double t = 0;
            double d = 1.0 / (double)s;

            t = d;

            int n = 3;

            CadPoint t0 = p0;
            CadPoint t1 = p0;

            ret.Add(t0);

            while (t <= 1.0)
            {
                t1 = default(CadPoint);
                t1 += p0 * CadMath.BernsteinBasisF(n - 1, 0, t);
                t1 += p1 * CadMath.BernsteinBasisF(n - 1, 1, t);
                t1 += p2 * CadMath.BernsteinBasisF(n - 1, 2, t);

                ret.Add(t1);

                t0 = t1;

                t += d;
            }
        }

        public static void BezierPoints(
            CadPoint p0, CadPoint p1, CadPoint p2, CadPoint p3, int s, List<CadPoint> ret)
        {
            double t = 0;
            double d = 1.0 / (double)s;

            t = d;

            int n = 4;

            CadPoint t0 = p0;
            CadPoint t1 = p0;

            ret.Add(t0);

            while (t <= 1.0)
            {
                t1 = default(CadPoint);
                t1 += p0 * CadMath.BernsteinBasisF(n - 1, 0, t);
                t1 += p1 * CadMath.BernsteinBasisF(n - 1, 1, t);
                t1 += p2 * CadMath.BernsteinBasisF(n - 1, 2, t);
                t1 += p3 * CadMath.BernsteinBasisF(n - 1, 3, t);

                ret.Add(t1);

                t0 = t1;

                t += d;
            }
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




        public static bool isPointInTriangle2D(CadPoint p, IReadOnlyList<CadPoint> triangle)
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

        public static bool isPointInTriangle3D(CadPoint p, IReadOnlyList<CadPoint> triangle)
        {
            if (triangle.Count < 3)
            {
                return false;
            }

            CadPoint c1 = CadMath.crossProduct3D(p, triangle[0], triangle[1]);
            CadPoint c2 = CadMath.crossProduct3D(p, triangle[1], triangle[2]);
            CadPoint c3 = CadMath.crossProduct3D(p, triangle[2], triangle[0]);

            double ip12 = CadMath.innerProduct3D(c1, c2);
            double ip13 = CadMath.innerProduct3D(c1, c3);


            // When all corossProduct result's sign are same, Point is in triangle
            if (ip12 > 0 && ip13>0)
            {
                return true;
            }

            return false;
        }

        public static int findMaxDistantPointIndex(CadPoint p0, IReadOnlyList<CadPoint> points)
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

        // 線分apと点pの距離
        // 垂線がab内に無い場合は、点a,bで近い方への距離を返す
        public static double distancePtoSeg(CadPoint a, CadPoint b, CadPoint p)
        {
            double t;

            CadPoint ab = b - a;
            CadPoint ap = p - a;

            t = CadMath.innerProduct3D(ab, ap);

            if (t < 0)
            {
                return vectAbs2D(ap);
            }

            CadPoint ba = a - b;
            CadPoint bp = p - b;

            t = CadMath.innerProduct3D(ba, bp);

            if (t < 0)
            {
                return vectAbs(bp);
            }


            CadPoint norm = CadMath.crossProduct3D(ab, ap);

            double d = norm.length();

            double abL = vectAbs(ab);

            return d / abL;
        }

        // 点pから線分abに向かう垂線との交点を求める
        public static CrossInfo getNormCross(CadPoint a, CadPoint b, CadPoint p)
        {
            CrossInfo ret = default(CrossInfo);

            CadPoint ab = b - a;
            CadPoint ap = p - a;

            CadPoint ba = a - b;
            CadPoint bp = p - b;

            // A-B 単位ベクトル
            CadPoint unit_ab = CadMath.unitVector(ab);

            // B-A 単位ベクトル　(A-B単位ベクトルを反転) B側の中外判定に使用
            CadPoint unit_ba = unit_ab * -1.0;

            // Aから交点までの距離 
            // A->交点->B or A->B->交点なら +
            // 交点<-A->B なら -
            double dist_ax = CadMath.innerProduct3D(unit_ab, ap);

            // Bから交点までの距離 B側の中外判定に使用
            double dist_bx = CadMath.innerProduct3D(unit_ba, bp);

            //Console.WriteLine("getNormCross dist_ax={0} dist_bx={1}" , dist_ax.ToString(), dist_bx.ToString());

            if (dist_ax > 0 && dist_bx > 0)
            {
                ret.isCross = true;
            }

            ret.CrossPoint.x = a.x + (unit_ab.x * dist_ax);
            ret.CrossPoint.y = a.y + (unit_ab.y * dist_ax);
            ret.CrossPoint.z = a.z + (unit_ab.z * dist_ax);

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

        public static CadRect getContainsRect(IReadOnlyList<CadPoint> list)
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
