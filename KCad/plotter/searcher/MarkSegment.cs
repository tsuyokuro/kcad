using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CadDataTypes;

namespace Plotter
{

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

        public CadVertex pA
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

        public CadVertex pB
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

        public CadVertex CrossPoint;

        public CadVertex CrossPointScrn;

        public CadVertex CenterPoint
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
