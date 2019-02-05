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
    public class DrawingGDIBmp : DrawingGDI
    {
        public DrawContextGDIBmp BmpDC
        {
            get => (DrawContextGDIBmp)DC;
        }

        public DrawingGDIBmp(DrawContextGDIBmp dc)
        {
            DC = dc;
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


            Color c = DC.PenColor(DrawTools.PEN_GRID);

            int argb = c.ToArgb();

            double n = grid.Decimate(DC, grid, 8);

            double sx, sy, sz;
            double szx = grid.GridSize.x * n;
            double szy = grid.GridSize.y * n;
            double szz = grid.GridSize.z * n;

            sx = Math.Round(minx / szx) * szx;
            sy = Math.Round(miny / szy) * szy;
            sz = Math.Round(minz / szz) * szz;

            DrawDots(sx, sy, sz, szx, szy, szz, maxx, maxy, maxz, argb);
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


            Bitmap tgt = BmpDC.Image;

            BitmapData bitmapData = BmpDC.LockBits();

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
                        up = DC.WorldPointToDevPoint(p);

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

                        up = DC.WorldPointToDevPoint(p);

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

                        up = DC.WorldPointToDevPoint(p);

                        if (up.x >= 0 && up.x < tgt.Width && up.y >= 0 && up.y < tgt.Height)
                        {
                            *(srcPixels + ((int)up.y * tgt.Width) + (int)up.x) = argb;
                        }

                        z += szz;
                    }

                    x += szx;
                }
            }

            BmpDC.UnlockBits();
        }

        public override void DrawDot(int pen, CadVector p)
        {
            CadVector p0 = DC.WorldPointToDevPoint(p);

            if (p0.x >= 0 && p0.y >= 0 && p0.x < DC.ViewWidth && p0.y < DC.ViewHeight)
            {
                BmpDC.Image.SetPixel((int)p0.x, (int)p0.y, DC.PenColor(pen));
            }
        }
    }
}
