using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public partial class PlotterController
    {
        public void SetOrigin(DrawContext dc, int pixX, int pixY)
        {
            CadPoint op = default(CadPoint);
            op.set(pixX, pixY, 0);

            dc.ViewOrg = op;

            Clear(dc);
            Draw(dc);
        }

        public void MoveOrigin(DrawContext dc, int pixDx, int pixDy)
        {
            CadPoint d = CadPoint.Create(pixDx, pixDy, 0);

            dc.ViewOrg += d;

            Clear(dc);
            Draw(dc);
        }

        public void AdjustOrigin(DrawContext dc, int pixX, int pixY, int vw, int vh)
        {
            int dx = vw / 2 - pixX;
            int dy = vh / 2 - pixY;

            Clear(dc);
            MoveOrigin(dc, dx, dy);
        }

        public void DpiUpDown(DrawContext dc, double f)
        {
            CadPoint op = dc.ViewOrg;

            CadPoint center = default(CadPoint); 
                
            center.set(dc.ViewWidth / 2, dc.ViewHeight / 2, 0);

            CadPoint d = center - op;

            d *= f;

            op = center - d;


            dc.ViewOrg = op;

            dc.UnitPerMilli *= f;

            Clear(dc);
            Draw(dc);
        }
    }
}
