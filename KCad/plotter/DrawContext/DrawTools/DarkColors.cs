using System.Drawing;

namespace Plotter
{
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
            PenColorTbl[DrawTools.PEN_CURSOR2] = Color.FromArgb(32, 64, 64);
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
            PenColorTbl[DrawTools.PEN_POINT_HIGHLIGHT2] = Color.FromArgb(64, 255, 255);
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
            PenColorTbl[DrawTools.PEN_NORMAL] = Color.FromArgb(0xFF, 0x00, 0xff, 0x7f);
            PenColorTbl[DrawTools.PEN_EXT_SNAP] = Color.FromArgb(0xFF, 0xff, 0x00, 0x00);
            PenColorTbl[DrawTools.PEN_HANDLE_LINE] = Color.YellowGreen;


            BrushColorTbl = new Color[DrawTools.BRUSH_TBL_SIZE];

            BrushColorTbl[DrawTools.BRUSH_DEFAULT] = Color.FromArgb(255, 255, 255);
            BrushColorTbl[DrawTools.BRUSH_BACKGROUND] = Color.FromArgb(0x8, 0x8, 0x8);
            BrushColorTbl[DrawTools.BRUSH_TEXT] = Color.White;
            BrushColorTbl[DrawTools.BRUSH_DEFAULT_MESH_FILL] = Color.FromArgb(204,204,204);
            BrushColorTbl[DrawTools.BRUSH_TRANSPARENT] = Color.FromArgb(0,0,0,0);
            BrushColorTbl[DrawTools.BRUSH_PALE_TEXT] = Color.FromArgb(0x7E, 0x7E, 0x7E);
        }
    }
}
