﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KCad
{
    public partial class SnapSettingsDialog : Window
    {
        public double PointSnapRange;
        public double LineSnapRange;

        public SnapSettingsDialog()
        {
            InitializeComponent();

            point_snap.PreviewTextInput += PreviewTextInputForNum;
            line_snap.PreviewTextInput += PreviewTextInputForNum;

            ok_button.Click += Ok_button_Click;
            cancel_button.Click += Cancel_button_Click;

            this.Loaded += SnapSettingsDialog_Loaded;
        }

        private void SnapSettingsDialog_Loaded(object sender, RoutedEventArgs e)
        {
            point_snap.Text = PointSnapRange.ToString();
            line_snap.Text = LineSnapRange.ToString();
        }

        private void Cancel_button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Ok_button_Click(object sender, RoutedEventArgs e)
        {
            bool ret = true;

            double v;

            ret &= Double.TryParse(point_snap.Text, out v);
            PointSnapRange = v;

            ret &= Double.TryParse(line_snap.Text, out v);
            LineSnapRange = v;

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
