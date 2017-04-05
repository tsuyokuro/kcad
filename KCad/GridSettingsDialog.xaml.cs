using Plotter;
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

namespace KCad
{
    public partial class GridSettingsDialog : Window
    {
        public CadPoint GridSize = default(CadPoint);

        public GridSettingsDialog()
        {
            InitializeComponent();

            grid_x_size.PreviewTextInput += PreviewTextInputForNum;
            grid_y_size.PreviewTextInput += PreviewTextInputForNum;
            grid_z_size.PreviewTextInput += PreviewTextInputForNum;

            ok_button.Click += Ok_button_Click;
            cancel_button.Click += Cancel_button_Click;

            this.Loaded += GridSettingsDialog_Loaded;
        }

        private void GridSettingsDialog_Loaded(object sender, RoutedEventArgs e)
        {
            grid_x_size.Text = GridSize.x.ToString();
            grid_y_size.Text = GridSize.y.ToString();
            grid_z_size.Text = GridSize.z.ToString();
        }

        private void Cancel_button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Ok_button_Click(object sender, RoutedEventArgs e)
        {
            bool ret = true;

            double v;

            ret &= Double.TryParse(grid_x_size.Text, out v);
            GridSize.x = v;

            ret &= Double.TryParse(grid_y_size.Text, out v);
            GridSize.y = v;

            ret &= Double.TryParse(grid_z_size.Text, out v);
            GridSize.z = v;

            this.DialogResult = ret;
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
