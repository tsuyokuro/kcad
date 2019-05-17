/**
 * GDI向け描画クラス
 * 
 */

using HalfEdgeNS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using CadDataTypes;

namespace Plotter
{
    public class DrawingGDI : DrawingBase
    {
        public DrawContextGDI DC;

        public DrawingGDI()
        {
        }

        public DrawingGDI(DrawContextGDI dc)
        {
            DC = dc;
        }

        public override void Clear(DrawBrush brush)
        {
            FillRectangleScrn(
                brush,
                0, 0, (int)DC.ViewWidth, (int)DC.ViewHeight);
        }

        public override void Draw(List<CadFigure> list, DrawPen pen)
        {
            foreach (CadFigure fig in list)
            {
                fig.ForEachFig((Action<CadFigure>)(a =>
                {
                    if (a.Current)
                    {
                        a.Draw(DC, DrawPen.New(DC, DrawTools.PEN_FIGURE_HIGHLIGHT));
                    }
                    else
                    {
                        a.Draw(DC, pen);
                    }
                }));
            }
        }

        public override void DrawSelected(List<CadFigure> list, DrawPen pen)
        {
            foreach (CadFigure fig in list)
            {
                fig.ForEachFig(a =>
                {
                    a.DrawSelected(DC, pen);
                });
            }
        }

        #region "Draw base"
        public override void DrawAxis()
        {
            CadVertex p0 = default(CadVertex);
            CadVertex p1 = default(CadVertex);

            // X軸
            p0.x = -100;
            p0.y = 0;
            p0.z = 0;

            p1.x = 100;
            p1.y = 0;
            p1.z = 0;

            p0 /= DC.WorldScale;
            p1 /= DC.WorldScale;

            DrawLine(DrawPen.New(DC, DrawTools.PEN_AXIS), p0, p1);

            // Y軸
            p0.x = 0;
            p0.y = -100;
            p0.z = 0;

            p1.x = 0;
            p1.y = 100;
            p1.z = 0;

            p0 /= DC.WorldScale;
            p1 /= DC.WorldScale;

            DrawLine(DrawPen.New(DC, DrawTools.PEN_AXIS), p0, p1);

            // Z軸
            p0.x = 0;
            p0.y = 0;
            p0.z = -100;

            p1.x = 0;
            p1.y = 0;
            p1.z = 100;

            p0 /= DC.WorldScale;
            p1 /= DC.WorldScale;

            DrawLine(DrawPen.New(DC, DrawTools.PEN_AXIS), p0, p1);

            //DrawAxis2();
        }

        public override void DrawGrid(Gridding grid)
        {
            CadVertex lt = CadVertex.Zero;
            CadVertex rb = CadVertex.Create(DC.ViewWidth, DC.ViewHeight, 0);

            CadVertex ltw = DC.DevPointToWorldPoint(lt);
            CadVertex rbw = DC.DevPointToWorldPoint(rb);

            double minx = Math.Min(ltw.x, rbw.x);
            double maxx = Math.Max(ltw.x, rbw.x);

            double miny = Math.Min(ltw.y, rbw.y);
            double maxy = Math.Max(ltw.y, rbw.y);

            double minz = Math.Min(ltw.z, rbw.z);
            double maxz = Math.Max(ltw.z, rbw.z);


            DrawPen pen = DrawPen.New(DC, DrawTools.PEN_GRID);

            CadVertex p = default(CadVertex);


            double n = grid.Decimate(DC, grid, 8);

            double x, y, z;
            double sx, sy, sz;
            double szx = grid.GridSize.x * n;
            double szy = grid.GridSize.y * n;
            double szz = grid.GridSize.z * n;

            sx = Math.Round(minx / szx) * szx;
            sy = Math.Round(miny / szy) * szy;
            sz = Math.Round(minz / szz) * szz;

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
        }

