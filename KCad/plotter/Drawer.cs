#define LOG_DEBUG

using System.Collections.Generic;

namespace Plotter
{
    using System;
    using System.Drawing;

    public class Drawer
    {
        public static void clear(DrawContext dc)
        {
            dc.FillRectangleScrn(
                DrawTools.BRUSH_BACKGROUND,
                0, 0, (int)dc.ViewWidth, (int)dc.ViewHeight);
        }

        public static void draw(DrawContext context, CadLayer layer)
        {
            draw(context, layer.FigureList);
        }

        public static void draw(DrawContext dc, IReadOnlyList<CadFigure> list, int pen=-1)
        {
            if (pen == -1)
            {
                pen = DrawTools.PEN_DEFAULT_FIGURE;
            }

            foreach (CadFigure fig in list)
            {
                fig.draw(dc, pen);
            }
        }

        public static void drawSelected(DrawContext dc, CadLayer layer)
        {
            drawSelected(dc, layer.FigureList);
            drawSelected(dc, layer.RelPointList);
        }

        public static void drawSelected(DrawContext dc, IReadOnlyList<CadFigure> list)
        {
            foreach (CadFigure fig in list)
            {
                fig.drawSelected(dc, DrawTools.PEN_DEFAULT_FIGURE);
            }
        }

        public static void drawSelected(DrawContext dc, List<CadRelativePoint> list)
        {
            foreach (CadRelativePoint relp in list)
            {
                relp.drawSelected(dc);
            }
        }

        public static void drawViewRect(DrawContext dc, double s)
        {
            CadRect vr = dc.getViewRect();

            CadPoint vc = CadPoint.GetNew(dc.ViewWidth, dc.ViewHeight, 0);
            vc = vc / 2;
            vc = dc.UnitPointToCadPoint(vc);

            CadPoint d = vr.p1 - vr.p0;

            s = s / 2.0;

            CadPoint nd = d * s;

            CadPoint p0 = vc - nd;
            CadPoint p1 = vc + nd;

            drawRect(dc, DrawTools.PEN_PAGE_FRAME, p0, p1);
        }

        #region "Draw base"
        public static void drawAxis(DrawContext dc)
        {
            CadRect vr = dc.getViewRect();

            //drawViewRect(dc, 0.5);

            CadPoint p0 = default(CadPoint);
            CadPoint p1 = default(CadPoint);

            // X軸
            p0.x = -100;
            p0.y = 0;
            p0.z = 0;

            p1.x = 100;
            p1.y = 0;
            p1.z = 0;

            drawLine(dc, DrawTools.PEN_AXIS, p0, p1);

            // Y軸
            p0.x = 0;
            p0.y = -100;
            p0.z = 0;

            p1.x = 0;
            p1.y = 100;
            p1.z = 0;

            drawLine(dc, DrawTools.PEN_AXIS, p0, p1);

            // Z軸
            p0.x = 0;
            p0.y = 0;
            p0.z = -100;

            p1.x = 0;
            p1.y = 0;
            p1.z = 100;

            drawLine(dc, DrawTools.PEN_AXIS, p0, p1);

            drawAxisDir(dc);
        }

        public static void drawAxisDir(DrawContext dc)
        {
            double w = 64;
            double vl = dc.UnitToMilli(w/2);
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

            vp0 = dc.CadPointToUnitPoint(p0);
            vp1 = dc.CadPointToUnitPoint(p1);

            c = vp1 - vp0;
            c /= 2.0;

            vp0 = c * -1;
            vp1 = c;

            vp0 += (w / 2) + ltvx;
            vp1 += (w / 2) + ltvy;

            if (c.norm() > 8)
            {
                drawLineScrn(dc, DrawTools.PEN_AXIS, vp0, vp1);
            }

            dc.DrawTextScrn(DrawTools.FONT_SMALL, DrawTools.BRUSH_TEXT, vp1.x-8, vp1.y, "x");


            // Y
            p0 = vc;
            p0.y = -vl;

            p1 = vc;
            p1.y = vl;

            vp0 = dc.CadPointToUnitPoint(p0);
            vp1 = dc.CadPointToUnitPoint(p1);

            c = vp1 - vp0;
            c /= 2.0;

            vp0 = c * -1;
            vp1 = c;

            vp0 += (w / 2) + ltvx;
            vp1 += (w / 2) + ltvy;

            if (c.norm() > 8)
            {
                drawLineScrn(dc, DrawTools.PEN_AXIS, vp0, vp1);
            }
            dc.DrawTextScrn(DrawTools.FONT_SMALL, DrawTools.BRUSH_TEXT, vp1.x - 8, vp1.y, "y");


            // Z
            p0 = vc;
            p0.z = -vl;

            p1 = vc;
            p1.z = vl;

            vp0 = dc.CadPointToUnitPoint(p0);
            vp1 = dc.CadPointToUnitPoint(p1);

            c = vp1 - vp0;
            c /= 2.0;

            vp0 = c * -1;
            vp1 = c;

            vp0 += (w / 2) + ltvx;
            vp1 += (w / 2) + ltvy;

            if (c.norm() > 8)
            {
                drawLineScrn(dc, DrawTools.PEN_AXIS, vp0, vp1);
            }
            dc.DrawTextScrn(DrawTools.FONT_SMALL, DrawTools.BRUSH_TEXT, vp1.x - 8, vp1.y, "z");
        }

