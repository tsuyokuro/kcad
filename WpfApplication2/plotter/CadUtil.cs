using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public struct CrossInfo
    {
        public bool isCross;
        public CadPoint CrossPoint;
    }

    public class CadUtil
    {
        public static double innrProduct2D(CadPoint vl, CadPoint vr)
        {
            return (vl.x * vr.x) + (vl.y * vr.y);
        }

        public static double exteriorProduct2D(CadPoint vl, CadPoint vr)
        {
            return (vl.x * vr.y) - (vl.y * vr.x);
        }

        public static double distancePtoSeg2D(CadPoint a, CadPoint b, CadPoint p)
        {
            double t;

            CadPoint ab = b - a;
            CadPoint ap = p - a;

            t = innrProduct2D(ab, ap);

            if (t < 0)
            {
                return vectAbs2D(ap);
            }

            CadPoint ba = a - b;
            CadPoint bp = p - b;

            t = innrProduct2D(ba, bp);

            if (t < 0)
            {
                return vectAbs2D(bp);
            }

            double d = Math.Abs(exteriorProduct2D(ab, ap));
            double abl = vectAbs2D(ab);

            return d / abl;
        }

        public static CrossInfo getNormCross2D(CadPoint a, CadPoint b, CadPoint p)
        {
            CrossInfo ret = default(CrossInfo);

            double t1;

            CadPoint ab = b - a;
            CadPoint ap = p - a;

            t1 = innrProduct2D(ab, ap);

            if (t1 < 0)
            {
                return ret;
            }

            double t2;

            CadPoint ba = a - b;
            CadPoint bp = p - b;

            t2 = innrProduct2D(ba, bp);

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

        public static double rad2deg(double rad)
        {
            return 180.0 * rad / Math.PI;
        }

        public static double deg2rad(double deg)
        {
            return Math.PI * deg / 180.0;
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
