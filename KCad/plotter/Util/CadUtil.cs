using OpenTK;
using System;
using System.Collections.Generic;

namespace Plotter
{
    public struct CrossInfo
    {
        public bool IsCross;
        public CadVector CrossPoint;
        public double Distance;
    }

    public struct Centroid
    {
        public bool IsInvalid;
        public double Area;
        public CadVector Point;
        public List<CadFigure> SplitList;
    }

    public class CadUtil
    {
        // 三角形の面積 3D対応
        public static double TriangleArea(IReadOnlyList<CadVector> triangle)
        {
            CadVector v1 = triangle[0] - triangle[1];
            CadVector v2 = triangle[2] - triangle[1];

            CadVector cp = CadMath.CrossProduct(v1, v2);

            double area = cp.Norm() / 2.0;

            return area;
        }

        // 三角形の重心を求める
        public static CadVector TriangleCentroid(IReadOnlyList<CadVector> triangle)
        {
            CadVector gp = default(CadVector);

            gp.x = (triangle[0].x + triangle[1].x + triangle[2].x) / 3.0;
            gp.y = (triangle[0].y + triangle[1].y + triangle[2].y) / 3.0;
            gp.z = (triangle[0].z + triangle[1].z + triangle[2].z) / 3.0;

            return gp;
        }

        // 三角形群の重心を求める
        public static Centroid TriangleListCentroid(List<CadFigure> triangles)
        {
            Centroid c0 = default(Centroid);
            Centroid c1 = default(Centroid);
            Centroid ct = default(Centroid);

            int i = 1;

            c0.Area= TriangleArea(triangles[0].PointList);
            c0.Point = TriangleCentroid(triangles[0].PointList);

            for (; i < triangles.Count; i++)
            {
                c1.Area = TriangleArea(triangles[i].PointList);
                c1.Point = TriangleCentroid(triangles[i].PointList);

                ct = MergeCentroid(c0, c1);

                c0 = ct;
            }

            c0.SplitList = triangles;

            return c0;
        }


        // 二つの重心情報から重心を求める
        public static Centroid MergeCentroid(Centroid c0, Centroid c1, bool mergeSplitList = false)
        {
            CadVector gpt = default(CadVector);

            double ratio = c1.Area / (c0.Area + c1.Area);

            gpt.x = (c1.Point.x - c0.Point.x) * ratio + c0.Point.x;
            gpt.y = (c1.Point.y - c0.Point.y) * ratio + c0.Point.y;
            gpt.z = (c1.Point.z - c0.Point.z) * ratio + c0.Point.z;

            Centroid ret = default(Centroid); ;

            ret.Area = c0.Area + c1.Area;
            ret.Point = gpt;

            if (mergeSplitList)
            {
                ret.SplitList = new List<CadFigure>();

                if (c0.SplitList != null)
                {
                    ret.SplitList.AddRange(c0.SplitList);
                }

                if (c1.SplitList != null)
                {
                    ret.SplitList.AddRange(c1.SplitList);
                }
            }

            return ret;
        }

        public static double AroundLength(CadFigure fig)
        {
            if (fig == null)
            {
                return 0;
            }

            int cnt = fig.PointCount;

            if (cnt < 2)
            {
                return 0;
            }

            List<CadVector> list = fig.PointList;

            CadVector p0;
            CadVector p1;

            CadVector pd;

            double d = 0;

            for (int i = 0; i < cnt - 1; i++)
            {
                p0 = list[i];
                p1 = list[i + 1];

                pd = p1 - p0;

                d += pd.Norm();
            }

            return d;
        }


        public static void BezierPoints(
            CadVector p0, CadVector p1, CadVector p2, int s, List<CadVector> ret)
        {
            double t = 0;
            double d = 1.0 / (double)s;

            t = d;

            int n = 3;

            CadVector t0 = p0;
            CadVector t1 = p0;

            ret.Add(t0);

            while (t <= 1.0)
            {
                t1 = default(CadVector);
                t1 += p0 * CadMath.BernsteinBasisF(n - 1, 0, t);
                t1 += p1 * CadMath.BernsteinBasisF(n - 1, 1, t);
                t1 += p2 * CadMath.BernsteinBasisF(n - 1, 2, t);

                ret.Add(t1);

                t0 = t1;

                t += d;
            }
        }

