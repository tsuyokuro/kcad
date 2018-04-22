using HalfEdgeNS;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadDataTypes;

namespace Plotter
{
    public class DrawingBase : IDrawing
    {
        public virtual void Clear(int brush = DrawTools.BRUSH_BACKGROUND)
        {
        }

        public virtual void Draw(CadLayer layer, int pen = DrawTools.PEN_DEFAULT_FIGURE)
        {
        }

        public virtual void Draw(List<CadFigure> list, int pen = DrawTools.PEN_DEFAULT_FIGURE)
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

        public virtual void DrawSelectedPoint(CadVector pt, int pen = DrawTools.PEN_SELECT_POINT)
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

        public virtual void DrawFace(int pen, VectorList pointList)
        {
            DrawFace(pen, pointList, default(CadVector), true);
        }

        public virtual void DrawFace(int pen, VectorList pointList, CadVector normal, bool drawOutline)
        {
        }

        public virtual void DrawHarfEdgeModel(int pen, HeModel model)
        {
            for (int i = 0; i < model.FaceStore.Count; i++)
            {
                HeFace f = model.FaceStore[i];

                HalfEdge head = f.Head;

                HalfEdge c = head;

                CadVector v;

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

        public virtual void DrawCursorScrn(CadVector pp)
        {
        }

        public virtual void DrawCrossCursorScrn(CadCursor pp)
        {
        }

        public virtual void DrawRectScrn(int pen, CadVector p0, CadVector p1)
        {
        }

        public virtual void DrawCrossScrn(int pen, CadVector p, double size)
        {
        }
    }
}
