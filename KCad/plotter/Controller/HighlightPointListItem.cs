using CadDataTypes;

namespace Plotter
{
    public class HighlightPointListItem
    {
        public CadVector Point;
        public int Pen;

        public HighlightPointListItem(CadVector p, int pen = DrawTools.PEN_POINT_HIGHLIGHT)
        {
            Point = p;
            Pen = pen;
        }
    }
}
