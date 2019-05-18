using System;
using CadDataTypes;
using OpenTK;

namespace Plotter
{
    public partial class CadMath
    {
        // 内積
        #region inner product
        public static double InnrProduct2D(Vector3d v1, Vector3d v2)
        {
            return (v1.X * v2.X) + (v1.Y * v2.Y);
        }

        public static double InnrProduct2D(Vector3d v0, Vector3d v1, Vector3d v2)
        {
            return InnrProduct2D(v1 - v0, v2 - v0);
        }

        public static double InnerProduct(Vector3d v1, Vector3d v2)
        {
            return (v1.X * v2.X) + (v1.Y * v2.Y) + (v1.Z * v2.Z);
        }

        public static double InnerProduct(Vector3d v0, Vector3d v1, Vector3d v2)
        {
            return InnerProduct(v1 - v0, v2 - v0);
        }
        #endregion


        // 外積
        #region Cross product
        public static double CrossProduct2D(Vector3d v1, Vector3d v2)
        {
            return (v1.X * v2.Y) - (v1.Y * v2.X);
        }

        public static double CrossProduct2D(Vector3d v0, Vector3d v1, Vector3d v2)
        {
            return CrossProduct2D(v1 - v0, v2 - v0);
        }

        public static Vector3d CrossProduct(Vector3d v1, Vector3d v2)
        {
            Vector3d res = default(Vector3d);

            res.X = v1.Y * v2.Z - v1.Z * v2.Y;
            res.Y = v1.Z * v2.X - v1.X * v2.Z;
            res.Z = v1.X * v2.Y - v1.Y * v2.X;

            return res;
        }

        public static Vector3d CrossProduct(Vector3d v0, Vector3d v1, Vector3d v2)
        {
            return CrossProduct(v1 - v0, v2 - v0);
        }
        #endregion

        /**
         * 法線を求める
         * 
         *      v2
         *     / 
         *    /
         * v0/_________v1
         *
         */
        public static Vector3d? Normal(Vector3d v0, Vector3d v1, Vector3d v2)
        {
            Vector3d va = v1 - v0;
            Vector3d vb = v2 - v0;

            Vector3d normal = CrossProduct(va, vb);

            if (normal.IsZero())
            {
                return normal;
            }

            normal.Normalize();

            return normal;
        }

        /**
         * 法線を求める
         * 
         *       vb
         *      / 
         *     /
         * 0 /_________va
         * 
         */
        public static Vector3d Normal(Vector3d va, Vector3d vb)
        {
            Vector3d normal = CadMath.CrossProduct(va, vb);

            if (normal.IsZero())
            {
                return normal;
            }

            normal.Normalize();

            return normal;
        }

        public static bool IsParallel(Vector3d v1, Vector3d v2)
        {
            v1.Normalize();
            v2.Normalize();

            double a = InnerProduct(v1, v2);
            return Near_P1(a) || Near_M1(a);
        }
    }
}

