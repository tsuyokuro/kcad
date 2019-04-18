/**
 * GDI向け描画クラス
 * 
 */

//#define USE_LONG_TERM_LOCK_BITS // ある程度長い期間LockBitsし続ける

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

        public override void Clear(int brush)
        {
            FillRectangleScrn(
                brush,
                0, 0, (int)DC.ViewWidth, (int)DC.ViewHeight);
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

        public override void DrawSelected(List<CadFigure> list, int pen = DrawTools.PEN_DEFAULT_FIGURE)
        {
#if USE_LONG_TERM_LOCK_BITS
                DC.LockBits();
#endif

            foreach (CadFigure fig in list)
            {
                fig.ForEachFig(a =>
                {
                    a.DrawSelected(DC, pen);
                });
            }

#if USE_LONG_TERM_LOCK_BITS
                DC.UnlockBits();
#endif
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

            //DrawAxis2();
        }

        public override void DrawGrid(Gridding grid)
        {
            CadVector lt = CadVector.Zero;
            CadVector rb = CadVector.Create(DC.ViewWidth, DC.ViewHeight, 0);

            CadVector ltw = DC.DevPointToWorldPoint(lt);
            CadVector rbw = DC.DevPointToWorldPoint(rb);

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
        public override void DrawHighlightPoint(CadVector pt, int pen = DrawTools.PEN_POINT_HIGHLIGHT)
        {
            CadVector pp = DC.WorldPointToDevPoint(pt);

            //DrawCircleScrn(pen, pp, 3);

            DrawCrossScrn(pen, pp, 4);
        }

        public override void DrawSelectedPoint(CadVector pt, int pen = DrawTools.PEN_SELECT_POINT)
        {
            CadVector pp = DC.WorldPointToDevPoint(pt);

            int size = 2;

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

                    int dpen = edge ? edgePen : pen;

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

        public override void DrawRect(int pen, CadVector p0, CadVector p1)
        {
            CadVector pp0 = DC.WorldPointToDevPoint(p0);
            CadVector pp1 = DC.WorldPointToDevPoint(p1);

            DrawRectangleScrn(pen, pp0.x, pp0.y, pp1.x, pp1.y);
        }

        public override void DrawCross(int pen, CadVector p, double size)
        {
            CadVector a = DC.WorldPointToDevPoint(p);

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
            if (DC.Pen(pen) == null) return;

            CadVector pa = DC.WorldPointToDevPoint(a);
            CadVector pb = DC.WorldPointToDevPoint(b);

            DC.GdiGraphics.DrawLine(DC.Pen(pen), (int)pa.x, (int)pa.y, (int)pb.x, (int)pb.y);
        }

        public override void DrawDot(int pen, CadVector p)
        {
            CadVector p0 = DC.WorldPointToDevPoint(p);
            CadVector p1 = p0;
            p0.x = (int)p0.x;
            p1.x = p0.x + 0.1;

            DC.GdiGraphics.DrawLine(DC.Pen(pen), (float)p0.x, (float)p0.y, (float)p1.x, (float)p1.y);

            //if (p0.x >= 0 && p0.y >= 0 && p0.x < DC.ViewWidth && p0.y < DC.ViewHeight)
            //{
            //    DC.Image.SetPixel((int)p0.x, (int)p0.y, DC.PenColor(pen));
            //}
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

        public override void DrawText(int font, int brush, CadVector a, CadVector xdir, CadVector ydir, DrawTextOption opt, string s)
        {
            CadVector pa = DC.WorldPointToDevPoint(a);
            CadVector d = DC.WorldVectorToDevVector(xdir);

            DrawTextScrn(font, brush, pa, d, opt, s);
        }

        public override void DrawTextScrn(int font, int brush, CadVector a, CadVector dir, DrawTextOption opt, string s)
        {
            if (DC.Brush(brush) == null) return;
            if (DC.Font(font) == null) return;

            if (opt.Option != 0)
            {
                CadVector sz = MeasureText(font, s);

                if ((opt.Option | DrawTextOption.H_CENTER) != 0)
                {
                    double slen = sz.x / 2;

                    CadVector ud = CadVector.UnitX;

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
            Brush b = DC.Brush(brush);

            DC.GdiGraphics.DrawString(s, f, b, 0, 0);

            DC.GdiGraphics.ResetTransform();
        }

        public override CadVector MeasureText(int font, string s)
        {
            if (DC.Font(font) == null)
            {
                return CadVector.Zero;
            }

            SizeF size = DC.GdiGraphics.MeasureString(s, DC.Font(font));

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

        protected void DrawLineScrn(int pen, CadVector a, CadVector b)
        {
            if (DC.Pen(pen) == null) return;

            DC.GdiGraphics.DrawLine(DC.Pen(pen), (int)a.x, (int)a.y, (int)b.x, (int)b.y);
        }

        protected void DrawLineScrn(int pen, double x1, double y1, double x2, double y2)
        {
            if (DC.Pen(pen) == null) return;

            DC.GdiGraphics.DrawLine(DC.Pen(pen), (int)x1, (int)y1, (int)x2, (int)y2);
        }

        protected void DrawRectangleScrn(int pen, double x0, double y0, double x1, double y1)
        {
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

            DC.GdiGraphics.DrawRectangle(DC.Pen(pen), lx, ty, dx, dy);
        }

        protected void DrawCircleScrn(int pen, CadVector cp, CadVector p1)
        {
            if (DC.Pen(pen) == null) return;

            double r = CadUtil.SegNorm(cp, p1);
            DrawCircleScrn(pen, cp, r);
        }

        protected void DrawCircleScrn(int pen, CadVector cp, double r)
        {
            if (DC.Pen(pen) == null) return;

            DC.GdiGraphics.DrawEllipse(
                DC.Pen(pen), (int)(cp.x - r), (int)(cp.y - r), (int)(r * 2), (int)(r * 2));
        }

        protected void FillRectangleScrn(int brush, double x0, double y0, double x1, double y1)
        {
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

            DC.GdiGraphics.FillRectangle(DC.Brush(brush), lx, ty, dx, dy);
        }

        protected void DrawAxis2()
        {
            double size = 20;


            CadVector uv = CadVector.Create(size, 0, 0);

            CadVector cv = DC.DevVectorToWorldVector(uv);

            double len = cv.Norm();


            CadVector up = CadVector.Create(size+5, size+5, 0);

            CadVector cp = DC.DevPointToWorldPoint(up);


            CadVector p0 = default;
            CadVector p1 = default;

            CadVector tp = default;

            MinMax2D minMax2D = MinMax2D.Create();

            CadVector xp = default;
            CadVector yp = default;
            CadVector zp = default;

            // X軸
            p0.x = -len + cp.x;
            p0.y = 0 + cp.y;
            p0.z = 0 + cp.z;

            p1.x = len + cp.x;
            p1.y = 0 + cp.y;
            p1.z = 0 + cp.z;

            DrawLine(DrawTools.PEN_AXIS2, p0, p1);

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

            DrawLine(DrawTools.PEN_AXIS2, p0, p1);

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

            DrawLine(DrawTools.PEN_AXIS2, p0, p1);

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

            DrawTextScrn(DrawTools.FONT_SMALL, DrawTools.BRUSH_TEXT, xp, CadVector.UnitX, default(DrawTextOption), "x");
            DrawTextScrn(DrawTools.FONT_SMALL, DrawTools.BRUSH_TEXT, yp, CadVector.UnitX, default(DrawTextOption), "y");
            DrawTextScrn(DrawTools.FONT_SMALL, DrawTools.BRUSH_TEXT, zp, CadVector.UnitX, default(DrawTextOption), "z");
        }
    }
}
