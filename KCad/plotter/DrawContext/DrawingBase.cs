using HalfEdgeNS;
using OpenTK;
using System.Collections.Generic;

namespace Plotter
{
    public class DrawingBase : IDrawing
    {
        public virtual void Clear(DrawBrush brush)
        {
        }

        public virtual void Draw(List<CadFigure> list, DrawPen pen)
        {
        }

        public virtual void DrawSelected(List<CadFigure> list, DrawPen pen)
        {
        }

        public virtual void DrawAxis()
        {
        }

        public virtual void DrawPageFrame(double w, double h, Vector3d center)
        {
        }

        public virtual void DrawGrid(Gridding grid)
        {
        }

        public virtual void DrawHighlightPoint(Vector3d pt, DrawPen pen)
        {
        }

        public virtual void DrawSelectedPoint(Vector3d pt, DrawPen pen)
        {
        }

        public virtual void DrawMarkCursor(DrawPen pen, Vector3d p, double pix_size)
        {
        }

        public virtual void DrawRect(DrawPen pen, Vector3d p0, Vector3d p1)
        {
        }

        public virtual void DrawCross(DrawPen pen, Vector3d p, double size)
        {
        }

        public virtual void DrawLine(DrawPen pen, Vector3d a, Vector3d b)
        {
        }

        public virtual void DrawDot(DrawPen pen, Vector3d p)
        {
        }

        public virtual void DrawHarfEdgeModel(
            DrawBrush brush, DrawPen pen, DrawPen edgePen, double edgeThreshold, HeModel model)
        {
        }

        public virtual void DrawText(int font, DrawBrush brush, Vector3d a, Vector3d xdir, Vector3d ydir, DrawTextOption opt, string s)
        {
        }

        public virtual void DrawTextScrn(int font, DrawBrush brush, Vector3d a, Vector3d dir, DrawTextOption opt, string s)
        {
        }

        public virtual Vector3d MeasureText(int font, string s)
        {
            return Vector3d.Zero;
        }


        public virtual void DrawArrow(DrawPen pen, Vector3d pt0, Vector3d pt1, ArrowTypes type, ArrowPos pos, double len, double width)
        {
            DrawLine(pen, pt0, pt1);

            Vector3d d = pt1 - pt0;

            double dl = d.Length;

            if (dl < 0.00001)
            {
                return;
            }


            Vector3d tmp = new Vector3d(dl, 0, 0);

            double angle = Vector3d.CalculateAngle(tmp, d);

            Vector3d normal = CadMath.CrossProduct(tmp, d);  // 回転軸

            if (normal.Length < 0.0001)
            {
                normal = new Vector3d(0, 0, 1);
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

                DrawLine(pen, a.p0.vector, a.p1.vector);
                DrawLine(pen, a.p0.vector, a.p2.vector);
                DrawLine(pen, a.p0.vector, a.p3.vector);
                DrawLine(pen, a.p0.vector, a.p4.vector);
            }

            if (pos == ArrowPos.START || pos == ArrowPos.START_END)
            {
                a = ArrowHead.Create(type, ArrowPos.START, len, width);

                a.Rotate(q, r);

                a += pt0;

                DrawLine(pen, a.p0.vector, a.p1.vector);
                DrawLine(pen, a.p0.vector, a.p2.vector);
                DrawLine(pen, a.p0.vector, a.p3.vector);
                DrawLine(pen, a.p0.vector, a.p4.vector);
            }
        }

        public virtual void DrawCrossCursorScrn(CadCursor pp, DrawPen pen)
        {
        }

        public virtual void DrawRectScrn(DrawPen pen, Vector3d p0, Vector3d p1)
        {
        }

        public virtual void DrawCrossScrn(DrawPen pen, Vector3d p, double size)
        {
        }

        public virtual void Dispose()
        {
        }

        public virtual void DrawBouncingBox(DrawPen pen, MinMax3D mm)
        {
        }
    }
}
