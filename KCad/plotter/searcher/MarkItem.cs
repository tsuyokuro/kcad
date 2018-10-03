using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadDataTypes;

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

        public CadVector Point;     // Match座標 (World座標系)

        public CadVector PointScrn; // Match座標 (Screen座標系)

        public double DistanceX;    // X距離 (Screen座標系)
        public double DistanceY;    // Y距離 (Screen座標系)

        public void reset()
        {
            this = default(MarkPoint);

            IsValid = false;

            Figure = null;

            DistanceX = CadConst.MaxValue;
            DistanceY = CadConst.MaxValue;
        }
    }

    public struct MarkSeg
    {
        public FigureSegment FSegment;

        public CadFigure Figure
        {
            get
            {
                return FSegment.Figure;
            }
        }

        public uint FigureID
        {
            get
            {
                return FSegment.FigureID;
            }
        }

        public int PtIndexA
        {
            get
            {
                return FSegment.Index0;
            }
        }

        public CadVector pA
        {
            get
            {
                return FSegment.Point0;
            }
        }

        public int PtIndexB
        {
            get
            {
                return FSegment.Index1;
            }
        }

        public CadVector pB
        {
            get
            {
                return FSegment.Point1;
            }
        }


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

        public CadVector CrossPoint;

        public CadVector CrossPointScrn;

        public CadVector CenterPoint
        {
            get
            {
                return CadUtil.CenterPoint(FSegment.Point0, FSegment.Point1);
            }
        }

        public double Distance;

        public bool Valid { get { return FigureID != 0; } }

        public void dump(string name= "MarkSeg")
        {
            DbgOut.pln(name + " {");
            DbgOut.Indent++;
            FSegment.dump("FSegment");
            DbgOut.Indent--;
            DbgOut.pln("}");
        }

        public bool Update()
        {
            if (FSegment.Figure == null)
            {
                return true;
            }

            if (PtIndexA >= FSegment.Figure.PointList.Count)
            {
                return false;
            }

            if (PtIndexB >= FSegment.Figure.PointList.Count)
            {
                return false;
            }

            return true;
        }

        public void Clean()
        {
            CrossPoint.Valid = false;
            CrossPointScrn.Valid = false;
        }
    }
}
