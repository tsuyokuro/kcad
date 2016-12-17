using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using static System.Math;

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

        //private double XDir = 1;
        private double YDir = -1;
        //private double ZDir = 1;

        private CadPixelPoint mViewOrg;

        Matrix33 mMatrixTo3D;

        Matrix33 mMatrixTo2D;


        Matrix33 MatrixTo3D
        {
            set
            {
                mMatrixTo3D = value;
                mMatrixTo2D = mMatrixTo3D.invers();
            }
            get
            {
                return mMatrixTo3D;
            }
        }

        Matrix33 MatrixTo2D
        {
            set
            {
                mMatrixTo2D = value;
                mMatrixTo3D = mMatrixTo2D.invers();
            }
            get
            {
                return mMatrixTo2D;
            }
        }

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

        private Matrix33 MatrixXY = new Matrix33
            (
                1, 0, 0,
                0, 1, 0,
                0, 0, 1
            );

        // 1, 0, 0,
        // 0, Cos(PI/2), -Sin(PI/2),
        // 0, Sin(PI/2), Cos(PI/2)
        private Matrix33 MatrixXZ = new Matrix33
            (
                1, 0, 0,
                0, 0, -1,
                0, 1, 0
            );

        // Cos(PI/2), 0, Sin(PI/2),
        // 0, 1, 0,
        // -Sin(PI/2), 0, Cos(PI/2)
        private Matrix33 MatrixZY = new Matrix33
            (
                0, 0, 1,
                0, 1, 0,
                -1, 0, 0
            );

        public DrawContext()
        {
            setDotPerMilli(4, 4); // 1mm = 4dot
            mViewOrg.x = 0;
            mViewOrg.y = 0;

            MatrixTo3D = MatrixZY;
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
            pt = mMatrixTo2D * pt;

            CadPixelPoint p;
            p.x = (int)((pt.x) * UnitPerMilliX);
            p.y = (int)((pt.y) * UnitPerMilliY * YDir);

            p += mViewOrg;
            return p;
        }

        public CadPoint pixelPointToCadPoint(CadPixelPoint pt)
        {
            pt -= mViewOrg;

            CadPoint p = default(CadPoint);
            p.x = ((double)(pt.x) / UnitPerMilliX);
            p.y = ((double)(pt.y) / UnitPerMilliY) * YDir;
            p.z = ((double)0 / UnitPerMilliX);

            p = mMatrixTo3D * p;

            return p;
        }

        public CadPoint pixelPointToCadPoint(double x, double y)
        {
            x -= mViewOrg.x;
            y -= mViewOrg.y;

            CadPoint p = default(CadPoint);
            p.x = ((double)(x) / UnitPerMilliX);
            p.y = ((double)(y) / UnitPerMilliY) * YDir;
            p.z = ((double)0 / UnitPerMilliX);

            p = mMatrixTo3D * p;

            return p;
        }

        public CadRect getViewRect()
        {
            CadRect rect = default(CadRect);
            rect.p0 = pixelPointToCadPoint(new CadPixelPoint(0,0));
            rect.p1 = pixelPointToCadPoint(new CadPixelPoint(ViewWidth, ViewHeight));

            return rect;
        }

        public double pixelsToMilli(int d)
        {
            return ((double)(d) / UnitPerMilliX);
        }

        public double milliToPixels(double d)
        {
            return d * UnitPerMilliX;
        }
    }
}
