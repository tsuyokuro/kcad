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
    /// <summary>
    /// DocumentSettingsDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class DocumentSettingsDialog : Window
    {
        public double WoldScale = 1.0;

        public DocumentSettingsDialog()
        {
            InitializeComponent();

            reduced_scale.PreviewTextInput += PreviewTextInputForNum;

            ok_button.Click += Ok_button_Click;
            cancel_button.Click += Cancel_button_Click;

            Loaded += DocumentSettingsDialog_Loaded;

        }

        private void DocumentSettingsDialog_Loaded(object sender, RoutedEventArgs e)
        {
            reduced_scale.Text = WoldScale.ToString();
        }

        private void Cancel_button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Ok_button_Click(object sender, RoutedEventArgs e)
        {
            bool ret;
            double v;

            ret = Double.TryParse(reduced_scale.Text, out v);

            WoldScale = v;

            DialogResult = ret;
        }

        private void PreviewTextInputForNum(object sender, TextCompositionEventArgs e)
        {
            bool ok = false;

            TextBox tb = (TextBox)sender;

            double v;
            var tmp = tb.Text + e.Text;
            ok = Double.TryParse(tmp, out v);

            e.Handled = !ok;
        }
    }
}