        public static void BezierPoints(
            CadVector p0, CadVector p1, CadVector p2, CadVector p3, int s, List<CadVector> ret)
        {
            double t = 0;
            double d = 1.0 / (double)s;

            t = d;

            int n = 4;

            CadVector t0 = p0;
            CadVector t1 = p0;

            ret.Add(t0);

            while (t <= 1.0)
            {
                t1 = default(CadVector);
                t1 += p0 * CadMath.BernsteinBasisF(n - 1, 0, t);
                t1 += p1 * CadMath.BernsteinBasisF(n - 1, 1, t);
                t1 += p2 * CadMath.BernsteinBasisF(n - 1, 2, t);
                t1 += p3 * CadMath.BernsteinBasisF(n - 1, 3, t);

                ret.Add(t1);

                t0 = t1;

                t += d;
            }
        }

        // 点が三角形内にあるか 2D版
        public static bool IsPointInTriangle2D(CadVector p, IReadOnlyList<CadVector> triangle)
        {
            if (triangle.Count < 3)
            {
                return false;
            }

            double c1 = CadMath.CrossProduct2D(p, triangle[0], triangle[1]);
            double c2 = CadMath.CrossProduct2D(p, triangle[1], triangle[2]);
            double c3 = CadMath.CrossProduct2D(p, triangle[2], triangle[0]);


            // When all corossProduct result's sign are same, Point is in triangle
            if ((c1 > 0 && c2 > 0 && c3 > 0) || (c1 < 0 && c2 < 0 && c3 < 0))
            {
                return true;
            }

            return false;
        }

        // 点が三角形内にあるか
        public static bool IsPointInTriangle(CadVector p, IReadOnlyList<CadVector> triangle)
        {
            if (triangle.Count < 3)
            {
                return false;
            }

            CadVector c1 = CadMath.CrossProduct(p, triangle[0], triangle[1]);
            CadVector c2 = CadMath.CrossProduct(p, triangle[1], triangle[2]);
            CadVector c3 = CadMath.CrossProduct(p, triangle[2], triangle[0]);

            double ip12 = CadMath.InnerProduct(c1, c2);
            double ip13 = CadMath.InnerProduct(c1, c3);


            // When all corossProduct result's sign are same, Point is in triangle
            if (ip12 > 0 && ip13>0)
            {
                return true;
            }

            return false;
        }

        // 指定された座標から最も遠いPointのIndexを求める
        public static int FindMaxDistantPointIndex(CadVector p0, IReadOnlyList<CadVector> points)
        {
            int ret = -1;
            int i;

            CadVector t;

            double maxd = 0;

            for (i = 0; i < points.Count; i++)
            {
                CadVector fp = points[i];

                t = fp - p0;
                double d = t.Norm();

                if (d > maxd)
                {
                    maxd = d;
                    ret = i;
                }
            }

            return ret;
        }

        // 法線の代表値を求める
        public static Vector3d RepresentativeNormal(IReadOnlyList<CadVector> points)
        {
            if (points.Count < 3)
            {
                return Vector3d.Zero;
            }

            int idx = FindMaxDistantPointIndex(points[0], points);

            int idxA = idx - 1;
            int idxB = idx + 1;

            if (idxA < 0)
            {
                idxA = points.Count - 1;
            }

            if (idxB >= points.Count)
            {
                idxB = idxB - points.Count;
            }

            CadVector normal = CadMath.Normal(points[idx], points[idxA], points[idxB]);

            return normal.vector;
        }

