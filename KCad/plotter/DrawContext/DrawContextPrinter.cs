using System.Drawing;
using CadDataTypes;

namespace Plotter
{
    class DrawContextPrinter : DrawContextGDI
    {
        public DrawContextPrinter(DrawContext currentDC, Graphics g, CadSize2D pageSize, CadSize2D deviceSize)
        {
            GdiGraphics = g;
            SetupTools(DrawTools.ToolsType.PRINTER);

            WorldScale = currentDC.WorldScale;

            mUnitPerMilli = deviceSize.Width / pageSize.Width;

            if (currentDC.GetType() == typeof(DrawContextGLPers))
            {
                CopyCamera(currentDC);
                DeviceScaleX = currentDC.ViewWidth / 4;
                DeviceScaleY = -(currentDC.ViewHeight / 4);
            }
            else
            {
                SetCamera(currentDC.Eye, currentDC.LookAt, currentDC.UpVector);
                CalcProjectionMatrix();
            }

            CadVector org = default;

            org.x = deviceSize.Width / 2.0;
            org.y = deviceSize.Height / 2.0;
            
            SetViewOrg(org);

            mDrawing = new DrawingGDI(this);
        }
    }
}
