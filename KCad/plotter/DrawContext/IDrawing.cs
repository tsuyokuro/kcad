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

        void DrawPageFrame(double w, double h, CadVertex center);

        void DrawGrid(Gridding grid);

        void DrawHighlightPoint(CadVertex pt, DrawPen pen);

        void DrawSelectedPoint(CadVertex pt, DrawPen pen);

        void DrawMarkCursor(DrawPen pen, CadVertex p, double pix_size);

        void DrawRect(DrawPen pen, CadVertex p0, CadVertex p1);

        void DrawCross(DrawPen pen, CadVertex p, double size);

        void DrawLine(DrawPen pen, CadVertex a, CadVertex b);

        void DrawDot(DrawPen pen, CadVertex p);

        //void DrawFace(DrawPen pen, VertexList pointList);

        //void DrawFace(DrawPen pen, VertexList pointList, CadVertex normal, bool drawOutline);

        void DrawHarfEdgeModel(DrawPen pen, HeModel model);

        void DrawHarfEdgeModel(DrawPen pen, DrawPen edgePen, double edgeThreshold, HeModel model);

        void DrawText(int font, DrawBrush brush, CadVertex a, CadVertex xdir, CadVertex ydir, DrawTextOption opt, string s);

        void DrawTextScrn(int font, DrawBrush brush, CadVertex a, CadVertex direction, DrawTextOption opt, string s);

        CadVertex MeasureText(int font, string s);

        void DrawArrow(DrawPen pen, CadVertex pt0, CadVertex pt1, ArrowTypes type, ArrowPos pos, double len, double width);

        void DrawCrossCursorScrn(CadCursor pp, DrawPen pen);

        void DrawRectScrn(DrawPen pen, CadVertex p0, CadVertex p1);

        void DrawCrossScrn(DrawPen pen, CadVertex p, double size);
    }
}
