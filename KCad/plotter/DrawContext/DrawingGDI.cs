/**
 * GDI向け描画クラス
 * 
 */

//#define USE_LONG_TERM_LOCK_BITS // ある程度長い期間LockBitsし続ける

using HalfEdgeNS;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using CadDataTypes;

namespace Plotter
{
    public class DrawingGDI : DrawingBase
    {
        public DrawContextGDI DC;

        public DrawingGDI(DrawContextGDI dc)
        {
            DC = dc;
        }

        public override void Clear(int brush)
        {
            FillRectangleScrn(
                brush,
                0, 0, (int)DC.ViewWidth, (int)DC.ViewHeight);
        }

        public override void Draw(CadLayer layer, int pen = DrawTools.PEN_DEFAULT_FIGURE)
        {
            #if USE_LONG_TERM_LOCK_BITS
                DC.LockBits();
            #endif

            layer.ForEachFig(fig =>
            {
                if (fig.Current)
                {
                    fig.Draw(DC, DrawTools.PEN_FIGURE_HIGHLIGHT);
                }
                else
                {
                    fig.Draw(DC, pen);
                }
            });

            #if USE_LONG_TERM_LOCK_BITS
                DC.UnlockBits();
            #endif
        }

        public override void Draw(List<CadFigure> list, int pen = DrawTools.PEN_DEFAULT_FIGURE)
        {
            #if USE_LONG_TERM_LOCK_BITS
                DC.LockBits();
            #endif

            foreach (CadFigure fig in list)
            {
                fig.ForEachFig(a =>
                {
                    if (a.Current)
                    {
                        a.Draw(DC, DrawTools.PEN_FIGURE_HIGHLIGHT);
                    }
                    else
                    {
                        a.Draw(DC, pen);
                    }
                });
            }

            #if USE_LONG_TERM_LOCK_BITS
                DC.UnlockBits();
            #endif
        }

        public override void DrawSelected(CadLayer layer)
        {
            layer.ForEachFig(fig =>
            {
                fig.DrawSelected(DC, DrawTools.PEN_DEFAULT_FIGURE);
            });
        }

        #region "Draw base"
        public override void DrawAxis()
        {
            CadVector p0 = default(CadVector);
            CadVector p1 = default(CadVector);

            // X軸
            p0.x = -100;
            p0.y = 0;
            p0.z = 0;

            p1.x = 100;
            p1.y = 0;
            p1.z = 0;

            p0 /= DC.WorldScale;
            p1 /= DC.WorldScale;

            DrawLine(DrawTools.PEN_AXIS, p0, p1);

            // Y軸
            p0.x = 0;
            p0.y = -100;
            p0.z = 0;

            p1.x = 0;
            p1.y = 100;
            p1.z = 0;

            p0 /= DC.WorldScale;
            p1 /= DC.WorldScale;

            DrawLine(DrawTools.PEN_AXIS, p0, p1);

            // Z軸
            p0.x = 0;
            p0.y = 0;
            p0.z = -100;

            p1.x = 0;
            p1.y = 0;
            p1.z = 100;

            p0 /= DC.WorldScale;
            p1 /= DC.WorldScale;

            DrawLine(DrawTools.PEN_AXIS, p0, p1);

            DrawAxis2();
        }

        /*
        public override void DrawGrid(Gridding grid)
        {
            CadVector lt = CadVector.Zero;
            CadVector rb = CadVector.Create(DC.ViewWidth, DC.ViewHeight, 0);

            CadVector ltw = DC.UnitPointToCadPoint(lt);
            CadVector rbw = DC.UnitPointToCadPoint(rb);

            double minx = Math.Min(ltw.x, rbw.x);
            double maxx = Math.Max(ltw.x, rbw.x);

            double miny = Math.Min(ltw.y, rbw.y);
            double maxy = Math.Max(ltw.y, rbw.y);

            double minz = Math.Min(ltw.z, rbw.z);
            double maxz = Math.Max(ltw.z, rbw.z);


            int pen = DrawTools.PEN_GRID;

            CadVector p = default(CadVector);


            double n = grid.Decimate(DC, grid, 8);

            double x, y, z;
            double sx, sy, sz;
            double szx = grid.GridSize.x * n;
            double szy = grid.GridSize.y * n;
            double szz = grid.GridSize.z * n;

            sx = Math.Round(minx / szx) * szx;
            sy = Math.Round(miny / szy) * szy;
            sz = Math.Round(minz / szz) * szz;


            Stopwatch sw = new Stopwatch();
            sw.Start();

            x = sx;
            while (x < maxx)
            {
                p.x = x;
                p.z = 0;

                y = sy;

                while (y < maxy)
                {
                    p.y = y;
                    DrawDot(pen, p);
                    y += szy;
                }

                x += szx;
            }

            z = sz;
            y = sy;

            while (z < maxz)
            {
                p.z = z;
                p.x = 0;

                y = sy;

                while (y < maxy)
                {
                    p.y = y;
                    DrawDot(pen, p);
                    y += szy;
                }

                z += szz;
            }

            z = sz;
            x = sx;

            while (x < maxx)
            {
                p.x = x;
                p.y = 0;

                z = sz;

                while (z < maxz)
                {
                    p.z = z;
                    DrawDot(pen, p);
                    z += szz;
                }

                x += szx;
            }

            sw.Stop();
            DebugOut.StdPrintLn(sw.ElapsedMilliseconds.ToString());
        }
        */

