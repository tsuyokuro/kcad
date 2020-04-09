//#define USE_GDI_VIEW

using OpenTK;
using Plotter;
using Plotter.Controller;
using System.ComponentModel;

namespace KCad.ViewModel
{
    public class ViewManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModelContext mContext;

        private IPlotterView mPlotterView = null;
        public IPlotterView PlotterView
        {
            get => mPlotterView;
        }

        public DrawContext DrawContext
        {
            get => mPlotterView.DrawContext;
        }

#if USE_GDI_VIEW
        private PlotterViewGDI PlotterView1GDI1 = null;
#endif
        private PlotterViewGL PlotterViewGL1 = null;


        private ViewModes mViewMode = ViewModes.NONE;
        public ViewModes ViewMode
        {
            set
            {
#if (USE_GDI_VIEW)
                bool changed = ChangeViewModeGdi(value);
#else
                bool changed = ChangeViewMode(value);
#endif
                if (changed)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewMode)));
                }
            }

            get => mViewMode;
        }

        public ViewManager(ViewModelContext context)
        {
            mContext = context;
        }

        public void SetupViews()
        {
#if USE_GDI_VIEW
            PlotterView1GDI1 = new PlotterViewGDI();
#endif
            PlotterViewGL1 = PlotterViewGL.Create();

            ViewMode = ViewModes.FRONT;
            ViewMode = ViewModes.FREE;  // 一旦GL側を設定してViewをLoadしておく
            ViewMode = ViewModes.FRONT;
        }

        public void SetWorldScale(double scale)
        {
#if USE_GDI_VIEW
            PlotterView1GDI1.SetWorldScale(scale);
#endif
            PlotterViewGL1.SetWorldScale(scale);
        }

        public void DrawModeUpdated(DrawTools.DrawMode mode)
        {
#if USE_GDI_VIEW
            PlotterView1GDI1.DrawModeUpdated(mode);
#endif
            PlotterViewGL1.DrawModeUpdated(mode);
        }


        private bool ChangeViewMode(ViewModes newMode)
        {
            if (mViewMode == newMode)
            {
                return false;
            }

            mViewMode = newMode;

            DrawContext currentDC = mPlotterView == null ? null : mPlotterView.DrawContext;
            DrawContext nextDC = mPlotterView == null ? null : mPlotterView.DrawContext;
            IPlotterView view = mPlotterView;

            switch (mViewMode)
            {
                case ViewModes.FRONT:
                    PlotterViewGL1.EnablePerse(false);
                    view = PlotterViewGL1;
                    view.DrawContext.SetCamera(
                        Vector3d.UnitZ * DrawContext.STD_EYE_DIST,
                        Vector3d.Zero, Vector3d.UnitY);
                    nextDC = view.DrawContext;
                    break;

                case ViewModes.BACK:
                    PlotterViewGL1.EnablePerse(false);
                    view = PlotterViewGL1;
                    view.DrawContext.SetCamera(
                        -Vector3d.UnitZ * DrawContext.STD_EYE_DIST,
                        Vector3d.Zero, Vector3d.UnitY);

                    nextDC = view.DrawContext;
                    break;

                case ViewModes.TOP:
                    PlotterViewGL1.EnablePerse(false);
                    view = PlotterViewGL1;
                    view.DrawContext.SetCamera(
                        Vector3d.UnitY * DrawContext.STD_EYE_DIST,
                        Vector3d.Zero, -Vector3d.UnitZ);

                    nextDC = view.DrawContext;
                    break;

                case ViewModes.BOTTOM:
                    PlotterViewGL1.EnablePerse(false);
                    view = PlotterViewGL1;
                    view.DrawContext.SetCamera(
                        -Vector3d.UnitY * DrawContext.STD_EYE_DIST,
                        Vector3d.Zero, Vector3d.UnitZ);

                    nextDC = view.DrawContext;
                    break;

                case ViewModes.RIGHT:
                    PlotterViewGL1.EnablePerse(false);
                    view = PlotterViewGL1;
                    view.DrawContext.SetCamera(
                        Vector3d.UnitX * DrawContext.STD_EYE_DIST,
                        Vector3d.Zero, Vector3d.UnitY);

                    nextDC = view.DrawContext;
                    break;

                case ViewModes.LEFT:
                    PlotterViewGL1.EnablePerse(false);
                    view = PlotterViewGL1;
                    view.DrawContext.SetCamera(
                        -Vector3d.UnitX * DrawContext.STD_EYE_DIST,
                        Vector3d.Zero, Vector3d.UnitY);

                    nextDC = view.DrawContext;
                    break;

                case ViewModes.FREE:
                    PlotterViewGL1.EnablePerse(true);
                    view = PlotterViewGL1;
                    nextDC = view.DrawContext;
                    break;
            }

            if (currentDC != null) currentDC.Deactive();
            if (nextDC != null) nextDC.Active();

            SetView(view);
            mContext.Redraw();
            return true;
        }

