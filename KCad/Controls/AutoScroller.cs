using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Plotter;

namespace KCad.Controls
{
    public class AutoScroller
    {
        private DispatcherTimer Timer;

        public Action<double, double> Scroll = (x, y) => {};

        private FrameworkElement View;

        private FrameworkElement CheckView;

        private bool IsStarted = false;

        private double CheckInterval = 0.1;

        public AutoScroller(FrameworkElement view, double checkInterval)
        {
            Init(view, checkInterval);
        }

        public AutoScroller(FrameworkElement view)
        {
            Init(view, CheckInterval);
        }

        public void Init(FrameworkElement view, double checkInterval)
        {
            View = view;

            CheckView = view;

            CheckInterval = checkInterval;

            ScrollViewer parent = (ScrollViewer)view.Parent;

            if (parent is ScrollViewer)
            {
                CheckView = parent;
            }
        }

        public void Start()
        {
            if (!IsStarted)
            {
                DOut.tpl("AutoScroller.Start");

                IsStarted = true;

                Mouse.Capture(View, CaptureMode.Element);

                Timer = new DispatcherTimer();

                Timer.Interval = TimeSpan.FromSeconds(CheckInterval);
                Timer.Tick += TimerTick;
                Timer.Start();
            }
        }

        public void End()
        {
            if (IsStarted)
            {
                DOut.tpl("AutoScroller.End");

                IsStarted = false;
                Mouse.Capture(null);

                if (Timer != null)
                {
                    Timer.Stop();
                    Timer = null;
                }
            }
        }

        private void TimerTick(object sender, EventArgs e)
        {
            Check();
        }

        private void Check()
        {
            FrameworkElement v = CheckView;
            
            var Pos = Mouse.GetPosition(v);

            double x = 0;
            double y = 0;

            if (Pos.X < 0)
            {
                x = Pos.X;
            }
            
            if (Pos.X > v.ActualWidth)
            {
                x = Pos.X - v.ActualWidth;
            }

            if (Pos.Y < 0)
            {
               y = Pos.Y;
            }

            if (Pos.Y > v.ActualHeight)
            {
                y = Pos.Y - v.ActualHeight;
            }

            if (x != 0 || y != 0)
            {
                Scroll(x, y);
            }
        }
    }
}
