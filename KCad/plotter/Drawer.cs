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
            dc.graphics.FillRectangle(
                dc.Tools.BackgroundBrush,
                0, 0, (int)dc.ViewWidth, (int)dc.ViewHeight);
        }

        public static void draw(DrawContext context, CadLayer layer)
        {
            draw(context, layer.FigureList);
        }

        public static void draw(DrawContext dc, IReadOnlyList<CadFigure> list, Pen pen=null)
        {
            if (pen == null)
            {
                pen = dc.Tools.DefaultFigurePen;
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
                fig.drawSelected(dc, dc.Tools.DefaultFigurePen);
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

            drawRect(dc, dc.Tools.PageFramePen, p0, p1);
        }

        #region "Draw base"
        public static void drawAxis(DrawContext dc)
        {
            if (dc.Tools.AxesPen == null)
            {
                return;
            }

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

            drawLine(dc, dc.Tools.AxesPen, p0, p1);

            // Y軸
            p0.x = 0;
            p0.y = -100;
            p0.z = 0;

            p1.x = 0;
            p1.y = 100;
            p1.z = 0;

            drawLine(dc, dc.Tools.AxesPen, p0, p1);

            // Z軸
            p0.x = 0;
            p0.y = 0;
            p0.z = -100;

            p1.x = 0;
            p1.y = 0;
            p1.z = 100;

            drawLine(dc, dc.Tools.AxesPen, p0, p1);

            drawAxisDir(dc);
        }

        public static void drawAxisDir(DrawContext dc)
        {
            if (dc.Tools.AxesPen == null)
            {
                return;
            }

            double w = 64;
            double vl = dc.UnitToMilli(w/2);
            double ltvx = 12;
            double ltvy = 12;

            //CadPoint vc = dc.ViewCenter;
            CadPoint vc = default(CadPoint);

            CadPoint p0 = default(CadPoint);
            CadPoint p1 = default(CadPoint);
            CadPoint vp0 = default(CadPoint);
            CadPoint vp1 = default(CadPoint);
            CadPoint d0 = default(CadPoint);
            CadPoint d1 = default(CadPoint);
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
                drawLineScrn(dc, dc.Tools.ArrowAxesPen, vp0, vp1);
            }

            dc.graphics.DrawString("x", dc.Tools.SmallFont, dc.Tools.TextBrush, (int)vp1.x-8, (int)vp1.y);


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
                drawLineScrn(dc, dc.Tools.ArrowAxesPen, vp0, vp1);
            }
            dc.graphics.DrawString("y", dc.Tools.SmallFont, dc.Tools.TextBrush, (int)vp1.x - 8, (int)vp1.y);


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
                drawLineScrn(dc, dc.Tools.ArrowAxesPen, vp0, vp1);
            }
            dc.graphics.DrawString("z", dc.Tools.SmallFont, dc.Tools.TextBrush, (int)vp1.x - 8, (int)vp1.y);
        }

        public static void drawPageFrame(DrawContext dc)
        {
            if (dc.Tools.PageFramePen == null)
            {
                return;
            }

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

            Drawer.drawRectScrn(dc, dc.Tools.PageFramePen, p0, p1);
        }
        #endregion

        #region "Draw marker"
        public static void drawHighlitePoint(DrawContext dc, CadPoint pt)
        {
            CadPoint pp = dc.CadPointToUnitPoint(pt);

            dc.graphics.DrawEllipse(dc.Tools.PointHighlitePen, (int)pp.x - 6, (int)pp.y - 6, 12, 12);
        }

        public static void drawSelectedPoint(DrawContext dc, CadPoint pt)
        {
            CadPoint pp = dc.CadPointToUnitPoint(pt);

            int size = 3;

            dc.graphics.DrawRectangle(
                dc.Tools.SelectedPointPen, (int)pp.x - size, (int)pp.y - size, size * 2, size * 2);
        }

        public static void drawLastPointMarker(DrawContext dc, Pen pen, CadPoint p)
        {
            drawCross(dc, pen, p, 5);
        }
        #endregion

        #region "Draw cursor"

        public static void drawCursor(DrawContext dc, CadPoint pt)
        {
            Pen pen = dc.Tools.CursorPen;
            CadPoint pp = dc.CadPointToUnitPoint(pt);

            //int size = 16;
            int size = (int)Math.Max(dc.ViewWidth, dc.ViewHeight);

            dc.graphics.DrawLine(pen, (int)pp.x - size, (int)pp.y, (int)pp.x + size, (int)pp.y);
            dc.graphics.DrawLine(pen, (int)pp.x, (int)pp.y - size, (int)pp.x, (int)pp.y + size);
        }

        public static void drawCursorScrn(DrawContext dc, CadPoint pp)
        {
            Pen pen = dc.Tools.CursorPen;
            Pen pen2 = dc.Tools.CursorPen2;

            int size = 16;
            int size2 = (int)Math.Max(dc.ViewWidth, dc.ViewHeight);

            dc.graphics.DrawLine(pen2, (int)pp.x - size2, (int)pp.y, (int)pp.x + size2, (int)pp.y);
            dc.graphics.DrawLine(pen2, (int)pp.x, (int)pp.y - size2, (int)pp.x, (int)pp.y + size2);

            dc.graphics.DrawLine(pen, (int)pp.x - size, (int)pp.y, (int)pp.x + size, (int)pp.y);
            dc.graphics.DrawLine(pen, (int)pp.x, (int)pp.y - size, (int)pp.x, (int)pp.y + size);
        }
        #endregion

        #region "draw primitive"
        public static void drawLine(DrawContext dc, Pen pen, CadPoint a, CadPoint b)
        {
            if (dc.graphics == null) return;

            CadPoint pa = dc.CadPointToUnitPoint(a);
            CadPoint pb = dc.CadPointToUnitPoint(b);

            dc.graphics.DrawLine(pen, (int)pa.x, (int)pa.y, (int)pb.x, (int)pb.y);
        }

        public static void drawLineScrn(DrawContext dc, Pen pen, CadPoint a, CadPoint b)
        {
            if (dc.graphics == null) return;
            dc.graphics.DrawLine(pen, (int)a.x, (int)a.y, (int)b.x, (int)b.y);
        }

        public static void drawText(DrawContext dc, Font fnt, Brush brush, CadPoint a, string s)
        {
            if (dc.graphics == null) return;
            CadPoint pa = dc.CadPointToUnitPoint(a);
            dc.graphics.DrawString(s, fnt, brush, (int)pa.x, (int)pa.y);
        }


        /*
        public static void drawLine(DrawContext dc, Pen pen, double x0, double y0, double x1, double y1)
        {
            CadPoint p0 = default(CadPoint);
            CadPoint p1 = default(CadPoint);

            p0.x = x0;
            p0.y = y0;

            p1.x = x1;
            p1.y = y1;

            drawLine(dc, pen, p0, p1);
        }
        */

        public static void drawRect(DrawContext dc, Pen pen, CadPoint p0, CadPoint p1)
        {
            if (dc.graphics == null) return;

            CadPoint pp0 = dc.CadPointToUnitPoint(p0);
            CadPoint pp1 = dc.CadPointToUnitPoint(p1);

            drawRect(dc.graphics, pen, (int)pp0.x, (int)pp0.y, (int)pp1.x, (int)pp1.y);
        }

        public static void drawRectScrn(DrawContext dc, Pen pen, CadPoint pp0, CadPoint pp1)
        {
            drawRect(dc.graphics, pen, (int)pp0.x, (int)pp0.y, (int)pp1.x, (int)pp1.y);
        }

        /*
        public static void drawRect(DrawContext dc, Pen pen, double x0, double y0, double x1, double y1)
        {
            CadPoint p0 = default(CadPoint);
            CadPoint p1 = default(CadPoint);

            p0.x = x0;
            p0.y = y0;
            p1.x = x1;
            p1.y = y1;

            drawRect(dc, pen, p0, p1);
        }
        */

        private static void drawRect(Graphics g, Pen pen, int x0, int y0, int x1, int y1)
        {
            int lx = x0;
            int rx = x1;

            int ty = y0;
            int by = y1;

            if (x0 > x1)
            {
                lx = x1;
                rx = x0;
            }

            if (y0 > y1)
            {
                ty = y1;
                by = y0;
            }


            int dx = rx - lx;
            int dy = by - ty;

            g.DrawRectangle(pen, lx, ty, dx, dy);
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
            DrawContext dc, Pen pen,
            CadPoint p0, CadPoint p1, CadPoint p2)
        {
            if (dc.graphics == null) return;

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
            DrawContext dc, Pen pen,
            CadPoint p0, CadPoint p1, CadPoint p2, CadPoint p3)
        {
            if (dc.graphics == null) return;

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

        public static void drawCircle(DrawContext dc, Pen pen, CadPoint cp, CadPoint p1)
        {
            if (dc.graphics == null) return;

            double r = CadUtil.segNorm(cp, p1);

            CadPoint cpp =  dc.CadPointToUnitPoint(cp);

            r = dc.MilliToUnit(r);

            dc.graphics.DrawEllipse(
                pen, (int)(cpp.x - r), (int)(cpp.y - r), (int)(r*2), (int)(r *2));
        }


        public static void drawCircleScrn(DrawContext dc, Pen pen, CadPoint cp, CadPoint p1)
        {
            if (dc.graphics == null) return;

            double r = CadUtil.segNorm(cp, p1);

            dc.graphics.DrawEllipse(
                pen, (int)(cp.x - r), (int)(cp.y - r), (int)(r * 2), (int)(r * 2));
        }

        public static void drawCircleScrn(DrawContext dc, Pen pen, CadPoint cp, double r)
        {
            if (dc.graphics == null) return;

            dc.graphics.DrawEllipse(
                pen, (int)(cp.x - r), (int)(cp.y - r), (int)(r * 2), (int)(r * 2));
        }

        public static void drawCross(DrawContext dc, Pen pen, CadPoint p, int size)
        {
            if (dc.graphics == null) return;

            CadPoint a = dc.CadPointToUnitPoint(p);

            dc.graphics.DrawLine(pen, (int)a.x - size, (int)a.y + 0, (int)a.x + size, (int)a.y + 0);
            dc.graphics.DrawLine(pen, (int)a.x + 0, (int)a.y + size, (int)a.x + 0, (int)a.y - size);
        }
        #endregion
    }

 }
