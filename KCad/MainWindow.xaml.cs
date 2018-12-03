using Plotter;
using Plotter.Controller;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace KCad
{
    public partial class MainWindow : Window
    {
        public PlotterViewModel ViewModel;

        private bool KeyHandled = false;

        //private LBConsole mLBConsole;

        public MainWindow()
        {
            InitializeComponent();

            //FocusManager.SetIsFocusScope(this, true);

            if (App.UseConsole)
            {
                // DOutの出力はデフォルトでConsoleになっているので、UseConsoleの場合は、
                // あらためて設定する必要はない
                DOut.pl("DOut's output setting is Console");
            }
            else {
                DOut.PrintFunc = MyConsole.Print;
                DOut.PrintLnFunc = MyConsole.PrintLn;
                DOut.FormatPrintFunc = MyConsole.Printf;
            }


            ItConsole.PrintFunc = MyConsole.Print;
            ItConsole.PrintLnFunc = MyConsole.PrintLn;
            ItConsole.FormatPrintFunc = MyConsole.Printf;
            ItConsole.clear = MyConsole.Clear;

            ViewModel = new PlotterViewModel(this, viewContainer);

            viewContainer.Focusable = true;

            ViewModel.LayerListView = LayerListView;
            ViewModel.SetObjectTreeView(ObjTree);

            ViewModel.SetupTextCommandView(textCommand);
            textCommand.Determine += TextCommand_Determine;

            KeyDown += onKeyDown;
            KeyUp += onKeyUp;

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;

            AddLayerButton.Click += ViewModel.ButtonClicked;
            RemoveLayerButton.Click += ViewModel.ButtonClicked;
            RunTextCommandButton.Click += RunTextCommandButtonClicked;


            // Setup Data Context
            LayerListView.DataContext = ViewModel.LayerList;

            SlsectModePanel.DataContext = ViewModel;
            FigurePanel.DataContext = ViewModel;

            textBlockXYZ.DataContext = ViewModel.CursorPosVM;
            textBlockXYZ2.DataContext = ViewModel.CursorPosVM;

            ViewModePanel.DataContext = ViewModel;

            SnapMenu.DataContext = ViewModel;

            DrawOptionMenu.DataContext = ViewModel;

            ToolBar1.DataContext = ViewModel;

            TreeViewToolBar.DataContext = ViewModel;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            ViewModel.Close();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Open();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MenuItemClicked(sender, e);
        }

        #region TextCommand
        public void RunTextCommandButtonClicked(object sender, RoutedEventArgs e)
        {
            var s = textCommand.Text;

            textCommand.Text = "";

            if (s.Length > 0)
            {
                ViewModel.TextCommand(s);
                textCommand.History.Add(s);
                textCommand.Focus();
            }
        }

        private void TextCommand_Determine(object sender, AutoCompleteTextBox.TextEventArgs e)
        {
            var s = e.Text;

            textCommand.Text = "";

            if (s.Length > 0)
            {
                ViewModel.TextCommand(s);
                //viewContainer.Focus();
            }
        }
        #endregion

        #region "Key handling"
        private void onKeyDown(object sender, KeyEventArgs e)
        {
            if (!textCommand.IsFocused)
            {
                ViewModel.OnKeyDown(sender, e);
            }
        }

        private void onKeyUp(object sender, KeyEventArgs e)
        {
            if (KeyHandled)
            {
                KeyHandled = false;
                return;
            }

            if (!textCommand.IsFocused && !MyConsole.IsFocused)
            {
               e.Handled = ViewModel.OnKeyUp(sender, e);
            }
        }
        #endregion
    }
}
