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

            if (currentDC.GetType() == typeof(DrawContextGLPers))
            {
                WorldScale = currentDC.WorldScale;
                mUnitPerMilli = deviceSize.Width / pageSize.Width;
                CopyCamera(currentDC);
                CopyProjectionMatrix(currentDC);

                DeviceScaleX = currentDC.ViewWidth / 4;
                DeviceScaleY = -(currentDC.ViewHeight / 4);
            }
            else
            {
                WorldScale = currentDC.WorldScale;
                mUnitPerMilli = deviceSize.Width / pageSize.Width;
                CopyProjectionMetrics(currentDC);
                CopyCamera(currentDC);
                SetViewSize(deviceSize.Width, deviceSize.Height);
            }

            CadVector org = default;

            org.x = deviceSize.Width / 2.0;
            org.y = deviceSize.Height / 2.0;
            
            SetViewOrg(org);

            mDrawing = new DrawingGDI(this);
        }

        protected override void DisposeGraphics()
        {
            // NOP
        }

        protected override void CreateGraphics()
        {
            // NOP
        }
    }
}
