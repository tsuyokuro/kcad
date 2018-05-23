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

        private int CtrlPointCount = 0;


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
        public bool PassOnEdge
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
        
        public int KnotCount
        {
            get
            {
                return CtrlPointCount + mDegree + 1;
            }
        }

        // �m�b�g�B
        protected double[] Knots;

        // �d��
        double[] Weights;

        
        public double mLowKnot = 0;
        public double mHighKnot = 0;
        public double mStep = 0;
        

		public void SetPoints(VectorList points)
        {
            Points = points;

            CtrlPointCount = Points.Count;

            if (Closed)
            {
                CtrlPointCount += mDegree;
            }

			if (Knots == null || Knots.Length != KnotCount)
            {
                ResetKnots();
            }

            if (Weights == null || Weights.Length != CtrlPointCount)
            {
                ResetWeights(CtrlPointCount);
            }
        }

        // �d�݂��l�����A�w��̈ʒu�̍��W���擾�B
        protected CadVector CalcuratePoint(double t)
        {
            CadVector linePoint = CadVector.Zero;
			double weight = 0f;

			for (int i = 0; i < CtrlPointCount; ++i)
            {
				double bs = BSpline.BSplineBasisFunc(i, mDegree, t, Knots);

                linePoint += bs * Weights[i] * Points[i % Points.Count];

                weight += bs * Weights[i];
			}

			return linePoint / weight;
		}

        // Knot�̒l��ς�����Ăяo��
        public void RecalcSteppingParam()
        {
            mLowKnot = Knots[mDegree];
            mHighKnot = Knots[CtrlPointCount];
            mStep = (mHighKnot - mLowKnot) / (double)DividedCount;
        }

        // ���������_��]�����Ԃ��B
        public void Evaluate(VectorList linePoints)
        {
            if (mDegree < 1)
            {
                return;
            }

            if (Points == null || CtrlPointCount < 2 || CtrlPointCount < mDegree + 1)
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

            //double lowKnot = Knots[mDegree];
            //double highKnot = Knots[PointCount];
            //double step = (highKnot - lowKnot) / DividedCount;

            for (int p = 0; p <= DividedCount; ++p)
            {
                double t = p * mStep + mLowKnot;
                if (t >= mHighKnot)
                {
                    t = mHighKnot - BSpline.Epsilon;
                }

                linePoints.Add( CalcuratePoint(t) );
            }
        }

        public int GetPointCount()
        {
            return DividedCount + 1;
        }

        public CadVector GetPoint(int i)
        {
            double t = i * mStep + mLowKnot;
            if (t >= mHighKnot)
            {
                t = mHighKnot - BSpline.Epsilon;
            }

            return CalcuratePoint(t);
        }


        // �m�b�g�����Z�b�g�B
        protected void ResetKnots()
        {
            Knots = new double[KnotCount];

            double knot = 0;

            for (int i = 0; i < Knots.Length; ++i)
            {
                // �[�_��ʂ�l�ɂ���ɂ́A���[�� (����+1) ��Knot�𓯂��l�ɂ���
                if (mPassOnEdge && (i < mDegree || i > (KnotCount - mDegree - 2)))
                {
                    Knots[i] = knot;
                }
                else
                {
                    Knots[i] = knot;
                    knot += 1.0;
                }
            }

            RecalcSteppingParam();
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