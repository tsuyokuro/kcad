//#define PRINT_WITH_GL_ONLY
//#define PRINT_WITH_GDI_ONLY

using GLUtil;
using System.Drawing;

namespace Plotter.Controller
{
    public static class PlotterPrinter
    {
        public static void PrintPage(PlotterController pc, Graphics printerGraphics, CadSize2D pageSize, CadSize2D deviceSize)
        {
            DOut.pl($"Dev Width:{deviceSize.Width} Height:{deviceSize.Height}");
#if PRINT_WITH_GL_ONLY
            PrintPageGL(printerGraphics, pageSize, deviceSize);
#elif PRINT_WITH_GDI_ONLY
            PrintPageGDI(printerGraphics, pageSize, deviceSize);
#else
            PrintPageSwitch(pc, printerGraphics, pageSize, deviceSize);
#endif
        }

        private static void PrintPageSwitch(PlotterController pc, Graphics printerGraphics, CadSize2D pageSize, CadSize2D deviceSize)
        {
            if (!(pc.CurrentDC.GetType() == typeof(DrawContextGLPers)))
            {
                DrawContextPrinter dc = new DrawContextPrinter(pc.CurrentDC, printerGraphics, pageSize, deviceSize);
                pc.DrawAllFigure(dc);
            }
            else
            {
                Bitmap bmp = GetPrintableBmp(pc, pageSize, deviceSize);
                printerGraphics.DrawImage(bmp, 0, 0);
            }
        }

        private static Bitmap GetPrintableBmp(PlotterController pc, CadSize2D pageSize, CadSize2D deviceSize)
        {
            if (!(pc.CurrentDC is DrawContextGL))
            {
                return null;
            }

            DrawContext dc = pc.CurrentDC.CreatePrinterContext(pageSize, deviceSize);

            dc.SetupTools(DrawTools.ToolsType.PRINTER);

            FrameBufferW fb = new FrameBufferW();
            fb.Create((int)deviceSize.Width, (int)deviceSize.Height);

            fb.Begin();

            dc.StartDraw();

            dc.Drawing.Clear(dc.GetBrush(DrawTools.BRUSH_BACKGROUND));

            pc.DrawAllFigure(dc);

            dc.EndDraw();

            Bitmap bmp = fb.GetBitmap();

            fb.End();
            fb.Dispose();

            return bmp;
        }
    }
}