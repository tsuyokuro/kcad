// 4行4列の行列
using OpenTK;
using System;

namespace Plotter
{
    using static System.Math;

    public struct UMatrix4
    {
        public Matrix4d GLMatrix;

        public double M11
        {
            set { GLMatrix.M11 = value; }
            get { return GLMatrix.M11; }
        }

        public double M12
        {
            set { GLMatrix.M12 = value; }
            get { return GLMatrix.M12; }
        }

        public double M13
        {
            set { GLMatrix.M13 = value; }
            get { return GLMatrix.M13; }
        }

        public double M14
        {
            set { GLMatrix.M14 = value; }
            get { return GLMatrix.M14; }
        }


        public double M21
        {
            set { GLMatrix.M21 = value; }
            get { return GLMatrix.M21; }
        }

        public double M22
        {
            set { GLMatrix.M22 = value; }
            get { return GLMatrix.M22; }
        }

        public double M23
        {
            set { GLMatrix.M23 = value; }
            get { return GLMatrix.M23; }
        }

        public double M24
        {
            set { GLMatrix.M24 = value; }
            get { return GLMatrix.M24; }
        }


        public double M31
        {
            set { GLMatrix.M31 = value; }
            get { return GLMatrix.M31; }
        }

        public double M32
        {
            set { GLMatrix.M32 = value; }
            get { return GLMatrix.M32; }
        }

        public double M33
        {
            set { GLMatrix.M33 = value; }
            get { return GLMatrix.M33; }
        }

        public double M34
        {
            set { GLMatrix.M34 = value; }
            get { return GLMatrix.M34; }
        }


        public double M41
        {
            set { GLMatrix.M41 = value; }
            get { return GLMatrix.M41; }
        }

        public double M42
        {
            set { GLMatrix.M42 = value; }
            get { return GLMatrix.M42; }
        }

        public double M43
        {
            set { GLMatrix.M43 = value; }
            get { return GLMatrix.M43; }
        }

        public double M44
        {
            set { GLMatrix.M44 = value; }
            get { return GLMatrix.M44; }
        }

        public UMatrix4(
            double a11, double a12, double a13, double a14,
            double a21, double a22, double a23, double a24,
            double a31, double a32, double a33, double a34,
            double a41, double a42, double a43, double a44
            )
        {
            GLMatrix = default(Matrix4d);

            M11 = a11; M12 = a12; M13 = a13; M14 = a14;
            M21 = a21; M22 = a22; M23 = a23; M24 = a24;
            M31 = a31; M32 = a32; M33 = a33; M34 = a34;
            M41 = a41; M42 = a42; M43 = a43; M44 = a44;
        }

        public UMatrix4(
            double a11, double a12, double a13,
            double a21, double a22, double a23,
            double a31, double a32, double a33
            )
        {
            GLMatrix = default(Matrix4d);

            M11 = a11; M12 = a12; M13 = a13; M14 = 0;
            M21 = a21; M22 = a22; M23 = a23; M24 = 0;
            M31 = a31; M32 = a32; M33 = a33; M34 = 0;
            M41 = 0; M42 = 0; M43 = 0; M44 = 1;
        }


        public void set(
            double a11, double a12, double a13, double a14,
            double a21, double a22, double a23, double a24,
            double a31, double a32, double a33, double a34,
            double a41, double a42, double a43, double a44
            )
        {
            GLMatrix = default(Matrix4d);

            M11 = a11; M12 = a12; M13 = a13; M14 = a14;
            M21 = a21; M22 = a22; M23 = a23; M24 = a24;
            M31 = a31; M32 = a32; M33 = a33; M34 = a34;
            M41 = a41; M42 = a42; M43 = a43; M44 = a44;
        }

        public void setXRote(double t)
        {
            M11 = 1.0;
            M12 = 0;
            M13 = 0;
            M14 = 0;

            M21 = 0;
            M22 = Math.Cos(t);
            M23 = -Math.Sin(t);
            M24 = 0;

            M31 = 0;
            M32 = Math.Sin(t);
            M33 = Math.Cos(t);
            M34 = 0;

            M41 = 0;
            M42 = 0;
            M43 = 0;
            M44 = 1.0;
        }

