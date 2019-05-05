
using OpenTK.Graphics;
using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Resources;

namespace Plotter
{
    /*
    public class GLPen
    {
        public Color4 Color;
        public float Width;

        public GLPen(Color4 color, float t)
        {
            Color = color;
            Width = t;
        }
    }

    public class GLBrush
    {
        public Color4 Color;

        public GLBrush(Color4 color)
        {
            Color = color;
        }
    }
    */
    public class DrawTools : IDisposable
    {
        public const int PEN_DEFAULT = 1;
        public const int PEN_DEFAULT_FIGURE = 2;

        public const int PEN_SELECT_POINT = 3;
        public const int PEN_CURSOR = 4;
        public const int PEN_CURSOR2 = 5;
        public const int PEN_TEMP_FIGURE = 6;
        public const int PEN_POINT_HIGHLIGHT = 7;
        public const int PEN_MATCH_FIGURE = 8;
        public const int PEN_MATCH_SEG = 9;
        public const int PEN_LAST_POINT_MARKER = 10;
        public const int PEN_LAST_POINT_MARKER2 = 11;
        public const int PEN_AXIS = 12;
        public const int PEN_ARROW_AXIS = 13;
        public const int PEN_PAGE_FRAME = 14;
        public const int PEN_RELATIVE_POINT = 15;
        public const int PEN_TEST_FIGURE = 16;
        public const int PEN_GRID = 17;
        public const int PEN_POINT_HIGHLIGHT2 = 18;
        public const int PEN_FIGURE_HIGHLIGHT = 19;
        public const int PEN_AXIS2 = 20;
        public const int PEN_PALE_FIGURE = 21;
        public const int PEN_MEASURE_FIGURE = 22;
        public const int PEN_DIMENTION = 23;
        public const int PEN_BLACK = 24;
        public const int PEN_MESH_LINE = 25;
        public const int PEN_TEST = 26;
        public const int PEN_NURBS_CTRL_LINE = 27;
        public const int PEN_LINE_SNAP = 28;
        public const int PEN_DRAG_LINE = 29;
        public const int PEN_TBL_SIZE = 30;

        public const int BRUSH_DEFAULT = 1;
        public const int BRUSH_BACKGROUND = 2;
        public const int BRUSH_TEXT = 3;
        public const int BRUSH_TRANSPARENT = 4;
        public const int BRUSH_TBL_SIZE = 5;

        public const int FONT_DEFAULT = 1;
        public const int FONT_SMALL = 2;
        public const int FONT_TBL_SIZE = 3;

        public const int FONT_SIZE_DEFAULT = 11;
        public const int FONT_SIZE_SMALL = 11;

        public enum ToolsType
        {
            DARK,
            PRINTER,
            DARK_GL,
            PRINTER_GL,
        }


        public Color[] PenColorTbl;
        public Color[] BrushColorTbl;

        DrawPen[] PenTbl = null;
        DrawBrush[] BrushTbl = null;
        Font[] FontTbl = null;

        DrawPen[] GLPenTbl = null;
        DrawBrush[] GLBrushTbl = null;

        private void AllocGDITbl()
        {
            PenTbl = new DrawPen[PEN_TBL_SIZE];
            BrushTbl = new DrawBrush[BRUSH_TBL_SIZE];
            FontTbl = new Font[FONT_TBL_SIZE];
        }

        private void AllocGLTbl()
        {
            GLPenTbl = new DrawPen[PEN_TBL_SIZE];
            GLBrushTbl = new DrawBrush[BRUSH_TBL_SIZE];
        }

        public void Setup(ToolsType t)
        {
            if (t == ToolsType.DARK)
            {
                SetupDarkSet();
            }
            else if (t == ToolsType.PRINTER)
            {
                SetupPrinterSet();
            }
            else if (t == ToolsType.DARK_GL)
            {
                SetupDarkSetGL();
            }
            else if (t == ToolsType.PRINTER_GL)
            {
                SetupPrinterSetGL();
            }
        }

        public static bool IsTypeForGL(ToolsType t)
        {
            if (t == ToolsType.DARK_GL || t == ToolsType.PRINTER_GL)
            {
                return true;
            }

            return false;
        }

        public static bool IsTypeForGDI(ToolsType t)
        {
            if (t == ToolsType.DARK || t == ToolsType.PRINTER)
            {
                return true;
            }

            return false;
        }


