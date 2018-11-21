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

        public bool IsSelected()
        {
            if (Figure == null)
            {
                return false;
            }

            return Figure.IsPointSelected(PointIndex);
        }

        public ulong Hash
        {
            get
            {
                return FigureID << 32 + PointIndex;
            }
        }

        public bool update()
        {
            if (Figure == null)
            {
                return true;
            }

            if (PointIndex >= Figure.PointList.Count)
            {
                return false;
            }

            return true;
        }
    }

    public struct MarkSegment
    {
        public FigureSegment FigSeg;

        public CadFigure Figure
        {
            get
            {
                return FigSeg.Figure;
            }
        }

        public uint FigureID
        {
            get
            {
                return FigSeg.FigureID;
            }
        }

        public int PtIndexA
        {
            get
            {
                return FigSeg.Index0;
            }
        }

        public CadVector pA
        {
            get
            {
                return FigSeg.Point0;
            }
        }

        public int PtIndexB
        {
            get
            {
                return FigSeg.Index1;
            }
        }

        public CadVector pB
        {
            get
            {
                return FigSeg.Point1;
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
                return CadUtil.CenterPoint(FigSeg.Point0, FigSeg.Point1);
            }
        }

        public double Distance;

        public bool Valid { get { return FigureID != 0; } }

        public void dump(string name= "MarkSeg")
        {
            DOut.pl(name + " {");
            DOut.Indent++;
            FigSeg.dump("FSegment");
            DOut.Indent--;
            DOut.pl("}");
        }

        public bool Update()
        {
            if (FigSeg.Figure == null)
            {
                return true;
            }

            if (PtIndexA >= FigSeg.Figure.PointList.Count)
            {
                return false;
            }

            if (PtIndexB >= FigSeg.Figure.PointList.Count)
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

        public bool IsSelected()
        {
            if (Figure == null)
            {
                return false;
            }

            return Figure.IsPointSelected(PtIndexA) && Figure.IsPointSelected(PtIndexB);
        }
    }
}
