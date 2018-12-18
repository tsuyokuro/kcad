using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadDataTypes;

namespace Plotter
{
    class DrawContextPrinter : DrawContextGDI
    {
        public DrawContextPrinter(DrawContext currentDC, Graphics g, CadSize2D pageSize, CadSize2D deviceSize)
        {
            GdiGraphics = g;
            SetupTools(DrawTools.ToolsType.PRINTER);

            UnitPerMilli = deviceSize.Width / pageSize.Width;

            CopyCamera(currentDC);

            if (currentDC is DrawContextGL)
            {
                DeviceScaleX = currentDC.DeviceScaleX;
                DeviceScaleY = currentDC.DeviceScaleY;
            }

            CadVector org = default(CadVector);

            org.x = deviceSize.Width / 2.0;
            org.y = deviceSize.Height / 2.0;

            ViewOrg = org;
        }
    }
}