        // 図形は凸である
        public static bool IsConvex(List<CadVector> points)
        {
            int p = 0;
            int cnt = points.Count;

            if (cnt<3)
            {
                return false;
            }

            int i = 0;
            CadVector n = default(CadVector);
            CadVector cn = default(CadVector);
            double scala = 0;

            for (;i < cnt - 2;)
            {
                n = CadMath.Normal(points[i], points[i + 1], points[i + 2]);

                i++;

                if (!n.IsZero())
                {
                    break;
                }
            }

            if (n.IsZero())
            {
                return false;
            }

            for (;i<cnt-2;)
            {
                cn = CadMath.Normal(points[i], points[i + 1], points[i + 2]);

                i++;


                scala = CadMath.InnerProduct(cn, n);

                if (Math.Abs(scala) < CadMath.VRange)
                {
                    continue;
                }

                if (scala < CadMath.R1Min)
                {
                    return false;
                }
            }


            cn = CadMath.Normal(points[i], points[i + 1], points[0]);

            scala = CadMath.InnerProduct(cn, n);

            if (Math.Abs(scala) < 0.000001)
            {
                return true;
            }

            if (scala < 0.999999)
            {
                return false;
            }

            return true;
        }


        // 線分apと点pの距離
        // 垂線がab内に無い場合は、点a,bで近い方への距離を返す
        // 2D
        public static double distancePtoSeg2D(CadVector a, CadVector b, CadVector p)
        {
            double t;

            CadVector ab = b - a;
            CadVector ap = p - a;

            t = CadMath.InnrProduct2D(ab, ap);

            if (t < 0)
            {
                return vectNorm2D(ap);
            }

            CadVector ba = a - b;
            CadVector bp = p - b;

            t = CadMath.InnrProduct2D(ba, bp);

            if (t < 0)
            {
                return vectNorm2D(bp);
            }

            double d = Math.Abs(CadMath.CrossProduct2D(ab, ap));
            double abl = vectNorm2D(ab);

            return d / abl;
        }

        // 線分apと点pの距離
        // 垂線がab内に無い場合は、点a,bで近い方への距離を返す
        // 3D対応
        public static double distancePtoSeg(CadVector a, CadVector b, CadVector p)
        {
            double t;

            CadVector ab = b - a;
            CadVector ap = p - a;

            t = CadMath.InnerProduct(ab, ap);

            if (t < 0)
            {
                return ap.Norm();
            }

            CadVector ba = a - b;
            CadVector bp = p - b;

            t = CadMath.InnerProduct(ba, bp);

            if (t < 0)
            {
                return bp.Norm();
            }

            CadVector cp = CadMath.CrossProduct(ab, ap);

            // 外積結果の長さが a->p a->b を辺とする平行四辺形の面積になる
            double s = cp.Norm();

            // 面積を底辺で割って高さを求める
            return s / ab.Norm();
        }

        // 点pから線分abに向かう垂線との交点を求める
        public static CrossInfo PerpendicularCrossSeg(CadVector a, CadVector b, CadVector p)
        {
            CrossInfo ret = default(CrossInfo);

            CadVector ab = b - a;
            CadVector ap = p - a;

            CadVector ba = a - b;
            CadVector bp = p - b;

            // A-B 単位ベクトル
            //CadPoint unit_ab = CadMath.unitVector(ab);
            CadVector unit_ab = ab.UnitVector();

            // B-A 単位ベクトル　(A-B単位ベクトルを反転) B側の中外判定に使用
            CadVector unit_ba = unit_ab * -1.0;

            // Aから交点までの距離 
            // A->交点->B or A->B->交点なら +
            // 交点<-A->B なら -
            double dist_ax = CadMath.InnerProduct(unit_ab, ap);

            // Bから交点までの距離 B側の中外判定に使用
            double dist_bx = CadMath.InnerProduct(unit_ba, bp);

            //Console.WriteLine("getNormCross dist_ax={0} dist_bx={1}" , dist_ax.ToString(), dist_bx.ToString());

            if (dist_ax > 0 && dist_bx > 0)
            {
                ret.IsCross = true;
            }

            ret.CrossPoint.x = a.x + (unit_ab.x * dist_ax);
            ret.CrossPoint.y = a.y + (unit_ab.y * dist_ax);
            ret.CrossPoint.z = a.z + (unit_ab.z * dist_ax);

            return ret;
        }