        public override void DrawPageFrame(double w, double h, CadVertex center)
        {
            CadVertex pt = default(CadVertex);

            // p0
            pt.x = -w / 2 + center.x;
            pt.y = h / 2 + center.y;
            pt.z = 0;

            CadVertex p0 = default(CadVertex);
            p0.x = pt.x * DC.UnitPerMilli;
            p0.y = pt.y * DC.UnitPerMilli;

            p0 += DC.ViewOrg;

            // p1
            pt.x = w / 2 + center.x;
            pt.y = -h / 2 + center.y;
            pt.z = 0;

            CadVertex p1 = default(CadVertex);
            p1.x = pt.x * DC.UnitPerMilli;
            p1.y = pt.y * DC.UnitPerMilli;

            p1 += DC.ViewOrg;

            DrawRectScrn(DrawPen.New(DC, DrawTools.PEN_PAGE_FRAME), p0, p1);
        }
        #endregion

        #region "Draw marker"
        public override void DrawHighlightPoint(CadVertex pt, DrawPen pen)
        {
            CadVertex pp = DC.WorldPointToDevPoint(pt);

            //DrawCircleScrn(pen, pp, 3);

            DrawCrossScrn(pen, pp, 4);
        }

        public override void DrawSelectedPoint(CadVertex pt, DrawPen pen)
        {
            CadVertex pp = DC.WorldPointToDevPoint(pt);

            int size = 2;

            DrawRectangleScrn(
                pen,
                (int)pp.x - size, (int)pp.y - size,
                (int)pp.x + size, (int)pp.y + size
                );
        }

        public override void DrawMarkCursor(DrawPen pen, CadVertex p, double pix_size)
        {
            DrawCross(pen, p, pix_size);
        }
        #endregion

        public override void DrawHarfEdgeModel(DrawPen pen, DrawPen edgePen, double edgeThreshold, HeModel model)
        {
            for (int i = 0; i < model.FaceStore.Count; i++)
            {
                HeFace f = model.FaceStore[i];

                HalfEdge head = f.Head;

                HalfEdge c = head;

                HalfEdge pair;

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

                    DrawPen dpen = edge ? edgePen : pen;

                    DrawLine(dpen,
                        model.VertexStore.Ref(c.Vertex),
                        model.VertexStore.Ref(next.Vertex)
                        );

                    c = next;

                    if (c == head)
                    {
                        break;
                    }
                }
            }
        }

        public override void DrawRect(DrawPen pen, CadVertex p0, CadVertex p1)
        {
            CadVertex pp0 = DC.WorldPointToDevPoint(p0);
            CadVertex pp1 = DC.WorldPointToDevPoint(p1);

            DrawRectangleScrn(pen, pp0.x, pp0.y, pp1.x, pp1.y);
        }

        public override void DrawCross(DrawPen pen, CadVertex p, double size)
        {
            CadVertex a = DC.WorldPointToDevPoint(p);

            DrawLineScrn(pen, a.x - size, a.y + 0, a.x + size, a.y + 0);
            DrawLineScrn(pen, a.x + 0, a.y + size, a.x + 0, a.y - size);
        }

        public override void DrawCrossScrn(DrawPen pen, CadVertex p, double size)
        {
            DrawLineScrn(pen, p.x - size, p.y + 0, p.x + size, p.y + 0);
            DrawLineScrn(pen, p.x + 0, p.y + size, p.x + 0, p.y - size);
        }

        public override void DrawLine(DrawPen pen, CadVertex a, CadVertex b)
        {
            if (pen.GdiPen == null) return;

            CadVertex pa = DC.WorldPointToDevPoint(a);
            CadVertex pb = DC.WorldPointToDevPoint(b);

            DC.GdiGraphics.DrawLine(pen.GdiPen, (int)pa.x, (int)pa.y, (int)pb.x, (int)pb.y);
        }

        public override void DrawDot(DrawPen pen, CadVertex p)
        {
            CadVertex p0 = DC.WorldPointToDevPoint(p);
            CadVertex p1 = p0;
            p0.x = (int)p0.x;
            p1.x = p0.x + 0.1;

            DC.GdiGraphics.DrawLine(pen.GdiPen, (float)p0.x, (float)p0.y, (float)p1.x, (float)p1.y);

            //if (p0.x >= 0 && p0.y >= 0 && p0.x < DC.ViewWidth && p0.y < DC.ViewHeight)
            //{
            //    DC.Image.SetPixel((int)p0.x, (int)p0.y, DC.PenColor(pen));
            //}
        }

