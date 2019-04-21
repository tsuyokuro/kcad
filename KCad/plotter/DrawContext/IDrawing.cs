using HalfEdgeNS;
using System.Collections.Generic;
using CadDataTypes;

namespace Plotter
{
    public struct DrawTextOption
    {
        public const uint H_CENTER = 1;

        public uint Option;

        public DrawTextOption(uint option)
        {
            Option = option;
        }
    }

    public interface IDrawing
    {
        void Clear(int brush = DrawTools.BRUSH_BACKGROUND);

        void Draw(List<CadFigure> list, int pen = DrawTools.PEN_DEFAULT_FIGURE);

        void DrawSelected(List<CadFigure> list, int pen = DrawTools.PEN_DEFAULT_FIGURE);

        void DrawAxis();

        void DrawPageFrame(double w, double h, CadVector center);

        void DrawGrid(Gridding grid);

        void DrawHighlightPoint(CadVector pt, int pen = DrawTools.PEN_POINT_HIGHLIGHT);

        void DrawSelectedPoint(CadVector pt, int pen = DrawTools.PEN_SELECT_POINT);

        void DrawMarkCursor(int pen, CadVector p, double pix_size);

        void DrawRect(int pen, CadVector p0, CadVector p1);

        void DrawCross(int pen, CadVector p, double size);

        void DrawLine(int pen, CadVector a, CadVector b);

        void DrawDot(int pen, CadVector p);

        void DrawFace(int pen, VectorList pointList);

        void DrawFace(int pen, VectorList pointList, CadVector normal, bool drawOutline);

        void DrawHarfEdgeModel(int pen, HeModel model);

        void DrawHarfEdgeModel(int pen, int edgePen, double edgeThreshold, HeModel model);

        void DrawText(int font, int brush, CadVector a, CadVector xdir, CadVector ydir, DrawTextOption opt, string s);

        void DrawTextScrn(int font, int brush, CadVector a, CadVector direction, DrawTextOption opt, string s);

        CadVector MeasureText(int font, string s);

        void DrawArrow(int pen, CadVector pt0, CadVector pt1, ArrowTypes type, ArrowPos pos, double len, double width);

        void DrawCrossCursorScrn(CadCursor pp, int pen = DrawTools.PEN_CURSOR2);

        void DrawRectScrn(int pen, CadVector p0, CadVector p1);

        void DrawCrossScrn(int pen, CadVector p, double size);
    }
}
