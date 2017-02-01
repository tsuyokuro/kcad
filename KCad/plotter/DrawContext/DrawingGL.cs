using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    class DrawingGL : DrawingX
    {
        private DrawContextGL DC;

        public DrawingGL(DrawContextGL dc)
        {
            DC = dc;
        }
    }
}
