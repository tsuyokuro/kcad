using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public enum ArrowTypes
    {
        CROSS,  // X
        PLUS,   // +
    }

    public class DrawingBase
    {
        public virtual void Clear()
        {
        }

        public virtual void Draw(CadLayer layer)
        {
        }

        public virtual void Draw(IReadOnlyList<CadFigure> list, int pen = -1)
        {
        }

        public virtual void DrawSelected(CadLayer layer)
        {
        }

        public virtual void DrawSelected(IReadOnlyList<CadFigure> list)
        {
        }

        public virtual void DrawSelected(List<CadRelativePoint> list)
        {
        }

        public virtual void DrawViewRect(double s)
        {
        }

        public virtual void DrawAxis()
        {
        }

        public virtual void DrawPageFrame()
        {
        }

        public virtual void DrawHighlitePoint(CadPoint pt)
        {
        }

        public virtual void DrawSelectedPoint(CadPoint pt)
        {
        }

        public virtual void DrawLastPointMarker(int pen, CadPoint p)
        {
        }

        public virtual void DrawCursor(CadPoint pt)
        {
        }

        public virtual void DrawCursorScrn(CadPoint pp)
        {
        }

        public virtual void DrawRect(int pen, CadPoint p0, CadPoint p1)
        {
        }

        public virtual void DrawRectScrn(int pen, CadPoint pp0, CadPoint pp1)
        {
        }

        public virtual void DrawCross(int pen, CadPoint p, int size)
        {
        }

        public virtual void DrawLine(int pen, CadPoint a, CadPoint b)
        {
        }

        public virtual void DrawLineScrn(int pen, CadPoint a, CadPoint b)
        {
        }

        public virtual void DrawLineScrn(int pen, double x1, double y1, double x2, double y2)
        {
        }

        public virtual void DrawRectangleScrn(int pen, double x0, double y0, double x1, double y1)
        {
        }

        public virtual void DrawCircle(int pen, CadPoint cp, CadPoint pa, CadPoint pb)
        {
            CadPoint va = pa - cp;
            CadPoint vb = pb - cp;

            if (va.norm() < 0.01)
            {
                return;
            }


            CadPoint normal = CadMath.crossProduct3D(va, vb);

            normal = normal.unitVector();

            int div = 64;

            double dt = (double)(2.0 * Math.PI) / (double)div;

            CadQuaternion q = CadQuaternion.RotateQuaternion(dt, normal);
            CadQuaternion r = q.Conjugate();

            CadPoint p = va;
            CadPoint tp1 = pa;
            CadPoint tp2 = pa;


            int i = 0;
            for (; i < div - 1; i++)
            {
                CadQuaternion qp = CadQuaternion.FromPoint(p);
                qp = r * qp;
                qp = qp * q;

                p = qp.ToPoint();

                tp2 = p + cp;

                DrawLine(pen, tp1, tp2);
                tp1 = tp2;
            }

            DrawLine(pen, tp1, pa);
        }

        public virtual void DrawCircleScrn(int pen, CadPoint cp, CadPoint p1)
        {
        }

        public virtual void DrawCircleScrn(int pen, CadPoint cp, double r)
        {
        }

        public virtual void DrawText(int font, int brush, CadPoint a, string s)
        {
        }

        public virtual void DrawTextScrn(int font, int brush, CadPoint a, string s)
        {
        }

        public virtual void DrawTextScrn(int font, int brush, double x, double y, string s)
        {
        }


        public virtual void FillRectangleScrn(int brush, double x0, double y0, double x1, double y1)
        {
        }

        public virtual void DrawArrow(int pen, CadPoint pt0, CadPoint pt1, ArrowTypes type, double len, double width)
        {
            DrawLine(pen, pt0, pt1);

            CadPoint d = pt1 - pt0;

            double w2 = width / 2.0;

            CadPoint ap0;
            CadPoint ap1;
            CadPoint ap2;
            CadPoint ap3;
            CadPoint ap4;

            if (type == ArrowTypes.CROSS)
            {
                ap0 = CadPoint.Create(0, 0, 0);
                ap1 = CadPoint.Create(-len, w2, w2);
                ap2 = CadPoint.Create(-len, w2, -w2);
                ap3 = CadPoint.Create(-len, -w2, -w2);
                ap4 = CadPoint.Create(-len, -w2, w2);
            }
            else if (type == ArrowTypes.PLUS)
            {
                ap0 = CadPoint.Create(0, 0, 0);
                ap1 = CadPoint.Create(-len, w2, 0);
                ap2 = CadPoint.Create(-len, 0, -w2);
                ap3 = CadPoint.Create(-len, -w2, 0);
                ap4 = CadPoint.Create(-len, 0, w2);
            }

            

        }

        public virtual void DrawBezier(
            int pen,
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

                DrawLine(pen, t0, t1);

                t0 = t1;

                t += d;
            }
        }

        public virtual void DrawBezier(
            int pen,
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

                DrawLine(pen, t0, t1);

                t0 = t1;

                t += d;
            }
        }
    }
}
