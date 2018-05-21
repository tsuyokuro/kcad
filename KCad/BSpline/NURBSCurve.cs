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
        protected VectorList Points = null;

        private int PointCount = 0;


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

            PointCount = Points.Count;

            if (Closed)
            {
                PointCount += mDegree;
            }

			if (Knots == null || PointCount + mDegree + 1 != Knots.Length)
            {
                ResetKnots(PointCount + mDegree + 1);
            }

            if (Weights == null || Weights.Length != PointCount)
            {
                ResetWeights(PointCount);
            }
        }

		// �d�݂��l�����A�w��̈ʒu�̍��W���擾�B
		protected CadVector CalcuratePoint(double t)
        {
            CadVector linePoint = CadVector.Zero;
			double weight = 0f;

			for (int i = 0; i < PointCount; ++i)
            {
				double bs = BSpline.BSplineBasisFunc(i, mDegree, t, Knots);

                linePoint += bs * Weights[i] * Points[i % Points.Count];

                weight += bs * Weights[i];
			}

			return linePoint / weight;
		}

        // ���������_��]�����Ԃ��B
        public void Evaluate(VectorList linePoints)
        {
            if (mDegree < 1)
            {
                return;
            }

            if (Points == null || PointCount < 2 || PointCount < mDegree + 1)
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
            double highKnot = Knots[PointCount];
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
        protected void ResetKnots(int cnt)
        {
            Knots = new double[cnt];

            double knot = 0;

            for (int i = 0; i < Knots.Length; ++i)
            {
                Knots[i] = knot;
                if (!mPassOnEdge || (i >= mDegree && i <= Knots.Length - mDegree - 2))
                {
                    ++knot;
                }
            }
        }

        // �d�݂̃��Z�b�g�B
        void ResetWeights(int cnt)
        {
            Weights = new double[cnt];

            for (int i = 0; i < Weights.Length; ++i)
            {
                Weights[i] = 1f;
            }
        }
    }
}