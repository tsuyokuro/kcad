using CadDataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public class HighlightPointListItem
    {
        public CadVector Point;
        public int Pen;

        public HighlightPointListItem(CadVector p, int pen = DrawTools.PEN_POINT_HIGHLITE)
        {
            Point = p;
            Pen = pen;
        }
    }
}
