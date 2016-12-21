#define LOG_DEBUG

using System.Collections.Generic;

namespace Plotter
{
    using System.Drawing;

    public class Drawer
    {
        public static void clear(DrawContext dc)
        {
            dc.graphics.FillRectangle(
                dc.Tools.BackgroundBrush,
                0, 0, dc.ViewWidth, dc.ViewHeight);
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

        #region "Draw base"
        public static void drawAxis(DrawContext dc)
        {
            if (dc.Tools.AxesPen == null)
            {
                return;
            }

            CadRect vr = dc.getViewRect();

            CadPoint p0;
            CadPoint p1;

            // X軸
            p0 = vr.p0;
            p0.y = p0.z = 0;

            p1 = vr.p1;
            p1.y = p1.z = 0;

            Drawer.drawLine(dc, dc.Tools.AxesPen, p0, p1);

            // Y軸
            p0 = vr.p0;
            p0.x = p0.z = 0;

            p1 = vr.p1;
            p1.x = p1.z = 0;

            Drawer.drawLine(dc, dc.Tools.AxesPen, p0, p1);

            // Z軸
            p0 = vr.p0;
            p0.x = p0.y = 0;

            p1 = vr.p1;
            p1.x = p1.y = 0;

            Drawer.drawLine(dc, dc.Tools.AxesPen, p0, p1);

            drawAxisDir(dc);
        }

        public static void drawAxisDir(DrawContext dc)
        {
            if (dc.Tools.AxesPen == null)
            {
                return;
            }

            CadPoint rp0 = dc.pixelPointToCadPoint(16, 16);
            CadPoint rpc = dc.pixelPointToCadPoint(16 + 40, 16 + 40);
            CadPoint rp1 = dc.pixelPointToCadPoint(16 + 80, 16 + 80);

            Drawer.drawRect(dc, dc.Tools.AxesPen, rp0, rp1);

            CadPoint p0;
            CadPoint p1;

            CadPoint np = default(CadPoint);
            CadPoint pp = default(CadPoint);

            // X軸
            p0 = rp0;
            p0.y = rpc.y;
            p0.z = rpc.z;

            p1 = rp1;
            p1.y = rpc.y;
            p1.z = rpc.z;

            drawLine(dc, dc.Tools.AxesPen, p0, p1);

            np = p0.x > p1.x ? p0 : p1;

            pp = dc.pointToPixelPoint(np);
            if (pp.x >= 16 + 80) pp.x -= 9;
            if (pp.y >= 16 + 80) pp.y -= 9;
            dc.graphics.DrawString("x", dc.Tools.SmallFont, dc.Tools.TextBrush, (int)pp.x, (int)pp.y);


            // Y軸
            p0 = rp0;
            p0.x = rpc.x;
            p0.z = rpc.z;

            p1 = rp1;
            p1.x = rpc.x;
            p1.z = rpc.z;

            drawLine(dc, dc.Tools.AxesPen, p0, p1);

            np = p0.y > p1.y ? p0 : p1;

            pp = dc.pointToPixelPoint(np);
            if (pp.x >= 16 + 80) pp.x -= 9;
            if (pp.y >= 16 + 80) pp.y -= 9;
            dc.graphics.DrawString("y", dc.Tools.SmallFont, dc.Tools.TextBrush, (int)pp.x, (int)pp.y);


            // Z軸
            p0 = rp0;
            p0.x = rpc.x;
            p0.y = rpc.y;

            p1 = rp1;
            p1.x = rpc.x;
            p1.y = rpc.y;

            drawLine(dc, dc.Tools.AxesPen, p0, p1);

            np = p0.z > p1.z ? p0 : p1;

            pp = dc.pointToPixelPoint(np);
            if (pp.x >= 16 + 80) pp.x -= 9;
            if (pp.y >= 16 + 80) pp.y -= 9;
            dc.graphics.DrawString("z", dc.Tools.SmallFont, dc.Tools.TextBrush, (int)pp.x, (int)pp.y);
        }

        public static void drawPageFrame(DrawContext dc)
        {
            if (dc.Tools.PageFramePen == null)
            {
                return;
            }

            CadPoint p0 = default(CadPoint);
            CadPoint p1 = default(CadPoint);

            p0.x = -dc.PageSize.width / 2;
            p0.y = dc.PageSize.height / 2;

            p1.x = dc.PageSize.width / 2;
            p1.y = -dc.PageSize.height / 2;

            p0.z = 0;
            p1.z = 0;

            Drawer.drawRect(dc, dc.Tools.PageFramePen, p0, p1);

            p0.x = -dc.PageSize.width / 2;
            p0.z = dc.PageSize.height / 2;

            p1.x = dc.PageSize.width / 2;
            p1.z = -dc.PageSize.height / 2;

            p0.y = 0;
            p1.y = 0;

            Drawer.drawRect(dc, dc.Tools.PageFramePen, p0, p1);

            p0.z = -dc.PageSize.width / 2;
            p0.y = dc.PageSize.height / 2;

            p1.z = dc.PageSize.width / 2;
            p1.y = -dc.PageSize.height / 2;

            p0.x = 0;
            p1.x = 0;

            Drawer.drawRect(dc, dc.Tools.PageFramePen, p0, p1);
        }
        #endregion

        #region "Draw marker"
        public static void drawHighlitePoint(DrawContext dc, CadPoint pt)
        {
            CadPoint pp = dc.pointToPixelPoint(pt);

            dc.graphics.DrawEllipse(dc.Tools.PointHighlitePen, (int)pp.x - 6, (int)pp.y - 6, 12, 12);
        }

        public static void drawSelectedPoint(DrawContext dc, CadPoint pt)
        {
            CadPoint pp = dc.pointToPixelPoint(pt);

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
            drawCursor(dc, dc.Tools.CursorPen, pt);
        }


        public static void drawCursor(DrawContext dc, Pen pen, CadPoint pt)
        {
            CadPoint pp = dc.pointToPixelPoint(pt);

            int size = 16;

            dc.graphics.DrawLine(pen, (int)pp.x - size, (int)pp.y, (int)pp.x + size, (int)pp.y);
            dc.graphics.DrawLine(pen, (int)pp.x, (int)pp.y - size, (int)pp.x, (int)pp.y + size);
        }

        public static void drawCursorScrn(DrawContext dc, CadPoint pt)
        {
            drawCursorScrn(dc, dc.Tools.CursorPen, pt);
        }


        public static void drawCursorScrn(DrawContext dc, Pen pen, CadPoint pp)
        {
            int size = 16;

            dc.graphics.DrawLine(pen, (int)pp.x - size, (int)pp.y, (int)pp.x + size, (int)pp.y);
            dc.graphics.DrawLine(pen, (int)pp.x, (int)pp.y - size, (int)pp.x, (int)pp.y + size);
        }
        #endregion

        #region "draw primitive"
        public static void drawLine(DrawContext dc, Pen pen, CadPoint a, CadPoint b)
        {
            CadPoint pa = dc.pointToPixelPoint(a);
            CadPoint pb = dc.pointToPixelPoint(b);

            dc.graphics.DrawLine(pen, (int)pa.x, (int)pa.y, (int)pb.x, (int)pb.y);
        }

        public static void drawText(DrawContext dc, Font fnt, Brush brush, CadPoint a, string s)
        {
            CadPoint pa = dc.pointToPixelPoint(a);
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
            CadPoint pp0 = dc.pointToPixelPoint(p0);
            CadPoint pp1 = dc.pointToPixelPoint(p1);

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
            double r = CadUtil.segNorm(cp, p1);

            CadPoint cpp =  dc.pointToPixelPoint(cp);

            r = dc.milliToPixels(r);

            dc.graphics.DrawEllipse(
                pen, (int)(cpp.x - r), (int)(cpp.y - r), (int)(r*2), (int)(r *2));
        }

        public static void drawCross(DrawContext dc, Pen pen, CadPoint p, int size)
        {
            CadPoint a = dc.pointToPixelPoint(p);

            dc.graphics.DrawLine(pen, (int)a.x - size, (int)a.y + 0, (int)a.x + size, (int)a.y + 0);
            dc.graphics.DrawLine(pen, (int)a.x + 0, (int)a.y + size, (int)a.x + 0, (int)a.y - size);
        }
        #endregion
    }

 }
