using CadDataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter.Controller
{
    public struct LayerListInfo
    {
        public List<CadLayer> LayerList;
        public uint CurrentID;
    }

    public struct PlotterStateInfo
    {
        public PlotterController.States State;

        public SelectModes SelectMode;

        public CadFigure.Types CreatingFigureType;

        public int CreatingFigurePointCnt;

        public MeasureModes MeasureMode;

        public bool HasSelect;


        public void set(PlotterController pc)
        {
            State = pc.State;
            SelectMode = pc.SelectMode;
            CreatingFigureType = pc.CreatingFigType;
            CreatingFigurePointCnt = 0;

            if (pc.FigureCreator != null)
            {
                CreatingFigurePointCnt = pc.FigureCreator.Figure.PointCount;
            }

            MeasureMode = pc.MeasureMode;

            HasSelect = pc.HasSelect();
        }
    }

    public struct SelectContext
    {
        public DrawContext DC;

        public CadVector CursorScrPt;
        public CadVector CursorWorldPt;
        public CadCursor Cursor;

        public bool PointSelected;
        public MarkPoint MarkPt;

        public bool SegmentSelected;
        public MarkSegment MarkSeg;
    }
}
