using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CadDataTypes;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Plotter
{
    class DrawContextGLPers : DrawContextGL
    {
        public DrawContextGLPers()
        {
            Init(null);
            mUnitPerMilli = 1;
        }

        public DrawContextGLPers(Control control)
        {
            Init(control);
            mUnitPerMilli = 1;
        }

        public override void StartDraw()
        {
            GL.Viewport(0, 0, (int)mViewWidth, (int)mViewHeight);

            GL.Enable(EnableCap.DepthTest);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref mViewMatrix.Matrix);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref mProjectionMatrix.Matrix);

            SetupLight();
        }

        public override void SetViewSize(double w, double h)
        {
            mViewWidth = w;
            mViewHeight = h;

            mViewOrg.X = w / 2.0;
            mViewOrg.Y = h / 2.0;

            DeviceScaleX = w / 2.0;
            DeviceScaleY = -h / 2.0;

            GL.Viewport(0, 0, (int)mViewWidth, (int)mViewHeight);

            CalcProjectionMatrix();
            CalcProjectionZW();

            Matrix2D = Matrix4d.CreateOrthographicOffCenter(
                                        0, mViewWidth,
                                        mViewHeight, 0,
                                        0, mProjectionFar);
        }

        public override void CalcProjectionMatrix()
        {
            double aspect = mViewWidth / mViewHeight;
            mProjectionMatrix = Matrix4d.CreatePerspectiveFieldOfView(
                                            mFovY,
                                            aspect,
                                            mProjectionNear,
                                            mProjectionFar
                                            );

            mProjectionMatrixInv = mProjectionMatrix.Invert();
        }

        public override DrawContext CreatePrinterContext(CadSize2D pageSize, CadSize2D deviceSize)
        {
            DrawContextGL dc = new DrawContextGLPers();

            dc.CopyProjectionMetrics(this);
            dc.WorldScale = WorldScale;

            dc.CopyCamera(this);
            dc.SetViewSize(deviceSize.Width, deviceSize.Height);

            Vector3d org = default;
            org.X = deviceSize.Width / 2.0;
            org.Y = deviceSize.Height / 2.0;

            dc.SetViewOrg(org);

            dc.UnitPerMilli = deviceSize.Width / pageSize.Width;

            return dc;
        }

        public void RotateEyePoint(Vector2 prev, Vector2 current)
        {
            Vector2 d = current - prev;

            double ry = (d.X / 10.0) * (Math.PI / 20);
            double rx = (d.Y / 10.0) * (Math.PI / 20);

            CadQuaternion q;
            CadQuaternion r;
            CadQuaternion qp;

            q = CadQuaternion.RotateQuaternion(Vector3d.UnitY, ry);

            r = q.Conjugate();

            qp = CadQuaternion.FromVector(mEye);
            qp = r * qp;
            qp = qp * q;
            mEye = qp.ToVector3d();

            qp = CadQuaternion.FromVector(mUpVector);
            qp = r * qp;
            qp = qp * q;
            mUpVector = qp.ToVector3d();

            Vector3d ev = mLookAt - mEye;

            Vector3d a = new Vector3d(ev);
            Vector3d b = new Vector3d(mUpVector);

            Vector3d axis = CadMath.Normal(a, b);

            if (!axis.IsZero())
            {

                q = CadQuaternion.RotateQuaternion(axis, rx);

                r = q.Conjugate();

                qp = CadQuaternion.FromVector(mEye);
                qp = r * qp;
                qp = qp * q;

                mEye = qp.ToVector3d();

                qp = CadQuaternion.FromVector(mUpVector);
                qp = r * qp;
                qp = qp * q;
                mUpVector = qp.ToVector3d();
            }

            CalcViewMatrix();
            CalcViewDir();
            CalcProjectionZW();
        }

        public void MoveForwardEyePoint(double d, bool withLookAt = false)
        {
            Vector3d dv = ViewDir * d;

            if (withLookAt)
            {
                mEye += dv;
                mLookAt += dv;
            }
            else
            {
                Vector3d eye = mEye + dv;

                Vector3d viewDir = mLookAt - eye;

                viewDir.Normalize();

                if ((ViewDir - viewDir).Length > 1.0)
                {
                    return;
                }

                mEye += dv;
            }

            CalcViewMatrix();
            CalcProjectionMatrix();
            CalcProjectionZW();
            CalcViewDir();
        }
    }
}
