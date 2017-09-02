using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public class DrawingBase : IDrawing
    {
        public virtual void Clear()
        {
        }

        public virtual void Draw(CadLayer layer, int pen = DrawTools.PEN_DEFAULT_FIGURE)
        {
        }

        public virtual void Draw(IReadOnlyList<CadFigure> list, int pen = DrawTools.PEN_DEFAULT_FIGURE)
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

        public virtual void DrawHighlightPoint(CadVector pt, int pen=DrawTools.PEN_POINT_HIGHTLITE)
        {
        }

        public virtual void DrawSelectedPoint(CadVector pt, int pen = DrawTools.PEN_SLECT_POINT)
        {
        }

        public virtual void DrawDownPointCursor(int pen, CadVector p)
        {
        }

        public virtual void DrawCursor(CadVector pt)
        {
        }

        public virtual void DrawRect(int pen, CadVector p0, CadVector p1)
        {
        }

        public virtual void DrawCross(int pen, CadVector p, double size)
        {
        }

        public virtual void DrawLine(int pen, CadVector a, CadVector b)
        {
        }

        public virtual void DrawDot(int pen, CadVector p)
        {
        }

        public virtual void DrawFace(int pen, IReadOnlyList<CadVector> pointList)
        {
            DrawFace(pen, pointList, default(CadVector), true);
        }

        public virtual void DrawFace(int pen, IReadOnlyList<CadVector> pointList, CadVector normal, bool drawOutline)
        {
        }

        public virtual void DrawCircle(int pen, CadVector cp, CadVector pa, CadVector pb)
        {
            CadVector va = pa - cp;
            CadVector vb = pb - cp;

            if (va.Norm() < 0.01)
            {
                return;
            }

            CadVector normal = CadMath.Normal(va, vb);

            int div = 128;

            double dt = (double)(2.0 * Math.PI) / (double)div;

            CadQuaternion q = CadQuaternion.RotateQuaternion(normal, dt);
            CadQuaternion r = q.Conjugate();

            CadVector p = va;
            CadVector tp1 = pa;
            CadVector tp2 = pa;


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

        public virtual void DrawText(int font, int brush, CadVector a, string s)
        {
        }

        public virtual void DrawTextScrn(int font, int brush, CadVector a, CadVector direction, string s)
        {
        }

        public virtual CadVector MeasureText(int font, string s)
        {
            return CadVector.Zero;
        }


        public virtual void DrawArrow(int pen, CadVector pt0, CadVector pt1, ArrowTypes type, ArrowPos pos, double len, double width)
        {
            DrawLine(pen, pt0, pt1);

            CadVector d = pt1 - pt0;

            double dl = d.Norm();

            if (dl < 0.00001)
            {
                return;
            }


            CadVector tmp = CadVector.Create(dl, 0, 0);

            double angle = Vector3d.CalculateAngle(tmp.vector, d.vector);

            CadVector normal = CadMath.CrossProduct(tmp, d);  // 回転軸

            if (normal.Norm() < 0.0001)
            {
                normal = CadVector.Create(0, 0, 1);
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
            CadVector p0, CadVector p1, CadVector p2)
        {
            double t = 0;
            double d = 1.0 / 64;

            t = d;

            int n = 3;

            CadVector t0 = p0;
            CadVector t1 = p0;

            while (t <= 1.0)
            {
                t1 = default(CadVector);
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
            CadVector p0, CadVector p1, CadVector p2, CadVector p3)
        {
            double t = 0;
            double d = 1.0 / 64;

            t = d;

            int n = 4;

            CadVector t0 = p0;
            CadVector t1 = p0;

            while (t <= 1.0)
            {
                t1 = default(CadVector);
                t1 += p0 * CadMath.BernsteinBasisF(n - 1, 0, t);
                t1 += p1 * CadMath.BernsteinBasisF(n - 1, 1, t);
                t1 += p2 * CadMath.BernsteinBasisF(n - 1, 2, t);
                t1 += p3 * CadMath.BernsteinBasisF(n - 1, 3, t);

                DrawLine(pen, t0, t1);

                t0 = t1;

                t += d;
            }
        }


        public virtual void DrawCursorScrn(CadVector pp)
        {
        }

        public virtual void DrawCrossCursorScrn(CadVector pp)
        {
        }
    }
}
