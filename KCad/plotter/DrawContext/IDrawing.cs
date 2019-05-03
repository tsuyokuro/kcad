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
        void Clear(DrawBrush brush);

        void Draw(List<CadFigure> list, DrawPen pen);

        void DrawSelected(List<CadFigure> list, DrawPen pen);

        void DrawAxis();

        void DrawPageFrame(double w, double h, CadVector center);

        void DrawGrid(Gridding grid);

        void DrawHighlightPoint(CadVector pt, DrawPen pen);

        void DrawSelectedPoint(CadVector pt, DrawPen pen);

        void DrawMarkCursor(DrawPen pen, CadVector p, double pix_size);

        void DrawRect(DrawPen pen, CadVector p0, CadVector p1);

        void DrawCross(DrawPen pen, CadVector p, double size);

        void DrawLine(DrawPen pen, CadVector a, CadVector b);

        void DrawDot(DrawPen pen, CadVector p);

        void DrawFace(DrawPen pen, VectorList pointList);

        void DrawFace(DrawPen pen, VectorList pointList, CadVector normal, bool drawOutline);

        void DrawHarfEdgeModel(DrawPen pen, HeModel model);

        void DrawHarfEdgeModel(DrawPen pen, DrawPen edgePen, double edgeThreshold, HeModel model);

        void DrawText(int font, DrawBrush brush, CadVector a, CadVector xdir, CadVector ydir, DrawTextOption opt, string s);

        void DrawTextScrn(int font, DrawBrush brush, CadVector a, CadVector direction, DrawTextOption opt, string s);

        CadVector MeasureText(int font, string s);

        void DrawArrow(DrawPen pen, CadVector pt0, CadVector pt1, ArrowTypes type, ArrowPos pos, double len, double width);

        void DrawCrossCursorScrn(CadCursor pp, DrawPen pen);

        void DrawRectScrn(DrawPen pen, CadVector p0, CadVector p1);

        void DrawCrossScrn(DrawPen pen, CadVector p, double size);
    }
}
