using OpenTK;
using System;
using System.Collections.Generic;

namespace Plotter
{
    public class DrawingGDI : DrawingBase
    {
        public DrawContextGDI DC;

        public DrawingGDI(DrawContextGDI dc)
        {
            DC = dc;
        }

        public override void Clear()
        {
            FillRectangleScrn(
                DrawTools.BRUSH_BACKGROUND,
                0, 0, (int)DC.ViewWidth, (int)DC.ViewHeight);
        }

        public override void Draw(CadLayer layer, int pen = DrawTools.PEN_DEFAULT_FIGURE)
        {
            Draw(layer.FigureList, pen);
        }

        public override void Draw(IReadOnlyList<CadFigure> list, int pen = DrawTools.PEN_DEFAULT_FIGURE)
        {
            foreach (CadFigure fig in list)
            {
                if (fig.Current)
                {
                    fig.Draw(DC, DrawTools.PEN_FIGURE_HIGHLIGHT);
                }
                else
                {
                    fig.Draw(DC, pen);
                }
            }
        }

        public override void DrawSelected(CadLayer layer)
        {
            DrawSelectedFigurePoint(layer.FigureList);
        }

        #region "Draw base"
        public override void DrawAxis()
        {
            CadVector p0 = default(CadVector);
            CadVector p1 = default(CadVector);

            // X軸
            p0.x = -100;
            p0.y = 0;
            p0.z = 0;

            p1.x = 100;
            p1.y = 0;
            p1.z = 0;

            DrawLine(DrawTools.PEN_AXIS, p0, p1);

            // Y軸
            p0.x = 0;
            p0.y = -100;
            p0.z = 0;

            p1.x = 0;
            p1.y = 100;
            p1.z = 0;

            DrawLine(DrawTools.PEN_AXIS, p0, p1);

            // Z軸
            p0.x = 0;
            p0.y = 0;
            p0.z = -100;

            p1.x = 0;
            p1.y = 0;
            p1.z = 100;

            DrawLine(DrawTools.PEN_AXIS, p0, p1);

            DrawAxis2();
        }

        public override void DrawGrid(Gridding grid)
        {
            CadVector lt = CadVector.Zero;
            CadVector rb = CadVector.Create(DC.ViewWidth, DC.ViewHeight, 0);

            CadVector ltw = DC.UnitPointToCadPoint(lt);
            CadVector rbw = DC.UnitPointToCadPoint(rb);

            double minx = Math.Min(ltw.x, rbw.x);
            double maxx = Math.Max(ltw.x, rbw.x);

            double miny = Math.Min(ltw.y, rbw.y);
            double maxy = Math.Max(ltw.y, rbw.y);

            double minz = Math.Min(ltw.z, rbw.z);
            double maxz = Math.Max(ltw.z, rbw.z);


            int pen = DrawTools.PEN_GRID;

            CadVector p = default(CadVector);


            double n = grid.Decimate(DC, grid, 8);

            double x, y, z;
            double sx, sy, sz;
            double szx = grid.GridSize.x * n;
            double szy = grid.GridSize.y * n;
            double szz = grid.GridSize.z * n;

            sx = Math.Round(minx / szx) * szx;
            sy = Math.Round(miny / szy) * szy;
            sz = Math.Round(minz / szz) * szz;

            x = sx;
            while (x < maxx)
            {
                p.x = x;
                p.z = 0;

                y = sy;

                while (y < maxy)
                {
                    p.y = y;
                    DrawDot(pen, p);
                    y += szy;
                }

                x += szx;
            }

            z = sz;
            y = sy;

            while (z < maxz)
            {
                p.z = z;
                p.x = 0;

                y = sy;

                while (y < maxy)
                {
                    p.y = y;
                    DrawDot(pen, p);
                    y += szy;
                }

                z += szz;
            }

            z = sz;
            x = sx;

            while (x < maxx)
            {
                p.x = x;
                p.y = 0;

                z = sz;

                while (z < maxz)
                {
                    p.z = z;
                    DrawDot(pen, p);
                    z += szz;
                }

                x += szx;
            }
        }

        public override void DrawPageFrame()
        {
            CadVector pt = default(CadVector);

            // p0
            pt.x = -DC.PageSize.Width / 2;
            pt.y = DC.PageSize.Height / 2;
            pt.z = 0;

            CadVector p0 = default(CadVector);
            p0.x = pt.x * DC.UnitPerMilli * DC.DeviceScaleX;
            p0.y = pt.y * DC.UnitPerMilli * DC.DeviceScaleY;

            p0 += DC.ViewOrg;

            // p1
            pt.x = DC.PageSize.Width / 2;
            pt.y = -DC.PageSize.Height / 2;
            pt.z = 0;

            CadVector p1 = default(CadVector);
            p1.x = pt.x * DC.UnitPerMilli * DC.DeviceScaleX;
            p1.y = pt.y * DC.UnitPerMilli * DC.DeviceScaleY;

            p1 += DC.ViewOrg;

            DrawRectScrn(DrawTools.PEN_PAGE_FRAME, p0, p1);
        }
        #endregion

