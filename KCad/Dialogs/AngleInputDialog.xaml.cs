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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KCad.Dialogs
{
    public partial class AngleInputDialog : Window
    {
        public AngleInputDialog()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;

            cancel_button.Click += Cancel_button_Click;
            ok_button.Click += Ok_button_Click;
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
            return Double.Parse(s);
        }

        public string GetInputString()
        {
            String s = input.Text;
            return s;
        }
    }
}
