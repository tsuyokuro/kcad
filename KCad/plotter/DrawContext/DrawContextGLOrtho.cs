using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using CadDataTypes;
using System.Windows.Forms;

namespace Plotter
{
    class DrawContextGLOrtho : DrawContextGL
    {
        CadVector Center = default;

        public override double UnitPerMilli
        {
            set
            {
                mUnitPerMilli = value;
                CalcProjectionMatrix();
            }

            get => mUnitPerMilli;
        }

        public DrawContextGLOrtho()
        {
            Init(null);
            mUnitPerMilli = 4;
        }

        public DrawContextGLOrtho(Control control)
        {
            Init(control);
            mUnitPerMilli = 4;
        }

        public override void Active()
        {
            CalcProjectionMatrix();
        }

        public override void StartDraw()
        {
            GL.Viewport(0, 0, (int)mViewWidth, (int)mViewHeight);

            GL.Enable(EnableCap.DepthTest);

            #region ModelView
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref mViewMatrix.Matrix);
            #endregion

            #region Projection            
            GL.MatrixMode(MatrixMode.Projection);

            Matrix4d proj = mProjectionMatrix.Matrix;

            double dx = ViewOrg.x - (ViewWidth / 2.0);
            double dy = ViewOrg.y - (ViewHeight / 2.0);

            // x,yの平行移動成分を設定
            // Set x and y translational components
            proj.M41 = dx / (ViewWidth / 2.0);
            proj.M42 = -dy / (ViewHeight / 2.0);

            GL.LoadMatrix(ref proj);
            #endregion

            SetupLight();
        }

        public override CadVector WorldPointToDevPoint(CadVector pt)
        {
            CadVector p = WorldVectorToDevVector(pt);
            p = p + mViewOrg;

            return p;
        }

        public override CadVector DevPointToWorldPoint(CadVector pt)
        {
            pt = pt - mViewOrg;
            CadVector p = DevVectorToWorldVector(pt);

            return p;
        }

        public override CadVector WorldVectorToDevVector(CadVector pt)
        {
            pt *= WorldScale;

            Vector4d wv = (Vector4d)pt;

            wv.W = 1.0f;

            Vector4d sv = wv * mViewMatrix;
            Vector4d pv = sv * mProjectionMatrix;

            Vector4d dv;

            dv.X = pv.X / pv.W;
            dv.Y = pv.Y / pv.W;
            dv.Z = pv.Z / pv.W;
            dv.W = pv.W;

            dv.X = dv.X * (DeviceScaleX);
            dv.Y = dv.Y * (DeviceScaleY);
            dv.Z = 0;

            return CadVector.Create(dv);
        }

        public override CadVector DevVectorToWorldVector(CadVector pt)
        {
            pt.x = pt.x / (DeviceScaleX);
            pt.y = pt.y / (DeviceScaleY);

            Vector4d wv;

            wv.W = mProjectionW;
            wv.Z = mProjectionZ;

            wv.X = pt.x * wv.W;
            wv.Y = pt.y * wv.W;

            wv = wv * mProjectionMatrixInv;
            wv = wv * mViewMatrixInv;

            wv /= WorldScale;

            return CadVector.Create(wv);
        }

        public override void SetViewOrg(CadVector org)
        {
            mViewOrg = org;
            CalcProjectionMatrix();
        }

        public override void SetViewSize(double w, double h)
        {
            mViewWidth = w;
            mViewHeight = h;

            DeviceScaleX = w / 2.0;
            DeviceScaleY = -h / 2.0;

            GL.Viewport(0, 0, (int)mViewWidth, (int)mViewHeight);

            CalcProjectionMatrix();
            CalcProjectionZW();

            Center.x = w / 2;
            Center.y = h / 2;

            Matrix2D = Matrix4d.CreateOrthographicOffCenter(
                                        0, mViewWidth,
                                        mViewHeight, 0,
                                        0, mProjectionFar);
        }

        public override void CalcProjectionMatrix()
        {
            mProjectionMatrix = Matrix4d.CreateOrthographic(
                                            ViewWidth / mUnitPerMilli, ViewHeight / mUnitPerMilli,
                                            mProjectionNear,
                                            mProjectionFar
                                            );

            mProjectionMatrixInv = mProjectionMatrix.Invert();
        }
    }
}