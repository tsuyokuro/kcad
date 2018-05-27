using CadDataTypes;
using Plotter;
using System;

namespace BSpline {
	public class NURBSCurve
    {
        // 閉包したカーブとするか。
        public bool Closed;

        // ラインの分割数。
        public int DividedCount = 32;

        // 制御店リスト
        protected VectorList CtrlPoints = null;

        // 制御点の個数
        // Closedの場合は、 + 次数
        private int CtrlPointCount = 0;

        private VectorList mLinePoints;


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

        public int PointCount
        {
            get
            {
                return DividedCount + 1;
            }
        }



        // ノット。
        protected double[] Knots;

        // 重み
        double[] Weights;

        
        public double mLowKnot = 0;
        public double mHighKnot = 0;
        public double mStep = 0;
        

		public void SetCotrolPoints(VectorList points)
        {
            CtrlPoints = points;

            CtrlPointCount = CtrlPoints.Count;

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

            //DebugOut.println("{");


            for (int i = 0; i < CtrlPointCount; ++i)
            {
				double bs = BSpline.BasisFunc(i, mDegree, t, Knots);

                //DebugOut.println("bs:" + bs.ToString());

                linePoint += bs * Weights[i] * CtrlPoints[i % CtrlPoints.Count];

                weight += bs * Weights[i];
			}

            //DebugOut.println("}");

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
        public VectorList Evaluate()
        {
            if (CtrlPoints == null || CtrlPointCount < 2 || CtrlPointCount < mDegree + 1)
            {
                return null;
            }

            if (mLinePoints == null)
            {
                mLinePoints = new VectorList(PointCount);
            }
            else
            {
                // 内部配列の再利用
                mLinePoints.Clear();
            }

            // 分割数のチェック
            if (DividedCount < 1)
            {
                DividedCount = 1;
            }

            // 1次の場合は直線で結ぶ
            if (mDegree == 1)
            {
                mLinePoints.AddRange(CtrlPoints);
                return mLinePoints;
            }

            for (int p = 0; p <= DividedCount; ++p)
            {
                double t = p * mStep + mLowKnot;
                if (t >= mHighKnot)
                {
                    t = mHighKnot - BSpline.Epsilon;
                }

                mLinePoints.Add( CalcuratePoint(t) );
            }

            return mLinePoints;
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