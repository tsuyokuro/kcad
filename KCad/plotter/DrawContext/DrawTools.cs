
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
            PenColorTbl[DrawTools.PEN_CURSOR2] = Color.DarkSlateBlue;
            PenColorTbl[DrawTools.PEN_DEFAULT_FIGURE] = Color.White;
            PenColorTbl[DrawTools.PEN_TEMP_FIGURE] = Color.CadetBlue;
            PenColorTbl[DrawTools.PEN_POINT_HIGHTLITE] = Color.Orange;
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
            PenColorTbl[DrawTools.PEN_POINT_HIGHTLITE2] = Color.SpringGreen;
            PenColorTbl[DrawTools.PEN_FIGURE_HIGHLIGHT] = Color.HotPink;
            PenColorTbl[DrawTools.PEN_AXIS2] = Color.LightSeaGreen;
            PenColorTbl[DrawTools.PEN_PALE_FIGURE] = Color.FromArgb(0x7E, 0x7E, 0x7E);
            PenColorTbl[DrawTools.PEN_MEASURE_FIGURE] = Color.OrangeRed;
            PenColorTbl[DrawTools.PEN_DIMENTION] = Color.PaleGreen;


            BrushColorTbl = new Color[DrawTools.BRUSH_TBL_SIZE];

            BrushColorTbl[DrawTools.BRUSH_DEFAULT] = Color.FromArgb(255, 255, 255);
            BrushColorTbl[DrawTools.BRUSH_BACKGROUND] = Color.FromArgb(0x1e, 0x1e, 0x1e);
            BrushColorTbl[DrawTools.BRUSH_TEXT] = Color.White;
        }
    }

    public class DrawTools : IDisposable
    {
        public const int PEN_DEFAULT = 0;
        public const int PEN_SELECT_POINT = 1;
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

        public const int FONT_DEFAULT = 0;
        public const int FONT_SMALL = 1;
        public const int FONT_TBL_SIZE = 2;

        public enum ToolsType
        {
            DARK,
            PRINTER,
            DARK_GL,
        }


        public Color[] PenColorTbl;
        PenHolder[] PenTbl = null;

        public Color[] BrushColorTbl;
        BrushHolder[] BrushTbl = null;


        FontHolder[] FontTbl = null;

        GLPen[] GLPenTbl = null;
        Color4[] GLColorTbl = null;

        private void allocGDITbl()
        {
            PenTbl = new PenHolder[PEN_TBL_SIZE];
            BrushTbl = new BrushHolder[BRUSH_TBL_SIZE];
            FontTbl = new FontHolder[FONT_TBL_SIZE];
        }

        private void allocGLTbl()
        {
            GLPenTbl = new GLPen[PEN_TBL_SIZE];
            GLColorTbl = new Color4[BRUSH_TBL_SIZE];
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

            PenColorTbl = DarkColors.PenColorTbl;
            BrushColorTbl = DarkColors.BrushColorTbl;

            for (int i=0; i<PEN_TBL_SIZE; i++)
            {
                PenTbl[i] = new PenHolder(new Pen(DarkColors.PenColorTbl[i]), true);
            }

            for (int i = 0; i < BRUSH_TBL_SIZE; i++)
            {
                BrushTbl[i] = new BrushHolder(new SolidBrush(DarkColors.BrushColorTbl[i]), true);
            }

            FontTbl[FONT_DEFAULT]           = new FontHolder(new Font("MS UI Gothic", 9), true);
            FontTbl[FONT_SMALL]             = new FontHolder(new Font("MS UI Gothic", 9), true);
        }

        private void setupPrinterSet()
        {
            allocGDITbl();

            for (int i = 0; i < PEN_TBL_SIZE; i++)
            {
                PenTbl[i] = new PenHolder(null, false);
            }

            PenTbl[PEN_DEFAULT]             = new PenHolder(new Pen(Brushes.Black, 0), true);
            PenTbl[PEN_DEFAULT_FIGURE]      = new PenHolder(new Pen(Brushes.Black, 0), true);
            PenTbl[PEN_PALE_FIGURE]         = new PenHolder(new Pen(Brushes.Black, 0), true);
            PenTbl[PEN_DIMENTION]           = new PenHolder(new Pen(Brushes.Black, 0), true);

            BrushTbl[BRUSH_DEFAULT]         = new BrushHolder(new SolidBrush(Color.Black), false);
            BrushTbl[BRUSH_BACKGROUND]      = new BrushHolder(null, false);
            BrushTbl[BRUSH_TEXT]            = new BrushHolder(new SolidBrush(Color.Black), false);

            FontTbl[FONT_DEFAULT]           = new FontHolder(new Font("MS UI Gothic", 9), true);
            FontTbl[FONT_SMALL]             = new FontHolder(new Font("MS UI Gothic", 9), true);
        }

        private void setupDarkSetGL()
        {
            allocGLTbl();

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

        public Color PenColor(int id)
        {
            return PenColorTbl[id];
        }

        public Brush brush(int id)
        {
            return BrushTbl[id].ToolObj;
        }

        public Color BrushColor(int id)
        {
            return BrushColorTbl[id];
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
