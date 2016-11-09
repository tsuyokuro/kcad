#define LOG_DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static void draw(DrawContext dc, List<CadFigure> list)
        {
            foreach (CadFigure fig in list)
            {
                fig.draw(dc, dc.Tools.DefaultFigurePen);
            }
        }

        public static void drawSelected(DrawContext dc, CadLayer layer)
        {
            drawSelected(dc, layer.FigureList);
            drawSelected(dc, layer.RelPointList);
        }

        public static void drawSelected(DrawContext dc, List<CadFigure> list)
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

            double w = dc.PageSize.width / 2;
            double h = dc.PageSize.height / 2;
            Drawer.drawLine(dc, dc.Tools.AxesPen, -w, 0, w, 0);
            Drawer.drawLine(dc, dc.Tools.AxesPen, 0, h, 0, -h);
        }

        public static void drawPageFrame(DrawContext dc)
        {
            if (dc.Tools.PageFramePen == null)
            {
                return;
            }

            double x0 = -dc.PageSize.width / 2;
            double y0 = dc.PageSize.height / 2;
            double x1 = dc.PageSize.width / 2;
            double y1 = -dc.PageSize.height / 2;

            Drawer.drawRect(dc, dc.Tools.PageFramePen, x0, y0, x1, y1);
        }
        #endregion

        #region "Draw marker"
        public static void drawHighlitePoint(DrawContext dc, CadPoint pt)
        {
            CadPixelPoint pp = dc.pointToPixelPoint(pt);

            dc.graphics.DrawEllipse(dc.Tools.PointHighlitePen, (int)pp.x - 6, (int)pp.y - 6, 12, 12);
        }

        public static void drawSelectedPoint(DrawContext dc, CadPoint pt)
        {
            CadPixelPoint pp = dc.pointToPixelPoint(pt);

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
            CadPixelPoint pp = dc.pointToPixelPoint(pt);

            int size = 16;

            dc.graphics.DrawLine(pen, (int)pp.x - size, (int)pp.y, (int)pp.x + size, (int)pp.y);
            dc.graphics.DrawLine(pen, (int)pp.x, (int)pp.y - size, (int)pp.x, (int)pp.y + size);
        }
        #endregion

        #region "draw primitive"
        public static void drawLine(DrawContext dc, Pen pen, CadPoint a, CadPoint b)
        {
            CadPixelPoint pa = dc.pointToPixelPoint(a);
            CadPixelPoint pb = dc.pointToPixelPoint(b);

            dc.graphics.DrawLine(pen, (int)pa.x, (int)pa.y, (int)pb.x, (int)pb.y);
        }

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

        public static void drawRect(DrawContext dc, Pen pen, CadPoint p0, CadPoint p1)
        {
            CadPixelPoint pp0 = dc.pointToPixelPoint(p0);
            CadPixelPoint pp1 = dc.pointToPixelPoint(p1);

            drawRect(dc.graphics, pen, (int)pp0.x, (int)pp0.y, (int)pp1.x, (int)pp1.y);
        }

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
                t1 += p0 * BernsteinBasisF(n - 1, 0, t);
                t1 += p1 * BernsteinBasisF(n - 1, 1, t);
                t1 += p2 * BernsteinBasisF(n - 1, 2, t);

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
                t1 += p0 * BernsteinBasisF(n - 1, 0, t);
                t1 += p1 * BernsteinBasisF(n - 1, 1, t);
                t1 += p2 * BernsteinBasisF(n - 1, 2, t);
                t1 += p3 * BernsteinBasisF(n - 1, 3, t);

                Drawer.drawLine(dc, pen, t0, t1);

                t0 = t1;

                t += d;
            }
        }

        public static void drawCircle(DrawContext dc, Pen pen, CadPoint cp, CadPoint p1)
        {
            double r = CadUtil.lineAbs2D(cp, p1);

            CadPixelPoint cpp =  dc.pointToPixelPoint(cp);

            r = dc.cadVToPixelV(r);

            dc.graphics.DrawEllipse(
                pen, (int)(cpp.x - r), (int)(cpp.y - r), (int)(r*2), (int)(r *2));
        }

        public static void drawCross(DrawContext dc, Pen pen, CadPoint p, int size)
        {
            CadPixelPoint a = dc.pointToPixelPoint(p);

            dc.graphics.DrawLine(pen, (int)a.x - size, (int)a.y + 0, (int)a.x + size, (int)a.y + 0);
            dc.graphics.DrawLine(pen, (int)a.x + 0, (int)a.y + size, (int)a.x + 0, (int)a.y - size);
        }
        #endregion


        static double[] FactorialTbl =
        {
            1.0, // 0!
            1.0,
            2.0 * 1.0,
            3.0 * 2.0 * 1.0,
            4.0 * 3.0 * 2.0 * 1.0,
            5.0 * 4.0 * 3.0 * 2.0 * 1.0,
            6.0 * 5.0 * 4.0 * 3.0 * 2.0 * 1.0,
        };

        // Bernstein basis polynomials
        public static double BernsteinBasisF(int n, int i, double t)
        {
            return BinomialCoefficientsF(n, i) * Math.Pow(t, i) * Math.Pow(1 - t, n - i);
        }

        // Binomial coefficient
        public static double BinomialCoefficientsF(int n, int k)
        {
            return FactorialTbl[n] / (FactorialTbl[k] * FactorialTbl[n - k]);
        }



        // Bernstein basis polynomials
        public static double BernsteinBasis(int n, int i, double t)
        {
            return BinomialCoefficients(n, i) * Math.Pow(t, i) * Math.Pow(1 - t, n - i);
        }

        // Binomial coefficient
        public static double BinomialCoefficients(int n, int k)
        {
            return Factorial(n) / (Factorial(k) * Factorial(n - k));
        }

        // e.g 6! = 6*5*4*3*2*1
        public static double Factorial(int a)
        {
            double r = 1.0;
            for (int i = 2; i <= a; i++)
            {
                r *= (double)i;
            }

            return r;
        }
    }

 }
