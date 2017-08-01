using OpenTK;
using System.Drawing;

namespace Plotter
{
    public class DrawContextGDI : DrawContext
    {
        private int GraphicsRef = 0;

        protected Graphics mGraphics = null;

        public Graphics graphics
        {
            set { mGraphics = value; }
            get { return mGraphics; }
        }

        public DrawContextGDI()
        {
            SetUnitPerMilli(4); // 1mm = 2.5dot
            mViewOrg.x = 0;
            mViewOrg.y = 0;

            /*
            mViewMatrix = UMatrixs.ViewXY;
            mViewMatrixInv = UMatrixs.ViewXYInv;
            */

            mProjectionMatrix = UMatrix4.Unit;
            mProjectionMatrixInv = UMatrix4.Unit;


            mDrawing = new DrawingGDI(this);
        }

        public override void SetViewSize(double w, double h)
        {
            mViewWidth = w;
            mViewHeight = h;

            if (w == 0 || h==0)
            {
                return;
            }

            /*
            mProjectionMatrix.GLMatrix =
                Matrix4d.CreateOrthographic(mViewWidth, mViewHeight, ProjectionNear, ProjectionFar);

            mProjectionMatrixInv.GLMatrix = Matrix4d.Invert(mProjectionMatrix.GLMatrix);

            CadUtil.Dump(DebugOut.Std, mProjectionMatrix, "Proj");
            CadUtil.Dump(DebugOut.Std, mProjectionMatrixInv, "Proj inv");
            */
        }

        public override void StartDraw(Bitmap image)
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

        public override void EndDraw()
        {
            GraphicsRef--;
            if (GraphicsRef <= 0)
            {
                DisposeGraphics();
                GraphicsRef = 0;
            }
        }

        private void DisposeGraphics()
        {
            if (mGraphics == null)
            {
                return;
            }

            mGraphics.Dispose();
            mGraphics = null;
        }

        public override CadVector CadPointToUnitPoint(CadVector pt)
        {
            CadVector p = CadVectorToUnitVector(pt);
            p = p + mViewOrg;
            return p;
        }

        public override CadVector UnitPointToCadPoint(CadVector pt)
        {
            pt = pt - mViewOrg;
            return UnitVectorToCadVector(pt);
        }

        public override CadVector CadVectorToUnitVector(CadVector pt)
        {
            Vector4d ptv = (Vector4d)pt;

            ptv.W = 1.0f;

            ptv = ptv * mViewMatrix;
            ptv = ptv * mProjectionMatrix;

            ptv.X /= ptv.W;
            ptv.Y /= ptv.W;
            ptv.Z /= ptv.W;

            CadVector p = default(CadVector);

            p.x = ptv.X * (UnitPerMilli * DeviceScaleX);
            p.y = ptv.Y * (UnitPerMilli * DeviceScaleY);
            //p.z = ptv.Z * UnitPerMilli;
            p.z = 0;

            return p;
        }

        public override CadVector UnitVectorToCadVector(CadVector pt)
        {
            Vector4d wv = default(Vector4d);
            wv.X = pt.x / (UnitPerMilli * DeviceScaleX);
            wv.Y = pt.y / (UnitPerMilli * DeviceScaleY);
            //wv.Z = pt.z / UnitPerMilli;
            wv.Z = pt.z;

            wv = wv * mProjectionMatrixInv;

            wv = wv * mViewMatrixInv;

            wv /= WoldScale;

            return CadVector.Create(wv);
        }

        public Pen Pen(int id)
        {
            return Tools.pen(id);
        }

        public Font Font(int id)
        {
            return Tools.font(id);
        }

        public Brush Brush(int id)
        {
            return Tools.brush(id);
        }
    }
}
