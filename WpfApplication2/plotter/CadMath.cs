using System;

namespace Plotter
{
    class CadMath
    {
        // 内積
        #region inner product
        public static double innrProduct2D(CadPoint v1, CadPoint v2)
        {
            return (v1.x * v2.x) + (v1.y * v2.y);
        }

        public static double innrProduct2D(CadPoint v0, CadPoint v1, CadPoint v2)
        {
            return innrProduct2D(v1 - v0, v2 - v0);
        }

        public static double innerProduct3D(CadPoint v1, CadPoint v2)
        {
            return (v1.x * v2.x) + (v1.y * v2.y) + (v1.z * v2.z);
        }

        public static double innerProduct3D(CadPoint v0, CadPoint v1, CadPoint v2)
        {
            return innerProduct3D(v1 - v0, v2 - v0);
        }
        #endregion


        // 外積
        #region Cross product
        public static double crossProduct2D(CadPoint v1, CadPoint v2)
        {
            return (v1.x * v2.y) - (v1.y * v2.x);
        }

        public static double crossProduct2D(CadPoint v0, CadPoint v1, CadPoint v2)
        {
            return crossProduct2D(v1 - v0, v2 - v0);
        }

        public static CadPoint crossProduct3D(CadPoint v1, CadPoint v2)
        {
            CadPoint res = default(CadPoint);

            res.x = v1.y * v2.z - v1.z * v2.y;
            res.y = v1.z * v2.x - v1.x * v2.z;
            res.z = v1.x * v2.y - v1.y * v2.x;

            return res;
        }

        public static CadPoint crossProduct3D(CadPoint v0, CadPoint v1, CadPoint v2)
        {
            return crossProduct3D(v1 - v0, v2 - v0);
        }
        #endregion


        public static double rad2deg(double rad)
        {
            return 180.0 * rad / Math.PI;
        }

        public static double deg2rad(double deg)
        {
            return Math.PI * deg / 180.0;
        }

        #region For bezier
        static double[] FactorialTbl =
        {
            1.0, // 0!
            1.0,
            2.0 * 1.0,
            3.0 * 2.0 * 1.0,
            4.0 * 3.0 * 2.0 * 1.0,
            5.0 * 4.0 * 3.0 * 2.0 * 1.0,
            6.0 * 5.0 * 4.0 * 3.0 * 2.0 * 1.0,
        };

        // Bernstein basis polynomials
        public static double BernsteinBasisF(int n, int i, double t)
        {
            return BinomialCoefficientsF(n, i) * Math.Pow(t, i) * Math.Pow(1 - t, n - i);
        }

        // Binomial coefficient
        public static double BinomialCoefficientsF(int n, int k)
        {
            return FactorialTbl[n] / (FactorialTbl[k] * FactorialTbl[n - k]);
        }



        // Bernstein basis polynomials
        public static double BernsteinBasis(int n, int i, double t)
        {
            return BinomialCoefficients(n, i) * Math.Pow(t, i) * Math.Pow(1 - t, n - i);
        }

        // Binomial coefficient
        public static double BinomialCoefficients(int n, int k)
        {
            return Factorial(n) / (Factorial(k) * Factorial(n - k));
        }

        // e.g 6! = 6*5*4*3*2*1
        public static double Factorial(int a)
        {
            double r = 1.0;
            for (int i = 2; i <= a; i++)
            {
                r *= (double)i;
            }

            return r;
        }
        #endregion
    }
}
