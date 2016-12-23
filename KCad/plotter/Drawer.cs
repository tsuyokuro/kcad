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

        public static void drawViewRect(DrawContext dc, double s)
        {
            CadRect vr = dc.getViewRect();

            CadPoint vc = CadPoint.GetNew(dc.ViewWidth, dc.ViewHeight, 0);
            vc = vc / 2;
            vc = dc.pixelPointToCadPoint(vc);

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
            p0.x = -100000;
            p0.y = 0;
            p0.z = 0;

            p1.x = 100000;
            p1.y = 0;
            p1.z = 0;

            Drawer.drawLine(dc, dc.Tools.AxesPen, p0, p1);

            // Y軸
            p0.x = 0;
            p0.y = -100000;
            p0.z = 0;

            p1.x = 0;
            p1.y = 100000;
            p1.z = 0;

            Drawer.drawLine(dc, dc.Tools.AxesPen, p0, p1);

            // Z軸
            p0.x = 0;
            p0.y = 0;
            p0.z = -100000;

            p1.x = 0;
            p1.y = 0;
            p1.z = 100000;

            Drawer.drawLine(dc, dc.Tools.AxesPen, p0, p1);

            drawAxisDir(dc);
        }

        public static void drawAxisDir(DrawContext dc)
        {
            if (dc.Tools.AxesPen == null)
            {
                return;
            }

            double ltpx = 16;
            double ltpy = 16;

            double w = 80;


            CadPoint sp0 = CadPoint.GetNew(ltpx, ltpy, 0);
            CadPoint sp1 = CadPoint.GetNew(ltpx + w, ltpy + w, 0);

            CadPoint spc = CadPoint.GetNew(ltpx + w/2, ltpy + w/2, 0);

            Drawer.drawCircleScrn(dc, dc.Tools.AxesPen, spc, w/2);

            //Drawer.drawRectScrn(dc, dc.Tools.AxesPen, sp0, sp1);

            CadPoint wc = dc.pixelPointToCadPoint(spc);

            CadPoint p0;
            CadPoint p1;

            CadPoint sp = default(CadPoint);

            // X軸
            p0 = wc;
            p0.x = wc.x - dc.pixelsToMilli(w/2);

            p1 = wc;
            p1.x = wc.x + dc.pixelsToMilli(w/2);

            drawLine(dc, dc.Tools.AxesPen, p0, p1);

            sp = dc.pointToPixelPoint(p1);
            dc.graphics.DrawString("x", dc.Tools.SmallFont, dc.Tools.TextBrush, (int)sp.x, (int)sp.y);


            // Y軸
            p0 = wc;
            p0.y = wc.y - dc.pixelsToMilli(w/2);

            p1 = wc;
            p1.y = wc.y + dc.pixelsToMilli(w/2);

            drawLine(dc, dc.Tools.AxesPen, p0, p1);

            sp = dc.pointToPixelPoint(p1);
            dc.graphics.DrawString("y", dc.Tools.SmallFont, dc.Tools.TextBrush, (int)sp.x, (int)sp.y);


            // Z軸
            p0 = wc;
            p0.z = wc.z - dc.pixelsToMilli(w/2);

            p1 = wc;
            p1.z = wc.z + dc.pixelsToMilli(w/2);

            drawLine(dc, dc.Tools.AxesPen, p0, p1);

            sp = dc.pointToPixelPoint(p1);
            dc.graphics.DrawString("z", dc.Tools.SmallFont, dc.Tools.TextBrush, (int)sp.x, (int)sp.y);
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


        public static void drawCircleScrn(DrawContext dc, Pen pen, CadPoint cp, CadPoint p1)
        {
            double r = CadUtil.segNorm(cp, p1);

            dc.graphics.DrawEllipse(
                pen, (int)(cp.x - r), (int)(cp.y - r), (int)(r * 2), (int)(r * 2));
        }

        public static void drawCircleScrn(DrawContext dc, Pen pen, CadPoint cp, double r)
        {
            dc.graphics.DrawEllipse(
                pen, (int)(cp.x - r), (int)(cp.y - r), (int)(r * 2), (int)(r * 2));
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
