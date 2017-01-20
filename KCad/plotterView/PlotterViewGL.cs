using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Plotter
{
    class PlotterViewGL : GLControl
    {
        public static PlotterViewGL Create()
        {
            GraphicsMode mode = GraphicsMode.Default;
            return Create(mode);
        }

        public static PlotterViewGL Create(GraphicsMode mode)
        {
            PlotterViewGL v = new PlotterViewGL(mode);
            return v;
        }

        private PlotterViewGL(GraphicsMode mode) : base(mode)
        {
            Load += onLoad;
            Resize += onResize;
            Paint += onPaint;
            MouseMove += onMouseMove;
        }

        private void onMouseMove(object sender, MouseEventArgs e)
        {
        }

        private void onPaint(object sender, PaintEventArgs e)
        {
        }

        private void onResize(object sender, EventArgs e)
        {
        }

        private void onLoad(object sender, EventArgs e)
        {
        }
    }
}
