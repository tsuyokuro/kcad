//#define PRINT_WITH_GL_ONLY
//#define PRINT_WITH_GDI_ONLY

using GLUtil;
using System.Drawing;

namespace Plotter.Controller
{
    public partial class PlotterController
    {
        public void PrintPage(Graphics printerGraphics, CadSize2D pageSize, CadSize2D deviceSize)
        {
            DOut.pl($"Dev Width:{deviceSize.Width} Height:{deviceSize.Height}");
#if PRINT_WITH_GL_ONLY
            PrintPageGL(printerGraphics, pageSize, deviceSize);
#elif PRINT_WITH_GDI_ONLY
            PrintPageGDI(printerGraphics, pageSize, deviceSize);
#else
            PrintPageSwitch(printerGraphics, pageSize, deviceSize);
#endif
        }


        private void PrintPageSwitch(Graphics printerGraphics, CadSize2D pageSize, CadSize2D deviceSize)
        {
            if (!(CurrentDC.GetType() == typeof(DrawContextGLPers)))
            {
                DrawContextPrinter dc = new DrawContextPrinter(CurrentDC, printerGraphics, pageSize, deviceSize);
                DrawAllFigure(dc);
            }
            else
            {
                Bitmap bmp = GetPrintableBmp(pageSize, deviceSize);
                printerGraphics.DrawImage(bmp, 0, 0);
            }
        }

        private Bitmap GetPrintableBmp(CadSize2D pageSize, CadSize2D deviceSize)
        {
            if (!(CurrentDC is DrawContextGL))
            {
                return null;
            }

            DrawContext dc = CurrentDC.CreatePrinterContext(pageSize, deviceSize);

            dc.SetupTools(DrawTools.ToolsType.PRINTER_GL);

            FrameBufferW fb = new FrameBufferW();
            fb.Create((int)deviceSize.Width, (int)deviceSize.Height);

            fb.Begin();

            dc.StartDraw();

            dc.Drawing.Clear(dc.GetBrush(DrawTools.BRUSH_BACKGROUND));

            DrawAllFigure(dc);

            dc.EndDraw();

            Bitmap bmp = fb.GetBitmap();

            fb.End();
            fb.Dispose();

            return bmp;
        }
    }
}