        #region "Draw marker"
        public override void DrawHighlightPoint(CadVector pt, int pen = DrawTools.PEN_POINT_HIGHTLITE)
        {
            CadVector pp = DC.CadPointToUnitPoint(pt);

            DrawCircleScrn(pen, pp, 3);
        }

        public override void DrawSelectedPoint(CadVector pt, int pen = DrawTools.PEN_SLECT_POINT)
        {
            CadVector pp = DC.CadPointToUnitPoint(pt);

            int size = 3;

            DrawRectangleScrn(
                pen,
                (int)pp.x - size, (int)pp.y - size,
                (int)pp.x + size, (int)pp.y + size
                );
        }

        public override void DrawDownPointCursor(int pen, CadVector p)
        {
            DrawCross(pen, p, 10);
        }
        #endregion

        #region "Draw cursor"

        public override void DrawCursor(CadVector pt)
        {
            int pen = DrawTools.PEN_CURSOR;
            CadVector pp = DC.CadPointToUnitPoint(pt);

            //double size = 16;
            double size = Math.Max(DC.ViewWidth, DC.ViewHeight);

            DrawLineScrn(pen, pp.x - size, pp.y, pp.x + size, pp.y);
            DrawLineScrn(pen, pp.x, pp.y - size, pp.x, pp.y + size);
        }
        #endregion

        #region "draw primitive"
        public override void DrawRect(int pen, CadVector p0, CadVector p1)
        {
            CadVector pp0 = DC.CadPointToUnitPoint(p0);
            CadVector pp1 = DC.CadPointToUnitPoint(p1);

            DrawRectangleScrn(pen, pp0.x, pp0.y, pp1.x, pp1.y);
        }

        public override void DrawCross(int pen, CadVector p, double size)
        {
            CadVector a = DC.CadPointToUnitPoint(p);

            DrawLineScrn(pen, a.x - size, a.y + 0, a.x + size, a.y + 0);
            DrawLineScrn(pen, a.x + 0, a.y + size, a.x + 0, a.y - size);
        }
        #endregion

        public override void DrawLine(int pen, CadVector a, CadVector b)
        {
            if (DC.graphics == null) return;
            if (DC.Pen(pen) == null) return;

            CadVector pa = DC.CadPointToUnitPoint(a);
            CadVector pb = DC.CadPointToUnitPoint(b);

            DC.graphics.DrawLine(DC.Pen(pen), (int)pa.x, (int)pa.y, (int)pb.x, (int)pb.y);
        }

        public override void DrawDot(int pen, CadVector p)
        {
            if (DC.graphics == null)
            {
                return;
            }
 
            CadVector p0 = DC.CadPointToUnitPoint(p);
            CadVector p1 = p0;
            p0.x = (int)p0.x;
            p1.x = p0.x + 0.1;

            DC.graphics.DrawLine(DC.Pen(pen), (float)p0.x, (float)p0.y, (float)p1.x, (float)p1.y);
        }

        public override void DrawFace(int pen, IReadOnlyList<CadVector> pointList, CadVector Normal)
        {
            int cnt = pointList.Count;
            if (cnt == 0)
            {
                return;
            }

            CadVector p0 = pointList[0];
            CadVector p1;

            int i;
            for (i = 1; i < cnt; i++)
            {
                p1 = pointList[i];
                DrawLine(pen, p0, p1);
                p0 = p1;
            }

            p1 = pointList[0];
            DrawLine(pen, p0, p1);
        }

        public override void DrawText(int font, int brush, CadVector a, string s)
        {
            if (DC.graphics == null) return;
            if (DC.Brush(brush) == null) return;
            if (DC.Font(font) == null) return;

            CadVector pa = DC.CadPointToUnitPoint(a);
            DC.graphics.DrawString(s, DC.Font(font), DC.Brush(brush), (int)pa.x, (int)pa.y);
        }

        public override void DrawCursorScrn(CadVector pp)
        {
            int pen = DrawTools.PEN_CURSOR;

            double size = 16;

            DrawLineScrn(pen, pp.x - size, pp.y, pp.x + size, pp.y);
            DrawLineScrn(pen, pp.x, pp.y - size, pp.x, pp.y + size);
        }

        public override void DrawCrossCursorScrn(CadVector pp)
        {
            int pen = DrawTools.PEN_CURSOR2;

            double size = Math.Max(DC.ViewWidth, DC.ViewHeight);

            DrawLineScrn(pen, pp.x - size, pp.y, pp.x + size, pp.y);
            DrawLineScrn(pen, pp.x, pp.y - size, pp.x, pp.y + size);
        }

        private void DrawSelectedFigurePoint(IReadOnlyList<CadFigure> list)
        {
            foreach (CadFigure fig in list)
            {
                fig.DrawSelected(DC, DrawTools.PEN_DEFAULT_FIGURE);
            }
        }

        private void DrawRectScrn(int pen, CadVector pp0, CadVector pp1)
        {
            DrawRectangleScrn(pen, pp0.x, pp0.y, pp1.x, pp1.y);
        }

