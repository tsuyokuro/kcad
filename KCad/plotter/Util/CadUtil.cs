using OpenTK;
using System;
using System.Collections.Generic;
using CadDataTypes;

namespace Plotter
{
    public delegate bool ForEachDelegate<T>(T obj);

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

        // 三角形から作成
        public static Centroid Create(CadVector p0, CadVector p1, CadVector p2)
        {
            Centroid ret = default(Centroid);
            ret.set(p0, p1, p2);
            return ret;
        }

        // 三角形で設定
        public void set(CadVector p0, CadVector p1, CadVector p2)
        {
            Area = CadUtil.TriangleArea(p0, p1, p2);
            Point = CadUtil.TriangleCentroid(p0, p1, p2);
        }
    }

    public class CadUtil
    {
        /**
         * 法線を求める
         * 
         *           list[2]
         *          / 
         *         /
         * list[0]/_________list[1]
         * 
         */
        public static CadVector Normal(CadFigure fig)
        {
            if (fig.PointCount < 3)
            {
                return CadVector.Zero;
            }

            return CadMath.Normal(fig.GetPointAt(0), fig.GetPointAt(1), fig.GetPointAt(2));
        }

        public static void RotateFigure(CadFigure fig, CadVector org, CadVector axis, double t)
        {
            CadQuaternion q = CadQuaternion.RotateQuaternion(axis, t);
            CadQuaternion r = q.Conjugate(); ;

            CadQuaternion qp;

            int n = fig.PointList.Count;

            for (int i = 0; i < n; i++)
            {
                CadVector p = fig.PointList[i];

                p -= org;

                qp = CadQuaternion.FromPoint(p);

                qp = r * qp;
                qp = qp * q;

                p = qp.ToPoint();

                p += org;

                fig.SetPointAt(i, p);
            }
        }

        public static void ScaleFigure(CadFigure fig, CadVector org, double scale)
        {
            int n = fig.PointList.Count;

            for (int i = 0; i < n; i++)
            {
                CadVector p = fig.PointList[i];
                p -= org;
                p *= scale;
                p += org;

                fig.SetPointAt(i, p);
            }
        }

        // 三角形の面積 3D対応
        public static double TriangleArea(CadVector p0, CadVector p1, CadVector p2)
        {
            CadVector v1 = p0 - p1;
            CadVector v2 = p2 - p1;

            CadVector cp = CadMath.CrossProduct(v1, v2);

            double area = cp.Norm() / 2.0;

            return area;
        }

        // 三角形の面積 3D対応
        public static double TriangleArea(CadFigure fig)
        {
            return TriangleArea(
                fig.GetPointAt(0),
                fig.GetPointAt(1),
                fig.GetPointAt(2)
                );
        }


        // 三角形の重心を求める
        public static CadVector TriangleCentroid(CadVector p0, CadVector p1, CadVector p2)
        {
            CadVector gp = default(CadVector);

            gp.x = (p0.x + p1.x + p2.x) / 3.0;
            gp.y = (p0.y + p1.y + p2.y) / 3.0;
            gp.z = (p0.z + p1.z + p2.z) / 3.0;

            return gp;
        }

        // 三角形の重心を求める
        public static CadVector TriangleCentroid(CadFigure fig)
        {
            return TriangleCentroid(
                fig.GetPointAt(0),
                fig.GetPointAt(1),
                fig.GetPointAt(2)
                );
        }

        // 三角形群の重心を求める
        public static Centroid TriangleListCentroid(List<CadFigure> triangles)
        {
            Centroid c0 = default(Centroid);
            Centroid c1 = default(Centroid);
            Centroid ct = default(Centroid);

            int i = 1;

            c0.Area= TriangleArea(triangles[0]);
            c0.Point = TriangleCentroid(triangles[0]);

            for (; i < triangles.Count; i++)
            {
                c1.Area = TriangleArea(triangles[i]);
                c1.Point = TriangleCentroid(triangles[i]);

                ct = MergeCentroid(c0, c1);

                c0 = ct;
            }

            return c0;
        }
        
        // 二つの重心情報から重心を求める
        public static Centroid MergeCentroid(Centroid c0, Centroid c1)
        {
            CadVector gpt = default(CadVector);

            double ratio = c1.Area / (c0.Area + c1.Area);

            gpt.x = (c1.Point.x - c0.Point.x) * ratio + c0.Point.x;
            gpt.y = (c1.Point.y - c0.Point.y) * ratio + c0.Point.y;
            gpt.z = (c1.Point.z - c0.Point.z) * ratio + c0.Point.z;

            Centroid ret = default(Centroid);

            ret.Area = c0.Area + c1.Area;
            ret.Point = gpt;

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

            CadVector p0;
            CadVector p1;

            CadVector pd;

            double d = 0;

            for (int i = 0; i < cnt - 1; i++)
            {
                p0 = fig.GetPointAt(i);
                p1 = fig.GetPointAt(i + 1);

                pd = p1 - p0;

                d += pd.Norm();
            }

            return d;
        }

        public static int InitBezier(CadFigure fig, int idx1, int idx2)
        {
            if (idx1 > idx2)
            {
                int t = idx1;
                idx1 = idx2;
                idx2 = t;
            }

            CadVector a = fig.GetPointAt(idx1);
            CadVector b = fig.GetPointAt(idx2);

            CadVector hp1 = b - a;
            hp1 = hp1 / 3;
            hp1 = hp1 + a;

            CadVector hp2 = a - b;
            hp2 = hp2 / 3;
            hp2 = hp2 + b;

            hp1.IsHandle = true;
            hp2.IsHandle = true;

            fig.InsertPointAt(idx1 + 1, hp1);
            fig.InsertPointAt(idx1 + 2, hp2);

            return 2;
        }

        public static void BezierPoints(
            CadVector p0, CadVector p1, CadVector p2, int s, VectorList ret)
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

        public static bool ForEachBezierPoints(
            CadVector p0, CadVector p1, CadVector p2, int s, Action<CadVector> action)
        {
            double t = 0;
            double d = 1.0 / (double)s;

            t = d;

            int n = 3;

            CadVector t0 = p0;
            CadVector t1 = p0;

            action(t0);

            while (t <= 1.0)
            {
                t1 = default(CadVector);
                t1 += p0 * CadMath.BernsteinBasisF(n - 1, 0, t);
                t1 += p1 * CadMath.BernsteinBasisF(n - 1, 1, t);
                t1 += p2 * CadMath.BernsteinBasisF(n - 1, 2, t);

                action(t1);

                t0 = t1;

                t += d;
            }

            return true;
        }

        public static void BezierPoints(
            CadVector p0, CadVector p1, CadVector p2, CadVector p3, int s, VectorList ret)
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

        public static void ForEachBezierPoints(
            CadVector p0, CadVector p1, CadVector p2, CadVector p3, int s, Action<CadVector> action)
        {
            double t = 0;
            double d = 1.0 / (double)s;

            t = d;

            int n = 4;

            CadVector t0 = p0;
            CadVector t1 = p0;

            action(t0);

            while (t <= 1.0)
            {
                t1 = default(CadVector);
                t1 += p0 * CadMath.BernsteinBasisF(n - 1, 0, t);
                t1 += p1 * CadMath.BernsteinBasisF(n - 1, 1, t);
                t1 += p2 * CadMath.BernsteinBasisF(n - 1, 2, t);
                t1 += p3 * CadMath.BernsteinBasisF(n - 1, 3, t);

                action(t1);

                t0 = t1;

                t += d;
            }
        }

        // 点が三角形内にあるか 2D版
        public static bool IsPointInTriangle2D(CadVector p, CadFigure fig)
        {
            if (fig.PointCount < 3)
            {
                return false;
            }

            CadVector p0 = fig.GetPointAt(0);
            CadVector p1 = fig.GetPointAt(1);
            CadVector p2 = fig.GetPointAt(2);


            double c1 = CadMath.CrossProduct2D(p, p0, p1);
            double c2 = CadMath.CrossProduct2D(p, p1, p2);
            double c3 = CadMath.CrossProduct2D(p, p2, p0);


            // When all corossProduct result's sign are same, Point is in triangle
            if ((c1 > 0 && c2 > 0 && c3 > 0) || (c1 < 0 && c2 < 0 && c3 < 0))
            {
                return true;
            }

            return false;
        }

        // 点が三角形内にあるか
        public static bool IsPointInTriangle(CadVector p, CadFigure fig)
        {
            if (fig.PointCount < 3)
            {
                return false;
            }

            CadVector p0 = fig.GetPointAt(0);
            CadVector p1 = fig.GetPointAt(1);
            CadVector p2 = fig.GetPointAt(2);

            CadVector c1 = CadMath.CrossProduct(p, p0, p1);
            CadVector c2 = CadMath.CrossProduct(p, p1, p2);
            CadVector c3 = CadMath.CrossProduct(p, p2, p0);

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
        public static int FindMaxDistantPointIndex(CadVector p0, VectorList points)
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
        public static CadVector RepresentativeNormal(VectorList points)
        {
            if (points.Count < 3)
            {
                return CadVector.Zero;
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

            return normal;
        }

        // 図形は凸である
        public static bool IsConvex(VectorList points)
        {
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

                if (Math.Abs(scala) < CadMath.Epsilon)
                {
                    continue;
                }

                if (scala < CadMath.RP1Min)
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
        public static double DistancePointToSeg2D(CadVector a, CadVector b, CadVector p)
        {
            double t;

            CadVector ab = b - a;
            CadVector ap = p - a;

            t = CadMath.InnrProduct2D(ab, ap);

            if (t < 0)
            {
                return ap.Norm2D();
            }

            CadVector ba = a - b;
            CadVector bp = p - b;

            t = CadMath.InnrProduct2D(ba, bp);

            if (t < 0)
            {
                return bp.Norm2D();
            }

            // 外積結果が a->p a->b を辺とする平行四辺形の面積になる
            double d = Math.Abs(CadMath.CrossProduct2D(ab, ap));

            double abl = ab.Norm2D();

            // 高さ = 面積 / 底辺の長さ
            return d / abl;
        }

        // 線分apと点pの距離
        // 垂線がab内に無い場合は、点a,bで近い方への距離を返す
        // 3D対応
        public static double DistancePointToSeg(CadVector a, CadVector b, CadVector p)
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

            // 高さ = 面積 / 底辺の長さ
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

            double abl = ab.Norm2D();
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

            if (a.Equals(b))
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

            double norm = ab.Norm2D();
            double norm2 = norm * norm;

            ret.IsCross = true;
            ret.CrossPoint.x = ab.x * t1 / norm2 + a.x;
            ret.CrossPoint.y = ab.y * t1 / norm2 + a.y;

            return ret;
        }

        /// <summary>
        /// a b の中点を求める
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static CadVector CenterPoint(CadVector a, CadVector b)
        {
            CadVector c = b - a;
            c /= 2;
            c += a;

            return c;
        }

        /// <summary>
        /// a b を通る直線上で a からの距離がlenの座標を求める
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static CadVector LinePoint(CadVector a, CadVector b, double len)
        {
            CadVector v = b - a;

            v = v.UnitVector();

            v *= len;

            v += a;

            return v;
        }

        //public static double VectorNorm2D(CadVector v)
        //{
        //    return Math.Sqrt(v.x * v.x + v.y * v.y);
        //}

        public static double SegNorm(CadVector a, CadVector b)
        {
            CadVector v = b - a;
            return v.Norm();
        }

        public static double SegNorm2D(CadVector a, CadVector b)
        {
            double dx = b.x - a.x;
            double dy = b.y - a.y;

            return Math.Sqrt((dx * dx) + (dy * dy));
        }

        public static void MovePoints(VectorList list, CadVector delta)
        {
            for (int i = 0; i < list.Count; i++)
            {
                CadVector op = list[i];
                list[i] = op + delta;
            }
        }

        public static CadRect GetContainsRect(VectorList list)
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

        public static MinMax3D GetFigureMinMax(CadFigure fig)
        {
            MinMax3D mm = MinMax3D.Create();

            int i = 0;
            for (;i<fig.PointCount; i++)
            {
                mm.Check(fig.PointList[i]);
            }

            return mm;
        }

        public static MinMax3D GetFigureMinMaxIncludeChild(CadFigure fig)
        {
            MinMax3D mm = MinMax3D.Create();

            fig.ForEachFig(item =>
            {
                mm.Check(GetFigureMinMax(item));
            });

            return mm;
        }

        public static MinMax3D GetFigureMinMaxIncludeChild(List<CadFigure> figList)
        {
            MinMax3D mm = MinMax3D.Create();

            foreach (CadFigure fig in figList)
            {
                MinMax3D tmm = GetFigureMinMaxIncludeChild(fig);
                mm.Check(tmm);
            }

            return mm;
        }

        public static CadRect GetContainsRectScrn(DrawContext dc, List<CadFigure> list)
        {
            CadRect rect = default(CadRect);
            CadRect fr;

            double minx = CadConst.MaxValue;
            double miny = CadConst.MaxValue;

            double maxx = CadConst.MinValue;
            double maxy = CadConst.MinValue;

            foreach (CadFigure fig in list)
            {
                fr = fig.GetContainsRectScrn(dc);

                fr.Normalize();

                minx = Math.Min(minx, fr.p0.x);
                miny = Math.Min(miny, fr.p0.y);
                maxx = Math.Max(maxx, fr.p1.x);
                maxy = Math.Max(maxy, fr.p1.y);
            }

            rect.p0 = default(CadVector);
            rect.p1 = default(CadVector);

            rect.p0.x = minx;
            rect.p0.y = miny;
            rect.p0.z = 0;

            rect.p1.x = maxx;
            rect.p1.y = maxy;
            rect.p1.z = 0;

            return rect;
        }

        public static CadRect GetContainsRectScrn(DrawContext dc, VectorList list)
        {
            CadRect rect = default(CadRect);

            double minx = CadConst.MaxValue;
            double miny = CadConst.MaxValue;

            double maxx = CadConst.MinValue;
            double maxy = CadConst.MinValue;

            list.ForEach(p =>
            {
                CadVector v = dc.WorldPointToDevPoint(p);

                minx = Math.Min(minx, v.x);
                miny = Math.Min(miny, v.y);

                maxx = Math.Max(maxx, v.x);
                maxy = Math.Max(maxy, v.y);
            });

            rect.p0 = default(CadVector);
            rect.p1 = default(CadVector);

            rect.p0.x = minx;
            rect.p0.y = miny;
            rect.p0.z = 0;

            rect.p1.x = maxx;
            rect.p1.y = maxy;
            rect.p1.z = 0;

            return rect;
        }

        /// <summary>
        /// 点aに最も近い平面上の点を求める
        /// </summary>
        /// <param name="a">点</param>
        /// <param name="p">平面上の点</param>
        /// <param name="normal">平面の法線</param>
        /// <returns>
        /// 点aに最も近い平面上の点
        /// </returns>
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

        /// <summary>
        /// 直線 a b と p と normalが示す平面との交点を求める
        /// </summary>
        /// <param name="a">直線上の点</param>
        /// <param name="b">直線上の点</param>
        /// <param name="p">平面上の点</param>
        /// <param name="normal">平面の法線</param>
        /// <returns>交点</returns>
        /// 
        public static CadVector CrossPlane(CadVector a, CadVector b, CadVector p, CadVector normal)
        {
            CadVector cp = default(CadVector);

            CadVector e = b - a;

            double de = CadMath.InnerProduct(normal, e);

            if (de > CadMath.R0Min && de < CadMath.R0Max)
            {
                //DebugOut.Std.println("CrossPlane is parallel");

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

        /// <summary>
        /// 直線 a b と p と normalが示す平面との交点を求める
        /// </summary>
        /// <param name="a">直線上の点</param>
        /// <param name="b">直線上の点</param>
        /// <param name="p">平面上の点</param>
        /// <param name="normal">平面の法線</param>
        /// <returns>交点</returns>
        /// 
        public static CadVector CrossSegPlane(CadVector a, CadVector b, CadVector p, CadVector normal)
        {
            CadVector cp = CrossPlane(a, b, p, normal);

            if (!cp.Valid)
            {
                return cp;
            }

            if (CadMath.InnerProduct((b - a), (cp - a)) < 0)
            {
                cp.Valid = false;
                return cp;
            }

            if (CadMath.InnerProduct((a - b), (cp - b)) < 0)
            {
                cp.Valid = false;
                return cp;
            }

            return cp;
        }

        /// <summary>
        /// 直線Aと直線Bの交点を求める
        /// </summary>
        /// <param name="a1">直線A上の点1</param>
        /// <param name="a2">直線A上の点2</param>
        /// <param name="b1">直線B上の点1</param>
        /// <param name="b2">直線B上の点2</param>
        /// <returns></returns>
        /// 
        public static CadVector CrossLine2D(CadVector a1, CadVector a2, CadVector b1, CadVector b2)
        {
            CadVector a = (a2 - a1);
            CadVector b = (b2 - b1);

            if (a.IsZero() || b.IsZero())
            {
                return CadVector.InvalidValue;
            }

            double cpBA = CadMath.CrossProduct2D(b, a);

            if (cpBA == 0)
            {
                return CadVector.InvalidValue;
            }

            return a1 + a * CadMath.CrossProduct2D(b, b1 - a1) / cpBA;
        }

        /// <summary>
        /// 線分と直線の交点
        /// </summary>
        /// <param name="segp1">線分</param>
        /// <param name="segp2">線分</param>
        /// <param name="lp1">直線上の点</param>
        /// <param name="lp2">直線上の点</param>
        /// <returns></returns>
        /// 
        public static CadVector CrossSegLine2D(CadVector segp1, CadVector segp2, CadVector lp1, CadVector lp2)
        {
            CadVector p = CrossLine2D(segp1, segp2, lp1, lp2);

            if (!p.Valid)
            {
                return p;
            }

            CadVector segv = segp2 - segp1;
            CadVector pv = p - segp1;

            double ip = CadMath.InnerProduct(segv, pv);

            if (ip < 0)
            {
                p.Valid = false;
                return p;
            }

            double sd = (segp2 - segp1).Norm();

            double pd = (p - segp1).Norm();

            if (pd > sd)
            {
                p.Valid = false;
                return p;
            }

            p.Valid = true;
            return p;
        }

        /// <summary>
        /// 線分同士の交点が存在するかチェックする
        /// </summary>
        /// <param name="p1">線分A</param>
        /// <param name="p2">線分A</param>
        /// <param name="p3">線分B</param>
        /// <param name="p4">線分B</param>
        /// <returns>
        /// 交点が存在す場合は、true
        /// 存在しない場合は、false
        /// また、同一線上にある場合は、false (交点が無限に存在する)
        /// </returns>
        /// 
        public static bool CheckCrossSegSeg2D(CadVector p1, CadVector p2, CadVector p3, CadVector p4)
        {
            if (p1.x >= p2.x)
            {
                if ((p1.x < p3.x && p1.x < p4.x) || (p2.x > p3.x && p2.x > p4.x))
                {
                    return false;
                }
            }
            else
            {
                if((p2.x < p3.x && p2.x < p4.x) || (p1.x > p3.x && p1.x > p4.x))
                {
                    return false;
                }
            }

            if (p1.y >= p2.y)
            {
                if ((p1.y < p3.y && p1.y < p4.y) || (p2.y > p3.y && p2.y > p4.y))
                {
                    return false;
                }
            }
            else
            {
                if ((p2.y < p3.y && p2.y < p4.y) || (p1.y > p3.y && p1.y > p4.y))
                {
                    return false;
                }
            }

            if (((p1.x - p2.x) * (p3.y - p1.y) + (p1.y - p2.y) * (p1.x - p3.x)) *
                ((p1.x - p2.x) * (p4.y - p1.y) + (p1.y - p2.y) * (p1.x - p4.x)) > 0)
            {
                return false;
            }

            if (((p3.x - p4.x) * (p1.y - p3.y) + (p3.y - p4.y) * (p3.x - p1.x)) *
                ((p3.x - p4.x) * (p2.y - p3.y) + (p3.y - p4.y) * (p3.x - p2.x)) > 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 点が矩形内あるかチェック
        /// </summary>
        /// <param name="minp">矩形の最小頂点</param>
        /// <param name="maxp">矩形の最大頂点</param>
        /// <param name="p">検査対象点</param>
        /// <returns>
        /// true:  点は矩形内
        /// false: 点は矩形外
        /// </returns>
        /// 
        public static bool IsInRect2D(CadVector minp, CadVector maxp, CadVector p)
        {
            if (p.x < minp.x) return false;
            if (p.x > maxp.x) return false;

            if (p.y < minp.y) return false;
            if (p.y > maxp.y) return false;

            return true;
        }

        /// <summary>
        /// 線分と水平線との交点を求める
        /// </summary>
        /// <param name="p0">線分の端点</param>
        /// <param name="p1">線分の端点</param>
        /// <param name="lineY">水平線のY座標</param>
        /// <returns>交点 交点が存在しない場合は、Invalid==true</returns>
        public static CadVector CrossSegHLine2D(CadVector p0, CadVector p1, double lineY )
        {
            CadVector cp = default(CadVector);
            CadVector sp;
            CadVector ep;

            if (p0.y < p1.y)
            {
                sp = p0;
                ep = p1;
            }
            else
            {
                sp = p1;
                ep = p0;
            }

            if (lineY < sp.y)
            {
                cp.Invalid = true;
                return cp;
            }

            if (lineY > ep.y)
            {
                cp.Invalid = true;
                return cp;
            }

            double dx = ep.x - sp.x;
            double dy = ep.y - sp.y;

            double a = dx / dy;

            cp.x = a * (lineY - sp.y) + sp.x;
            cp.y = lineY;

            return cp;
        }

        /// <summary>
        /// 線分と垂直線の交点を求める
        /// </summary>
        /// <param name="p0">線分の端点</param>
        /// <param name="p1">線分の端点</param>
        /// <param name="lineX">垂直線のX座標</param>
        /// <returns>交点 交点が存在しない場合は、Invalid==true</returns>
        public static CadVector CrossSegVLine2D(CadVector p0, CadVector p1, double lineX)
        {
            CadVector cp = default(CadVector);
            CadVector sp;
            CadVector ep;

            if (p0.x < p1.x)
            {
                sp = p0;
                ep = p1;
            }
            else
            {
                sp = p1;
                ep = p0;
            }

            if (lineX < sp.x)
            {
                cp.Invalid = true;
                return cp;
            }

            if (lineX > ep.x)
            {
                cp.Invalid = true;
                return cp;
            }

            double dx = ep.x - sp.x;
            double dy = ep.y - sp.y;

            double a = dy / dx;

            cp.x = lineX;
            cp.y = a * (lineX - sp.x) + sp.y;

            return cp;
        }

        /// <summary>
        /// クリッピング
        /// </summary>
        /// <param name="ox">矩形の原点X</param>
        /// <param name="oy">矩形の原点Y</param>
        /// <param name="w">矩形の幅</param>
        /// <param name="h">矩形の高さ</param>
        /// <param name="p0">線分端点0</param>
        /// <param name="p1">線分端点1</param>
        /// <returns>クリップされた線分 Valid==falseの場合は線分の全てがクリップ外</returns>
        /// 
        public static CadSegment Clipping2D(
            double ox, double oy,
            double w, double h,
            CadVector p0, CadVector p1)
        {
            CadSegment seg = new CadSegment(p0, p1);

            double ex = ox + w - 1;
            double ey = oy + h - 1;

            // Clip X
            if (p0.x < ox)
            {
                if (p1.x >= ox)
                {
                    p0 = CrossSegVLine2D(p0, p1, ox);
                }
                else
                {
                    seg.Valid = false;
                    return seg;
                }
            }

            if (p0.x > ex)
            {
                if (p1.x <= ex)
                {
                    p0 = CrossSegVLine2D(p0, p1, ex);
                }
                else
                {
                    seg.Valid = false;
                    return seg;
                }
            }

            if (p1.x < ox)
            {
                if (p0.x >= ox)
                {
                    p1 = CrossSegVLine2D(p0, p1, ox);
                }
                else
                {
                    seg.Valid = false;
                    return seg;
                }
            }

            if (p1.x > ex)
            {
                if (p0.x <= ex)
                {
                    p1 = CrossSegVLine2D(p0, p1, ex);
                }
                else
                {
                    seg.Valid = false;
                    return seg;
                }
            }


            // Clip Y
            if (p0.y < oy)
            {
                if (p1.y >= oy)
                {
                    p0 = CrossSegHLine2D(p0, p1, oy);
                }
                else
                {
                    seg.Valid = false;
                    return seg;
                }
            }

            if (p0.y > ey)
            {
                if (p1.y <= ey)
                {
                    p0 = CrossSegHLine2D(p0, p1, ey);
                }
                else
                {
                    seg.Valid = false;
                    return seg;
                }
            }

            if (p1.y < oy)
            {
                if (p0.y >= oy)
                {
                    p1 = CrossSegHLine2D(p0, p1, oy);
                }
                else
                {
                    seg.Valid = false;
                    return seg;
                }
            }

            if (p1.y > ey)
            {
                if (p0.y <= ey)
                {
                    p1 = CrossSegHLine2D(p0, p1, ey);
                }
                else
                {
                    seg.Valid = false;
                    return seg;
                }
            }

            seg.P0 = p0;
            seg.P1 = p1;

            return seg;
        }

        /// <summary>
        /// ベクトルの水平線に対する角度を求める(ラジアン)
        /// </summary>
        /// <param name="v">ベクトル</param>
        /// <returns>水平線に対する角度(ラジアン)</returns>
        public static double Angle2D(CadVector v)
        {
            return Math.Atan2(v.y, v.x);
        }

        /// <summary>
        /// 浮動小数点数を文字列に変換
        /// </summary>
        /// <param name="v">値</param>
        /// <returns>当プログラムでの標準的な変換方法で変換された文字列</returns>
        public static string ValToString(double v)
        {
            return v.ToString("f2");
        }

        // 1inchは何ミリ?
        public const double MILLI_PER_INCH = 25.4;

        public double MilliToInch(double mm)
        {
            return mm / MILLI_PER_INCH;
        }

        public double InchToMilli(double inchi)
        {
            return inchi * MILLI_PER_INCH;
        }

        public static PointPair LeftTopRightBottom2D(CadVector p0, CadVector p1)
        {
            double lx = p0.x;
            double rx = p1.x;

            double ty = p0.y;
            double by = p1.y;

            if (p0.x > p1.x)
            {
                lx = p1.x;
                rx = p0.x;
            }

            if (p0.y > p1.y)
            {
                ty = p1.y;
                by = p0.y;
            }

            return new PointPair(CadVector.Create(lx, ty, 0), CadVector.Create(rx, by, 0));
        }

        public static void Dump(Vector4d v, string prefix)
        {
            DOut.Begin();

            DOut.p(prefix);
            DOut.pl("{");
            DOut.Indent++;
            DOut.pl("x:" + v.X.ToString());
            DOut.pl("y:" + v.Y.ToString());
            DOut.pl("z:" + v.Z.ToString());
            DOut.pl("w:" + v.W.ToString());
            DOut.Indent--;
            DOut.pl("}");

            DOut.End();
        }

        public static void Dump(UMatrix4 m, string prefix)
        {
            DOut.p(prefix);
            DOut.pl("{");
            DOut.Indent++;
            DOut.pl(m.M11.ToString() + "," + m.M12.ToString() + "," + m.M13.ToString() + "," + m.M14.ToString());
            DOut.pl(m.M21.ToString() + "," + m.M22.ToString() + "," + m.M23.ToString() + "," + m.M24.ToString());
            DOut.pl(m.M31.ToString() + "," + m.M32.ToString() + "," + m.M33.ToString() + "," + m.M34.ToString());
            DOut.pl(m.M41.ToString() + "," + m.M42.ToString() + "," + m.M43.ToString() + "," + m.M44.ToString());
            DOut.Indent--;
            DOut.pl("}");
        }
    }
}