        public override void DrawGrid(Gridding grid)
        {
            CadVector lt = CadVector.Zero;
            CadVector rb = CadVector.Create(DC.ViewWidth, DC.ViewHeight, 0);

            CadVector ltw = DC.UnitPointToCadPoint(lt);
            CadVector rbw = DC.UnitPointToCadPoint(rb);

            double minx = Math.Min(ltw.x, rbw.x);
            double maxx = Math.Max(ltw.x, rbw.x);

            double miny = Math.Min(ltw.y, rbw.y);
            double maxy = Math.Max(ltw.y, rbw.y);

            double minz = Math.Min(ltw.z, rbw.z);
            double maxz = Math.Max(ltw.z, rbw.z);


            int pen = DrawTools.PEN_GRID;

            Color c = DC.PenColor(DrawTools.PEN_GRID);

            int argb = c.ToArgb();

            CadVector p = default(CadVector);


            double n = grid.Decimate(DC, grid, 8);

            double x, y, z;
            double sx, sy, sz;
            double szx = grid.GridSize.x * n;
            double szy = grid.GridSize.y * n;
            double szz = grid.GridSize.z * n;

            sx = Math.Round(minx / szx) * szx;
            sy = Math.Round(miny / szy) * szy;
            sz = Math.Round(minz / szz) * szz;


            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            DrawDots(sx, sy, sz, szx, szy, szz, maxx, maxy, maxz, argb);

            //sw.Stop();
            //DebugOut.StdPrintLn(sw.ElapsedMilliseconds.ToString());
        }

        private void DrawDots(
            double sx,
            double sy,
            double sz,
            double szx,
            double szy,
            double szz,
            double maxx,
            double maxy,
            double maxz,
            int argb
            )
        {
            double x;
            double y;
            double z;

            CadVector p = default(CadVector);
            CadVector up = default(CadVector);


            Bitmap tgt = DC.Image;

            BitmapData bitmapData = DC.LockBits();

            unsafe
            {
                int* srcPixels = (int*)bitmapData.Scan0;

                x = sx;
                while (x < maxx)
                {
                    p.x = x;
                    p.z = 0;

                    y = sy;

                    while (y < maxy)
                    {
                        p.y = y;
                        up = DC.CadPointToUnitPoint(p);

                        if (up.x >= 0 && up.x < tgt.Width && up.y >= 0 && up.y < tgt.Height)
                        {
                            *(srcPixels + ((int)up.y * tgt.Width) + (int)up.x) = argb;
                        }

                        y += szy;
                    }

                    x += szx;
                }

                z = sz;
                while (z < maxz)
                {
                    p.z = z;
                    p.x = 0;

                    y = sy;

                    while (y < maxy)
                    {
                        p.y = y;

                        up = DC.CadPointToUnitPoint(p);

                        if (up.x >= 0 && up.x < tgt.Width && up.y >= 0 && up.y < tgt.Height)
                        {
                            *(srcPixels + ((int)up.y * tgt.Width) + (int)up.x) = argb;
                        }

                        y += szy;
                    }

                    z += szz;
                }

                x = sx;
                while (x < maxx)
                {
                    p.x = x;
                    p.y = 0;

                    z = sz;

                    while (z < maxz)
                    {
                        p.z = z;

                        up = DC.CadPointToUnitPoint(p);

                        if (up.x >= 0 && up.x < tgt.Width && up.y >= 0 && up.y < tgt.Height)
                        {
                            *(srcPixels + ((int)up.y * tgt.Width) + (int)up.x) = argb;
                        }

                        z += szz;
                    }

                    x += szx;
                }
            }

            DC.UnlockBits();

            //tgt.UnlockBits(bitmapData);
        }

