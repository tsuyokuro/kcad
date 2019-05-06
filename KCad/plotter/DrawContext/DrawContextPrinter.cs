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

        public override void SetViewSize(double w, double h)
        {
            mViewWidth = w;
            mViewHeight = h;

            if (w == 0 || h == 0)
            {
                return;
            }

            DeviceScaleX = w / 2.0;
            DeviceScaleY = -h / 2.0;

            CalcProjectionMatrix();
            CalcProjectionZW();
        }
    }
}
