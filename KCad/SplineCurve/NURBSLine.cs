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

        // 制御点リスト
        public VectorList CtrlPoints = null;

        // Control point の見かけ上の数
        // Closeの場合は、実際の数 + Degreeに設定する
        public int CtrlCnt = 0;

        // Control pointのリスト上での数
        public int CtrlDataCnt;

        public int[] Order;


        // 出力されるPointの個数
        public int OutCnt
        {
            get
            {
                return BSplineP.OutputCnt;
            }
        }

        public BSplineParam BSplineP = new BSplineParam();

        public double[] Weights;

        public NURBSLine()
        {
        }

        public NURBSLine(
            int deg,
            int ctrlCnt,
            int divCnt,
            bool edge,
            bool close)
        {
            Setup(deg, ctrlCnt, divCnt, edge, close);
        }

        public void Setup(
            int deg,
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

            Order = new int[CtrlCnt];

            for (int i=0; i< CtrlCnt; i++)
            {
                Order[i] = i % ctrlCnt;
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

                di = Order[i];

                linePoint += bs * Weights[di] * CtrlPoints[di];

                weight += bs * Weights[di];
			}

            return linePoint / weight;
		}

        public void Eval(VectorList vl)
        {
            for (int p = 0; p <= BSplineP.DivCnt; ++p)
            {
                double t = p * BSplineP.Step + BSplineP.LowKnot;
                if (t >= BSplineP.HighKnot)
                {
                    t = BSplineP.HighKnot - BSpline.Epsilon;
                }

                vl.Add( CalcPoint(t) );
            }
        }

        public void ResetWeights()
        {
            Weights = new double[CtrlDataCnt];

            for (int i = 0; i < Weights.Length; ++i)
            {
                Weights[i] = 1f;
            }
        }

        public double GetWeight(int u, int v)
        {
            return Weights[v * CtrlDataCnt + u];
        }

        public void SetWeight(int u, int v, double val)
        {
            Weights[v * CtrlDataCnt + u] = val;
        }
    }
}