        public override void DrawPageFrame(double w, double h, CadVector center)
        {
            CadVector pt = default(CadVector);

            // p0
            pt.x = -w / 2 + center.x;
            pt.y = h / 2 + center.y;
            pt.z = 0;

            CadVector p0 = default(CadVector);
            p0.x = pt.x * DC.UnitPerMilli * DC.DeviceScaleX;
            p0.y = pt.y * DC.UnitPerMilli * DC.DeviceScaleY;

            p0 += DC.ViewOrg;

            // p1
            pt.x = w / 2 + center.x;
            pt.y = -h / 2 + center.y;
            pt.z = 0;

            CadVector p1 = default(CadVector);
            p1.x = pt.x * DC.UnitPerMilli * DC.DeviceScaleX;
            p1.y = pt.y * DC.UnitPerMilli * DC.DeviceScaleY;

            p1 += DC.ViewOrg;

            DrawRectScrn(DrawTools.PEN_PAGE_FRAME, p0, p1);
        }
        #endregion

        #region "Draw marker"
        public override void DrawHighlightPoint(CadVector pt, int pen = DrawTools.PEN_POINT_HIGHTLITE)
        {
            CadVector pp = DC.CadPointToUnitPoint(pt);

            //DrawCircleScrn(pen, pp, 3);

            DrawCrossScrn(pen, pp, 4);
        }

        public override void DrawSelectedPoint(CadVector pt, int pen = DrawTools.PEN_SELECT_POINT)
        {
            CadVector pp = DC.CadPointToUnitPoint(pt);

            int size = 3;

            DrawRectangleScrn(
                pen,
                (int)pp.x - size, (int)pp.y - size,
                (int)pp.x + size, (int)pp.y + size
                );
        }

        public override void DrawMarkCursor(int pen, CadVector p, double size)
        {
            DrawCross(pen, p, size);
        }
        #endregion

        public override void DrawHarfEdgeModel(int pen, int edgePen, double edgeThreshold, HeModel model)
        {
            Vector3d t = DC.ViewDir * (-0.2f / DC.WorldScale);

            CadVector shift = (CadVector)t;


            CadVector p0;
            CadVector p1;


            for (int i = 0; i < model.FaceStore.Count; i++)
            {
                HeFace f = model.FaceStore[i];

                HalfEdge head = f.Head;

                HalfEdge c = head;

                HalfEdge pair;

                CadVector v;

                for (; ; )
                {
                    bool edge = false;

                    pair = c.Pair;

                    if (pair == null)
                    {
                        edge = true;
                    }
                    else
                    {
                        double s = CadMath.InnerProduct(model.NormalStore[c.Normal], model.NormalStore[pair.Normal]);

                        if (Math.Abs(s) < edgeThreshold)
                        {
                            edge = true;
                        }
                    }

                    HalfEdge next = c.Next;

                    if (edge)
                    {
                        DrawLine(edgePen,
                            model.VertexStore.Ref(c.Vertex) + shift,
                            model.VertexStore.Ref(next.Vertex) + shift
                            );
                    }
                    else
                    {
                        DrawLine(pen,
                            model.VertexStore.Ref(c.Vertex) + shift,
                            model.VertexStore.Ref(next.Vertex) + shift
                            );
                    }

                    c = next;

                    if (c == head)
                    {
                        break;
                    }
                }
            }
        }

        public override void DrawRect(int pen, CadVector p0, CadVector p1)
        {
            CadVector pp0 = DC.CadPointToUnitPoint(p0);
            CadVector pp1 = DC.CadPointToUnitPoint(p1);

            DrawRectangleScrn(pen, pp0.x, pp0.y, pp1.x, pp1.y);
        }

        public override void DrawCross(int pen, CadVector p, double size)
        {
            CadVector a = DC.CadPointToUnitPoint(p);

            DrawLineScrn(pen, a.x - size, a.y + 0, a.x + size, a.y + 0);
            DrawLineScrn(pen, a.x + 0, a.y + size, a.x + 0, a.y - size);
        }

        public override void DrawCrossScrn(int pen, CadVector p, double size)
        {
            DrawLineScrn(pen, p.x - size, p.y + 0, p.x + size, p.y + 0);
            DrawLineScrn(pen, p.x + 0, p.y + size, p.x + 0, p.y - size);
        }

