using CadDataTypes;
using Plotter;
using System;

namespace BSpline {

    public class NURBSSerface
    {
        public int UDivCnt = 12;
        public int VDivCnt = 12;

        public int UCtrlCnt = 5;
        public int VCtrlCnt = 5;

        public int UDegree = 3;
        public int VDegree = 3;

        public VectorList CtrlPoints = null;

        public int UPointCnt;

        public int VPointCnt;

        public int UKnotCnt;

        public int VKnotCnt;

        public int UWeightCnt;

        public int VWeightCnt;

        private double[] UKnots;
        private double[] VKnots;

        private double[,] Weights;

        public double ULowKnot = 0;
        public double VLowKnot = 0;

        public double UHighKnot = 0;
        public double VHighKnot = 0;

        public double UStep = 0;
        public double VStep = 0;

        public NURBSSerface(int deg, int uCtrlCnt, int vCtrlCnt, int uDivCnt, int vDivCnt)
        {
            UDegree = deg;
            VDegree = deg;

            UCtrlCnt = uCtrlCnt;
            VCtrlCnt = vCtrlCnt;

            UDivCnt = uDivCnt;
            VDivCnt = vDivCnt;

            UPointCnt = UDivCnt + 1;
            VPointCnt = VDivCnt + 1;

            UKnotCnt = UCtrlCnt + UDegree + 1;
            VKnotCnt = VCtrlCnt + VDegree + 1;

            UWeightCnt = UCtrlCnt;
            VWeightCnt = VCtrlCnt;

            ResetKnots();
            ResetWeights();

            RecalcSteppingParam();
        }

        bool UPassOnEdge = true;
        bool VPassOnEdge = true;

        public void ResetKnots()
        {
            UKnots = new double[UKnotCnt];
            VKnots = new double[VKnotCnt];

            double x = 0.0;

            for (int i = 0; i < UKnotCnt; i++)
            {
                if (UPassOnEdge && (i < UDegree || i > (UKnotCnt - UDegree - 2)))
                {
                    UKnots[i] = x;
                }
                else
                {
                    UKnots[i] = x;
                    x += 1.0;
                }
            }

            x = 0;

            for (int i = 0; i < VKnotCnt; i++)
            {
                if (VPassOnEdge && (i < VDegree || i > (VKnotCnt - VDegree - 2)))
                {
                    VKnots[i] = x;
                }
                else
                {
                    VKnots[i] = x;
                    x += 1.0;
                }
            }
        }

        public void ResetWeights()
        {
            Weights = new double[UWeightCnt, VWeightCnt];

            for (int j=0; j<VWeightCnt; j++)
            {
                for (int i = 0; i < VWeightCnt; i++)
                {
                    Weights[i, j] = 1.0;
                }
            }
        }

        public void RecalcSteppingParam()
        {
            ULowKnot = UKnots[UDegree];
            UHighKnot = UKnots[UCtrlCnt];
            UStep = (UHighKnot - ULowKnot) / (double)UDivCnt;

            VLowKnot = VKnots[VDegree];
            VHighKnot = VKnots[VCtrlCnt];
            VStep = (VHighKnot - VLowKnot) / (double)VDivCnt;
        }

        public CadVector CalcuratePoint(double u, double v)
        {
            CadVector pt = CadVector.Zero;

            double weight = 0f;

            for (int j = 0; j < VCtrlCnt; ++j)
            {
                for (int i=0; i< UCtrlCnt; ++i)
                {
                    double ubs = BSpline.BasisFunc(i, UDegree, u, UKnots);
                    double vbs = BSpline.BasisFunc(j, VDegree, v, VKnots);

                    int cp = j * UCtrlCnt + i;

                    pt += (ubs * vbs * Weights[i, j]) * CtrlPoints[cp];

                    weight += ubs * vbs * Weights[i, j];
                }
            }

            return pt / weight;
        }

        public void Eval(VectorList vl)
        {
            double u;
            double v;

            for (int j = 0; j <= VDivCnt; ++j)
            {
                v = j * VStep + VLowKnot;
                if (v >= VHighKnot)
                {
                    v = VHighKnot - BSpline.Epsilon;
                }

                for (int i=0; i <= UDivCnt; ++i)
                {
                    u = i * UStep + ULowKnot;
                    if (u >= UHighKnot)
                    {
                        u = UHighKnot - BSpline.Epsilon;
                    }

                    vl.Add(CalcuratePoint(u, v));
                }
            }
        }
    }

    public class NURBSCurve
    {
        // 閉包したカーブとするか。
        public bool Closed;

        // ラインの分割数。
        public int DividedCount = 24;

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
                ResetWeights();
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
        void ResetWeights()
        {
            Weights = new double[CtrlPointCount];

            for (int i = 0; i < Weights.Length; ++i)
            {
                Weights[i] = 1f;
            }
        }
    }
}