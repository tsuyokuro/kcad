
using OpenTK.Graphics;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Plotter
{
    using PenHolder = ToolHolder<Pen>;
    using BrushHolder = ToolHolder<Brush>;
    using ColorHolder = ToolHolder<Color>;
    using FontHolder = ToolHolder<Font>;
    //using ArrowCapHolder = ToolHolder<AdjustableArrowCap>;

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

    public class ToolHolder<T>
    {
        public bool NeedDispose;
        public T ToolObj;

        public ToolHolder(T obj, bool dispose)
        {
            ToolObj = obj;
            NeedDispose = dispose;
        }

        public ToolHolder()
        {
            ToolObj = default(T);
            NeedDispose = false;
        }

        public void Dispose()
        {
            if (NeedDispose)
            {
                if (ToolObj != null)
                {
                    ((IDisposable)ToolObj).Dispose();
                }
            }
        }

        public static implicit operator T(ToolHolder<T> holder)
        {
            return holder.ToolObj;
        }
    }

    public class DrawTools : IDisposable
    {
        public const int PEN_DEFAULT = 0;
        public const int PEN_SLECT_POINT = 1;
        public const int PEN_CURSOR = 2;
        public const int PEN_CURSOR2 = 3;
        public const int PEN_DEFAULT_FIGURE = 4;
        public const int PEN_TEMP_FIGURE = 5;
        public const int PEN_POINT_HIGHTLITE = 6;
        public const int PEN_MATCH_FIGURE = 7;
        public const int PEN_MATCH_SEG = 8;
        public const int PEN_LAST_POINT_MARKER = 9;
        public const int PEN_LAST_POINT_MARKER2 = 10;
        public const int PEN_AXIS = 11;
        public const int PEN_ARROW_AXIS = 12;
        public const int PEN_PAGE_FRAME = 13;
        public const int PEN_RELATIVE_POINT = 14;
        public const int PEN_TEST_FIGURE = 15;
        public const int PEN_GRID = 16;
        public const int PEN_POINT_HIGHTLITE2 = 17;
        public const int PEN_FIGURE_HIGHLIGHT = 18;
        public const int PEN_AXIS2 = 19;
        public const int PEN_PALE_FIGURE = 20;
        public const int PEN_MEASURE_FIGURE = 21;
        public const int PEN_DIMENTION = 22;
        public const int PEN_TBL_SIZE = 23;

        public const int BRUSH_DEFAULT = 0;
        public const int BRUSH_BACKGROUND = 1;
        public const int BRUSH_TEXT = 2;
        public const int BRUSH_TBL_SIZE = 3;

        public const int COLOR_DEFAULT = 0;
        public const int COLOR_BACKGROUND = 1;
        public const int COLOR_TBL_SIZE = 2;

        public const int FONT_DEFAULT = 0;
        public const int FONT_SMALL = 1;
        public const int FONT_TBL_SIZE = 2;

        public enum ToolsType
        {
            DARK,
            PRINTER,
            DARK_GL,
        }


        PenHolder[] PenTbl = null;
        BrushHolder[] BrushTbl = null;
        ColorHolder[] ColorTbl = null;
        FontHolder[] FontTbl = null;

        GLPen[] GLPenTbl = null;
        Color4[] GLColorTbl = null;

        private void allocGDITbl()
        {
            PenTbl = new PenHolder[PEN_TBL_SIZE];
            BrushTbl = new BrushHolder[BRUSH_TBL_SIZE];
            ColorTbl = new ColorHolder[COLOR_TBL_SIZE];
            FontTbl = new FontHolder[FONT_TBL_SIZE];
        }

        private void allocGLTbl()
        {
            GLPenTbl = new GLPen[PEN_TBL_SIZE];
            GLColorTbl = new Color4[COLOR_TBL_SIZE];
        }

        public void Setup(ToolsType t)
        {
            if (t == ToolsType.DARK)
            {
                setupDarkSet();
            }
            else if (t == ToolsType.PRINTER)
            {
                setupPrinterSet();
            }
            else if (t == ToolsType.DARK_GL)
            {
                setupDarkSetGL();
            }
        }

        private void setupDarkSet()
        {
            allocGDITbl();

            PenTbl[PEN_DEFAULT]             = new PenHolder(Pens.White, false);
            PenTbl[PEN_SLECT_POINT]         = new PenHolder(Pens.LightGreen, false);
            PenTbl[PEN_CURSOR]              = new PenHolder(Pens.LightBlue, false);
            PenTbl[PEN_CURSOR2]             = new PenHolder(Pens.DarkSlateBlue, false);
            PenTbl[PEN_DEFAULT_FIGURE]      = new PenHolder(Pens.White, false);
            PenTbl[PEN_TEMP_FIGURE]         = new PenHolder(Pens.CadetBlue, false);
            PenTbl[PEN_POINT_HIGHTLITE]     = new PenHolder(Pens.Orange, false);
            PenTbl[PEN_MATCH_FIGURE]        = new PenHolder(Pens.Red, false);
            PenTbl[PEN_MATCH_SEG]           = new PenHolder(Pens.Green, false);
            PenTbl[PEN_LAST_POINT_MARKER]   = new PenHolder(Pens.CornflowerBlue, false);
            PenTbl[PEN_LAST_POINT_MARKER2]  = new PenHolder(Pens.YellowGreen, false);
            PenTbl[PEN_AXIS]                = new PenHolder(new Pen(Color.FromArgb(60, 60, 92), 0), true);
            PenTbl[PEN_ARROW_AXIS]          = new PenHolder(new Pen(Color.FromArgb(82, 82, 112), 0), true);
            PenTbl[PEN_PAGE_FRAME]          = new PenHolder(new Pen(Color.FromArgb(92, 92, 92), 0), true);
            PenTbl[PEN_RELATIVE_POINT]      = new PenHolder(Pens.CornflowerBlue, false);
            PenTbl[PEN_TEST_FIGURE]         = new PenHolder(Pens.Yellow, false);
            PenTbl[PEN_GRID]                = new PenHolder(new Pen(Color.FromArgb(192, 128, 92), 0), true);
            PenTbl[PEN_POINT_HIGHTLITE2]    = new PenHolder(Pens.SpringGreen, false);
            PenTbl[PEN_FIGURE_HIGHLIGHT]    = new PenHolder(Pens.HotPink, false);
            PenTbl[PEN_AXIS2]               = new PenHolder(Pens.LightSeaGreen, false);
            PenTbl[PEN_PALE_FIGURE]         = new PenHolder(new Pen(Color.FromArgb(0x7E, 0x7E, 0x7E), 0), true);
            PenTbl[PEN_MEASURE_FIGURE]      = new PenHolder(Pens.OrangeRed, false);
            PenTbl[PEN_DIMENTION]           = new PenHolder(Pens.PaleGreen, false);

            ColorTbl[COLOR_DEFAULT]         = new ColorHolder(Color.FromArgb(255, 255, 255), false);
            ColorTbl[COLOR_BACKGROUND]      = new ColorHolder(Color.FromArgb(0x1e, 0x1e, 0x1e), false);

            BrushTbl[BRUSH_DEFAULT]         = new BrushHolder(new SolidBrush(ColorTbl[COLOR_DEFAULT]), true);
            BrushTbl[BRUSH_BACKGROUND]      = new BrushHolder(new SolidBrush(ColorTbl[COLOR_BACKGROUND]), true);
            BrushTbl[BRUSH_TEXT]            = new BrushHolder(new SolidBrush(Color.White), true);

            FontTbl[FONT_DEFAULT]           = new FontHolder(new Font("MS UI Gothic", 9), true);
            FontTbl[FONT_SMALL]             = new FontHolder(new Font("MS UI Gothic", 9), true);
        }

        private void setupPrinterSet()
        {
            allocGDITbl();

            PenTbl[PEN_DEFAULT]             = new PenHolder(new Pen(Brushes.Black, 0), true);
            PenTbl[PEN_SLECT_POINT]         = new PenHolder(null, false);
            PenTbl[PEN_CURSOR]              = new PenHolder(null, false);
            PenTbl[PEN_CURSOR2]             = new PenHolder(null, false);
            PenTbl[PEN_DEFAULT_FIGURE]      = new PenHolder(new Pen(Brushes.Black, 0), true);
            PenTbl[PEN_TEMP_FIGURE]         = new PenHolder(null, false);
            PenTbl[PEN_POINT_HIGHTLITE]     = new PenHolder(null, false);
            PenTbl[PEN_MATCH_FIGURE]        = new PenHolder(null, false);
            PenTbl[PEN_MATCH_SEG]           = new PenHolder(null, false);
            PenTbl[PEN_LAST_POINT_MARKER]   = new PenHolder(null, false);
            PenTbl[PEN_LAST_POINT_MARKER2]  = new PenHolder(null, false);
            PenTbl[PEN_AXIS]                = new PenHolder(null, false);
            PenTbl[PEN_ARROW_AXIS]          = new PenHolder(null, false);
            PenTbl[PEN_PAGE_FRAME]          = new PenHolder(null, false);
            PenTbl[PEN_RELATIVE_POINT]      = new PenHolder(null, false);
            PenTbl[PEN_TEST_FIGURE]         = new PenHolder(null, false);
            PenTbl[PEN_GRID]                = new PenHolder(null, false);
            PenTbl[PEN_POINT_HIGHTLITE2]    = new PenHolder(null, false);
            PenTbl[PEN_FIGURE_HIGHLIGHT]    = new PenHolder(null, false);
            PenTbl[PEN_AXIS2]               = new PenHolder(null, false);
            PenTbl[PEN_PALE_FIGURE]         = new PenHolder(new Pen(Brushes.Black, 0), true);
            PenTbl[PEN_MEASURE_FIGURE]      = new PenHolder(null, false);
            PenTbl[PEN_DIMENTION]           = new PenHolder(new Pen(Brushes.Black, 0), true);


            ColorTbl[COLOR_DEFAULT]         = new ColorHolder(Color.Black, false);
            ColorTbl[COLOR_BACKGROUND]      = new ColorHolder(Color.Black, false);

            BrushTbl[BRUSH_DEFAULT]         = new BrushHolder(new SolidBrush(ColorTbl[COLOR_DEFAULT]), false);
            BrushTbl[BRUSH_BACKGROUND]      = new BrushHolder(null, false);
            BrushTbl[BRUSH_TEXT]            = new BrushHolder(new SolidBrush(ColorTbl[COLOR_DEFAULT]), false);

            FontTbl[FONT_DEFAULT]           = new FontHolder(new Font("MS UI Gothic", 9), true);
            FontTbl[FONT_SMALL]             = new FontHolder(new Font("MS UI Gothic", 9), true);
        }

        private void setupDarkSetGL()
        {
            allocGLTbl();

            float width = 1.0f;

            GLPenTbl[PEN_DEFAULT]           = new GLPen(Pens.White.Color, width);
            GLPenTbl[PEN_SLECT_POINT]       = new GLPen(Pens.LightGreen.Color, width);
            GLPenTbl[PEN_CURSOR]            = new GLPen(Pens.LightBlue.Color, width);
            GLPenTbl[PEN_CURSOR2]           = new GLPen(Pens.DarkSlateBlue.Color, width);
            GLPenTbl[PEN_DEFAULT_FIGURE]    = new GLPen(Pens.White.Color, width);
            GLPenTbl[PEN_TEMP_FIGURE]       = new GLPen(Pens.CadetBlue.Color, width);
            GLPenTbl[PEN_POINT_HIGHTLITE]   = new GLPen(Pens.BlueViolet.Color, width);
            GLPenTbl[PEN_MATCH_FIGURE]      = new GLPen(Pens.Red.Color, width);
            GLPenTbl[PEN_MATCH_SEG]         = new GLPen(Pens.Green.Color, width);
            GLPenTbl[PEN_LAST_POINT_MARKER] = new GLPen(Pens.Aqua.Color, width);
            GLPenTbl[PEN_LAST_POINT_MARKER2] = new  GLPen(Pens.YellowGreen.Color, width);
            GLPenTbl[PEN_AXIS]              = new GLPen(Color.FromArgb(60, 60, 92), width);
            GLPenTbl[PEN_ARROW_AXIS]        = new GLPen(Color.FromArgb(82, 82, 112), width);
            GLPenTbl[PEN_PAGE_FRAME]        = new GLPen(Color.FromArgb(92, 92, 92), width);
            GLPenTbl[PEN_RELATIVE_POINT]    = new GLPen(Pens.CornflowerBlue.Color, width);
            GLPenTbl[PEN_TEST_FIGURE]       = new GLPen(Pens.Yellow.Color, width);
            GLPenTbl[PEN_GRID]              = new GLPen(Color.FromArgb(192, 128, 92), width);
            GLPenTbl[PEN_POINT_HIGHTLITE2]  = new GLPen(Pens.Yellow.Color, width);
            GLPenTbl[PEN_FIGURE_HIGHLIGHT]  = new GLPen(Pens.HotPink.Color, width);
            GLPenTbl[PEN_AXIS2]             = new GLPen(Pens.LightSeaGreen.Color, width);
            GLPenTbl[PEN_PALE_FIGURE]       = new GLPen(Color.FromArgb(0x7E, 0x7E, 0x7E), width);
            GLPenTbl[PEN_MEASURE_FIGURE]    = new GLPen(Pens.OrangeRed.Color, width);
            GLPenTbl[PEN_DIMENTION]         = new GLPen(Pens.PaleGreen.Color, width);

            GLColorTbl[COLOR_DEFAULT]       = Color.FromArgb(255, 255, 255);
            GLColorTbl[COLOR_BACKGROUND]    = Color.FromArgb(0x1e, 0x1e, 0x1e);
        }


        public void Dispose()
        {
            if (PenTbl != null)
            {
                foreach (ToolHolder<Pen> penHolder in PenTbl)
                {
                    if (penHolder != null)
                    {
                        penHolder.Dispose();
                    }
                }

                PenTbl = null;
            }

            if (BrushTbl != null)
            {
                foreach (ToolHolder<Brush> brushHolder in BrushTbl)
                {
                    if (brushHolder != null)
                    {
                        brushHolder.Dispose();
                    }
                }

                BrushTbl = null;
            }

            ColorTbl = null;

            if (FontTbl != null)
            {
                foreach (ToolHolder<Font> fontHolder in FontTbl)
                {
                    if (fontHolder != null)
                    {
                        fontHolder.Dispose();
                    }
                }

                FontTbl = null;
            }
        }

        ~DrawTools()
        {
            Dispose();
        }

        public Pen pen(int id)
        {
            return PenTbl[id].ToolObj;
        }

        public Brush brush(int id)
        {
            return BrushTbl[id].ToolObj;
        }

        public Color color(int id)
        {
            return ColorTbl[id].ToolObj;
        }

        public Font font(int id)
        {
            return FontTbl[id].ToolObj;
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
