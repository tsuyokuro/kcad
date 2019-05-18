using System;
using CadDataTypes;

namespace Plotter
{
    public partial class CadMath
    {
        public const double Epsilon = 0.0000005;

        public const double RP1Min = 1.0 - Epsilon;
        public const double RP1Max = 1.0 + Epsilon;

        public const double RM1Min = -1.0 - Epsilon;
        public const double RM1Max = -1.0 + Epsilon;

        public const double R0Min = -Epsilon;
        public const double R0Max = Epsilon;

        public static bool Near_P1(double v)
        {
            return (v > RP1Min && v < RP1Max);
        }

        public static bool Near_M1(double v)
        {
            return (v > RM1Min && v < RM1Max);
        }

        public static bool Near_0(double v)
        {
            return (v > R0Min && v < R0Max);
        }

        /**
         * ラジアンを角度に変換
         * 
         */
        public static double Rad2Deg(double rad)
        {
            return 180.0 * rad / Math.PI;
        }

        /**
         * 角度をラジアンに変換
         * 
         */
        public static double Deg2Rad(double deg)
        {
            return Math.PI * deg / 180.0;
        }


        /// <summary>
        /// 2つのVectorのなす角を求める 
        ///
        /// 内積の定義を使う
        /// cosθ = ( AとBの内積 ) / (Aの長さ * Bの長さ)
        ///
        /// </summary>
        /// <param name="v1">Vector1</param>
        /// <param name="v2">Vector2</param>
        /// <returns>なす角</returns>
        /// 
        public static double AngleOfVector(CadVertex v1, CadVertex v2)
        {
            double v1n = v1.Norm();
            double v2n = v2.Norm();

            double cost = InnerProduct(v1, v2) / (v1n * v2n);

            double t = Math.Acos(cost);

            return t;
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
