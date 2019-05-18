using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public static class VectorUtil
    {
        public static void Set(out Vector3d v, double x, double y, double z)
        {
            v.X = x;
            v.Y = y;
            v.Z = z;
        }

        public static void Set(out Vector4d v, double x, double y, double z, double w)
        {
            v.X = x;
            v.Y = y;
            v.Z = z;
            v.W = w;
        }
    }

    public static class VectorExtentions
    {
        public static Vector4d ToVector4d(this Vector3d v, double w)
        {
            return new Vector4d(v.X, v.Y, v.Z, w);
        }

        public static Vector3d ToVector3d(this Vector4d v)
        {
            return new Vector3d(v.X, v.Y, v.Z);
        }

        public static bool IsZero(this Vector3d v)
        {
            return (v.X + v.Y + v.Z) == 0;
        }

        public static Vector3d UnitVector()
        {
            Vector3d ret = default;

            double norm = ret.Length;

            double f = 1.0 / norm;

            ret.X *= f;
            ret.Y *= f;
            ret.Z *= f;

            return ret;
        }

        public static void dump(this Vector3d v, string prefix = nameof(Vector3d))
        {
            DOut.pl(prefix + "{");
            DOut.Indent++;
            DOut.pl("x:" + v.X.ToString());
            DOut.pl("y:" + v.Y.ToString());
            DOut.pl("z:" + v.Z.ToString());
            DOut.Indent--;
            DOut.pl("}");
        }
    }
}
