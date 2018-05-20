using CadDataTypes;
using System;

namespace BSpline
{
	public class BSplineCurve
    {
        // ����J�[�u�Ƃ��邩�B
        public bool Closed;

        // ���C���̕������B
        public int DividedCount = 100;

        // �C�ӂ̓_�B
        protected VectorList Points;


        protected static float Epsilon = 0.000001f;

        // B�X�v���C�����֐��B
        protected static double BSplineBasisFunc(int i, int degree, double t, double[] knots)
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
                firstTerm = w1 * BSplineCurve.BSplineBasisFunc(i, degree - 1, t, knots);
            }

            if (w2 != 0f)
            {
                secondTerm = w2 * BSplineCurve.BSplineBasisFunc(i + 1, degree - 1, t, knots);
            }

            return firstTerm + secondTerm;
		}

		// �����B
		protected int mDegree = 3;
		public int Degree
        {
			get
            {
				return mDegree;
			}

            set
            {
				mDegree = value;
			}
		}

		// �[�_��ʂ邩�ǂ����B
		protected bool mPassOnEdge = false;
		public virtual bool PassOnEdge
        {
			get
            {
				return mPassOnEdge;
			}
			set
            {
				mPassOnEdge = value;
			}
		}

		// �m�b�g�B
		protected double[] Knots;

		// �_�̐ݒ�B
		public virtual void SetPoints(VectorList points) {
			Points = Closed ? CreatePointsAsClosed(points) : points;
			ResetKnots();
		}

		// ���������_��]�����Ԃ��B
		public virtual void Evaluate(VectorList linePoints)
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

			// 2����������3���ň�l�ł���Ε��������Ē�`
			if (GetType() == typeof(BSplineCurve) && (mDegree == 2 || mDegree == 3) && !PassOnEdge)
            {
				int lineCount = Points.Count - 1;
				int repeatCount = Points.Count - mDegree;

				if (DividedCount % repeatCount != 0)
                {
                    DividedCount = repeatCount * (DividedCount / repeatCount);
                }

                if (DividedCount < 1)
                {
                    DividedCount = repeatCount;
                }

				int eachDividedCount = DividedCount / repeatCount;
				float step = 1f / eachDividedCount;

				for (int i = 0; i < repeatCount; ++i)
                {
					for (int p = 0; p < eachDividedCount; ++p)
                    {
                        CadVector v = mDegree == 2 ?
                            CalcurateUniformBSplinePointWithDegree2(i, p * step) :
                            CalcurateUniformBSplinePointWithDegree3(i, p * step);

                        linePoints.Add(v);
					}

					if (i == repeatCount - 1)
                    {
                        CadVector v = mDegree == 2 ?
                            CalcurateUniformBSplinePointWithDegree2(i, 1f) :
                            CalcurateUniformBSplinePointWithDegree3(i, 1f);

                        linePoints.Add(v);
                    }
                }
			}
			else
            {
				double lowKnot = Knots[mDegree];
                double highKnot = Knots[Points.Count];
                double step = (highKnot - lowKnot) / DividedCount;

				for (int p = 0; p <= DividedCount; ++p) {
					double t = p * step + lowKnot;
					if (t >= highKnot) t = highKnot - BSplineCurve.Epsilon;

					linePoints.Add(CalcuratePoint(t));
				}
			}
		}

		// �w��̈ʒu�̍��W���擾�B
		protected virtual CadVector CalcuratePoint(double t)
        {
            CadVector linePoint = CadVector.Zero;

			for (int i = 0; i < Points.Count; ++i)
            {
				double bs = BSplineCurve.BSplineBasisFunc(i, mDegree, t, Knots);
				linePoint += bs * Points[i];
			}

			return linePoint;
		}

        // 2���ł̈�l��B�X�v���C�����֐��B
        CadVector CalcurateUniformBSplinePointWithDegree2(int i, double t)
        {
			return 0.5f * (t * t - 2f * t + 1f) * Points[i] +
				0.5f * (-2f * t * t + 2f * t + 1f) * Points[i + 1] +
				0.5f * t * t * Points[i + 2];
		}

        // 3���ł̈�l��B�X�v���C�����֐��B
        CadVector CalcurateUniformBSplinePointWithDegree3(int i, double t)
        {
			return 1f/6f * (-Points[i] + 3f * Points[i + 1] - 3f * Points[i + 2] + Points[i + 3]) * t * t * t +
				0.5f * (Points[i] - 2f * Points[i + 1] + Points[i + 2]) * t * t +
				0.5f * (-Points[i] + Points[i + 2]) * t +
				1f/6f * (Points[i] + 4f * Points[i + 1] + Points[i + 2]);
		}

		// �m�b�g�����Z�b�g�B
		protected void ResetKnots()
        {
			if (Points == null)
            {
                Knots = null;
                return;
            }

            Knots = new double[Points.Count + mDegree + 1];

			float knot = 0;

			for (int i = 0; i < Knots.Length; ++i) {
				Knots[i] = knot;
				if (!mPassOnEdge || (i >= mDegree && i <= Knots.Length - mDegree - 2)) {
					++knot;
				}
			}
		}

		// �ŏI�̍��W���n�_�Ɠ����ɂȂ�|�C���g�̔z������B
		protected VectorList CreatePointsAsClosed(VectorList points)
        {
            VectorList tmpPoints = new VectorList(points); 

			for (int i = 0; i < mDegree; ++i) {
				tmpPoints.Add(tmpPoints[i]);
			}

			return tmpPoints;
		}
	}
}