        private void SetupDarkSet()
        {
            AllocGDITbl();

            PenColorTbl = DarkColors.PenColorTbl;
            BrushColorTbl = DarkColors.BrushColorTbl;

            for (int i=0; i<PEN_TBL_SIZE; i++)
            {
                PenTbl[i] = DrawPen.New(new Pen(PenColorTbl[i]));
                PenTbl[i].ID = i;
            }

            for (int i = 0; i < BRUSH_TBL_SIZE; i++)
            {
                BrushTbl[i] = DrawBrush.New(new SolidBrush(BrushColorTbl[i]));
                BrushTbl[i].ID = i;
            }

            //FontFamily fontFamily = LoadFontFamily("/Fonts/mplus-1m-thin.ttf");
            FontFamily fontFamily = new FontFamily("MS UI Gothic");
            //FontFamily fontFamily = new FontFamily("ＭＳ ゴシック");

            FontTbl[FONT_DEFAULT] = new Font(fontFamily, FONT_SIZE_DEFAULT);
            FontTbl[FONT_SMALL]   = new Font(fontFamily, FONT_SIZE_SMALL);
        }

        private void SetupPrinterSet()
        {
            AllocGDITbl();

            PenColorTbl = PrintColors.PenColorTbl;
            BrushColorTbl = PrintColors.BrushColorTbl;

            for (int i = 0; i < PEN_TBL_SIZE; i++)
            {
                PenTbl[i] = DrawPen.New(new Pen(PenColorTbl[i]));
                PenTbl[i].ID = i;
            }

            for (int i = 0; i < BRUSH_TBL_SIZE; i++)
            {
                BrushTbl[i] = DrawBrush.New(new SolidBrush(BrushColorTbl[i]));
                BrushTbl[i].ID = i;
            }

            BrushTbl[BRUSH_BACKGROUND].DisposeGdiBrush();

            //FontFamily fontFamily = LoadFontFamily("/Fonts/mplus-1m-thin.ttf");
            //FontFamily fontFamily = new FontFamily("MS UI Gothic");
            FontFamily fontFamily = new FontFamily("ＭＳ ゴシック");

            FontTbl[FONT_DEFAULT]           = new Font(fontFamily, FONT_SIZE_DEFAULT);
            FontTbl[FONT_SMALL]             = new Font(fontFamily, FONT_SIZE_SMALL);
        }

        private void SetupDarkSetGL()
        {
            AllocGLTbl();

            PenColorTbl = DarkColors.PenColorTbl;
            BrushColorTbl = DarkColors.BrushColorTbl;

            float width = 1.0f;

            for (int i = 0; i < PEN_TBL_SIZE; i++)
            {
                GLPenTbl[i] = DrawPen.New(PenColorTbl[i], width);
                GLPenTbl[i].ID = i;
            }

            for (int i = 0; i < BRUSH_TBL_SIZE; i++)
            {
                GLBrushTbl[i] = DrawBrush.New(BrushColorTbl[i]);
                GLBrushTbl[i].ID = i;
            }
        }

        private void SetupPrinterSetGL()
        {
            AllocGLTbl();

            PenColorTbl = PrintColors.PenColorTbl;

            BrushColorTbl = new Color[PrintColors.BrushColorTbl.Length];
            Array.Copy(PrintColors.BrushColorTbl, BrushColorTbl, PrintColors.BrushColorTbl.Length);

            float width = 1.0f;

            for (int i = 0; i < PEN_TBL_SIZE; i++)
            {
                GLPenTbl[i] = DrawPen.New(PenColorTbl[i], width);
            }

            BrushColorTbl[BRUSH_BACKGROUND] = Color.FromArgb(255, 255, 255, 255);

            for (int i = 0; i < BRUSH_TBL_SIZE; i++)
            {
                GLBrushTbl[i] = DrawBrush.New(BrushColorTbl[i]);
            }
        }

        public void Dispose()
        {
            if (PenTbl != null)
            {
                foreach (DrawPen pen in PenTbl)
                {
                    pen.DisposeGdiPen();
                }

                PenTbl = null;
            }

            if (BrushTbl != null)
            {
                foreach (DrawBrush brush in BrushTbl)
                {
                    brush.DisposeGdiBrush();
                }

                BrushTbl = null;
            }

            if (FontTbl != null)
            {
                foreach (Font font in FontTbl)
                {
                    if (font != null)
                    {
                        font.Dispose();
                    }
                }

                FontTbl = null;
            }
        }

        public DrawTools()
        {
        }

        ~DrawTools()
        {
            Dispose();
        }

        #region Utilities
        public static FontFamily LoadFontFamilyFromResource(string fname)
        {
            StreamResourceInfo si = System.Windows.Application.GetResourceStream(
                new Uri(fname, UriKind.Relative));

            return LoadFontFamily(si.Stream);
        }