        // 点pから線分abに向かう垂線との交点を求める2D
        public static CrossInfo PerpendicularCrossSeg2D(CadVector a, CadVector b, CadVector p)
        {
            CrossInfo ret = default(CrossInfo);

            double t1;

            CadVector ab = b - a;
            CadVector ap = p - a;

            t1 = CadMath.InnrProduct2D(ab, ap);

            if (t1 < 0)
            {
                return ret;
            }

            double t2;

            CadVector ba = a - b;
            CadVector bp = p - b;

            t2 = CadMath.InnrProduct2D(ba, bp);

            if (t2 < 0)
            {
                return ret;
            }

            double abl = vectNorm2D(ab);
            double abl2 = abl * abl;

            ret.IsCross = true;
            ret.CrossPoint.x = ab.x * t1 / abl2 + a.x;
            ret.CrossPoint.y = ab.y * t1 / abl2 + a.y;

            return ret;
        }


        // 点pから直線abに向かう垂線との交点を求める
        public static CrossInfo PerpendicularCrossLine(CadVector a, CadVector b, CadVector p)
        {
            CrossInfo ret = default(CrossInfo);

            if (a.coordEquals(b))
            {
                return ret;
            }


            CadVector ab = b - a;
            CadVector ap = p - a;

            // A-B 単位ベクトル
            CadVector unit_ab = ab.UnitVector();

            // Aから交点までの距離 
            double dist_ax = CadMath.InnerProduct(unit_ab, ap);

            ret.CrossPoint.x = a.x + (unit_ab.x * dist_ax);
            ret.CrossPoint.y = a.y + (unit_ab.y * dist_ax);
            ret.CrossPoint.z = a.z + (unit_ab.z * dist_ax);

            ret.IsCross = true;

            return ret;
        }


        //
        // 点pを通り、a - b に平行で、a-bに垂直な線分を求める
        //
        //   +----------p------------+
        //   |                       |
        //   |                       |
        //   a                       b
        //
        public static CadSegment PerpendicularSeg(CadVector a, CadVector b, CadVector p)
        {
            CadSegment seg = default(CadSegment);

            seg.P0 = a;
            seg.P1 = b;

            CrossInfo ci = CadUtil.PerpendicularCrossLine(a, b, p);

            if (ci.IsCross)
            {
                CadVector nv = p - ci.CrossPoint;

                seg.P0 += nv;
                seg.P1 += nv;
            }

            return seg;
        }


        // 点pから直線abに向かう垂線との交点を求める2D
        public static CrossInfo PerpendicularCrossLine2D(CadVector a, CadVector b, CadVector p)
        {
            CrossInfo ret = default(CrossInfo);

            double t1;

            CadVector ab = b - a;
            CadVector ap = p - a;

            t1 = CadMath.InnrProduct2D(ab, ap);

            double norm = vectNorm2D(ab);
            double norm2 = norm * norm;

            ret.IsCross = true;
            ret.CrossPoint.x = ab.x * t1 / norm2 + a.x;
            ret.CrossPoint.y = ab.y * t1 / norm2 + a.y;

            return ret;
        }

        // a b の中点を求める
        public static CadVector CenterPoint(CadVector a, CadVector b)
        {
            CadVector c = b - a;
            c /= 2;
            c += a;

            return c;
        }

        // a b を通る直線上で a からの距離がlenの座標を求める
        public static CadVector LinePoint(CadVector a, CadVector b, double len)
        {
            CadVector v = b - a;

            v = v.UnitVector();

            v *= len;

            v += a;

            return v;
        }

        public static double vectNorm2D(CadVector v)
        {
            return Math.Sqrt(v.x * v.x + v.y * v.y);
        }

        public static double segNorm(CadVector a, CadVector b)
        {
            CadVector v = b - a;
            return v.Norm();
        }

        public static double segNorm2D(CadVector a, CadVector b)
        {
            double dx = b.x - a.x;
            double dy = b.y - a.y;

            return Math.Sqrt((dx * dx) + (dy * dy));
        }

        public static void movePoints(List<CadVector> list, CadVector delta)
        {
            for (int i = 0; i < list.Count; i++)
            {
                CadVector op = list[i];
                list[i] = op + delta;
            }
        }

