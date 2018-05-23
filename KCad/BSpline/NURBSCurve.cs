using CadDataTypes;
using System;

namespace BSpline {
	public class NURBSCurve
    {
        // 閉包したカーブとするか。
        public bool Closed;

        // ラインの分割数。
        public int DividedCount = 100;

        // 任意の点。
        protected VectorList Points = null;

        private int CtrlPointCount = 0;


        // 次数。
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

        // 端点を通るかどうか。
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

        // ノット。
        protected double[] Knots;

        // 重み
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

        // 重みも考慮し、指定の位置の座標を取得。
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

        // Knotの値を変えたら呼び出す
        public void RecalcSteppingParam()
        {
            mLowKnot = Knots[mDegree];
            mHighKnot = Knots[CtrlPointCount];
            mStep = (mHighKnot - mLowKnot) / (double)DividedCount;
        }

        // 線を引く点を評価し返す。
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

            // 分割数のチェック
            if (DividedCount < 1)
            {
                DividedCount = 1;
            }

            // 1次の場合は直線で結ぶ
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


        // ノットをリセット。
        protected void ResetKnots()
        {
            Knots = new double[KnotCount];

            double knot = 0;

            for (int i = 0; i < Knots.Length; ++i)
            {
                // 端点を通る様にするには、両端の (次数+1) のKnotを同じ値にする
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

        // 重みのリセット。
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