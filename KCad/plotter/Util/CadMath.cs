using System;
using System.Collections.Generic;

namespace Plotter
{
    public class CadMath
    {
        public const double VRange = 0.0000005;

        public const double R1Min = 1.0 - VRange;
        public const double R1Max = 1.0 + VRange;

        public const double R0Min = -VRange;
        public const double R0Max = VRange;


        // 内積
        #region inner product
        public static double InnrProduct2D(CadVector v1, CadVector v2)
        {
            return (v1.x * v2.x) + (v1.y * v2.y);
        }

        public static double InnrProduct2D(CadVector v0, CadVector v1, CadVector v2)
        {
            return InnrProduct2D(v1 - v0, v2 - v0);
        }

        public static double InnerProduct(CadVector v1, CadVector v2)
        {
            return (v1.x * v2.x) + (v1.y * v2.y) + (v1.z * v2.z);
        }

        public static double InnerProduct(CadVector v0, CadVector v1, CadVector v2)
        {
            return InnerProduct(v1 - v0, v2 - v0);
        }
        #endregion


        // 外積
        #region Cross product
        public static double CrossProduct2D(CadVector v1, CadVector v2)
        {
            return (v1.x * v2.y) - (v1.y * v2.x);
        }

        public static double CrossProduct2D(CadVector v0, CadVector v1, CadVector v2)
        {
            return CrossProduct2D(v1 - v0, v2 - v0);
        }

        public static CadVector CrossProduct(CadVector v1, CadVector v2)
        {
            CadVector res = default(CadVector);

            res.x = v1.y * v2.z - v1.z * v2.y;
            res.y = v1.z * v2.x - v1.x * v2.z;
            res.z = v1.x * v2.y - v1.y * v2.x;

            return res;
        }

        public static CadVector CrossProduct(CadVector v0, CadVector v1, CadVector v2)
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
        public static CadVector Normal(CadVector v0, CadVector v1, CadVector v2)
        {
            CadVector va = v1 - v0;
            CadVector vb = v2 - v0;

            CadVector normal = CadMath.CrossProduct(va, vb);

            if (normal.IsZero())
            {
                return normal;
            }

            normal = normal.UnitVector();

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
        public static CadVector Normal(CadVector va, CadVector vb)
        {
            CadVector normal = CadMath.CrossProduct(va, vb);

            if (normal.IsZero())
            {
                return normal;
            }

            normal = normal.UnitVector();

            return normal;
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
        public static double AngleOfVector(CadVector v1, CadVector v2)
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


    /**
     *  汎用行列
     *  
     */  
    public class MatrixMN
    {
        public int RN = 0;
        public int CN = 0;

        public double[,] v; // RowNum, ColNum

        // 初期化例
        // MatrixMN m1 = new MatrixMN(new double[,]
        // {
        //     { 11, 12, 13 },
        //     { 21, 22, 23 },
        //     { 31, 32, 33 }
        // });

        public MatrixMN(double[,] a)
        {
            Attach(a);
        }

        public MatrixMN(int rownum, int colnum)
        {
            v = new double[rownum, colnum];
            RN = v.GetLength(0);
            CN = v.GetLength(1);
        }

        public void Set(MatrixMN m)
        {
            v = new double[m.RN, m.CN];
            RN = m.RN;
            CN = m.CN;

            v = new double[RN, CN];

            for (int r = 0; r < RN; r++)
            {
                for (int c = 0; c < RN; r++)
                {
                    v[r, c] = m.v[r, c];
                }
            }
        }

        public void Set(double[,] a)
        {
            v = a;
            RN = a.GetLength(0);
            CN = a.GetLength(1);

            v = new double[RN, CN];

            for (int r = 0; r < RN; r++)
            {
                for (int c = 0; c < RN; r++)
                {
                    v[r, c] = a[r, c];
                }
            }
        }

        public void Attach(double[,] a)
        {
            v = a;
            RN = v.GetLength(0);
            CN = v.GetLength(1);
        }

        public MatrixMN Product(MatrixMN right)
        {
            return Product(this, right);
        }

        public static MatrixMN operator *(MatrixMN m1, MatrixMN m2)
        {
            return Product(m1, m2);
        }

        public static MatrixMN Product(MatrixMN m1, MatrixMN m2)
        {
            if (m1.CN != m2.RN)
            {
                return null;
            }

            int row3 = Math.Min(m1.RN, m2.RN);
            int col3 = Math.Min(m1.CN, m2.CN);

            MatrixMN ret = new MatrixMN(row3, col3);

            int col1 = m1.CN;
            int row1 = m1.RN;

            int col2 = m2.CN;
            int row2 = m2.RN;


            for (int r = 0; r < row3; r++)
            {
                for (int c = 0; c < col3; c++)
                {
                    for (int k = 0; k < col1; k++)
                    {
                        ret.v[r, c] += m1.v[r, k] * m2.v[k, c];
                    }
                }
            }

            return ret;
        }

        public void dump()
        {
            DebugOut.println(nameof(MatrixMN) + "{");
            DebugOut.Indent++;

            for (int r = 0; r < RN; r++)
            {
                for (int c = 0; c < CN; c++)
                {
                    DebugOut.print(v[r, c].ToString() + ",");
                }
                DebugOut.println("");
            }

            DebugOut.Indent--;
            DebugOut.println("}");
        }
    }
}
