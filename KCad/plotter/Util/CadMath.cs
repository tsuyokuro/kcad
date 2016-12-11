using System;

namespace Plotter
{
    public class CadMath
    {
        // 単位ベクトル
        public static CadPoint unitVector(CadPoint v)
        {
            double len = v.length();
            return v / len;
        }

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

        //
        // | v11, v12, v13, v14 |  | x |
        // | v21, v22, v23, v24 |  | y |
        // | v31, v32, v33, v34 |  | z |
        // | v41, v42, v43, v44 |  | 1 |
        //
        public static CadPoint matrixProduct(Matrix44 m, CadPoint p)
        {
            CadPoint rp = default(CadPoint);

            rp.x = m.v11 * p.x + m.v12 * p.y + m.v13 * p.z + m.v14;
            rp.y = m.v21 * p.x + m.v22 * p.y + m.v23 * p.z + m.v24;
            rp.z = m.v31 * p.x + m.v32 * p.y + m.v33 * p.z + m.v34;
            // w = m.v41 * p.x + m.v42 * p.y + m.v43 * p.z + m.v44;

            return rp;
        }
    }

    public struct Matrix44
    {
        // v11, v12, v13, v14
        // v21, v22, v23, v24
        // v31, v32, v33, v34
        // v41, v42, v43, v44
        public double v11, v12, v13, v14;
        public double v21, v22, v23, v24;
        public double v31, v32, v33, v34;
        public double v41, v42, v43, v44;

        public void setXRote(double t)
        {
            v11 = 1.0;
            v12 = 0;
            v13 = 0;
            v14 = 0;

            v21 = 0;
            v22 = Math.Cos(t);
            v23 = -Math.Sin(t);
            v24 = 0;

            v31 = 0;
            v32 = Math.Sin(t);
            v33 = Math.Cos(t);
            v34 = 0;

            v41 = 0;
            v42 = 0;
            v43 = 0;
            v44 = 1.0;
        }
    }
}
