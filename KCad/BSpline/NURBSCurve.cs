using CadDataTypes;
using System;

namespace BSpline {
	public class NURBSCurve : BSplineCurve
    {
		double[] Weights;

		public override void SetPoints(VectorList points)
        {
			Points = Closed ? CreatePointsAsClosed(points) : points;

			if (Knots == null || Points.Count + mDegree + 1 != Knots.Length)
            {
                ResetKnots();
            }

            if (Weights == null || Weights.Length != Points.Count + (Closed ? mDegree : 0))
            {
                ResetWeights();
            }
        }

		// �d�݂��l�����A�w��̈ʒu�̍��W���擾�B
		protected override CadVector CalcuratePoint(double t)
        {
            CadVector linePoint = CadVector.Zero;
			double weight = 0f;

			for (int i = 0; i < Points.Count; ++i)
            {
				double bs = BSplineCurve.BSplineBasisFunc(i, mDegree, t, Knots);

                linePoint += bs * Weights[i] * Points[i];

                weight += bs * Weights[i];
			}

			return linePoint / weight;
		}

		// �d�݂̃��Z�b�g�B
		void ResetWeights()
        {
			if (Points == null)
            {
                return;
            }

            Weights = new double[Points.Count + (Closed ? mDegree : 0)];

			for (int i = 0; i < Weights.Length; ++i)
            {
				Weights[i] = 1f;
			}
		}

        // ���������_��]�����Ԃ��B
        public override void Evaluate(VectorList linePoints)
        {
            if (mDegree < 1)
            {
                return;
            }

            if (Points == null || Points.Count < 2 || Points.Count < mDegree + 1)
            {
                return;
            }

            linePoints.Clear();

            // �������̃`�F�b�N
            if (DividedCount < 1)
            {
                DividedCount = 1;
            }

            // 1���̏ꍇ�͒����Ō���
            if (mDegree == 1)
            {
                linePoints.AddRange(Points);
                return;
            }

            double lowKnot = Knots[mDegree];
            double highKnot = Knots[Points.Count];
            double step = (highKnot - lowKnot) / DividedCount;

            for (int p = 0; p <= DividedCount; ++p)
            {
                double t = p * step + lowKnot;
                if (t >= highKnot) t = highKnot - BSplineCurve.Epsilon;

                linePoints.Add(CalcuratePoint(t));
            }
        }
    }
}