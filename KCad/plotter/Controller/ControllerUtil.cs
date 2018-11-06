using CadDataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
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
