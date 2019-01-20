using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace KCad.Dialogs
{
    public partial class AngleInputDialog : Window
    {
        public AngleInputDialog()
        {
            InitializeComponent();
            MainWindow wnd = (MainWindow)Application.Current.MainWindow;

            Point p = wnd.viewContainer.PointToScreen(new Point(0,0));

            this.Left = p.X;
            this.Top = p.Y;

            cancel_button.Click += Cancel_button_Click;
            ok_button.Click += Ok_button_Click;

            LayoutRoot.MouseLeftButtonDown += LayoutRoot_MouseLeftButtonDown;

            PreviewKeyDown += AngleInputDialog_PreviewKeyDown;
        }

        private void AngleInputDialog_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.DialogResult = true;
            }
            else if (e.Key == Key.Escape)
            {
                this.DialogResult = false;
            }
        }

        private void LayoutRoot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Ok_button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Cancel_button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        public double GetDouble()
        {
            String s = input.Text;
            double v;
            Double.TryParse(s, out v);

            return v;
        }

        public string GetInputString()
        {
            String s = input.Text;
            return s;
        }
    }
}