        public static void drawPageFrame(DrawContext dc)
        {
            CadPoint pt = default(CadPoint);

            // p0
            pt.x = -dc.PageSize.width / 2;
            pt.y = dc.PageSize.height / 2;
            pt.z = 0;

            CadPoint p0 = default(CadPoint);
            p0.x = pt.x * dc.UnitPerMilli;
            p0.y = pt.y * dc.UnitPerMilli * dc.YDir;

            p0 += dc.ViewOrg;

            // p1
            pt.x = dc.PageSize.width / 2;
            pt.y = -dc.PageSize.height / 2;
            pt.z = 0;

            CadPoint p1 = default(CadPoint);
            p1.x = pt.x * dc.UnitPerMilli;
            p1.y = pt.y * dc.UnitPerMilli * dc.YDir;

            p1 += dc.ViewOrg;

            Drawer.drawRectScrn(dc, DrawTools.PEN_PAGE_FRAME, p0, p1);
        }
        #endregion

        #region "Draw marker"
        public static void drawHighlitePoint(DrawContext dc, CadPoint pt)
        {
            CadPoint pp = dc.CadPointToUnitPoint(pt);

            dc.DrawCircleScrn(DrawTools.PEN_POINT_HIGHTLITE, pp, 3);
        }

        public static void drawSelectedPoint(DrawContext dc, CadPoint pt)
        {
            CadPoint pp = dc.CadPointToUnitPoint(pt);

            int size = 3;

            dc.DrawRectangleScrn(
                DrawTools.PEN_SLECT_POINT,
                (int)pp.x - size, (int)pp.y - size,
                (int)pp.x + size, (int)pp.y + size
                );
        }

        public static void drawLastPointMarker(DrawContext dc, int pen, CadPoint p)
        {
            drawCross(dc, pen, p, 5);
        }
        #endregion

        #region "Draw cursor"

        public static void drawCursor(DrawContext dc, CadPoint pt)
        {
            int pen = DrawTools.PEN_CURSOR;
            CadPoint pp = dc.CadPointToUnitPoint(pt);

            //double size = 16;
            double size = Math.Max(dc.ViewWidth, dc.ViewHeight);

            dc.DrawLineScrn(pen, pp.x - size, pp.y, pp.x + size, pp.y);
            dc.DrawLineScrn(pen, pp.x, pp.y - size, pp.x, pp.y + size);
        }

        public static void drawCursorScrn(DrawContext dc, CadPoint pp)
        {
            int pen = DrawTools.PEN_CURSOR;
            int pen2 = DrawTools.PEN_CURSOR2;

            double size = 16;
            double size2 = Math.Max(dc.ViewWidth, dc.ViewHeight);

            dc.DrawLineScrn(pen2, pp.x - size2, pp.y, pp.x + size2, pp.y);
            dc.DrawLineScrn(pen2, pp.x, pp.y - size2, pp.x, pp.y + size2);

            dc.DrawLineScrn(pen, pp.x - size, pp.y, pp.x + size, pp.y);
            dc.DrawLineScrn(pen, pp.x, pp.y - size, pp.x, pp.y + size);
        }
        #endregion

        #region "draw primitive"
        public static void drawLine(DrawContext dc, int pen, CadPoint a, CadPoint b)
        {
            dc.DrawLine(pen, a, b);
        }

        public static void drawLineScrn(DrawContext dc, int pen, CadPoint a, CadPoint b)
        {
            dc.DrawLineScrn(pen, a, b);
        }

        public static void drawText(DrawContext dc, int font, int brush, CadPoint a, string s)
        {
            dc.DrawText(font, brush, a, s);
        }

        public static void drawRect(DrawContext dc, int pen, CadPoint p0, CadPoint p1)
        {
            CadPoint pp0 = dc.CadPointToUnitPoint(p0);
            CadPoint pp1 = dc.CadPointToUnitPoint(p1);

            dc.DrawRectangleScrn(pen, pp0.x, pp0.y, pp1.x, pp1.y);
        }

