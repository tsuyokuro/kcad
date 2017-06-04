using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public struct MarkSeg
    {
        public uint LayerID;
        public uint FigureID
        {
            get
            {
                if (Figure == null)
                {
                    return 0;
                }

                return Figure.ID;
            }
        }

        public CadFigure Figure;

        public int PtIndexA;
        public CadPoint pA;

        public int PtIndexB;
        public CadPoint pB;

        public CadPoint CrossPoint;

        public CadPoint CrossViewPoint;

        public CadPoint CenterPoint
        {
            get
            {
                CadPoint t = pB - pA;
                t /= 2;
                t += pA;
                return t;
            }
        }

        public double Distance;

        public bool Valid { get { return FigureID != 0; } }

        public void dump(DebugOut dout)
        {
            dout.println("MarkSeg {");
            dout.Indent++;
            dout.println("FigureID:" + FigureID.ToString());
            dout.println("PtIndexA:" + PtIndexA.ToString());
            dout.println("PtIndexB:" + PtIndexB.ToString());
            dout.Indent--;
            dout.println("}");
        }

        public bool Update()
        {
            if (Figure == null)
            {
                return true;
            }

            if (PtIndexA >= Figure.PointList.Count)
            {
                return false;
            }

            if (PtIndexB >= Figure.PointList.Count)
            {
                return false;
            }


            pA = Figure.PointList[PtIndexA];
            pB = Figure.PointList[PtIndexB];

            return true;
        }
    }
}
