// 4行4列の行列
using OpenTK;
using System;
using CadDataTypes;

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

        public static UMatrix4 operator *(UMatrix4 m1, UMatrix4 m2)
        {
            return product(m1, m2);
        }

        // 通常OpenGLのVectorは、列Vectorらしいのだが、
        // Vector4d.Transformは、行Vectorとして扱うみたい
        //              |m11, m12, m13, m14|
        //              |m21, m22, m23, m24|
        // (x, y, z, w) |m31, m32, m33, m34|
        //              |m41, m42, m43, m44|
        public static CadVector operator *(CadVector p, UMatrix4 m)
        {
            return product(p, m);
        }

        public static Vector4d operator *(Vector4d v, UMatrix4 m)
        {
            return product(v, m);
        }

        public static CadVector product(CadVector p, UMatrix4 m)
        {
            CadVector rp = default(CadVector);

            Vector4d v = Vector4d.Transform((Vector4d)p, m.GLMatrix);

            rp = (CadVector)v;

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
            return r;
        }

        public static UMatrix4 Invert(UMatrix4 m)
        {
            Matrix4d inv = Matrix4d.Invert(m.GLMatrix);
            UMatrix4 ret = default(UMatrix4);
            ret.GLMatrix = inv;

            return ret;
        }

        public static readonly UMatrix4 Unit = new UMatrix4
            (
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            );

        public void dump()
        {
            DbgOut.pln(nameof(UMatrix4) + "{");
            DbgOut.Indent++;
            DbgOut.pln(M11.ToString() + "," + M12.ToString() + "," + M13.ToString() + "," + M14.ToString());
            DbgOut.pln(M21.ToString() + "," + M22.ToString() + "," + M23.ToString() + "," + M24.ToString());
            DbgOut.pln(M31.ToString() + "," + M32.ToString() + "," + M33.ToString() + "," + M34.ToString());
            DbgOut.pln(M41.ToString() + "," + M42.ToString() + "," + M43.ToString() + "," + M44.ToString());
            DbgOut.Indent--;
            DbgOut.pln("}");
        }
    }
}