﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Plotter
{
    public class PaperPageSize
    {
        public double width;
        public double height;

        public double widthInch
        {
            set
            {
                width = value * 25.4;
            }

            get
            {
                return width / 25.4;
            }
        }

        public double heightInch
        {
            set
            {
                height = value * 25.4;
            }

            get
            {
                return height / 25.4;
            }
        }

        public PaperPageSize()
        {
            A4Land();
        }

        public void A4()
        {
            width = 210.0;
            height = 297.0;
        }

        public void A4Land()
        {
            width = 297.0;
            height = 210.0;
        }

        public PaperPageSize clone()
        {
            return (PaperPageSize)MemberwiseClone();
        }
    }


    public class DrawContext
    {
        public PaperPageSize PageSize = new PaperPageSize();

        public double UnitPerMilliX = 1; // Output unit per milli X.
        public double UnitPerMilliY = 1; // Output unit per milli Y.

        private const double MILLI_PER_INCH = 25.4;

        private double XDir = 1;
        private double YDir = -1;
        private double ZDir = 1;

        private CadPixelPoint mViewOrg;

        public CadPixelPoint ViewOrg
        {
            set { mViewOrg = value; }
            get { return mViewOrg; }
        }

        public int ViewWidth { get; set; } = 100;
        public int ViewHeight { get; set; } = 100;

        private Graphics mGraphics = null;

        public Graphics graphics {
            get { return mGraphics; }
            set { mGraphics = value; }
        }

        private int GraphicsRef = 0;

        public DrawSettings Tools = new DrawSettings();

        public DrawContext()
        {
            setDotPerMilli(4, 4); // 1mm = 4dot
            mViewOrg.x = 0;
            mViewOrg.y = 0;
        }

        public void startDraw(Bitmap image)
        {
            if (mGraphics == null)
            {
                mGraphics = Graphics.FromImage(image);
            }
            GraphicsRef++;
        }

        public void endDraw()
        {
            GraphicsRef--;
            if (GraphicsRef <= 0)
            {
                disposeGraphics();
                GraphicsRef = 0;
            }
        }

        public void disposeGraphics()
        {
            if (mGraphics == null)
            {
                return;
            }

            mGraphics.Dispose();
            mGraphics = null;
        }

        // set dots per milli.
        public void setDotPerMilli(double dpmx, double dpmy)
        {
            UnitPerMilliX = dpmx;
            UnitPerMilliY = dpmy;
        }

        // Calc inch units per milli.
        public void setUnitPerInch(double unitX, double unitY)
        {
            UnitPerMilliX = unitX / MILLI_PER_INCH;
            UnitPerMilliY = unitY / MILLI_PER_INCH;
        }

        public CadPixelPoint pointToPixelPoint(CadPoint pt)
        {
            CadPixelPoint p;
            p.x = (int)((pt.x) * UnitPerMilliX * XDir);
            p.y = (int)((pt.y) * UnitPerMilliY * YDir);

            p += mViewOrg;

            return p;
        }

        public CadPixelPoint pointToPixelPoint0(CadPoint pt)
        {
            CadPixelPoint p;
            p.x = (int)((pt.x) * UnitPerMilliX * XDir);
            p.y = (int)((pt.y) * UnitPerMilliY * YDir);

            return p;
        }

        public CadPoint pixelPointToCadPoint(ref CadPixelPoint pt)
        {
            pt -= mViewOrg;

            CadPoint p = default(CadPoint);
            p.x = ((double)(pt.x) / UnitPerMilliX) * XDir;
            p.y = ((double)(pt.y) / UnitPerMilliY) * YDir;
            p.z = ((double)0 / UnitPerMilliX) * ZDir;

            return p;
        }

        public CadPoint pixelPointToCadPoint(double x, double y)
        {
            x -= mViewOrg.x;
            y -= mViewOrg.y;

            CadPoint p = default(CadPoint);
            p.x = ((double)(x) / UnitPerMilliX) * XDir;
            p.y = ((double)(y) / UnitPerMilliY) * YDir;
            p.z = ((double)0 / UnitPerMilliX) * ZDir;

            return p;
        }

        public CadRect getViewRect()
        {
            CadRect rect = default(CadRect);
            rect.p0 = pixelPointToCadPoint(0, 0);
            rect.p1 = pixelPointToCadPoint(ViewWidth, ViewHeight);

            return rect;
        }

        public double pixelVToCadV(int d)
        {
            return ((double)(d) / UnitPerMilliX) * XDir;
        }

        public double cadVToPixelV(double d)
        {
            return d * UnitPerMilliX;
        }
    }
}