        public override void DrawLine(int pen, CadVector a, CadVector b)
        {
            if (DC.graphics == null) return;
            if (DC.Pen(pen) == null) return;

            CadVector pa = DC.CadPointToUnitPoint(a);
            CadVector pb = DC.CadPointToUnitPoint(b);

           BitmapData bd = DC.GetLockedBits();

            if (bd == null)
            {
                DC.graphics.DrawLine(DC.Pen(pen), (int)pa.x, (int)pa.y, (int)pb.x, (int)pb.y);
            }
            else
            {
                CadSegment seg = CadUtil.Clipping2D(0, 0, DC.ViewWidth, DC.ViewHeight, pa, pb);

                if (seg.Valid)
                {
                    BitmapUtil.BresenhamLine(bd, seg.P0, seg.P1, (uint)(DC.Pen(pen).Color.ToArgb()));
                }
            }
        }

        public override void DrawDot(int pen, CadVector p)
        {
            if (DC.graphics == null)
            {
                return;
            }
 
            CadVector p0 = DC.CadPointToUnitPoint(p);
            //CadVector p1 = p0;
            //p0.x = (int)p0.x;
            //p1.x = p0.x + 0.1;

            //DC.graphics.DrawLine(DC.Pen(pen), (float)p0.x, (float)p0.y, (float)p1.x, (float)p1.y);

            if (p0.x >= 0 && p0.y >= 0 && p0.x < DC.ViewWidth && p0.y < DC.ViewHeight)
            {
                DC.Image.SetPixel((int)p0.x, (int)p0.y, DC.PenColor(pen));
            }
        }

        public override void DrawFace(int pen, VectorList pointList, CadVector Normal, bool drawOutline)
        {
            int cnt = pointList.Count;
            if (cnt == 0)
            {
                return;
            }

            CadVector p0 = pointList[0];
            CadVector p1;

            int i;
            for (i = 1; i < cnt; i++)
            {
                p1 = pointList[i];
                DrawLine(pen, p0, p1);
                p0 = p1;
            }

            p1 = pointList[0];
            DrawLine(pen, p0, p1);
        }

        public override void DrawHarfEdgeModel(int pen, HeModel model)
        {
            base.DrawHarfEdgeModel(pen, model);
        }

        public override void DrawText(int font, int brush, CadVector a, string s)
        {
            CadVector pa = DC.CadPointToUnitPoint(a);
            DrawTextScrn(font, brush, pa, CadVector.UnitX, s);
        }

        public override void DrawTextScrn(int font, int brush, CadVector a, CadVector direction, string s)
        {
            if (DC.graphics == null) return;
            if (DC.Brush(brush) == null) return;
            if (DC.Font(font) == null) return;

            double angle = 0;

            if (direction.x != 0 || direction.y != 0)
            {
                angle = CadUtil.Angle2D(direction);
            }

            angle = CadMath.Rad2Deg(angle);

            BitmapData bd = DC.GetLockedBits();
            if (bd != null)
            {
                DC.UnlockBits();
            }

            DC.graphics.TranslateTransform((int)a.x, (int)a.y);

            DC.graphics.RotateTransform((float)angle);

            DC.graphics.DrawString(s, DC.Font(font), DC.Brush(brush), 0, 0);

            DC.graphics.ResetTransform();


            if (bd != null)
            {
                DC.LockBits();
            }
        }

        public override CadVector MeasureText(int font, string s)
        {
            if (DC.Font(font) == null)
            {
                return CadVector.Zero;
            }

            SizeF size = DC.graphics.MeasureString(s, DC.Font(font));

            CadVector v = CadVector.Create(size.Width, size.Height, 0);

            return v;
        }

        public override void DrawCrossCursorScrn(CadCursor pp, int pen)
        {
            double size = Math.Max(DC.ViewWidth, DC.ViewHeight);

            CadVector p0 = pp.Pos - (pp.DirX * size);
            CadVector p1 = pp.Pos + (pp.DirX * size);

            DrawLineScrn(pen, p0.x, p0.y, p1.x, p1.y);

            p0 = pp.Pos - (pp.DirY * size);
            p1 = pp.Pos + (pp.DirY * size);

            DrawLineScrn(pen, p0.x, p0.y, p1.x, p1.y);
        }

        public override void DrawRectScrn(int pen, CadVector pp0, CadVector pp1)
        {
            DrawRectangleScrn(pen, pp0.x, pp0.y, pp1.x, pp1.y);
        }

        private void DrawLineScrn(int pen, CadVector a, CadVector b)
        {
            if (DC.graphics == null) return;
            if (DC.Pen(pen) == null) return;

            DC.graphics.DrawLine(DC.Pen(pen), (int)a.x, (int)a.y, (int)b.x, (int)b.y);
        }

        private void DrawLineScrn(int pen, double x1, double y1, double x2, double y2)
        {
            if (DC.graphics == null) return;
            if (DC.Pen(pen) == null) return;

            DC.graphics.DrawLine(DC.Pen(pen), (int)x1, (int)y1, (int)x2, (int)y2);
        }

