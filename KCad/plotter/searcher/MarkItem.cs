using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public struct MarkPoint
    {
        public bool IsValid;

        public CadLayer Layer;

        public uint LayerID
        {
            get
            {
                if (Layer == null)
                {
                    return 0;
                }

                return Layer.ID;
            }
        }

        public CadFigure Figure;

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

        public int PointIndex;

        public CadVector Point;

        public CadVector ViewPoint;

        public double DistanceX;
        public double DistanceY;

        public void reset()
        {
            this = default(MarkPoint);

            IsValid = false;

            Figure = null;

            DistanceX = CadConst.MaxValue;
            DistanceY = CadConst.MaxValue;
        }

        public void dump(DebugOut dout)
        {
            dout.println("MarkPoint {");
            dout.println("FigureID:" + FigureID.ToString());
            dout.println("PointIndex:" + PointIndex.ToString());
            dout.println("}");
        }
    }

    
    /// <summary>
    /// 
    /// </summary>
    public struct MarkSeg
    {
        public CadLayer Layer;

        public uint LayerID
        {
            get
            {
                if (Layer == null)
                {
                    return 0;
                }

                return Layer.ID;
            }
        }


        public CadFigure Figure;

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

        public int PtIndexA;
        public CadVector pA;

        public int PtIndexB;
        public CadVector pB;

        public CadVector CrossPoint;

        public CadVector CrossPointScrn;

        public CadVector CenterPoint
        {
            get
            {
                CadVector t = pB - pA;
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
