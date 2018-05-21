using CadDataTypes;
using System;

namespace BSpline {
	public class NURBSCurve
    {
        // ����J�[�u�Ƃ��邩�B
        public bool Closed;

        // ���C���̕������B
        public int DividedCount = 100;

        // �C�ӂ̓_�B
        protected VectorList Points;

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


        double[] Weights;

		public void SetPoints(VectorList points)
        {
            Points = points;

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
		protected CadVector CalcuratePoint(double t)
        {
            CadVector linePoint = CadVector.Zero;
			double weight = 0f;

			for (int i = 0; i < Points.Count; ++i)
            {
				double bs = BSpline.BSplineBasisFunc(i, mDegree, t, Knots);

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
        public void Evaluate(VectorList linePoints)
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
                if (t >= highKnot)
                {
                    t = highKnot - BSpline.Epsilon;
                }

                linePoints.Add( CalcuratePoint(t) );
            }
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

            for (int i = 0; i < Knots.Length; ++i)
            {
                Knots[i] = knot;
                if (!mPassOnEdge || (i >= mDegree && i <= Knots.Length - mDegree - 2))
                {
                    ++knot;
                }
            }
        }

        // �ŏI�̍��W���n�_�Ɠ����ɂȂ�|�C���g�̔z������B
        protected VectorList CreatePointsAsClosed(VectorList points)
        {
            VectorList tmpPoints = new VectorList(points);

            for (int i = 0; i < mDegree; ++i)
            {
                tmpPoints.Add(tmpPoints[i]);
            }

            return tmpPoints;
        }
    }
}