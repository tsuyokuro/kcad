
using OpenTK.Graphics;
using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Resources;

namespace Plotter
{
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
        public const int BRUSH_DEFAULT_MESH_FILL = 5;
        public const int BRUSH_TBL_SIZE = 6;

        public const int FONT_DEFAULT = 1;
        public const int FONT_SMALL = 2;
        public const int FONT_TBL_SIZE = 3;

        public const int FONT_SIZE_DEFAULT = 11;
        public const int FONT_SIZE_SMALL = 11;

        public enum ToolsType
        {
            DARK,
            PRINTER,
        }


        public Color[] PenColorTbl;
        public Color[] BrushColorTbl;

        DrawPen[] PenTbl = null;
        DrawBrush[] BrushTbl = null;
        Font[] FontTbl = null;

        private void AllocGDITbl()
        {
            PenTbl = new DrawPen[PEN_TBL_SIZE];
            BrushTbl = new DrawBrush[BRUSH_TBL_SIZE];
            FontTbl = new Font[FONT_TBL_SIZE];
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

        public DrawPen Pen(int id)
        {
            return PenTbl[id];
        }

        public DrawBrush Brush(int id)
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

    public class DrawPen
    {
        public int ID;

        public int Argb;

        public float Width;

        public Pen GdiPen;

        public static DrawPen NullPen = New(Color.FromArgb(0, 0, 0, 0), 0);

        public bool IsNullPen
        {
            get => ((uint)Argb & 0xff000000) == 0;
        }

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

        public static DrawPen New(Pen pen)
        {
            DrawPen dt = new DrawPen();

            dt.GdiPen = pen;
            dt.Argb = pen.Color.ToArgb();
            dt.Width = pen.Width;
            return dt;
        }

        public static DrawPen New(Color color, float width)
        {
            DrawPen dt = new DrawPen();

            dt.Argb = color.ToArgb();
            dt.Width = width;
            return dt;
        }

        public static DrawPen New(Color4 color, float width)
        {
            DrawPen dt = new DrawPen();

            dt.Argb = color.ToArgb();
            dt.Width = width;
            return dt;
        }
    }

    public class DrawBrush
    {
        public int ID;

        public int Argb;

        public SolidBrush GdiBrush;

        public static DrawBrush NullBrush = New(Color.FromArgb(0, 0, 0, 0));

        public bool IsNullBrush
        {
            get => ((uint)Argb & 0xff000000) == 0;
        }

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

        public static DrawBrush New(SolidBrush brush)
        {
            DrawBrush dt = new DrawBrush();
            dt.GdiBrush = brush;
            dt.Argb = brush.Color.ToArgb();
            return dt;
        }

        public static DrawBrush New(Color color)
        {
            DrawBrush dt = new DrawBrush();
            dt.Argb = color.ToArgb();
            return dt;
        }

        public static DrawBrush New(Color4 color)
        {
            DrawBrush dt = new DrawBrush();
            dt.Argb = color.ToArgb();
            return dt;
        }
    }
}
