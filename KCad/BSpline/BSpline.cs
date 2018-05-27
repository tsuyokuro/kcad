using CadDataTypes;
using System;

namespace BSpline
{
    public class BSplineUtil
    {
        public static VectorList CreateControlPoints(int ucnt, int vcnt, CadVector uunit, CadVector vunit)
        {
            VectorList vl = new VectorList(ucnt * vcnt);

            CadVector ud = ((double)(ucnt-1) / 2.0) * uunit;
            CadVector vd = ((double)(vcnt-1) / 2.0) * vunit;

            CadVector p = CadVector.Zero;

            p -= ud;
            p -= vd;

            CadVector lp = p;

            for (int v = 0; v < vcnt; v++)
            {
                p = lp;

                for (int u = 0; u < ucnt; u++)
                {
                    vl.Add(p);
                    p += uunit;
                }

                lp += vunit;
            }

            return vl;
        }
    }


    public class BSpline
    {
        public static double Epsilon = 0.000001f;   // とても小さい値

        //
        // Bスプライン基底関数
        //
        // i:制御点番号 P[i]
        // degree: 次数
        // t: Knotベクトル上を動く媒介変数
        // knots[]: Knotベクトル
        // 
        public static double BasisFunc(int i, int degree, double t, double[] knots)
        {
            if (degree == 0)
            {
                if (t >= knots[i] && t < knots[i + 1])
                {
                    return 1f;
                }
                else
                {
                    return 0f;
                }
            }

            double w1 = 0d;
            double w2 = 0d;
            double d1 = knots[i + degree] - knots[i];
            double d2 = knots[i + degree + 1] - knots[i + 1];

            if (d1 != 0d)
            {
                w1 = (t - knots[i]) / d1;
            }

            if (d2 != 0d)
            {
                w2 = (knots[i + degree + 1] - t) / d2;
            }

            double term1 = 0d;
            double term2 = 0d;

            if (w1 != 0d)
            {
                term1 = w1 * BasisFunc(i, degree - 1, t, knots);
            }

            if (w2 != 0d)
            {
                term2 = w2 * BasisFunc(i + 1, degree - 1, t, knots);
            }

            return term1 + term2;
        }

        // 2次での一様なBスプライン基底関数。
        public static CadVector CalcurateUniformBSplinePointWithDegree2(VectorList vl, int i, double t)
        {
            return 0.5f * (t * t - 2f * t + 1f) * vl[i] +
                0.5f * (-2f * t * t + 2f * t + 1f) * vl[i + 1] +
                0.5f * t * t * vl[i + 2];
        }

        // 3次での一様なBスプライン基底関数。
        public static CadVector CalcurateUniformBSplinePointWithDegree3(VectorList vl, int i, double t)
        {
            return 1f / 6f * (-vl[i] + 3f * vl[i + 1] - 3f * vl[i + 2] + vl[i + 3]) * t * t * t +
                0.5f * (vl[i] - 2f * vl[i + 1] + vl[i + 2]) * t * t +
                0.5f * (-vl[i] + vl[i + 2]) * t +
                1f / 6f * (vl[i] + 4f * vl[i + 1] + vl[i + 2]);
        }
    }
}
