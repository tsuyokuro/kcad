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


        // 視点座標系からワールド座標系への変換行列
        public UMatrix4 ViewMatrixInv;

        // ワールド座標系から視点座標系への変換行列
        public UMatrix4 ViewMatrix;


        // 視点座標系から投影座標系への変換行列
        public UMatrix4 ProjectionMatrix;

        // 投影座標系から視点座標系への変換行列
        public UMatrix4 ProjectionMatrixInv;

        public bool Perspective = false;

        public CadPoint ViewOrg
        {
            set
            {
                mViewOrg = value;
                calcViewCenter();
            }

            get
            {
                return mViewOrg;
            }
        }

        public double mViewWidth = 100;
        public double mViewHeight = 100;

        // Screenのサイズ
        public double ViewWidth
        {
            get
            {
                return mViewWidth;
            }
        }

        public double ViewHeight
        {
            get
            {
                return mViewHeight;
            }
        }

        public CadPoint ViewCenter = default(CadPoint);

        public DrawSettings Tools = new DrawSettings();

        protected Graphics mGraphics = null;

        public Graphics graphics
        {
            get { return mGraphics; }
            set { mGraphics = value; }
        }


        public void calcViewCenter()
        {
            CadPoint t = default(CadPoint);

            t.x = (mViewWidth / 2);
            t.y = (mViewHeight / 2);
            t.z = 0;

            t -= ViewOrg;

            t /= UnitPerMilli;

            ViewCenter = t;
        }

        public void setViewSize(double w, double h)
        {
            mViewWidth = w;
            mViewHeight = h;
        }

        public virtual void startDraw()
        {
        }

        public virtual void startDraw(Bitmap image)
        {
        }

        public virtual void endDraw()
        {
        }


        // set dots per milli.
        public virtual void setUnitPerMilli(double upm)
        {
            UnitPerMilli = upm;
        }

        // Calc inch units per milli.
        public virtual void setUnitPerInch(double unit)
        {
            UnitPerMilli = unit / MILLI_PER_INCH;
        }

        public virtual CadPoint CadPointToUnitPoint(CadPoint pt)
        {
            return default(CadPoint);
        }

        public virtual CadPoint UnitPointToCadPoint(CadPoint pt)
        {
            return default(CadPoint);
        }

        public virtual CadPoint UnitPointToCadPoint(double x, double y, double z = 0)
        {
            return default(CadPoint);
        }

        public virtual CadRect getViewRect()
        {
            CadRect rect = default(CadRect);

            CadPoint p0 = default(CadPoint);
            CadPoint p1 = default(CadPoint);

            p0.set(0, 0, 0);
            p1.set(ViewWidth, ViewHeight, 0);

            rect.p0 = UnitPointToCadPoint(p0);
            rect.p1 = UnitPointToCadPoint(p1);

            return rect;
        }

        public virtual double UnitToMilli(double d)
        {
            return d / UnitPerMilli;
        }

        public virtual double MilliToUnit(double d)
        {
            return d * UnitPerMilli;
        }

        public virtual void dump(DebugOut dout)
        {
        }
    }


    public class DrawContextWin : DrawContext
    {
        private int GraphicsRef = 0;

        public DrawContextWin()
        {
            setUnitPerMilli(4); // 1mm = 2.5dot
            mViewOrg.x = 0;
            mViewOrg.y = 0;

            ViewMatrix = UMatrixs.ViewXY;
            ViewMatrixInv = UMatrixs.ViewXYInv;

            ProjectionMatrix = UMatrixs.Unit;

            //MatrixToWorld = MatrixXZ;
            //MatrixToView = MatrixXZ.invers();

            //MatrixToWorld = MatrixXY_YQ_F * MatrixXY_XQ_F;
            //MatrixToView = MatrixXY_XQ_R * MatrixXY_YQ_R;
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

        // set dots per milli.
        public override void setUnitPerMilli(double upm)
        {
            UnitPerMilli = upm;
        }

        // Calc inch units per milli.
        public override void setUnitPerInch(double unit)
        {
            UnitPerMilli = unit / MILLI_PER_INCH;
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

            return UnitPointToCadPoint(p);
        }

        public override void dump(DebugOut dout)
        {
            dout.println("ViewOrg");
            ViewOrg.dump(dout);

            dout.println("ViewCenter");
            ViewCenter.dump(dout);

            dout.println("View Width=" + mViewWidth.ToString() + " Height=" + mViewHeight.ToString());
        }
    }
}