        public static UMatrix4 operator *(UMatrix4 m1, UMatrix4 m2)
        {
            return product(m1, m2);
        }

        public static CadPoint operator *(CadPoint p, UMatrix4 m)
        {
            return product(p, m);
        }

        public static Vector4d operator *(Vector4d v, UMatrix4 m)
        {
            return product(v, m);
        }

        //
        // | M11, M12, M13, M14 |  | x |
        // | M21, M22, M23, M24 |  | y |
        // | M31, M32, M33, M34 |  | z |
        // | M41, M42, M43, M44 |  | 1 |
        //
        public static CadPoint product(CadPoint p, UMatrix4 m)
        {
            CadPoint rp = default(CadPoint);

            Vector4d v = Vector4d.Transform((Vector4d)p, m.GLMatrix);

            rp = (CadPoint)v;

            //rp.x = m.M11 * p.x + m.M12 * p.y + m.M13 * p.z + m.M14;
            //rp.y = m.M21 * p.x + m.M22 * p.y + m.M23 * p.z + m.M24;
            //rp.z = m.M31 * p.x + m.M32 * p.y + m.M33 * p.z + m.M34;

            return rp;
        }

        public static Vector4d product(Vector4d p, UMatrix4 m)
        {
            Vector4d v = Vector4d.Transform(p, m.GLMatrix);
            return v;
        }

        public static UMatrix4 product(UMatrix4 m1, UMatrix4 m2)
        {
            UMatrix4 r = default(UMatrix4);

            r.GLMatrix = Matrix4d.Mult(m1.GLMatrix, m2.GLMatrix);

            /*
            r.M11 = m1.M11 * m2.M11 + m1.M12 * m2.M21 + m1.M13 * m2.M31 + m1.M14 * m2.M41;
            r.M12 = m1.M11 * m2.M12 + m1.M12 * m2.M22 + m1.M13 * m2.M32 + m1.M14 * m2.M42;
            r.M13 = m1.M11 * m2.M13 + m1.M12 * m2.M23 + m1.M13 * m2.M33 + m1.M14 * m2.M43;
            r.M14 = m1.M11 * m2.M14 + m1.M12 * m2.M24 + m1.M13 * m2.M34 + m1.M14 * m2.M44;

            r.M21 = m1.M21 * m2.M11 + m1.M22 * m2.M21 + m1.M23 * m2.M31 + m1.M24 * m2.M41;
            r.M22 = m1.M21 * m2.M12 + m1.M22 * m2.M22 + m1.M23 * m2.M32 + m1.M24 * m2.M42;
            r.M23 = m1.M21 * m2.M13 + m1.M22 * m2.M23 + m1.M23 * m2.M33 + m1.M24 * m2.M43;
            r.M24 = m1.M21 * m2.M14 + m1.M22 * m2.M24 + m1.M23 * m2.M34 + m1.M24 * m2.M44;

            r.M31 = m1.M31 * m2.M11 + m1.M32 * m2.M21 + m1.M33 * m2.M31 + m1.M34 * m2.M41;
            r.M32 = m1.M31 * m2.M12 + m1.M32 * m2.M22 + m1.M33 * m2.M32 + m1.M34 * m2.M42;
            r.M33 = m1.M31 * m2.M13 + m1.M32 * m2.M23 + m1.M33 * m2.M33 + m1.M34 * m2.M43;
            r.M34 = m1.M31 * m2.M14 + m1.M32 * m2.M24 + m1.M33 * m2.M34 + m1.M34 * m2.M44;

            r.M41 = m1.M41 * m2.M11 + m1.M42 * m2.M21 + m1.M43 * m2.M31 + m1.M44 * m2.M41;
            r.M42 = m1.M41 * m2.M12 + m1.M42 * m2.M22 + m1.M43 * m2.M32 + m1.M44 * m2.M42;
            r.M43 = m1.M41 * m2.M13 + m1.M42 * m2.M23 + m1.M43 * m2.M33 + m1.M44 * m2.M43;
            r.M44 = m1.M41 * m2.M14 + m1.M42 * m2.M24 + m1.M43 * m2.M34 + m1.M44 * m2.M44;
            */
            return r;
        }

