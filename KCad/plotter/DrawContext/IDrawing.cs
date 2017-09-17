using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public interface IDrawing
    {
        void Clear();

        void Draw(CadLayer layer, int pen = DrawTools.PEN_DEFAULT_FIGURE);

        void Draw(IReadOnlyList<CadFigure> list, int pen = DrawTools.PEN_DEFAULT_FIGURE);

        void DrawSelected(CadLayer layer);

        void DrawAxis();

        void DrawPageFrame();

        void DrawGrid(Gridding grid);

        void DrawHighlightPoint(CadVector pt, int pen = DrawTools.PEN_POINT_HIGHTLITE);

        void DrawSelectedPoint(CadVector pt, int pen = DrawTools.PEN_SLECT_POINT);

        void DrawDownPointCursor(int pen, CadVector p);

        void DrawCursor(CadVector pt);

        void DrawRect(int pen, CadVector p0, CadVector p1);

        void DrawCross(int pen, CadVector p, double size);

        void DrawLine(int pen, CadVector a, CadVector b);

        void DrawDot(int pen, CadVector p);

        void DrawFace(int pen, IReadOnlyList<CadVector> pointList);

        void DrawFace(int pen, IReadOnlyList<CadVector> pointList, CadVector normal, bool drawOutline);

        void DrawCircle(int pen, CadVector cp, CadVector pa, CadVector pb);

        void DrawText(int font, int brush, CadVector a, string s);

        void DrawTextScrn(int font, int brush, CadVector a, CadVector direction, string s);

        CadVector MeasureText(int font, string s);

        void DrawArrow(int pen, CadVector pt0, CadVector pt1, ArrowTypes type, ArrowPos pos, double len, double width);

        void DrawBezier(
            int pen,
            CadVector p0, CadVector p1, CadVector p2);

        void DrawBezier(
            int pen,
            CadVector p0, CadVector p1, CadVector p2, CadVector p3);


        void DrawCursorScrn(CadVector pp);

        void DrawCrossCursorScrn(CadCursor pp);

        void DrawRectScrn(int pen, CadVector p0, CadVector p1);
    }
}
