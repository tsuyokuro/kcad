﻿/**
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
            p0.X = -100;
            p0.Y = 0;
            p0.Z = 0;

            p1.X = 100;
            p1.Y = 0;
            p1.Z = 0;

            p0 /= DC.WorldScale;
            p1 /= DC.WorldScale;

            DrawLine(DrawPen.New(DC, DrawTools.PEN_AXIS), p0, p1);

            // Y軸
            p0.X = 0;
            p0.Y = -100;
            p0.Z = 0;

            p1.X = 0;
            p1.Y = 100;
            p1.Z = 0;

            p0 /= DC.WorldScale;
            p1 /= DC.WorldScale;

            DrawLine(DrawPen.New(DC, DrawTools.PEN_AXIS), p0, p1);

            // Z軸
            p0.X = 0;
            p0.Y = 0;
            p0.Z = -100;

            p1.X = 0;
            p1.Y = 0;
            p1.Z = 100;

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

            double minx = Math.Min(ltw.X, rbw.X);
            double maxx = Math.Max(ltw.X, rbw.X);

            double miny = Math.Min(ltw.Y, rbw.Y);
            double maxy = Math.Max(ltw.Y, rbw.Y);

            double minz = Math.Min(ltw.Z, rbw.Z);
            double maxz = Math.Max(ltw.Z, rbw.Z);


            DrawPen pen = DrawPen.New(DC, DrawTools.PEN_GRID);

            CadVertex p = default(CadVertex);


            double n = grid.Decimate(DC, grid, 8);

            double x, y, z;
            double sx, sy, sz;
            double szx = grid.GridSize.X * n;
            double szy = grid.GridSize.Y * n;
            double szz = grid.GridSize.Z * n;

            sx = Math.Round(minx / szx) * szx;
            sy = Math.Round(miny / szy) * szy;
            sz = Math.Round(minz / szz) * szz;

            x = sx;
            while (x < maxx)
            {
                p.X = x;
                p.Z = 0;

                y = sy;

                while (y < maxy)
                {
                    p.Y = y;
                    DrawDot(pen, p);
                    y += szy;
                }

                x += szx;
            }

            z = sz;
            y = sy;

            while (z < maxz)
            {
                p.Z = z;
                p.X = 0;

                y = sy;

                while (y < maxy)
                {
                    p.Y = y;
                    DrawDot(pen, p);
                    y += szy;
                }

                z += szz;
            }

            z = sz;
            x = sx;

            while (x < maxx)
            {
                p.X = x;
                p.Y = 0;

                z = sz;

                while (z < maxz)
                {
                    p.Z = z;
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
            pt.X = -w / 2 + center.X;
            pt.Y = h / 2 + center.Y;
            pt.Z = 0;

            CadVertex p0 = default(CadVertex);
            p0.X = pt.X * DC.UnitPerMilli;
            p0.Y = pt.Y * DC.UnitPerMilli;

            p0 += DC.ViewOrg;

            // p1
            pt.X = w / 2 + center.X;
            pt.Y = -h / 2 + center.Y;
            pt.Z = 0;

            CadVertex p1 = default(CadVertex);
            p1.X = pt.X * DC.UnitPerMilli;
            p1.Y = pt.Y * DC.UnitPerMilli;

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
                (int)pp.X - size, (int)pp.Y - size,
                (int)pp.X + size, (int)pp.Y + size
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

            DrawRectangleScrn(pen, pp0.X, pp0.Y, pp1.X, pp1.Y);
        }

        public override void DrawCross(DrawPen pen, CadVertex p, double size)
        {
            CadVertex a = DC.WorldPointToDevPoint(p);

            DrawLineScrn(pen, a.X - size, a.Y + 0, a.X + size, a.Y + 0);
            DrawLineScrn(pen, a.X + 0, a.Y + size, a.X + 0, a.Y - size);
        }

        public override void DrawCrossScrn(DrawPen pen, CadVertex p, double size)
        {
            DrawLineScrn(pen, p.X - size, p.Y + 0, p.X + size, p.Y + 0);
            DrawLineScrn(pen, p.X + 0, p.Y + size, p.X + 0, p.Y - size);
        }

        public override void DrawLine(DrawPen pen, CadVertex a, CadVertex b)
        {
            if (pen.GdiPen == null) return;

            CadVertex pa = DC.WorldPointToDevPoint(a);
            CadVertex pb = DC.WorldPointToDevPoint(b);

            DC.GdiGraphics.DrawLine(pen.GdiPen, (int)pa.X, (int)pa.Y, (int)pb.X, (int)pb.Y);
        }

        public override void DrawDot(DrawPen pen, CadVertex p)
        {
            CadVertex p0 = DC.WorldPointToDevPoint(p);
            CadVertex p1 = p0;
            p0.X = (int)p0.X;
            p1.X = p0.X + 0.1;

            DC.GdiGraphics.DrawLine(pen.GdiPen, (float)p0.X, (float)p0.Y, (float)p1.X, (float)p1.Y);

            //if (p0.x >= 0 && p0.y >= 0 && p0.x < DC.ViewWidth && p0.y < DC.ViewHeight)
            //{
            //    DC.Image.SetPixel((int)p0.x, (int)p0.y, DC.PenColor(pen));
            //}
        }

        //public override void DrawFace(DrawPen pen, VertexList pointList, CadVertex Normal, bool drawOutline)
        //{
        //    int cnt = pointList.Count;
        //    if (cnt == 0)
        //    {
        //        return;
        //    }

        //    CadVertex p0 = pointList[0];
        //    CadVertex p1;

        //    int i;
        //    for (i = 1; i < cnt; i++)
        //    {
        //        p1 = pointList[i];
        //        DrawLine(pen, p0, p1);
        //        p0 = p1;
        //    }

        //    p1 = pointList[0];
        //    DrawLine(pen, p0, p1);
        //}

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
                    double slen = sz.X / 2;

                    CadVertex ud = CadVertex.UnitX;

                    if (!dir.IsZero())
                    {
                        ud = dir.UnitVector();
                    }

                    a = a - (ud * slen);
                }
            }

            double angle = 0;

            if (!(dir.X == 0 && dir.Y == 0))
            {
                angle = CadUtil.Angle2D(dir);
            }

            angle = CadMath.Rad2Deg(angle);

            DC.GdiGraphics.TranslateTransform((int)a.X, (int)a.Y);

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

            DrawLineScrn(pen, p0.X, p0.Y, p1.X, p1.Y);

            p0 = pp.Pos - (pp.DirY * size);
            p1 = pp.Pos + (pp.DirY * size);

            DrawLineScrn(pen, p0.X, p0.Y, p1.X, p1.Y);
        }

        public override void DrawRectScrn(DrawPen pen, CadVertex pp0, CadVertex pp1)
        {
            DrawRectangleScrn(pen, pp0.X, pp0.Y, pp1.X, pp1.Y);
        }

        protected void DrawLineScrn(DrawPen pen, CadVertex a, CadVertex b)
        {
            if (pen.GdiPen == null) return;

            DC.GdiGraphics.DrawLine(pen.GdiPen, (int)a.X, (int)a.Y, (int)b.X, (int)b.Y);
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
                pen.GdiPen, (int)(cp.X - r), (int)(cp.Y - r), (int)(r * 2), (int)(r * 2));
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
            p0.X = -len + cp.X;
            p0.Y = 0 + cp.Y;
            p0.Z = 0 + cp.Z;

            p1.X = len + cp.X;
            p1.Y = 0 + cp.Y;
            p1.Z = 0 + cp.Z;

            DrawLine(DrawPen.New(DC, DrawTools.PEN_AXIS2), p0, p1);

            tp = DC.WorldPointToDevPoint(p0);
            minMax2D.Check(tp);
            tp = DC.WorldPointToDevPoint(p1);
            minMax2D.Check(tp);
            xp = tp;

            // Y軸
            p0.X = 0 + cp.X;
            p0.Y = -len + cp.Y;
            p0.Z = 0 + cp.Z;

            p1.X = 0 + cp.X;
            p1.Y = len + cp.Y;
            p1.Z = 0 + cp.Z;

            DrawLine(DrawPen.New(DC, DrawTools.PEN_AXIS2), p0, p1);

            tp = DC.WorldPointToDevPoint(p0);
            minMax2D.Check(tp);
            tp = DC.WorldPointToDevPoint(p1);
            minMax2D.Check(tp);

            yp = tp;

            // Z軸
            p0.X = 0 + cp.X;
            p0.Y = 0 + cp.Y;
            p0.Z = -len + cp.Z;

            p1.X = 0 + cp.X;
            p1.Y = 0 + cp.Y;
            p1.Z = len + cp.Z;

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

            xp.Y -= 7;
            yp.Y -= 7;
            zp.Y -= 7;

            DrawTextScrn(DrawTools.FONT_SMALL, DrawBrush.New(DC, DrawTools.BRUSH_TEXT), xp, CadVertex.UnitX, default(DrawTextOption), "x");
            DrawTextScrn(DrawTools.FONT_SMALL, DrawBrush.New(DC, DrawTools.BRUSH_TEXT), yp, CadVertex.UnitX, default(DrawTextOption), "y");
            DrawTextScrn(DrawTools.FONT_SMALL, DrawBrush.New(DC, DrawTools.BRUSH_TEXT), zp, CadVertex.UnitX, default(DrawTextOption), "z");
        }
    }
}
