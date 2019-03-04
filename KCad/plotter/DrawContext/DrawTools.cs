
using OpenTK.Graphics;
using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Resources;

namespace Plotter
{
    public class GLPen
    {
        public Color4 Color;
        public float LineWidth;

        public GLPen(Color4 color, float w)
        {
            Color = color;
            LineWidth = w;
        }
    }

    public class DarkColors
    {
        public static Color[] PenColorTbl;
        public static Color[] BrushColorTbl;

        static DarkColors()
        {
            PenColorTbl = new Color[DrawTools.PEN_TBL_SIZE];

            PenColorTbl[DrawTools.PEN_DEFAULT] = Color.White;
            PenColorTbl[DrawTools.PEN_SELECT_POINT] = Color.FromArgb(128, 255, 0);
            PenColorTbl[DrawTools.PEN_CURSOR] = Color.LightBlue;
            PenColorTbl[DrawTools.PEN_CURSOR2] = Color.FromArgb(128, 255, 128);
            PenColorTbl[DrawTools.PEN_DEFAULT_FIGURE] = Color.White;
            PenColorTbl[DrawTools.PEN_TEMP_FIGURE] = Color.CadetBlue;
            PenColorTbl[DrawTools.PEN_POINT_HIGHLIGHT] = Color.Orange;
            PenColorTbl[DrawTools.PEN_MATCH_FIGURE] = Color.Red;
            PenColorTbl[DrawTools.PEN_MATCH_SEG] = Color.Green;
            PenColorTbl[DrawTools.PEN_LAST_POINT_MARKER] = Color.CornflowerBlue;
            PenColorTbl[DrawTools.PEN_LAST_POINT_MARKER2] = Color.YellowGreen;
            PenColorTbl[DrawTools.PEN_AXIS] = Color.FromArgb(60, 60, 92);
            PenColorTbl[DrawTools.PEN_ARROW_AXIS] = Color.FromArgb(82, 82, 112);
            PenColorTbl[DrawTools.PEN_PAGE_FRAME] = Color.FromArgb(92, 92, 92);
            PenColorTbl[DrawTools.PEN_RELATIVE_POINT] = Color.CornflowerBlue;
            PenColorTbl[DrawTools.PEN_TEST_FIGURE] = Color.Yellow;
            PenColorTbl[DrawTools.PEN_GRID] = Color.FromArgb(192, 128, 92);
            PenColorTbl[DrawTools.PEN_POINT_HIGHLIGHT2] = Color.SpringGreen;
            PenColorTbl[DrawTools.PEN_FIGURE_HIGHLIGHT] = Color.HotPink;
            PenColorTbl[DrawTools.PEN_AXIS2] = Color.LightSeaGreen;
            PenColorTbl[DrawTools.PEN_PALE_FIGURE] = Color.FromArgb(0x7E, 0x7E, 0x7E);
            PenColorTbl[DrawTools.PEN_MEASURE_FIGURE] = Color.OrangeRed;
            PenColorTbl[DrawTools.PEN_DIMENTION] = Color.FromArgb(0xFF, 128, 192, 255);
            PenColorTbl[DrawTools.PEN_BLACK] = Color.Black;
            PenColorTbl[DrawTools.PEN_MESH_LINE] = Color.FromArgb(0xFF, 0x70, 0x70, 0x70);
            PenColorTbl[DrawTools.PEN_TEST] = Color.FromArgb(0xFF, 0xBB, 0xCC, 0xDD);
            PenColorTbl[DrawTools.PEN_NURBS_CTRL_LINE] = Color.FromArgb(0xFF, 0x60, 0xC0, 0x60);
            PenColorTbl[DrawTools.PEN_LINE_SNAP] = Color.FromArgb(0xFF, 0x00, 0xC0, 0x60);
            PenColorTbl[DrawTools.PEN_DRAG_LINE] = Color.FromArgb(0xFF, 0x60, 0x60, 0x80);


            BrushColorTbl = new Color[DrawTools.BRUSH_TBL_SIZE];

            BrushColorTbl[DrawTools.BRUSH_DEFAULT] = Color.FromArgb(255, 255, 255);
            BrushColorTbl[DrawTools.BRUSH_BACKGROUND] = Color.FromArgb(0x8, 0x8, 0x8);
            BrushColorTbl[DrawTools.BRUSH_TEXT] = Color.White;
            BrushColorTbl[DrawTools.BRUSH_TRANSPARENT] = Color.FromArgb(0,0,0,0);
        }
    }

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

