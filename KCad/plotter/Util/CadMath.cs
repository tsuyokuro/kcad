using System;

namespace Plotter
{
    public class CadMath
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

        public static CadPoint Normal(CadPoint va, CadPoint vb)
        {
            CadPoint normal = CadMath.crossProduct3D(va, vb);

            normal = normal.unitVector();

            return normal;
        }


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


    // 汎用行列
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
            attach(a);
        }

        public MatrixMN(int rownum, int colnum)
        {
            v = new double[rownum, colnum];
            RN = v.GetLength(0);
            CN = v.GetLength(1);
        }

        public void set(MatrixMN m)
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

        public void set(double[,] a)
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

        public void attach(double[,] a)
        {
            v = a;
            RN = v.GetLength(0);
            CN = v.GetLength(1);
        }

        public MatrixMN product(MatrixMN right)
        {
            return product(this, right);
        }

        public static MatrixMN operator *(MatrixMN m1, MatrixMN m2)
        {
            return product(m1, m2);
        }

        public static MatrixMN product(MatrixMN m1, MatrixMN m2)
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

        public void dump(DebugOut o)
        {
            o.println(nameof(MatrixMN) + "{");
            o.Indent++;

            for (int r = 0; r < RN; r++)
            {
                for (int c = 0; c < CN; c++)
                {
                    o.print(v[r, c].ToString() + ",");
                }
                o.println("");
            }

            o.Indent--;
            o.println("}");
        }
    }
}