        // Load font family from stream
        public static FontFamily LoadFontFamily(Stream stream)
        {
            var buffer = new byte[stream.Length];

            stream.Read(buffer, 0, buffer.Length);

            return LoadFontFamily(buffer);
        }
        

        static PrivateFontCollection PrivateFonts = new PrivateFontCollection();

        // load font family from byte array
        public static FontFamily LoadFontFamily(byte[] buffer)
        {
            IntPtr data = Marshal.AllocCoTaskMem(buffer.Length);

            Marshal.Copy(buffer, 0, data, buffer.Length);

            PrivateFonts.AddMemoryFont(data, buffer.Length);

            Marshal.FreeCoTaskMem(data);

            return PrivateFonts.Families[0];
        }

        #endregion

        public DrawPen pen(int id)
        {
            return PenTbl[id];
        }

        public DrawBrush brush(int id)
        {
            return BrushTbl[id];
        }

        public Color PenColor(int id)
        {
            return PenColorTbl[id];
        }

        public Color BrushColor(int id)
        {
            return BrushColorTbl[id];
        }

        public Font font(int id)
        {
            return FontTbl[id];
        }

        public DrawPen glpen(int id)
        {
            return GLPenTbl[id];
        }

        public DrawBrush glbrush(int id)
        {
            return GLBrushTbl[id];
        }
    }


    public enum ToolType : byte
    {
        INDEX,
        COLOR,
        OBJECT,
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct DrawColor
    {
        [FieldOffset(0)]
        public int Argb;

        [FieldOffset(3)]
        public byte A;

        [FieldOffset(2)]
        public byte R;

        [FieldOffset(1)]
        public byte G;

        [FieldOffset(0)]
        public byte B;
    }

    public static class Color4Util
    {
        public static Color4 FromArgb(int argb)
        {
            DrawColor c = default;
            c.Argb = argb;

            return new Color4(
                    c.R,
                    c.G,
                    c.B,
                    c.A
                );
        }
    }

    public struct DrawPen
    {
        public int ID;

        public int Argb;

        public float Width;

        public Pen GdiPen;

        public void DisposeGdiPen()
        {
            if (GdiPen != null)
            {
                GdiPen.Dispose();
                GdiPen = null;
            }
        }

        public Color4 Color4()
        {
            return Color4Util.FromArgb(Argb);
        }

        public Color GdiColor()
        {
            return Color.FromArgb(Argb);
        }

        public static DrawPen New(DrawContextGDI dc, int id)
        {
            DrawPen dt = dc.Tools.pen(id);
            return dt;
        }

        public static DrawPen New(DrawContextGL dc, int id)
        {
            DrawPen dt = dc.Tools.glpen(id);
            return dt;
        }

        public static DrawPen New(Pen pen)
        {
            DrawPen dt = default;

            dt.GdiPen = pen;
            dt.Argb = pen.Color.ToArgb();
            dt.Width = pen.Width;
            return dt;
        }

        public static DrawPen New(Color color, float width)
        {
            DrawPen dt = default;

            dt.Argb = color.ToArgb();
            dt.Width = width;
            return dt;
        }

        public static DrawPen New(Color4 color, float width)
        {
            DrawPen dt = default;

            dt.Argb = color.ToArgb();
            dt.Width = width;
            return dt;
        }
    }

    public struct DrawBrush
    {
        public int ID;

        public int Argb;

        public SolidBrush GdiBrush;

        public void DisposeGdiBrush()
        {
            if (GdiBrush != null)
            {
                GdiBrush.Dispose();
                GdiBrush = null;
            }
        }

        public Color4 Color4()
        {
            return Color4Util.FromArgb(Argb);
        }

        public Color GdiColor()
        {
            return Color.FromArgb(Argb);
        }

        public static DrawBrush New()
        {
            DrawBrush dt = default;
            return dt;
        }

        public static DrawBrush New(DrawContextGL dc, int id)
        {
            DrawBrush dt = dc.Tools.glbrush(id);
            return dt;
        }

        public static DrawBrush New(DrawContextGDI dc, int id)
        {
            DrawBrush dt = dc.Tools.brush(id);
            return dt;
        }

        public static DrawBrush New(SolidBrush brush)
        {
            DrawBrush dt = default;
            dt.GdiBrush = brush;
            dt.Argb = brush.Color.ToArgb();
            return dt;
        }

        public static DrawBrush New(Color color)
        {
            DrawBrush dt = default;
            dt.Argb = color.ToArgb();
            return dt;
        }

        public static DrawBrush New(Color4 color)
        {
            DrawBrush dt = default;
            dt.Argb = color.ToArgb();
            return dt;
        }
    }
}
