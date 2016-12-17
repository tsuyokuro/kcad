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

    // 4行4列の行列
    public struct Matrix44
    {
        public double v11, v12, v13, v14;
        public double v21, v22, v23, v24;
        public double v31, v32, v33, v34;
        public double v41, v42, v43, v44;

        public void set(
            double a11, double a12, double a13, double a14,
            double a21, double a22, double a23, double a24,
            double a31, double a32, double a33, double a34,
            double a41, double a42, double a43, double a44
            )
        {
            v11 = a11; v12 = a12; v13 = a13; v14 = a14;
            v21 = a21; v22 = a22; v23 = a23; v24 = a24;
            v31 = a31; v32 = a32; v33 = a33; v34 = a34;
            v41 = a41; v42 = a42; v43 = a43; v44 = a44;
        }

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

        public static Matrix44 operator *(Matrix44 m1, Matrix44 m2)
        {
            return product(m1, m2);
        }

        public static CadPoint operator *(Matrix44 m, CadPoint p)
        {
            return product(m, p);
        }

        //
        // | v11, v12, v13, v14 |  | x |
        // | v21, v22, v23, v24 |  | y |
        // | v31, v32, v33, v34 |  | z |
        // | v41, v42, v43, v44 |  | 1 |
        //
        public static CadPoint product(Matrix44 m, CadPoint p)
        {
            CadPoint rp = default(CadPoint);

            rp.x = m.v11 * p.x + m.v12 * p.y + m.v13 * p.z + m.v14;
            rp.y = m.v21 * p.x + m.v22 * p.y + m.v23 * p.z + m.v24;
            rp.z = m.v31 * p.x + m.v32 * p.y + m.v33 * p.z + m.v34;
            // w = m.v41 * p.x + m.v42 * p.y + m.v43 * p.z + m.v44;

            return rp;
        }

        public static Matrix44 product(Matrix44 m1, Matrix44 m2)
        {
            Matrix44 r;

            r.v11 = m1.v11 * m2.v11 + m1.v12 * m2.v21 + m1.v13 * m2.v31 + m1.v14 * m2.v41;
            r.v12 = m1.v11 * m2.v12 + m1.v12 * m2.v22 + m1.v13 * m2.v32 + m1.v14 * m2.v42;
            r.v13 = m1.v11 * m2.v13 + m1.v12 * m2.v23 + m1.v13 * m2.v33 + m1.v14 * m2.v43;
            r.v14 = m1.v11 * m2.v14 + m1.v12 * m2.v24 + m1.v13 * m2.v34 + m1.v14 * m2.v44;

            r.v21 = m1.v21 * m2.v11 + m1.v22 * m2.v21 + m1.v23 * m2.v31 + m1.v24 * m2.v41;
            r.v22 = m1.v21 * m2.v12 + m1.v22 * m2.v22 + m1.v23 * m2.v32 + m1.v24 * m2.v42;
            r.v23 = m1.v21 * m2.v13 + m1.v22 * m2.v23 + m1.v23 * m2.v33 + m1.v24 * m2.v43;
            r.v24 = m1.v21 * m2.v14 + m1.v22 * m2.v24 + m1.v23 * m2.v34 + m1.v24 * m2.v44;

            r.v31 = m1.v31 * m2.v11 + m1.v32 * m2.v21 + m1.v33 * m2.v31 + m1.v34 * m2.v41;
            r.v32 = m1.v31 * m2.v12 + m1.v32 * m2.v22 + m1.v33 * m2.v32 + m1.v34 * m2.v42;
            r.v33 = m1.v31 * m2.v13 + m1.v32 * m2.v23 + m1.v33 * m2.v33 + m1.v34 * m2.v43;
            r.v34 = m1.v31 * m2.v14 + m1.v32 * m2.v24 + m1.v33 * m2.v34 + m1.v34 * m2.v44;

            r.v41 = m1.v41 * m2.v11 + m1.v42 * m2.v21 + m1.v43 * m2.v31 + m1.v44 * m2.v41;
            r.v42 = m1.v41 * m2.v12 + m1.v42 * m2.v22 + m1.v43 * m2.v32 + m1.v44 * m2.v42;
            r.v43 = m1.v41 * m2.v13 + m1.v42 * m2.v23 + m1.v43 * m2.v33 + m1.v44 * m2.v43;
            r.v44 = m1.v41 * m2.v14 + m1.v42 * m2.v24 + m1.v43 * m2.v34 + m1.v44 * m2.v44;

            return r;
        }

        public void dump(DebugOut o)
        {
            o.println(nameof(Matrix33) + "{");
            o.Indent++;
            o.println(v11.ToString() + "," + v12.ToString() + "," + v13.ToString() + "," + v14.ToString());
            o.println(v21.ToString() + "," + v22.ToString() + "," + v23.ToString() + "," + v24.ToString());
            o.println(v31.ToString() + "," + v32.ToString() + "," + v33.ToString() + "," + v34.ToString());
            o.println(v41.ToString() + "," + v42.ToString() + "," + v43.ToString() + "," + v44.ToString());
            o.Indent--;
            o.println("}");
        }
    }

    // 3行3列の行列
    public struct Matrix33
    {
        public double v11, v12, v13;
        public double v21, v22, v23;
        public double v31, v32, v33;

        public void set(
            double a11, double a12, double a13,
            double a21, double a22, double a23,
            double a31, double a32, double a33
            )
        {
            v11 = a11; v12 = a12; v13 = a13;
            v21 = a21; v22 = a22; v23 = a23;
            v31 = a31; v32 = a32; v33 = a33;
        }

        public static Matrix33 operator *(Matrix33 m1, Matrix33 m2)
        {
            return product(m1, m2);
        }

        public static CadPoint operator *(Matrix33 m, CadPoint p)
        {
            return product(m, p);
        }

        //
        // | v11, v12, v13 |  | x |
        // | v21, v22, v23 |  | y |
        // | v31, v32, v33 |  | z |
        //
        public static CadPoint product(Matrix33 m, CadPoint p)
        {
            CadPoint rp = default(CadPoint);

            rp.x = m.v11 * p.x + m.v12 * p.y + m.v13 * p.z;
            rp.y = m.v21 * p.x + m.v22 * p.y + m.v23 * p.z;
            rp.z = m.v31 * p.x + m.v32 * p.y + m.v33 * p.z;

            return rp;
        }

        public static Matrix33 product(Matrix33 m1, Matrix33 m2)
        {
            Matrix33 r;

            r.v11 = m1.v11 * m2.v11 + m1.v12 * m2.v21 + m1.v13 * m2.v31;
            r.v12 = m1.v11 * m2.v12 + m1.v12 * m2.v22 + m1.v13 * m2.v32;
            r.v13 = m1.v11 * m2.v13 + m1.v12 * m2.v23 + m1.v13 * m2.v33;

            r.v21 = m1.v21 * m2.v11 + m1.v22 * m2.v21 + m1.v23 * m2.v31;
            r.v22 = m1.v21 * m2.v12 + m1.v22 * m2.v22 + m1.v23 * m2.v32;
            r.v23 = m1.v21 * m2.v13 + m1.v22 * m2.v23 + m1.v23 * m2.v33;

            r.v31 = m1.v31 * m2.v11 + m1.v32 * m2.v21 + m1.v33 * m2.v31;
            r.v32 = m1.v31 * m2.v12 + m1.v32 * m2.v22 + m1.v33 * m2.v32;
            r.v33 = m1.v31 * m2.v13 + m1.v32 * m2.v23 + m1.v33 * m2.v33;

            return r;
        }

        // 逆行列
        public Matrix33 invers()
        {
            Matrix33 r;

            r.v11 = v22 * v33 - v23 * v32;
            r.v12 = v13 * v32 - v12 * v33;
            r.v13 = v12 * v23 - v13 * v22;

            r.v21 = v23 * v31 - v21 * v33;
            r.v22 = v11 * v33 - v13 * v31;
            r.v23 = v13 * v21 - v11 * v23;

            r.v31 = v21 * v32 - v22 * v31;
            r.v32 = v12 * v31 - v11 * v32;
            r.v33 = v11 * v22 - v12 * v21;

            return r;
        }

        public void dump(DebugOut o)
        {
            o.println(nameof(Matrix33) + "{");
            o.Indent++;
            o.println(v11.ToString() + "," + v12.ToString() + "," + v13.ToString());
            o.println(v21.ToString() + "," + v22.ToString() + "," + v23.ToString());
            o.println(v31.ToString() + "," + v32.ToString() + "," + v33.ToString());
            o.Indent--;
            o.println("}");
        }
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
