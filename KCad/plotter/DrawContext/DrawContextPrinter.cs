using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    class DrawContextPrinter : DrawContextGDI
    {
        public DrawContextPrinter(DrawContext currentDC, Graphics g, PaperPageSize pageSize)
        {
            graphics = g;
            SetupTools(DrawTools.ToolsType.PRINTER);
            PageSize = pageSize;

            // Default printers's unit is 1/100 inch
            SetUnitPerInch(100.0);

            CopyCamera(currentDC);

            if (currentDC is DrawContextGL)
            {
                WoldScale = 0.2;
                UnitPerMilli = 1.0;
                DeviceScaleX = currentDC.ViewWidth / 2.0;
                DeviceScaleY = -currentDC.ViewHeight / 2.0;
            }

            CadVector org = default(CadVector);

            org.x = PageSize.widthInch / 2.0 * 100;
            org.y = PageSize.heightInch / 2.0 * 100;

            ViewOrg = org;
        }

        public override void SetViewSize(double w, double h)
        {
        }
    }
}
