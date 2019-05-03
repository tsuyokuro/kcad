using CadDataTypes;

namespace Plotter
{
    public class HighlightPointListItem
    {
        public CadVector Point;
        public DrawPen Pen;

        public HighlightPointListItem(CadVector p, DrawPen pen)
        {
            Point = p;
            Pen = pen;
        }
    }
}
