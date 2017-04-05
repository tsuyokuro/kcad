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

        public override void Draw(CadLayer layer)
        {
            Draw(layer.FigureList);
        }

        public override void Draw(IReadOnlyList<CadFigure> list, int pen = -1)
        {
            if (pen == -1)
            {
                pen = DrawTools.PEN_DEFAULT_FIGURE;
            }

            foreach (CadFigure fig in list)
            {
                fig.draw(DC, pen);
            }
        }

        public override void DrawSelected(CadLayer layer)
        {
            DrawSelectedFigurePoint(layer.FigureList);
            DrawSelectedRelPoint(layer.RelPointList);
        }

        #region "Draw base"
        public override void DrawAxis()
        {
            CadPoint p0 = default(CadPoint);
            CadPoint p1 = default(CadPoint);

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

            drawAxisDir();
        }

        public override void DrawGrid(Gridding grid)
        {
            CadPoint lt = CadPoint.Zero;
            CadPoint rb = CadPoint.Create(DC.ViewWidth, DC.ViewHeight, 0);

            CadPoint ltw = DC.UnitPointToCadPoint(lt);
            CadPoint rbw = DC.UnitPointToCadPoint(rb);

            double minx = Math.Min(ltw.x, rbw.x);
            double maxx = Math.Max(ltw.x, rbw.x);

            double miny = Math.Min(ltw.y, rbw.y);
            double maxy = Math.Max(ltw.y, rbw.y);

            double minz = Math.Min(ltw.z, rbw.z);
            double maxz = Math.Max(ltw.z, rbw.z);


            int pen = DrawTools.PEN_GRID;

            CadPoint p = default(CadPoint);


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
            CadPoint pt = default(CadPoint);

            // p0
            pt.x = -DC.PageSize.Width / 2;
            pt.y = DC.PageSize.Height / 2;
            pt.z = 0;

            CadPoint p0 = default(CadPoint);
            p0.x = pt.x * DC.UnitPerMilli * DC.DeviceScaleX;
            p0.y = pt.y * DC.UnitPerMilli * DC.DeviceScaleY;

            p0 += DC.ViewOrg;

            // p1
            pt.x = DC.PageSize.Width / 2;
            pt.y = -DC.PageSize.Height / 2;
            pt.z = 0;

            CadPoint p1 = default(CadPoint);
            p1.x = pt.x * DC.UnitPerMilli * DC.DeviceScaleX;
            p1.y = pt.y * DC.UnitPerMilli * DC.DeviceScaleY;

            p1 += DC.ViewOrg;

            DrawRectScrn(DrawTools.PEN_PAGE_FRAME, p0, p1);
        }
        #endregion

        #region "Draw marker"
        public override void DrawHighlightPoint(CadPoint pt)
        {
            CadPoint pp = DC.CadPointToUnitPoint(pt);

            DrawCircleScrn(DrawTools.PEN_POINT_HIGHTLITE, pp, 3);
        }

        public override void DrawSelectedPoint(CadPoint pt)
        {
            CadPoint pp = DC.CadPointToUnitPoint(pt);

            int size = 3;

            DrawRectangleScrn(
                DrawTools.PEN_SLECT_POINT,
                (int)pp.x - size, (int)pp.y - size,
                (int)pp.x + size, (int)pp.y + size
                );
        }

        public override void DrawLastPointMarker(int pen, CadPoint p)
        {
            DrawCross(pen, p, 5);
        }
        #endregion

        #region "Draw cursor"

        public override void DrawCursor(CadPoint pt)
        {
            int pen = DrawTools.PEN_CURSOR;
            CadPoint pp = DC.CadPointToUnitPoint(pt);

            //double size = 16;
            double size = Math.Max(DC.ViewWidth, DC.ViewHeight);

            DrawLineScrn(pen, pp.x - size, pp.y, pp.x + size, pp.y);
            DrawLineScrn(pen, pp.x, pp.y - size, pp.x, pp.y + size);
        }
        #endregion

        #region "draw primitive"
        public override void DrawRect(int pen, CadPoint p0, CadPoint p1)
        {
            CadPoint pp0 = DC.CadPointToUnitPoint(p0);
            CadPoint pp1 = DC.CadPointToUnitPoint(p1);

            DrawRectangleScrn(pen, pp0.x, pp0.y, pp1.x, pp1.y);
        }

        public override void DrawCross(int pen, CadPoint p, int size)
        {
            CadPoint a = DC.CadPointToUnitPoint(p);

            DrawLineScrn(pen, a.x - size, a.y + 0, a.x + size, a.y + 0);
            DrawLineScrn(pen, a.x + 0, a.y + size, a.x + 0, a.y - size);
        }
        #endregion

        public override void DrawLine(int pen, CadPoint a, CadPoint b)
        {
            if (DC.graphics == null) return;
            if (DC.Pen(pen) == null) return;

            CadPoint pa = DC.CadPointToUnitPoint(a);
            CadPoint pb = DC.CadPointToUnitPoint(b);

            DC.graphics.DrawLine(DC.Pen(pen), (int)pa.x, (int)pa.y, (int)pb.x, (int)pb.y);
        }

        public override void DrawDot(int pen, CadPoint p)
        {
            if (DC.graphics == null)
            {
                return;
            }
 
            CadPoint p0 = DC.CadPointToUnitPoint(p);
            CadPoint p1 = p0;
            p0.x = (int)p0.x;
            p1.x = p0.x + 0.1;

            DC.graphics.DrawLine(DC.Pen(pen), (float)p0.x, (float)p0.y, (float)p1.x, (float)p1.y);
        }

        public override void DrawFace(int pen, IReadOnlyList<CadPoint> pointList)
        {
            int cnt = pointList.Count;
            if (cnt == 0)
            {
                return;
            }

            CadPoint p0 = pointList[0];
            CadPoint p1;

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

        public override void DrawText(int font, int brush, CadPoint a, string s)
        {
            if (DC.graphics == null) return;
            if (DC.Brush(brush) == null) return;
            if (DC.Font(font) == null) return;

            CadPoint pa = DC.CadPointToUnitPoint(a);
            DC.graphics.DrawString(s, DC.Font(font), DC.Brush(brush), (int)pa.x, (int)pa.y);
        }

        public override void DrawCursorScrn(CadPoint pp)
        {
            int pen = DrawTools.PEN_CURSOR;
            int pen2 = DrawTools.PEN_CURSOR2;

            double size = 16;
            double size2 = Math.Max(DC.ViewWidth, DC.ViewHeight);

            DrawLineScrn(pen2, pp.x - size2, pp.y, pp.x + size2, pp.y);
            DrawLineScrn(pen2, pp.x, pp.y - size2, pp.x, pp.y + size2);

            DrawLineScrn(pen, pp.x - size, pp.y, pp.x + size, pp.y);
            DrawLineScrn(pen, pp.x, pp.y - size, pp.x, pp.y + size);
        }


        private void DrawSelectedFigurePoint(IReadOnlyList<CadFigure> list)
        {
            foreach (CadFigure fig in list)
            {
                fig.drawSelected(DC, DrawTools.PEN_DEFAULT_FIGURE);
            }
        }

        private void DrawSelectedRelPoint(List<CadRelativePoint> list)
        {
            foreach (CadRelativePoint relp in list)
            {
                relp.drawSelected(DC);
            }
        }

        private void DrawRectScrn(int pen, CadPoint pp0, CadPoint pp1)
        {
            DrawRectangleScrn(pen, pp0.x, pp0.y, pp1.x, pp1.y);
        }

        private void DrawLineScrn(int pen, CadPoint a, CadPoint b)
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

        private void DrawCircleScrn(int pen, CadPoint cp, CadPoint p1)
        {
            if (DC.graphics == null) return;
            if (DC.Pen(pen) == null) return;

            double r = CadUtil.segNorm(cp, p1);
            DrawCircleScrn(pen, cp, r);
        }

        private void DrawCircleScrn(int pen, CadPoint cp, double r)
        {
            if (DC.graphics == null) return;
            if (DC.Pen(pen) == null) return;

            DC.graphics.DrawEllipse(
                DC.Pen(pen), (int)(cp.x - r), (int)(cp.y - r), (int)(r * 2), (int)(r * 2));
        }

        private void DrawTextScrn(int font, int brush, CadPoint a, string s)
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


        private void drawAxisDir()
        {
            double w = 64;
            double vl = DC.UnitToMilli(w / 2);
            double ltvx = 12;
            double ltvy = 12;

            CadPoint vc = default(CadPoint);

            CadPoint p0 = default(CadPoint);
            CadPoint p1 = default(CadPoint);
            CadPoint vp0 = default(CadPoint);
            CadPoint vp1 = default(CadPoint);
            CadPoint c = default(CadPoint);

            // X
            p0 = vc;
            p0.x = -vl;

            p1 = vc;
            p1.x = vl;

            vp0 = DC.CadPointToUnitPoint(p0);
            vp1 = DC.CadPointToUnitPoint(p1);

            c = vp1 - vp0;
            c /= 2.0;

            vp0 = c * -1;
            vp1 = c;

            vp0 += (w / 2) + ltvx;
            vp1 += (w / 2) + ltvy;

            if (c.Norm() > 8)
            {
                DrawLineScrn(DrawTools.PEN_AXIS, vp0, vp1);
            }

            DrawTextScrn(DrawTools.FONT_SMALL, DrawTools.BRUSH_TEXT, vp1.x - 8, vp1.y, "x");


            // Y
            p0 = vc;
            p0.y = -vl;

            p1 = vc;
            p1.y = vl;

            vp0 = DC.CadPointToUnitPoint(p0);
            vp1 = DC.CadPointToUnitPoint(p1);

            c = vp1 - vp0;
            c /= 2.0;

            vp0 = c * -1;
            vp1 = c;

            vp0 += (w / 2) + ltvx;
            vp1 += (w / 2) + ltvy;

            if (c.Norm() > 8)
            {
                DrawLineScrn(DrawTools.PEN_AXIS, vp0, vp1);
            }
            DrawTextScrn(DrawTools.FONT_SMALL, DrawTools.BRUSH_TEXT, vp1.x - 8, vp1.y, "y");


            // Z
            p0 = vc;
            p0.z = -vl;

            p1 = vc;
            p1.z = vl;

            vp0 = DC.CadPointToUnitPoint(p0);
            vp1 = DC.CadPointToUnitPoint(p1);

            c = vp1 - vp0;
            c /= 2.0;

            vp0 = c * -1;
            vp1 = c;

            vp0 += (w / 2) + ltvx;
            vp1 += (w / 2) + ltvy;

            if (c.Norm() > 8)
            {
                DrawLineScrn(DrawTools.PEN_AXIS, vp0, vp1);
            }
            DrawTextScrn(DrawTools.FONT_SMALL, DrawTools.BRUSH_TEXT, vp1.x - 8, vp1.y, "z");
        }
    }
}