        public override void DrawFace(DrawPen pen, VertexList pointList, CadVertex Normal, bool drawOutline)
        {
            int cnt = pointList.Count;
            if (cnt == 0)
            {
                return;
            }

            CadVertex p0 = pointList[0];
            CadVertex p1;

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

        public override void DrawHarfEdgeModel(DrawPen pen, HeModel model)
        {
            base.DrawHarfEdgeModel(pen, model);
        }

        public override void DrawText(int font, DrawBrush brush, CadVertex a, CadVertex xdir, CadVertex ydir, DrawTextOption opt, string s)
        {
            CadVertex pa = DC.WorldPointToDevPoint(a);
            CadVertex d = DC.WorldVectorToDevVector(xdir);

            DrawTextScrn(font, brush, pa, d, opt, s);
        }

        public override void DrawTextScrn(int font, DrawBrush brush, CadVertex a, CadVertex dir, DrawTextOption opt, string s)
        {
            if (brush.GdiBrush == null) return;
            if (DC.Font(font) == null) return;

            if (opt.Option != 0)
            {
                CadVertex sz = MeasureText(font, s);

                if ((opt.Option | DrawTextOption.H_CENTER) != 0)
                {
                    double slen = sz.x / 2;

                    CadVertex ud = CadVertex.UnitX;

                    if (!dir.IsZero())
                    {
                        ud = dir.UnitVector();
                    }

                    a = a - (ud * slen);
                }
            }

            double angle = 0;

            if (!(dir.x == 0 && dir.y == 0))
            {
                angle = CadUtil.Angle2D(dir);
            }

            angle = CadMath.Rad2Deg(angle);

            DC.GdiGraphics.TranslateTransform((int)a.x, (int)a.y);

            DC.GdiGraphics.RotateTransform((float)angle);

            Font f = DC.Font(font);
            Brush b = brush.GdiBrush;

            DC.GdiGraphics.DrawString(s, f, b, 0, 0);

            DC.GdiGraphics.ResetTransform();
        }

        public override CadVertex MeasureText(int font, string s)
        {
            if (DC.Font(font) == null)
            {
                return CadVertex.Zero;
            }

            SizeF size = DC.GdiGraphics.MeasureString(s, DC.Font(font));

            CadVertex v = CadVertex.Create(size.Width, size.Height, 0);

            return v;
        }

        public override void DrawCrossCursorScrn(CadCursor pp, DrawPen pen)
        {
            double size = Math.Max(DC.ViewWidth, DC.ViewHeight);

            CadVertex p0 = pp.Pos - (pp.DirX * size);
            CadVertex p1 = pp.Pos + (pp.DirX * size);

            DrawLineScrn(pen, p0.x, p0.y, p1.x, p1.y);

            p0 = pp.Pos - (pp.DirY * size);
            p1 = pp.Pos + (pp.DirY * size);

            DrawLineScrn(pen, p0.x, p0.y, p1.x, p1.y);
        }

        public override void DrawRectScrn(DrawPen pen, CadVertex pp0, CadVertex pp1)
        {
            DrawRectangleScrn(pen, pp0.x, pp0.y, pp1.x, pp1.y);
        }

        protected void DrawLineScrn(DrawPen pen, CadVertex a, CadVertex b)
        {
            if (pen.GdiPen == null) return;

            DC.GdiGraphics.DrawLine(pen.GdiPen, (int)a.x, (int)a.y, (int)b.x, (int)b.y);
        }

        protected void DrawLineScrn(DrawPen pen, double x1, double y1, double x2, double y2)
        {
            if (pen.GdiPen == null) return;

            DC.GdiGraphics.DrawLine(pen.GdiPen, (int)x1, (int)y1, (int)x2, (int)y2);
        }

        protected void DrawRectangleScrn(DrawPen pen, double x0, double y0, double x1, double y1)
        {
            if (pen.GdiPen == null) return;

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

            DC.GdiGraphics.DrawRectangle(pen.GdiPen, lx, ty, dx, dy);
        }

        protected void DrawCircleScrn(DrawPen pen, CadVertex cp, CadVertex p1)
        {
            double r = CadUtil.SegNorm(cp, p1);
            DrawCircleScrn(pen, cp, r);
        }

        protected void DrawCircleScrn(DrawPen pen, CadVertex cp, double r)
        {
            if (pen.GdiPen == null) return;

            DC.GdiGraphics.DrawEllipse(
                pen.GdiPen, (int)(cp.x - r), (int)(cp.y - r), (int)(r * 2), (int)(r * 2));
        }

        protected void FillRectangleScrn(DrawBrush brush, double x0, double y0, double x1, double y1)
        {
            if (brush.GdiBrush == null) return;

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

            DC.GdiGraphics.FillRectangle(brush.GdiBrush, lx, ty, dx, dy);
        }

        protected void DrawAxis2()
        {
            double size = 20;


            CadVertex uv = CadVertex.Create(size, 0, 0);

            CadVertex cv = DC.DevVectorToWorldVector(uv);

            double len = cv.Norm();


            CadVertex up = CadVertex.Create(size+5, size+5, 0);

            CadVertex cp = DC.DevPointToWorldPoint(up);


            CadVertex p0 = default;
            CadVertex p1 = default;

            CadVertex tp = default;

            MinMax2D minMax2D = MinMax2D.Create();

            CadVertex xp = default;
            CadVertex yp = default;
            CadVertex zp = default;

            // X軸
            p0.x = -len + cp.x;
            p0.y = 0 + cp.y;
            p0.z = 0 + cp.z;

            p1.x = len + cp.x;
            p1.y = 0 + cp.y;
            p1.z = 0 + cp.z;

            DrawLine(DrawPen.New(DC, DrawTools.PEN_AXIS2), p0, p1);

            tp = DC.WorldPointToDevPoint(p0);
            minMax2D.Check(tp);
            tp = DC.WorldPointToDevPoint(p1);
            minMax2D.Check(tp);
            xp = tp;

            // Y軸
            p0.x = 0 + cp.x;
            p0.y = -len + cp.y;
            p0.z = 0 + cp.z;

            p1.x = 0 + cp.x;
            p1.y = len + cp.y;
            p1.z = 0 + cp.z;

            DrawLine(DrawPen.New(DC, DrawTools.PEN_AXIS2), p0, p1);

            tp = DC.WorldPointToDevPoint(p0);
            minMax2D.Check(tp);
            tp = DC.WorldPointToDevPoint(p1);
            minMax2D.Check(tp);

            yp = tp;

            // Z軸
            p0.x = 0 + cp.x;
            p0.y = 0 + cp.y;
            p0.z = -len + cp.z;

            p1.x = 0 + cp.x;
            p1.y = 0 + cp.y;
            p1.z = len + cp.z;

            DrawLine(DrawPen.New(DC, DrawTools.PEN_AXIS2), p0, p1);

            tp = DC.WorldPointToDevPoint(p0);
            minMax2D.Check(tp);
            tp = DC.WorldPointToDevPoint(p1);
            minMax2D.Check(tp);
            zp = tp;

            minMax2D.MaxX -= 8;
            minMax2D.MaxY -= 8;

            xp = minMax2D.Inner(xp);
            yp = minMax2D.Inner(yp);
            zp = minMax2D.Inner(zp);

            xp.y -= 7;
            yp.y -= 7;
            zp.y -= 7;

            DrawTextScrn(DrawTools.FONT_SMALL, DrawBrush.New(DC, DrawTools.BRUSH_TEXT), xp, CadVertex.UnitX, default(DrawTextOption), "x");
            DrawTextScrn(DrawTools.FONT_SMALL, DrawBrush.New(DC, DrawTools.BRUSH_TEXT), yp, CadVertex.UnitX, default(DrawTextOption), "y");
            DrawTextScrn(DrawTools.FONT_SMALL, DrawBrush.New(DC, DrawTools.BRUSH_TEXT), zp, CadVertex.UnitX, default(DrawTextOption), "z");
        }
    }
}
