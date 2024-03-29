﻿using System;
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
    /// <summary>
    /// PrintSettingsDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class PrintSettingsDialog : Window
    {
        public bool PrintWithBitmap;
        public double MagnificationBitmapPrinting;

        public PrintSettingsDialog()
        {
            InitializeComponent();
            Loaded += PrintSettingsDialog_Loaded;
            PreviewKeyDown += PrintSettingsDialog_PreviewKeyDown;
            magnification_for_bitmap_printing.PreviewTextInput += PreviewTextInputForNum;
        }

        private void PrintSettingsDialog_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                HandleOK();
            }
            else if (e.Key == Key.Escape)
            {
                HandleCancel();
            }
        }

        private void PrintSettingsDialog_Loaded(object sender, RoutedEventArgs e)
        {
            print_with_bitmap.IsChecked = PrintWithBitmap;
            magnification_for_bitmap_printing.Text = MagnificationBitmapPrinting.ToString();

            ok_button.Click += Ok_button_Click;
            cancel_button.Click += Cancel_button_Click;
        }

        private void Cancel_button_Click(object sender, RoutedEventArgs e)
        {
            HandleCancel();
        }

        private void Ok_button_Click(object sender, RoutedEventArgs e)
        {
            HandleOK();
        }

        private void HandleCancel()
        {
            DialogResult = false;
        }

        private void HandleOK()
        {
            bool ret = true;

            double v;

            PrintWithBitmap = print_with_bitmap.IsChecked.Value;

            ret &= Double.TryParse(magnification_for_bitmap_printing.Text, out v);
            MagnificationBitmapPrinting = v;

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
