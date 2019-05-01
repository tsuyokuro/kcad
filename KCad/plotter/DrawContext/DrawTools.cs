﻿
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
                PenTbl[i] = new Pen(DarkColors.PenColorTbl[i]);
            }

            for (int i = 0; i < BRUSH_TBL_SIZE; i++)
            {
                BrushTbl[i] = new SolidBrush(DarkColors.BrushColorTbl[i]);
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
                GLPenTbl[i] = new GLPen(DarkColors.PenColorTbl[i], width);
            }

            for (int i = 0; i < BRUSH_TBL_SIZE; i++)
            {
                GLColorTbl[i] = DarkColors.BrushColorTbl[i];
            }
        }

        private void SetupPrinterSetGL()
        {
            AllocGLTbl();

            PenColorTbl = DarkColors.PenColorTbl;
            BrushColorTbl = DarkColors.BrushColorTbl;

            float width = 1.0f;

            for (int i = 0; i < PEN_TBL_SIZE; i++)
            {
                GLPenTbl[i] = new GLPen(DarkColors.PenColorTbl[i], width);
            }

            for (int i = 0; i < BRUSH_TBL_SIZE; i++)
            {
                GLColorTbl[i] = DarkColors.BrushColorTbl[i];
            }

            GLPenTbl[PEN_DEFAULT] = new GLPen(Color.FromArgb(255, 0, 0, 0), 1);
            GLPenTbl[PEN_DEFAULT_FIGURE] = new GLPen(Color.FromArgb(255,0,0,0), 1);
            GLPenTbl[PEN_PALE_FIGURE] = new GLPen(Color.FromArgb(255, 0, 0, 0), 1);
            GLPenTbl[PEN_DIMENTION] = new GLPen(Color.FromArgb(255, 0, 0, 0), 1);
            GLPenTbl[PEN_MESH_LINE] = new GLPen(Color.FromArgb(255, 0, 0, 0), 1);

            GLColorTbl[BRUSH_DEFAULT] = Color.FromArgb(255, 0, 0, 0);
            GLColorTbl[BRUSH_BACKGROUND] = Color.FromArgb(255, 255, 255, 255);
            GLColorTbl[BRUSH_TEXT] = Color.FromArgb(255, 0, 0, 0);
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

        public Pen pen(int id)
        {
            return PenTbl[id];
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
    
    public enum ToolType : byte
    {
        INDEX,
        COLOR,
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct DrawTool
    {
        [FieldOffset(0)]
        public ToolType Type;

        [FieldOffset(sizeof(ToolType))]
        public int Val;

        [FieldOffset(sizeof(ToolType)+3)]
        public byte A;

        [FieldOffset(sizeof(ToolType)+2)]
        public byte R;

        [FieldOffset(sizeof(ToolType)+1)]
        public byte G;

        [FieldOffset(sizeof(ToolType)+0)]
        public byte B;

        public static DrawTool New(int idx)
        {
            DrawTool dt = default;
            dt.Type = ToolType.INDEX;
            dt.Val = idx;
            return dt;
        }

        public static DrawTool New(Color color)
        {
            DrawTool dt = default;
            dt.Type = ToolType.COLOR;
            dt.Val = color.ToArgb();
            return dt;
        }

        public static DrawTool New(Color4 color)
        {
            DrawTool dt = default;
            dt.Type = ToolType.COLOR;
            dt.Val = color.ToArgb();
            return dt;
        }

        public static explicit operator Color4(DrawTool dt)
        {
            return new Color4(
                    dt.R,
                    dt.G,
                    dt.B,
                    dt.A
                );
        }

        public static explicit operator DrawTool(Color4 color)
        {
            return DrawTool.New(color);
        }

        public static explicit operator Color(DrawTool dt)
        {
            return Color.FromArgb(dt.Val);
        }

        public static explicit operator DrawTool(Color color)
        {
            return DrawTool.New(color);
        }

        public static explicit operator int(DrawTool dt)
        {
            return dt.Val;
        }

        public static explicit operator DrawTool(int idx)
        {
            return DrawTool.New(idx);
        }
    }
}
