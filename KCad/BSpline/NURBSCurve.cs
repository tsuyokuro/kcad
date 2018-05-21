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
        protected VectorList Points;

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

        // ノット。
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

		// 重みも考慮し、指定の位置の座標を取得。
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

		// 重みのリセット。
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

        // 線を引く点を評価し返す。
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

        // ノットをリセット。
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

        // 最終の座標が始点と同じになるポイントの配列を作る。
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