#if (USE_GDI_VIEW)
        private bool ChangeViewModeGdi(ViewModes newMode)
        {
            if (mViewMode == newMode)
            {
                return false;
            }

            mViewMode = newMode;

            DrawContext currentDC = mPlotterView == null ? null : mPlotterView.DrawContext;
            DrawContext nextDC = mPlotterView == null ? null : mPlotterView.DrawContext;
            IPlotterView view = mPlotterView;

            switch (mViewMode)
            {
                case ViewModes.FRONT:
                    view = PlotterView1GDI1;
                    view.DrawContext.SetCamera(
                        Vector3d.UnitZ * DrawContext.STD_EYE_DIST,
                        Vector3d.Zero, Vector3d.UnitY);
                    nextDC = view.DrawContext;
                    break;

                case ViewModes.BACK:
                    view = PlotterView1GDI1;
                    view.DrawContext.SetCamera(
                        -Vector3d.UnitZ * DrawContext.STD_EYE_DIST,
                        Vector3d.Zero, Vector3d.UnitY);

                    nextDC = view.DrawContext;
                    break;

                case ViewModes.TOP:
                    view = PlotterView1GDI1;
                    view.DrawContext.SetCamera(
                        Vector3d.UnitY * DrawContext.STD_EYE_DIST,
                        Vector3d.Zero, -Vector3d.UnitZ);

                    nextDC = view.DrawContext;
                    break;

                case ViewModes.BOTTOM:
                    view = PlotterView1GDI1;
                    view.DrawContext.SetCamera(
                        -Vector3d.UnitY * DrawContext.STD_EYE_DIST,
                        Vector3d.Zero, Vector3d.UnitZ);

                    nextDC = view.DrawContext;
                    break;

                case ViewModes.RIGHT:
                    view = PlotterView1GDI1;
                    view.DrawContext.SetCamera(
                        Vector3d.UnitX * DrawContext.STD_EYE_DIST,
                        Vector3d.Zero, Vector3d.UnitY);

                    nextDC = view.DrawContext;
                    break;

                case ViewModes.LEFT:
                    view = PlotterView1GDI1;
                    view.DrawContext.SetCamera(
                        -Vector3d.UnitX * DrawContext.STD_EYE_DIST,
                        Vector3d.Zero, Vector3d.UnitY);

                    nextDC = view.DrawContext;
                    break;

                case ViewModes.FREE:
                    PlotterViewGL1.EnablePerse(true);
                    view = PlotterViewGL1;
                    nextDC = view.DrawContext;
                    break;
            }

            if (currentDC != null) currentDC.Deactive();
            if (nextDC != null) nextDC.Active();

            SetView(view);
            mContext.Redraw();
            return true;
        }
#endif
        private void SetView(IPlotterView view)
        {
            //if (view == mPlotterView)
            //{
            //    return;
            //}

            if (mPlotterView != null)
            {
                mPlotterView.SetController(null);
            }

            mPlotterView = view;

            mPlotterView.SetController(mContext.Controller);

            mContext.Controller.DC = view.DrawContext;

            mContext.MainWindow.SetPlotterView(view);
        }
    }
}
