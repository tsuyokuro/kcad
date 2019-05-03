﻿using HalfEdgeNS;
using OpenTK;
using System.Collections.Generic;
using CadDataTypes;

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

        public virtual void DrawPageFrame(double w, double h, CadVector center)
        {
        }

        public virtual void DrawGrid(Gridding grid)
        {
        }

        public virtual void DrawHighlightPoint(CadVector pt, DrawPen pen)
        {
        }

        public virtual void DrawSelectedPoint(CadVector pt, DrawPen pen)
        {
        }

        public virtual void DrawMarkCursor(DrawPen pen, CadVector p, double pix_size)
        {
        }

        public virtual void DrawRect(DrawPen pen, CadVector p0, CadVector p1)
        {
        }

        public virtual void DrawCross(DrawPen pen, CadVector p, double size)
        {
        }

        public virtual void DrawLine(DrawPen pen, CadVector a, CadVector b)
        {
        }

        public virtual void DrawDot(DrawPen pen, CadVector p)
        {
        }

        public virtual void DrawFace(DrawPen pen, VectorList pointList)
        {
            DrawFace(pen, pointList, default(CadVector), true);
        }

        public virtual void DrawFace(DrawPen pen, VectorList pointList, CadVector normal, bool drawOutline)
        {
        }

        public virtual void DrawHarfEdgeModel(DrawPen pen, HeModel model)
        {
            for (int i = 0; i < model.FaceStore.Count; i++)
            {
                HeFace f = model.FaceStore[i];

                HalfEdge head = f.Head;

                HalfEdge c = head;

                for (; ; )
                {
                    HalfEdge next = c.Next;

                    DrawLine(pen,
                        model.VertexStore.Ref(c.Vertex),
                        model.VertexStore.Ref(next.Vertex));

                    c = next;

                    if (c == head)
                    {
                        break;
                    }
                }
            }
        }

        public virtual void DrawHarfEdgeModel(DrawPen pen, DrawPen edgePen, double edgeThreshold, HeModel model)
        {
            DrawHarfEdgeModel(pen, model);
        }

        public virtual void DrawText(int font, DrawBrush brush, CadVector a, CadVector xdir, CadVector ydir, DrawTextOption opt, string s)
        {
        }

        public virtual void DrawTextScrn(int font, DrawBrush brush, CadVector a, CadVector dir, DrawTextOption opt, string s)
        {
        }

        public virtual CadVector MeasureText(int font, string s)
        {
            return CadVector.Zero;
        }


        public virtual void DrawArrow(DrawPen pen, CadVector pt0, CadVector pt1, ArrowTypes type, ArrowPos pos, double len, double width)
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

        public virtual void DrawCrossCursorScrn(CadCursor pp, DrawPen pen)
        {
        }

        public virtual void DrawRectScrn(DrawPen pen, CadVector p0, CadVector p1)
        {
        }

        public virtual void DrawCrossScrn(DrawPen pen, CadVector p, double size)
        {
        }
    }
}