        public static void drawRectScrn(DrawContext dc, int pen, CadPoint pp0, CadPoint pp1)
        {
            dc.DrawRectangleScrn(pen, pp0.x, pp0.y, pp1.x, pp1.y);
        }


        /*
        public static void drawBezier(
            DrawContext dc, Pen pen,
            CadPoint p0, CadPoint p1, CadPoint p2)
        {
            CadPoint t0 = p0;
            CadPoint t1 = p0;

            double t = 0;
            double d = 1.0 / 32;

            t = d;

            while (t <= 1.0)
            {
                t1.x =
                    (1.0 - t) * (1.0 - t) * p0.x +
                    2.0 * (1.0 - t) * t * p1.x + t * t * p2.x;
                t1.y =
                    (1.0 - t) * (1.0 - t) * p0.y +
                    2.0 * (1.0 - t) * t * p1.y + t * t * p2.y;

                Drawer.drawLine(dc, pen, t0, t1);

                t0 = t1;

                t += d;
            }
        }
        */

        /*
        public static void drawBezier(
            DrawContext dc, Pen pen,
            CadPoint p0, CadPoint p1, CadPoint p2, CadPoint p3)
        {
            CadPoint t0 = p0;
            CadPoint t1 = p0;

            double t = 0;
            double d = 1.0 / 64;

            t = d;

            while (t <= 1.0)
            {
                t1.x =
                    (1.0 - t) * (1.0 - t) * (1.0 - t) * p0.x +
                    3.0 * (1.0 - t) * (1.0 - t) * t * p1.x +
                    3.0 * (1.0 - t) * t * t * p2.x + t * t * t * p3.x;
                t1.y =
                    (1.0 - t) * (1.0 - t) * (1.0 - t) * p0.y +
                    3.0 * (1.0 - t) * (1.0 - t) * t * p1.y +
                    3.0 * (1.0 - t) * t * t * p2.y + t * t * t * p3.y;

                Drawer.drawLine(dc, pen, t0, t1);

                t0 = t1;

                t += d;
            }
        }
        */


        public static void drawBezier(
            DrawContext dc, int pen,
            CadPoint p0, CadPoint p1, CadPoint p2)
        {
            double t = 0;
            double d = 1.0 / 64;

            t = d;

            int n = 3;

            CadPoint t0 = p0;
            CadPoint t1 = p0;

            while (t <= 1.0)
            {
                t1 = default(CadPoint);
                t1 += p0 * CadMath.BernsteinBasisF(n - 1, 0, t);
                t1 += p1 * CadMath.BernsteinBasisF(n - 1, 1, t);
                t1 += p2 * CadMath.BernsteinBasisF(n - 1, 2, t);

                Drawer.drawLine(dc, pen, t0, t1);

                t0 = t1;

                t += d;
            }
        }

        public static void drawBezier(
            DrawContext dc, int pen,
            CadPoint p0, CadPoint p1, CadPoint p2, CadPoint p3)
        {
            double t = 0;
            double d = 1.0 / 64;

            t = d;

            int n = 4;

            CadPoint t0 = p0;
            CadPoint t1 = p0;

            while (t <= 1.0)
            {
                t1 = default(CadPoint);
                t1 += p0 * CadMath.BernsteinBasisF(n - 1, 0, t);
                t1 += p1 * CadMath.BernsteinBasisF(n - 1, 1, t);
                t1 += p2 * CadMath.BernsteinBasisF(n - 1, 2, t);
                t1 += p3 * CadMath.BernsteinBasisF(n - 1, 3, t);

                Drawer.drawLine(dc, pen, t0, t1);

                t0 = t1;

                t += d;
            }
        }

        public static void drawCircle(DrawContext dc, int pen, CadPoint cp, CadPoint p1)
        {
            dc.DrawCircle(pen, cp, p1);
        }


        public static void drawCircleScrn(DrawContext dc, int pen, CadPoint cp, CadPoint p1)
        {
            dc.DrawCircleScrn(pen, cp, p1);
        }

        public static void drawCircleScrn(DrawContext dc, int pen, CadPoint cp, double r)
        {
            CadPoint p1 = cp;
            p1.x += r;
            dc.DrawCircleScrn(pen, cp, p1);
        }

        public static void drawCross(DrawContext dc, int pen, CadPoint p, int size)
        {
            CadPoint a = dc.CadPointToUnitPoint(p);

            dc.DrawLineScrn(pen, a.x - size, a.y + 0, a.x + size, a.y + 0);
            dc.DrawLineScrn(pen, a.x + 0, a.y + size, a.x + 0, a.y - size);
        }
        #endregion
    }

 }
