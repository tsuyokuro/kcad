using HalfEdgeNS;
using System.Collections.Generic;
using OpenTK;
using System;

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

    public interface IDrawing : IDisposable
    {
        void Clear(DrawBrush brush);

        void Draw(List<CadFigure> list, DrawPen pen);

        void DrawSelected(List<CadFigure> list, DrawPen pen);

        void DrawAxis();

        void DrawPageFrame(double w, double h, Vector3d center);

        void DrawGrid(Gridding grid);

        void DrawHighlightPoint(Vector3d pt, DrawPen pen);

        void DrawSelectedPoint(Vector3d pt, DrawPen pen);

        void DrawMarkCursor(DrawPen pen, Vector3d p, double pix_size);

        void DrawRect(DrawPen pen, Vector3d p0, Vector3d p1);

        void DrawCross(DrawPen pen, Vector3d p, double size);

        void DrawLine(DrawPen pen, Vector3d a, Vector3d b);

        void DrawDot(DrawPen pen, Vector3d p);

        //void DrawFace(DrawPen pen, VertexList pointList);

        //void DrawFace(DrawPen pen, VertexList pointList, Vector3d normal, bool drawOutline);

        //void DrawHarfEdgeModel(DrawPen pen, HeModel model);

        void DrawHarfEdgeModel(
            DrawBrush brush, DrawPen pen, DrawPen edgePen, double edgeThreshold, HeModel model);

        void DrawText(int font, DrawBrush brush, Vector3d a, Vector3d xdir, Vector3d ydir, DrawTextOption opt, string s);

        void DrawTextScrn(int font, DrawBrush brush, Vector3d a, Vector3d direction, DrawTextOption opt, string s);

        Vector3d MeasureText(int font, string s);

        void DrawArrow(DrawPen pen, Vector3d pt0, Vector3d pt1, ArrowTypes type, ArrowPos pos, double len, double width);

        void DrawCrossCursorScrn(CadCursor pp, DrawPen pen);

        void DrawRectScrn(DrawPen pen, Vector3d p0, Vector3d p1);

        void DrawCrossScrn(DrawPen pen, Vector3d p, double size);

        void DrawBouncingBox(DrawPen pen, MinMax3D mm);
    }
}
