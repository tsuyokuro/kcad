using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
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

        public virtual void DrawAxis()
        {
        }

        public virtual void DrawPageFrame()
        {
        }

        public virtual void DrawGrid(Gridding grid)
        {
        }

        public virtual void DrawHighlightPoint(CadPoint pt)
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

        public virtual void DrawRect(int pen, CadPoint p0, CadPoint p1)
        {
        }

        public virtual void DrawCross(int pen, CadPoint p, int size)
        {
        }

        public virtual void DrawLine(int pen, CadPoint a, CadPoint b)
        {
        }

        public virtual void DrawDot(int pen, CadPoint p)
        {
        }

        public virtual void DrawFace(int pen, IReadOnlyList<CadPoint> pointList)
        {
            DrawFace(pen, pointList, default(CadPoint));
        }

        public virtual void DrawFace(int pen, IReadOnlyList<CadPoint> pointList, CadPoint normal)
        {
        }

        public virtual void DrawCircle(int pen, CadPoint cp, CadPoint pa, CadPoint pb)
        {
            CadPoint va = pa - cp;
            CadPoint vb = pb - cp;

            if (va.Norm() < 0.01)
            {
                return;
            }

            CadPoint normal = CadMath.Normal(va, vb);

            int div = 64;

            double dt = (double)(2.0 * Math.PI) / (double)div;

            CadQuaternion q = CadQuaternion.RotateQuaternion(normal, dt);
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

        public virtual void DrawText(int font, int brush, CadPoint a, string s)
        {
        }

        public virtual void DrawArrow(int pen, CadPoint pt0, CadPoint pt1, ArrowTypes type, ArrowPos pos, double len, double width)
        {
            DrawLine(pen, pt0, pt1);

            CadPoint d = pt1 - pt0;

            double dl = d.Norm();

            if (dl < 0.00001)
            {
                return;
            }


            CadPoint tmp = CadPoint.Create(dl, 0, 0);

            double angle = Vector3d.CalculateAngle(tmp.vector, d.vector);

            CadPoint normal = CadMath.crossProduct3D(tmp, d);  // 回転軸

            if (normal.Norm() < 0.0001)
            {
                normal = CadPoint.Create(0, 0, 1);
            }
            else
            {
                normal = normal.UnitVector();
                normal = CadMath.Normal(tmp, d);
            }
 
            CadQuaternion q = CadQuaternion.RotateQuaternion(normal, -angle);
            CadQuaternion r = q.Conjugate();

            ArrowHead a;

            if (pos == ArrowPos.END || pos == ArrowPos.START_END)
            {
                a = ArrowHead.Create(type, ArrowPos.END, len, width);

                a.Rotate(q, r);

                a += pt1;

                DrawLine(pen, a.p0, a.p1);
                DrawLine(pen, a.p0, a.p2);
                DrawLine(pen, a.p0, a.p3);
                DrawLine(pen, a.p0, a.p4);
            }

            if (pos == ArrowPos.START || pos == ArrowPos.START_END)
            {
                a = ArrowHead.Create(type, ArrowPos.START, len, width);

                a.Rotate(q, r);

                a += pt0;

                DrawLine(pen, a.p0, a.p1);
                DrawLine(pen, a.p0, a.p2);
                DrawLine(pen, a.p0, a.p3);
                DrawLine(pen, a.p0, a.p4);
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


        public virtual void DrawCursorScrn(CadPoint pp)
        {
        }
    }
}
