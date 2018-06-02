using CadDataTypes;
using Plotter;
using System;

namespace SplineCurve
{
    public class NURBSLine
    {
        public bool Closed;

        // 端点を通る
        public bool PassEdge;

        // 分割数。
        public int DividedCount = 24;

        // 制御点リスト
        public VectorList CtrlPoints = null;

        // Control point の見かけ上の数
        // Closeの場合は、実際の数 + Degreeに設定する
        private int CtrlCnt = 0;

        // Control pointのリスト上での数
        private int CtrlDataCnt;

        // 出力されるPointの個数
        public int OutCnt
        {
            get
            {
                return DividedCount + 1;
            }
        }

        BSplineParam BSplineP = new BSplineParam();

        public double[] Weights;

        public NURBSLine(int deg,
            int ctrlCnt,
            int divCnt,
            bool edge,
            bool close)
        {
            Closed = close;

            PassEdge = edge;

            CtrlDataCnt = ctrlCnt;

            CtrlCnt = ctrlCnt;
            if (Closed)
            {
                CtrlCnt += deg;
            }

            BSplineP.Setup(deg, CtrlCnt, divCnt, edge);

            ResetWeights();
        }

        private CadVector CalcPoint(double t)
        {
            CadVector linePoint = CadVector.Zero;
			double weight = 0f;

            double bs;

            int i;

            int di;

            for (i = 0; i < CtrlCnt; ++i)
            {
				bs = BSplineP.BasisFunc(i, t);

                di = i % CtrlDataCnt;

                linePoint += bs * Weights[di] * CtrlPoints[di];

                weight += bs * Weights[di];
			}

            return linePoint / weight;
		}

        // 線を引く点を評価し返す。
        public void Eval(VectorList vl)
        {
            for (int p = 0; p <= DividedCount; ++p)
            {
                double t = p * BSplineP.Step + BSplineP.LowKnot;
                if (t >= BSplineP.HighKnot)
                {
                    t = BSplineP.HighKnot - BSpline.Epsilon;
                }

                vl.Add( CalcPoint(t) );
            }
        }

        // 重みのリセット。
        public void ResetWeights()
        {
            Weights = new double[CtrlDataCnt];

            for (int i = 0; i < Weights.Length; ++i)
            {
                Weights[i] = 1f;
            }
        }
    }
}