using CadDataTypes;

namespace Plotter
{
    public class HighlightPointListItem
    {
        public CadVertex Point;
        public DrawPen Pen;

        public HighlightPointListItem(CadVertex p, DrawPen pen)
        {
            Point = p;
            Pen = pen;
        }
    }
}
