using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            mViewMatrix = UMatrixs.ViewXY;
            mViewMatrixInv = UMatrixs.ViewXYInv;

            mProjectionMatrix = UMatrixs.Unit;
            mProjectionMatrixInv = UMatrixs.Unit;

            Drawing = new DrawingGDI(this);
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
            pt *= WoldScale;

            // 透視変換用にWが必要なので、Vector4に変換
            Vector4d ptv = (Vector4d)pt;

            ptv.W = 1.0f;

            ptv = ptv * mViewMatrix;
            ptv = ptv * mProjectionMatrix;

            ptv.X /= ptv.W;
            ptv.Y /= ptv.W;
            ptv.Z /= ptv.W;

            CadPoint p = default(CadPoint);

            p.x = ptv.X * (UnitPerMilli * DeviceScaleX);
            p.y = ptv.Y * (UnitPerMilli * DeviceScaleY);
            p.z = ptv.Z * UnitPerMilli;

            p = p + mViewOrg;

            return p;
        }

        public override CadPoint UnitPointToCadPoint(CadPoint pt)
        {
            pt = pt - mViewOrg;

            CadPoint p = default(CadPoint);
            p.x = pt.x / (UnitPerMilli * DeviceScaleX);
            p.y = pt.y / (UnitPerMilli * DeviceScaleY);
            p.z = pt.z / UnitPerMilli;

            p = p * mProjectionMatrixInv;

            p = p * mViewMatrixInv;

            p /= WoldScale;

            return p;
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
