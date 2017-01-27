using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public class DrawContextWin : DrawContext
    {
        private int GraphicsRef = 0;

        protected Graphics mGraphics = null;

        public Graphics graphics
        {
            set { mGraphics = value; }
        }

        public DrawContextWin()
        {
            setUnitPerMilli(4); // 1mm = 2.5dot
            mViewOrg.x = 0;
            mViewOrg.y = 0;

            ViewMatrix = UMatrixs.ViewXY;
            ViewMatrixInv = UMatrixs.ViewXYInv;

            ProjectionMatrix = UMatrixs.Unit;
        }

        public override void startDraw(Bitmap image)
        {
            if (image == null)
            {
                return;
            }

            if (mGraphics == null)
            {
                mGraphics = Graphics.FromImage(image);
            }
            GraphicsRef++;
        }

        public override void endDraw()
        {
            GraphicsRef--;
            if (GraphicsRef <= 0)
            {
                disposeGraphics();
                GraphicsRef = 0;
            }
        }

        private void disposeGraphics()
        {
            if (mGraphics == null)
            {
                return;
            }

            mGraphics.Dispose();
            mGraphics = null;
        }

        public override CadPoint CadPointToUnitPoint(CadPoint pt)
        {
            pt = ViewMatrix * pt;


            if (Perspective)
            {
                CadPoint vp = pt - ViewCenter;

                double d = 1 + (-vp.z / 400);

                vp.x = vp.x / d;
                vp.y = vp.y / d;
                vp.z = 0;

                pt = vp + ViewCenter;
            }


            CadPoint p = default(CadPoint);

            p.x = pt.x * UnitPerMilli;
            p.y = pt.y * UnitPerMilli * YDir;
            p.z = pt.z * UnitPerMilli;

            p = p + mViewOrg;
            return p;
        }

        public override CadPoint UnitPointToCadPoint(CadPoint pt)
        {
            pt = pt - mViewOrg;

            CadPoint p = default(CadPoint);
            p.x = pt.x / UnitPerMilli;
            p.y = pt.y / UnitPerMilli * YDir;
            p.z = pt.z / UnitPerMilli;

            if (Perspective)
            {
                CadPoint vp = p - ViewCenter;

                double d = 1 + (-vp.z / 400);

                vp.x = vp.x * d;
                vp.y = vp.y * d;
                vp.z = 0;

                p = vp + ViewCenter;
            }

            p = ViewMatrixInv * p;

            return p;
        }

        public override CadPoint UnitPointToCadPoint(double x, double y, double z = 0)
        {
            CadPoint p = default(CadPoint);

            p.x = x;
            p.y = y;
            p.z = z;
            p.w = 1.0;

            return UnitPointToCadPoint(p);
        }

        private Pen Pen(int id)
        {
            return Tools.pen(id);
        }

        private Font Font(int id)
        {
            return Tools.font(id);
        }

        private Brush Brush(int id)
        {
            return Tools.brush(id);
        }


        public override void DrawLine(int pen, CadPoint a, CadPoint b)
        {
            if (mGraphics == null) return;
            if (Pen(pen) == null) return;

            CadPoint pa = CadPointToUnitPoint(a);
            CadPoint pb = CadPointToUnitPoint(b);

            mGraphics.DrawLine(Pen(pen), (int)pa.x, (int)pa.y, (int)pb.x, (int)pb.y);
        }

        public override void DrawLineScrn(int pen, CadPoint a, CadPoint b)
        {
            if (mGraphics == null) return;
            if (Pen(pen) == null) return;

            mGraphics.DrawLine(Pen(pen), (int)a.x, (int)a.y, (int)b.x, (int)b.y);
        }

        public override void DrawLineScrn(int pen, double x1, double y1, double x2, double y2)
        {
            if (mGraphics == null) return;
            if (Pen(pen) == null) return;

            mGraphics.DrawLine(Pen(pen), (int)x1, (int)y1, (int)x2, (int)y2);
        }

        public override void DrawRectangleScrn(int pen, double x0, double y0, double x1, double y1)
        {
            if (mGraphics == null) return;
            if (Pen(pen) == null) return;

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

            mGraphics.DrawRectangle(Pen(pen), lx, ty, dx, dy);
        }

        public override void DrawCircle(int pen, CadPoint cp, CadPoint p1)
        {
            if (mGraphics == null) return;
            if (Pen(pen) == null) return;

            double r = CadUtil.segNorm(cp, p1);
            DrawCircle(pen, cp, r);
        }

        public override void DrawCircle(int pen, CadPoint cp, double r)
        {
            if (mGraphics == null) return;
            if (Pen(pen) == null) return;

            CadPoint cpp = CadPointToUnitPoint(cp);

            r = MilliToUnit(r);

            mGraphics.DrawEllipse(
                Pen(pen), (int)(cpp.x - r), (int)(cpp.y - r), (int)(r * 2), (int)(r * 2));
        }

        public override void DrawCircleScrn(int pen, CadPoint cp, CadPoint p1)
        {
            if (mGraphics == null) return;
            if (Pen(pen) == null) return;

            double r = CadUtil.segNorm(cp, p1);
            DrawCircleScrn(pen, cp, r);
        }

        public override void DrawCircleScrn(int pen, CadPoint cp, double r)
        {
            if (mGraphics == null) return;
            if (Pen(pen) == null) return;

            mGraphics.DrawEllipse(
                Pen(pen), (int)(cp.x - r), (int)(cp.y - r), (int)(r * 2), (int)(r * 2));
        }

        public override void DrawText(int font, int brush, CadPoint a, string s)
        {
            if (mGraphics == null) return;
            if (Brush(brush) == null) return;
            if (Font(font) == null) return;

            CadPoint pa = CadPointToUnitPoint(a);
            mGraphics.DrawString(s, Font(font), Brush(brush), (int)pa.x, (int)pa.y);
        }

        public override void DrawTextScrn(int font, int brush, CadPoint a, string s)
        {
            if (mGraphics == null) return;
            if (Brush(brush) == null) return;
            if (Font(font) == null) return;

            mGraphics.DrawString(s, Font(font), Brush(brush), (int)a.x, (int)a.y);
        }

        public override void DrawTextScrn(int font, int brush, double x, double y, string s)
        {
            if (mGraphics == null) return;
            if (Brush(brush) == null) return;
            if (Font(font) == null) return;

            mGraphics.DrawString(s, Font(font), Brush(brush), (int)x, (int)y);
        }

        public override void FillRectangleScrn(int brush, double x0, double y0, double x1, double y1)
        {
            if (mGraphics == null) return;
            if (Brush(brush) == null) return;

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

            mGraphics.FillRectangle(Brush(brush), lx, ty, dx, dy);
        }
    }
}
