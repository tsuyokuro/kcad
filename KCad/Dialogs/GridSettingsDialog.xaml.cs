﻿using Plotter;
using CadDataTypes;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KCad
{
    public partial class GridSettingsDialog : Window
    {
        public CadVertex GridSize = default(CadVertex);

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
            grid_x_size.Text = GridSize.X.ToString();
            grid_y_size.Text = GridSize.Y.ToString();
            grid_z_size.Text = GridSize.Z.ToString();
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
            GridSize.X = v;

            ret &= Double.TryParse(grid_y_size.Text, out v);
            GridSize.Y = v;

            ret &= Double.TryParse(grid_z_size.Text, out v);
            GridSize.Z = v;

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