        public static CadRect getContainsRect(IReadOnlyList<CadVector> list)
        {
            CadRect rect = default(CadRect);

            double minx = CadConst.MaxValue;
            double miny = CadConst.MaxValue;
            double minz = CadConst.MaxValue;

            double maxx = CadConst.MinValue;
            double maxy = CadConst.MinValue;
            double maxz = CadConst.MinValue;

            foreach (CadVector p in list)
            {
                minx = Math.Min(minx, p.x);
                miny = Math.Min(miny, p.y);
                minz = Math.Min(minz, p.z);

                maxx = Math.Max(maxx, p.x);
                maxy = Math.Max(maxy, p.y);
                maxz = Math.Max(maxz, p.z);
            }

            rect.p0 = default(CadVector);
            rect.p1 = default(CadVector);

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

            double minx = CadConst.MaxValue;
            double miny = CadConst.MaxValue;
            double minz = CadConst.MaxValue;

            double maxx = CadConst.MinValue;
            double maxy = CadConst.MinValue;
            double maxz = CadConst.MinValue;

            foreach (CadFigure fig in list)
            {
                fr = fig.GetContainsRect();

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

            rect.p0 = default(CadVector);
            rect.p1 = default(CadVector);

            rect.p0.x = minx;
            rect.p0.y = miny;
            rect.p0.z = minz;

            rect.p1.x = maxx;
            rect.p1.y = maxy;
            rect.p1.z = maxz;

            return rect;
        }

        // aに最も近い平面上の点を求める
        // a: チェック対象
        // p: 平面上の任意の点
        // normal: 平面の法線
        public static CadVector CrossPlane(CadVector a, CadVector p, CadVector normal)
        {
            CadVector pa = a - p;
    
            // 法線とpaの内積をとる
            // 法線の順方向に点Aがあれば d>0 逆方向だと d<0
            double d = CadMath.InnerProduct(normal, pa);

            //内積値から平面上の最近点を求める
            CadVector cp = default(CadVector);
            cp.x = a.x - (normal.x * d);
            cp.y = a.y - (normal.y * d);
            cp.z = a.z - (normal.z * d);

            return cp;
        }

        //
        // 直線 a b と p と normalが示す平面との交点を求める
        //
        public static CadVector CrossPlane(CadVector a, CadVector b, CadVector p, CadVector normal)
        {
            CadVector cp = default(CadVector);

            CadVector e = b - a;

            double de = CadMath.InnerProduct(normal, e);

            if (0.0 == de)
            {
                // 平面と直線は平行
                cp.Valid = false;
                return cp;
            }

            cp.Valid = true;

            double d = CadMath.InnerProduct(normal, p);
            double t = (d - CadMath.InnerProduct(normal, a)) / de;

            cp = a + (e * t);

            return cp;
        }

        public static void Dump(DebugOut dout, Vector4d v, string prefix)
        {
            dout.print(prefix);
            dout.println("{");
            dout.Indent++;
            dout.println("x:" + v.X.ToString());
            dout.println("y:" + v.Y.ToString());
            dout.println("z:" + v.Z.ToString());
            dout.println("w:" + v.W.ToString());
            dout.Indent--;
            dout.println("}");
        }

        public static void Dump(DebugOut o, UMatrix4 m, string prefix)
        {
            o.print(prefix);
            o.println("{");
            o.Indent++;
            o.println(m.M11.ToString() + "," + m.M12.ToString() + "," + m.M13.ToString() + "," + m.M14.ToString());
            o.println(m.M21.ToString() + "," + m.M22.ToString() + "," + m.M23.ToString() + "," + m.M24.ToString());
            o.println(m.M31.ToString() + "," + m.M32.ToString() + "," + m.M33.ToString() + "," + m.M34.ToString());
            o.println(m.M41.ToString() + "," + m.M42.ToString() + "," + m.M43.ToString() + "," + m.M44.ToString());
            o.Indent--;
            o.println("}");
        }

        public static void Dump(DebugOut dout, CadVector v, string prefix)
        {
            dout.print(prefix);
            dout.println("{");
            dout.Indent++;
            dout.println("x:" + v.x.ToString());
            dout.println("y:" + v.y.ToString());
            dout.println("z:" + v.z.ToString());
            dout.Indent--;
            dout.println("}");
        }
    }
}
