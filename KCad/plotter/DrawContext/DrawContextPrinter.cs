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
        public DrawContextPrinter(DrawContext currentDC, Graphics g, PaperPageSize pageSize, CadSize2D unitSize)
        {
            graphics = g;
            SetupTools(DrawTools.ToolsType.PRINTER);
            PageSize = pageSize;

            UnitPerMilli = unitSize.Width / PageSize.Width;

            CopyCamera(currentDC);

            if (currentDC is DrawContextGL)
            {
                WoldScale = 0.2;
                UnitPerMilli = 1.0;
                DeviceScaleX = currentDC.ViewWidth / 2.0;
                DeviceScaleY = -currentDC.ViewHeight / 2.0;
            }

            CadVector org = default(CadVector);

            org.x = unitSize.Width / 2.0;
            org.y = unitSize.Height / 2.0;

            ViewOrg = org;
        }

        //public override void SetViewSize(double w, double h)
        //{
        //}
    }
}