        private void DrawRectangleScrn(int pen, double x0, double y0, double x1, double y1)
        {
            if (DC.graphics == null) return;
            if (DC.Pen(pen) == null) return;

            int lx = (int)x0;
            int rx = (int)x1;

            int ty = (int)y0;
            int by = (int)y1;

            if (x0 > x1)
            {
                lx = (int)x1;
                rx = (int)x0;
            }

            if (y0 > y1)
            {
                ty = (int)y1;
                by = (int)y0;
            }

            int dx = rx - lx;
            int dy = by - ty;

            DC.graphics.DrawRectangle(DC.Pen(pen), lx, ty, dx, dy);
        }

        private void DrawCircleScrn(int pen, CadVector cp, CadVector p1)
        {
            if (DC.graphics == null) return;
            if (DC.Pen(pen) == null) return;

            double r = CadUtil.SegNorm(cp, p1);
            DrawCircleScrn(pen, cp, r);
        }

        private void DrawCircleScrn(int pen, CadVector cp, double r)
        {
            if (DC.graphics == null) return;
            if (DC.Pen(pen) == null) return;

            DC.graphics.DrawEllipse(
                DC.Pen(pen), (int)(cp.x - r), (int)(cp.y - r), (int)(r * 2), (int)(r * 2));
        }

        private void FillRectangleScrn(int brush, double x0, double y0, double x1, double y1)
        {
            if (DC.graphics == null) return;
            if (DC.Brush(brush) == null) return;

            int lx = (int)x0;
            int rx = (int)x1;

            int ty = (int)y0;
            int by = (int)y1;

            if (x0 > x1)
            {
                lx = (int)x1;
                rx = (int)x0;
            }

            if (y0 > y1)
            {
                ty = (int)y1;
                by = (int)y0;
            }

            int dx = rx - lx;
            int dy = by - ty;

            DC.graphics.FillRectangle(DC.Brush(brush), lx, ty, dx, dy);
        }

        private void DrawAxis2()
        {
            double size = 20;


            CadVector uv = CadVector.Create(size, 0, 0);

            CadVector cv = DC.UnitVectorToCadVector(uv);

            double len = cv.Norm();


            CadVector up = CadVector.Create(size+5, size+5, 0);

            CadVector cp = DC.UnitPointToCadPoint(up);


            CadVector p0 = default(CadVector);
            CadVector p1 = default(CadVector);



            // X軸
            p0.x = -len + cp.x;
            p0.y = 0 + cp.y;
            p0.z = 0 + cp.z;

            p1.x = len + cp.x;
            p1.y = 0 + cp.y;
            p1.z = 0 + cp.z;

            DrawLine(DrawTools.PEN_AXIS2, p0, p1);

            DrawText(DrawTools.FONT_SMALL, DrawTools.BRUSH_TEXT, p1, "x");

            // Y軸
            p0.x = 0 + cp.x;
            p0.y = -len + cp.y;
            p0.z = 0 + cp.z;

            p1.x = 0 + cp.x;
            p1.y = len + cp.y;
            p1.z = 0 + cp.z;

            DrawLine(DrawTools.PEN_AXIS2, p0, p1);
            DrawText(DrawTools.FONT_SMALL, DrawTools.BRUSH_TEXT, p1, "y");

            // Z軸
            p0.x = 0 + cp.x;
            p0.y = 0 + cp.y;
            p0.z = -len + cp.z;

            p1.x = 0 + cp.x;
            p1.y = 0 + cp.y;
            p1.z = len + cp.z;

            DrawLine(DrawTools.PEN_AXIS2, p0, p1);
            DrawText(DrawTools.FONT_SMALL, DrawTools.BRUSH_TEXT, p1, "z");
        }

        /*
        public override void DrawCircle(int pen, CadVector cp, CadVector pa, CadVector pb)
        {
            CadVector va = pa - cp;
            CadVector vb = pb - cp;

            if (va.Norm() < 0.01)
            {
                return;
            }

            CadVector uva = DC.CadVectorToUnitVector(va);
            CadVector uvb = DC.CadVectorToUnitVector(vb);

            double uva_n = uva.Norm();
            double uvb_n = uvb.Norm();

            double max_n = Math.Max(uva_n, uvb_n);

            double dt = Math.Atan2(4.0, max_n);

            if (dt > Math.PI/4.0)
            {
                dt = Math.PI / 4.0;
            }

            int div = (int)((2.0 * Math.PI) / dt);

            CadVector normal = CadMath.Normal(va, vb);

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
        */
    }
}
