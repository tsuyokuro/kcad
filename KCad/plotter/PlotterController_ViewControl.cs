using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public partial class PlotterController
    {
        public void setOrigin(DrawContext dc, int pixX, int pixY)
        {
            CadPixelPoint op = default(CadPixelPoint);
            op.set(pixX, pixY);

            dc.ViewOrg = op;

            clear(dc);
            draw(dc);
        }

        public void moveOrigin(DrawContext dc, int pixDx, int pixDy)
        {
            CadPixelPoint op = dc.ViewOrg;

            op.add(pixDx, pixDy, 0);

            dc.ViewOrg = op;

            clear(dc);
            draw(dc);
        }

        public void adjustOrigin(DrawContext dc, int pixX, int pixY, int vw, int vh)
        {
            int dx = vw / 2 - pixX;
            int dy = vh / 2 - pixY;

            clear(dc);
            moveOrigin(dc, dx, dy);
        }

        public void dpiUpDown(DrawContext dc, double f)
        {
            CadPixelPoint op = dc.ViewOrg;

            CadPixelPoint center = new CadPixelPoint(dc.ViewWidth / 2, dc.ViewHeight / 2);

            CadPixelPoint d = center - op;

            d *= f;

            op = center - d;


            dc.ViewOrg = op;

            dc.UnitPerMilli *= f;

            clear(dc);
            draw(dc);
        }
    }
}
