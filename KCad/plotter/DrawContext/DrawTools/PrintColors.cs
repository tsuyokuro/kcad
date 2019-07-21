using System.Drawing;

namespace Plotter
{
    public class PrintColors
    {
        public static Color[] PenColorTbl;
        public static Color[] BrushColorTbl;

        static PrintColors()
        {
            PenColorTbl = new Color[DrawTools.PEN_TBL_SIZE];

            PenColorTbl[DrawTools.PEN_DEFAULT] = Color.Black;
            PenColorTbl[DrawTools.PEN_SELECT_POINT] = Color.Black;
            PenColorTbl[DrawTools.PEN_CURSOR] = Color.Black;
            PenColorTbl[DrawTools.PEN_CURSOR2] = Color.Gray;
            PenColorTbl[DrawTools.PEN_DEFAULT_FIGURE] = Color.Black;
            PenColorTbl[DrawTools.PEN_TEMP_FIGURE] = Color.Black;
            PenColorTbl[DrawTools.PEN_POINT_HIGHLIGHT] = Color.Black;
            PenColorTbl[DrawTools.PEN_MATCH_FIGURE] = Color.Black;
            PenColorTbl[DrawTools.PEN_MATCH_SEG] = Color.Black;
            PenColorTbl[DrawTools.PEN_LAST_POINT_MARKER] = Color.Black;
            PenColorTbl[DrawTools.PEN_LAST_POINT_MARKER2] = Color.Black;
            PenColorTbl[DrawTools.PEN_AXIS] = Color.Black;
            PenColorTbl[DrawTools.PEN_ARROW_AXIS] = Color.Black;
            PenColorTbl[DrawTools.PEN_PAGE_FRAME] = Color.Black;
            PenColorTbl[DrawTools.PEN_RELATIVE_POINT] = Color.Black;
            PenColorTbl[DrawTools.PEN_TEST_FIGURE] = Color.Black;
            PenColorTbl[DrawTools.PEN_GRID] = Color.Black;
            PenColorTbl[DrawTools.PEN_POINT_HIGHLIGHT2] = Color.Black;
            PenColorTbl[DrawTools.PEN_FIGURE_HIGHLIGHT] = Color.Black;
            PenColorTbl[DrawTools.PEN_AXIS2] = Color.Black;
            PenColorTbl[DrawTools.PEN_PALE_FIGURE] = Color.Gray;
            PenColorTbl[DrawTools.PEN_MEASURE_FIGURE] = Color.Black;
            PenColorTbl[DrawTools.PEN_DIMENTION] = Color.Black;
            PenColorTbl[DrawTools.PEN_BLACK] = Color.Black;
            PenColorTbl[DrawTools.PEN_MESH_LINE] = Color.Gray;
            PenColorTbl[DrawTools.PEN_TEST] = Color.Black;
            PenColorTbl[DrawTools.PEN_NURBS_CTRL_LINE] = Color.Black;
            PenColorTbl[DrawTools.PEN_LINE_SNAP] = Color.Black;
            PenColorTbl[DrawTools.PEN_DRAG_LINE] = Color.Black;
            PenColorTbl[DrawTools.PEN_NORMAL] = Color.Black;
            PenColorTbl[DrawTools.PEN_EXT_SNAP] = Color.Transparent;


            BrushColorTbl = new Color[DrawTools.BRUSH_TBL_SIZE];

            BrushColorTbl[DrawTools.BRUSH_DEFAULT] = Color.Gray;
            BrushColorTbl[DrawTools.BRUSH_BACKGROUND] = Color.Transparent;
            BrushColorTbl[DrawTools.BRUSH_TEXT] = Color.Black;
            BrushColorTbl[DrawTools.BRUSH_DEFAULT_MESH_FILL] = Color.LightGray;
            BrushColorTbl[DrawTools.BRUSH_TRANSPARENT] = Color.Transparent;
        }
    }
}