        private void DrawLineScrn(int pen, CadVector a, CadVector b)
        {
            if (DC.graphics == null) return;
            if (DC.Pen(pen) == null) return;

            DC.graphics.DrawLine(DC.Pen(pen), (int)a.x, (int)a.y, (int)b.x, (int)b.y);
        }

        private void DrawLineScrn(int pen, double x1, double y1, double x2, double y2)
        {
            if (DC.graphics == null) return;
            if (DC.Pen(pen) == null) return;

            DC.graphics.DrawLine(DC.Pen(pen), (int)x1, (int)y1, (int)x2, (int)y2);
        }

        private void DrawRectangleScrn(int pen, double x0, double y0, double x1, double y1)
        {
            if (DC.graphics == null) return;
            if (DC.Pen(pen) == null) return;

            int lx = (int)x0;
            int rx = (int)x1;

            int ty = (int)y0;
            int by = (int)y1;

            if (x0 > x1)
            {
                lx = (int)x1;
                rx = (int)x0;
            }

            if (y0 > y1)
            {
                ty = (int)y1;
                by = (int)y0;
            }

            int dx = rx - lx;
            int dy = by - ty;

            DC.graphics.DrawRectangle(DC.Pen(pen), lx, ty, dx, dy);
        }

        private void DrawCircleScrn(int pen, CadVector cp, CadVector p1)
        {
            if (DC.graphics == null) return;
            if (DC.Pen(pen) == null) return;

            double r = CadUtil.segNorm(cp, p1);
            DrawCircleScrn(pen, cp, r);
        }

        private void DrawCircleScrn(int pen, CadVector cp, double r)
        {
            if (DC.graphics == null) return;
            if (DC.Pen(pen) == null) return;

            DC.graphics.DrawEllipse(
                DC.Pen(pen), (int)(cp.x - r), (int)(cp.y - r), (int)(r * 2), (int)(r * 2));
        }

        private void DrawTextScrn(int font, int brush, CadVector a, string s)
        {
            if (DC.graphics == null) return;
            if (DC.Brush(brush) == null) return;
            if (DC.Font(font) == null) return;

            DC.graphics.DrawString(s, DC.Font(font), DC.Brush(brush), (int)a.x, (int)a.y);
        }

        private void DrawTextScrn(int font, int brush, double x, double y, string s)
        {
            if (DC.graphics == null) return;
            if (DC.Brush(brush) == null) return;
            if (DC.Font(font) == null) return;

            DC.graphics.DrawString(s, DC.Font(font), DC.Brush(brush), (int)x, (int)y);
        }


        private void FillRectangleScrn(int brush, double x0, double y0, double x1, double y1)
        {
            if (DC.graphics == null) return;
            if (DC.Brush(brush) == null) return;

            int lx = (int)x0;
            int rx = (int)x1;

            int ty = (int)y0;
            int by = (int)y1;

            if (x0 > x1)
            {
                lx = (int)x1;
                rx = (int)x0;
            }

            if (y0 > y1)
            {
                ty = (int)y1;
                by = (int)y0;
            }

            int dx = rx - lx;
            int dy = by - ty;

            DC.graphics.FillRectangle(DC.Brush(brush), lx, ty, dx, dy);
        }

        private void DrawAxis2()
        {
            double size = 20;


            CadVector uv = CadVector.Create(size, 0, 0);

            CadVector cv = DC.UnitVectorToCadVector(uv);

            double len = cv.Norm();


            CadVector up = CadVector.Create(size+5, size+5, 0);

            CadVector cp = DC.UnitPointToCadPoint(up);


            CadVector p0 = default(CadVector);
            CadVector p1 = default(CadVector);



            // X軸
            p0.x = -len + cp.x;
            p0.y = 0 + cp.y;
            p0.z = 0 + cp.z;

            p1.x = len + cp.x;
            p1.y = 0 + cp.y;
            p1.z = 0 + cp.z;

            DrawLine(DrawTools.PEN_AXIS2, p0, p1);

            DrawText(DrawTools.FONT_SMALL, DrawTools.BRUSH_TEXT, p1, "x");

            // Y軸
            p0.x = 0 + cp.x;
            p0.y = -len + cp.y;
            p0.z = 0 + cp.z;

            p1.x = 0 + cp.x;
            p1.y = len + cp.y;
            p1.z = 0 + cp.z;

            DrawLine(DrawTools.PEN_AXIS2, p0, p1);
            DrawText(DrawTools.FONT_SMALL, DrawTools.BRUSH_TEXT, p1, "y");

            // Z軸
            p0.x = 0 + cp.x;
            p0.y = 0 + cp.y;
            p0.z = -len + cp.z;

            p1.x = 0 + cp.x;
            p1.y = 0 + cp.y;
            p1.z = len + cp.z;

            DrawLine(DrawTools.PEN_AXIS2, p0, p1);
            DrawText(DrawTools.FONT_SMALL, DrawTools.BRUSH_TEXT, p1, "z");
        }
    }
}