        public const int FONT_SIZE_DEFAULT = 9;
        public const int FONT_SIZE_SMALL = 9;

        public enum ToolsType
        {
            DARK,
            PRINTER,
            DARK_GL,
        }


        public Color[] PenColorTbl;
        Pen[] PenTbl = null;

        public Color[] BrushColorTbl;
        Brush[] BrushTbl = null;


        Font[] FontTbl = null;

        GLPen[] GLPenTbl = null;
        Color4[] GLColorTbl = null;

        private void AllocGDITbl()
        {
            PenTbl = new Pen[PEN_TBL_SIZE];
            BrushTbl = new Brush[BRUSH_TBL_SIZE];
            FontTbl = new Font[FONT_TBL_SIZE];
        }

        private void AllocGLTbl()
        {
            GLPenTbl = new GLPen[PEN_TBL_SIZE];
            GLColorTbl = new Color4[BRUSH_TBL_SIZE];
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
        }

        public static bool IsTypeForGL(ToolsType t)
        {
            if (t == ToolsType.DARK_GL)
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
                PenTbl[i] = new Pen(DarkColors.PenColorTbl[i]);
            }

            for (int i = 0; i < BRUSH_TBL_SIZE; i++)
            {
                BrushTbl[i] = new SolidBrush(DarkColors.BrushColorTbl[i]);
            }

            FontFamily fontFamily = LoadFontFamily("/Fonts/mplus-1m-thin.ttf");
            //FontFamily fontFamily = new FontFamily("MS UI Gothic");

            FontTbl[FONT_DEFAULT] = new Font(fontFamily, FONT_SIZE_DEFAULT);
            FontTbl[FONT_SMALL]   = new Font(fontFamily, FONT_SIZE_SMALL);
        }

        private void SetupPrinterSet()
        {
            AllocGDITbl();

            for (int i = 0; i < PEN_TBL_SIZE; i++)
            {
                PenTbl[i] = null;
            }

            PenTbl[PEN_DEFAULT]             = new Pen(Color.Black, 1);
            PenTbl[PEN_DEFAULT_FIGURE]      = new Pen(Color.Black, 1);
            PenTbl[PEN_PALE_FIGURE]         = new Pen(Color.Black, 1);
            PenTbl[PEN_DIMENTION]           = new Pen(Color.Black, 1);
            PenTbl[PEN_MESH_LINE]           = new Pen(Color.LightGray, 1);

            BrushTbl[BRUSH_DEFAULT]         = new SolidBrush(Color.Black);
            BrushTbl[BRUSH_BACKGROUND]      = null;
            BrushTbl[BRUSH_TEXT]            = new SolidBrush(Color.Black);

            FontFamily fontFamily = new FontFamily("MS UI Gothic");

            FontTbl[FONT_DEFAULT]           = new Font(fontFamily, FONT_SIZE_DEFAULT);
            FontTbl[FONT_SMALL]             = new Font(fontFamily, FONT_SIZE_SMALL);
        }

        private void SetupDarkSetGL()
        {
            AllocGLTbl();

            float width = 1.0f;

            for (int i = 0; i < PEN_TBL_SIZE; i++)
            {
                GLPenTbl[i] = new GLPen(DarkColors.PenColorTbl[i], width);
            }

            for (int i = 0; i < BRUSH_TBL_SIZE; i++)
            {
                GLColorTbl[i] = DarkColors.BrushColorTbl[i];
            }
        }

        public void Dispose()
        {
            if (PenTbl != null)
            {
                foreach (Pen pen in PenTbl)
                {
                    if (pen != null)
                    {
                        pen.Dispose();
                    }
                }

                PenTbl = null;
            }

            if (BrushTbl != null)
            {
                foreach (Brush brush in BrushTbl)
                {
                    if (brush != null)
                    {
                        brush.Dispose();
                    }
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
        public static FontFamily LoadFontFamily(string fname)
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

        public Pen pen(int id)
        {
            return PenTbl[id];
        }

        public Color PenColor(int id)
        {
            return PenColorTbl[id];
        }

        public Brush brush(int id)
        {
            return BrushTbl[id];
        }

        public Color BrushColor(int id)
        {
            return BrushColorTbl[id];
        }

        public Font font(int id)
        {
            return FontTbl[id];
        }

        public GLPen glpen(int id)
        {
            return GLPenTbl[id];
        }

        public Color4 glcolor(int id)
        {
            return GLColorTbl[id];
        }

    }
}
