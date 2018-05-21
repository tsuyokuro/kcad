using CadDataTypes;
using System;

namespace BSpline
{
    public class BSpline
    {
        public static double Epsilon = 0.000001f;

        // Bスプライン基底関数。
        public static double BSplineBasisFunc(int i, int degree, double t, double[] knots)
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

            double w1 = 0f;
            double w2 = 0f;
            double denominatorA = knots[i + degree] - knots[i];
            double denominatorB = knots[i + degree + 1] - knots[i + 1];

            if (denominatorA != 0f)
            {
                w1 = (t - knots[i]) / denominatorA;
            }

            if (denominatorB != 0f)
            {
                w2 = (knots[i + degree + 1] - t) / denominatorB;
            }

            double firstTerm = 0f;
            double secondTerm = 0f;

            if (w1 != 0f)
            {
                firstTerm = w1 * BSplineBasisFunc(i, degree - 1, t, knots);
            }

            if (w2 != 0f)
            {
                secondTerm = w2 * BSplineBasisFunc(i + 1, degree - 1, t, knots);
            }

            return firstTerm + secondTerm;
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
