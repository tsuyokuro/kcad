using System.Drawing;

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
        // 用紙サイズ
        public PaperPageSize PageSize = new PaperPageSize();

        // 画素/Milli
        // 1ミリあたりの画素数
        public double UnitPerMilli = 1;

        // 1inchは何ミリ?
        public const double MILLI_PER_INCH = 25.4;

        //private double XDir = 1;
        public double YDir = -1;
        //private double ZDir = 1;

        // Screen 座標系の原点 
        public CadPoint mViewOrg;

        // Screen座標系からワールド座標系への変換行列
        Matrix33 mMatrixToWorld;

        // ワールド座標系からScreen座標系への変換行列
        Matrix33 mMatrixToView;


        Matrix33 MatrixToWorld
        {
            set
            {
                mMatrixToWorld = value;
            }
            get
            {
                return mMatrixToWorld;
            }
        }

        Matrix33 MatrixToView
        {
            set
            {
                mMatrixToView = value;
            }
            get
            {
                return mMatrixToView;
            }
        }

        public CadPoint ViewOrg
        {
            set { mViewOrg = value; }
            get { return mViewOrg; }
        }

        // Screenのサイズ
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


        private static double xt = PI / 10.0;
        private static double yt = -PI / 6.0;

        private static Matrix33 MatrixXY_XQ_F = new Matrix33
            (
                1, 0, 0,
                0, Cos(xt), -Sin(xt),
                0, Sin(xt), Cos(xt)
            );

        private static Matrix33 MatrixXY_YQ_F = new Matrix33
            (
                Cos(yt), 0, Sin(yt),
                0, 1, 0,
                -Sin(yt), 0, Cos(yt)
            );


        private static Matrix33 MatrixXY_XQ_R = new Matrix33
            (
                1, 0, 0,
                0, Cos(-xt), -Sin(-xt),
                0, Sin(-xt), Cos(-xt)
            );

        private static Matrix33 MatrixXY_YQ_R = new Matrix33
            (
                Cos(-yt), 0, Sin(-yt),
                0, 1, 0,
                -Sin(-yt), 0, Cos(-yt)
            );


        public DrawContext()
        {
            setUnitPerMilli(4); // 1mm = 2.5dot
            mViewOrg.x = 0;
            mViewOrg.y = 0;

            MatrixToWorld = MatrixXY;
            MatrixToView = MatrixXY.invers();

            //MatrixTo3D = MatrixXY_YQ_F * MatrixXY_XQ_F;
            //MatrixTo2D = MatrixXY_XQ_R * MatrixXY_YQ_R;
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
        public void setUnitPerMilli(double upm)
        {
            UnitPerMilli = upm;
        }

        // Calc inch units per milli.
        public void setUnitPerInch(double unit)
        {
            UnitPerMilli = unit / MILLI_PER_INCH;
        }

        public CadPoint pointToPixelPoint(CadPoint pt)
        {
            pt = mMatrixToView * pt;

            CadPoint p = default(CadPoint);
            p.x = pt.x * UnitPerMilli;
            p.y = pt.y * UnitPerMilli * YDir;
            p.z = pt.z * UnitPerMilli;

            p = p + mViewOrg;
            return p;
        }

        public CadPoint pixelPointToCadPoint(CadPoint pt)
        {
            pt = pt - mViewOrg;

            CadPoint p = default(CadPoint);
            p.x = pt.x / UnitPerMilli;
            p.y = pt.y / UnitPerMilli * YDir;
            p.z = pt.z / UnitPerMilli;

            p = mMatrixToWorld * p;

            return p;
        }

        public CadPoint pixelPointToCadPoint(double x, double y, double z = 0)
        {
            x -= mViewOrg.x;
            y -= mViewOrg.y;

            CadPoint p = default(CadPoint);
            p.x = x / UnitPerMilli;
            p.y = y / UnitPerMilli * YDir;
            p.z = z / UnitPerMilli;

            p = mMatrixToWorld * p;

            return p;
        }

        public CadRect getViewRect()
        {
            CadRect rect = default(CadRect);

            CadPoint p0 = default(CadPoint);
            CadPoint p1 = default(CadPoint);

            p0.set(0, 0, 0);
            p1.set(ViewWidth, ViewHeight, 0);

            rect.p0 = pixelPointToCadPoint(p0);
            rect.p1 = pixelPointToCadPoint(p1);

            return rect;
        }

        public double pixelsToMilli(double d)
        {
            return d / UnitPerMilli;
        }

        public double milliToPixels(double d)
        {
            return d * UnitPerMilli;
        }
    }
}
