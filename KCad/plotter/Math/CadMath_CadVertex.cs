using System;
using CadDataTypes;

namespace Plotter
{
    public partial class CadMath
    {
        // 内積
        #region inner product
        //public static double InnrProduct2D(CadVertex v1, CadVertex v2)
        //{
        //    return (v1.X * v2.X) + (v1.Y * v2.Y);
        //}

        //public static double InnrProduct2D(CadVertex v0, CadVertex v1, CadVertex v2)
        //{
        //    return InnrProduct2D(v1 - v0, v2 - v0);
        //}

        //public static double InnerProduct(CadVertex v1, CadVertex v2)
        //{
        //    return (v1.X * v2.X) + (v1.Y * v2.Y) + (v1.Z * v2.Z);
        //}

        //public static double InnerProduct(CadVertex v0, CadVertex v1, CadVertex v2)
        //{
        //    return InnerProduct(v1 - v0, v2 - v0);
        //}
        #endregion


        // 外積
        #region Cross product
        //public static double CrossProduct2D(CadVertex v1, CadVertex v2)
        //{
        //    return (v1.X * v2.Y) - (v1.Y * v2.X);
        //}

        //public static double CrossProduct2D(CadVertex v0, CadVertex v1, CadVertex v2)
        //{
        //    return CrossProduct2D(v1 - v0, v2 - v0);
        //}

        //public static CadVertex CrossProduct(CadVertex v1, CadVertex v2)
        //{
        //    CadVertex res = default(CadVertex);

        //    res.X = v1.Y * v2.Z - v1.Z * v2.Y;
        //    res.Y = v1.Z * v2.X - v1.X * v2.Z;
        //    res.Z = v1.X * v2.Y - v1.Y * v2.X;

        //    return res;
        //}

        //public static CadVertex CrossProduct(CadVertex v0, CadVertex v1, CadVertex v2)
        //{
        //    return CrossProduct(v1 - v0, v2 - v0);
        //}
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
        //public static CadVertex Normal(CadVertex v0, CadVertex v1, CadVertex v2)
        //{
        //    CadVertex va = v1 - v0;
        //    CadVertex vb = v2 - v0;

        //    CadVertex normal = CadMath.CrossProduct(va, vb);

        //    if (normal.IsZero())
        //    {
        //        normal.Invalid = true;
        //        return normal;
        //    }

        //    normal = normal.UnitVector();

        //    return normal;
        //}

        /**
         * 法線を求める
         * 
         *       vb
         *      / 
         *     /
         * 0 /_________va
         * 
         */
        //public static CadVertex Normal(CadVertex va, CadVertex vb)
        //{
        //    CadVertex normal = CadMath.CrossProduct(va, vb);

        //    if (normal.IsZero())
        //    {
        //        return normal;
        //    }

        //    normal = normal.UnitVector();

        //    return normal;
        //}

        //public static bool IsParallel(CadVertex v1, CadVertex v2)
        //{
        //    v1 = v1.UnitVector();
        //    v2 = v2.UnitVector();

        //    double a = InnerProduct(v1, v2);
        //    return Near_P1(a) || Near_M1(a);
        //}

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
        //public static double AngleOfVector(CadVertex v1, CadVertex v2)
        //{
        //    double v1n = v1.Norm();
        //    double v2n = v2.Norm();

        //    double cost = InnerProduct(v1, v2) / (v1n * v2n);

        //    double t = Math.Acos(cost);

        //    return t;
        //}
    }
}