        public static UMatrix4 Invert(UMatrix4 m)
        {
            Matrix4d inv = Matrix4d.Invert(m.GLMatrix);
            UMatrix4 ret = default(UMatrix4);
            ret.GLMatrix = inv;

            return ret;
        }

        public void dump(DebugOut o)
        {
            o.println(nameof(UMatrix4) + "{");
            o.Indent++;
            o.println(M11.ToString() + "," + M12.ToString() + "," + M13.ToString() + "," + M14.ToString());
            o.println(M21.ToString() + "," + M22.ToString() + "," + M23.ToString() + "," + M24.ToString());
            o.println(M31.ToString() + "," + M32.ToString() + "," + M33.ToString() + "," + M34.ToString());
            o.println(M41.ToString() + "," + M42.ToString() + "," + M43.ToString() + "," + M44.ToString());
            o.Indent--;
            o.println("}");
        }
    }

    public class UMatrixs
    {
        public static readonly UMatrix4 Unit = new UMatrix4
            (
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            );


        public static readonly UMatrix4 ViewXY = new UMatrix4
            (
                1, 0, 0,
                0, 1, 0,
                0, 0, 1
            );

        public static readonly UMatrix4 ViewXYInv = new UMatrix4
            (
                1, 0, 0,
                0, 1, 0,
                0, 0, 1
            );

        // 1, 0, 0,
        // 0, Cos(-PI/2), -Sin(-PI/2),
        // 0, Sin(-PI/2), Cos(-PI/2)
        public static readonly UMatrix4 ViewXZ = new UMatrix4
            (
                1, 0, 0,
                0, 0, 1,
                0, -1, 0
            );

        // 1, 0, 0,
        // 0, Cos(PI/2), -Sin(PI/2),
        // 0, Sin(PI/2), Cos(PI/2)
        public static readonly UMatrix4 ViewXZInv = new UMatrix4
            (
                1, 0, 0,
                0, 0, -1,
                0, 1, 0
            );


        // Cos(PI/2), 0, Sin(PI/2),
        // 0, 1, 0,
        // -Sin(PI/2), 0, Cos(PI/2)
        public static readonly UMatrix4 ViewZY = new UMatrix4
            (
                0, 0, 1,
                0, 1, 0,
                -1, 0, 0
            );

        // Cos(PI/2), 0, Sin(PI/2),
        // 0, 1, 0,
        // -Sin(PI/2), 0, Cos(PI/2)
        public static readonly UMatrix4 ViewZYInv = new UMatrix4
            (
                0, 0, -1,
                0, 1, 0,
                1, 0, 0
            );


        // For test
        private static double xt = -Math.PI / 10.0;
        private static double yt = Math.PI / 10.0;

        public static readonly UMatrix4 MatrixXY_XQ_F = new UMatrix4
            (
                1, 0, 0,
                0, Cos(xt), -Sin(xt),
                0, Sin(xt), Cos(xt)
            );

        public static readonly UMatrix4 MatrixXY_YQ_F = new UMatrix4
            (
                Cos(yt), 0, Sin(yt),
                0, 1, 0,
                -Sin(yt), 0, Cos(yt)
            );


        public static readonly UMatrix4 MatrixXY_XQ_R = new UMatrix4
            (
                1, 0, 0,
                0, Cos(-xt), -Sin(-xt),
                0, Sin(-xt), Cos(-xt)
            );

        public static readonly UMatrix4 MatrixXY_YQ_R = new UMatrix4
            (
                Cos(-yt), 0, Sin(-yt),
                0, 1, 0,
                -Sin(-yt), 0, Cos(-yt)
            );

    }
}