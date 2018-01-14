using Plotter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private PlotterController.Interaction mInteractionOut = new PlotterController.Interaction();

        private bool KeyHandled = false;

        //private LBConsole mLBConsole;

        public MainWindow()
        {
            InitializeComponent();

            if (App.GetCurrent().InputThread != null)
            {
                App.GetCurrent().InputThread.OnLineArrived = DebugCommand;
            }

            //mLBConsole = new LBConsole(MyConsole, 100);

            mInteractionOut.println = MyConsole.PrintLn;
            mInteractionOut.print = MyConsole.Print;
            mInteractionOut.printf = MyConsole.Printf;
            mInteractionOut.clear = MyConsole.Clear;

            DebugOut.StdPrint = MyConsole.Print;
            DebugOut.StdPrintLn = MyConsole.PrintLn;
            DebugOut.StdPrintf = MyConsole.Printf;

            ViewModel = new PlotterViewModel(this, viewContainer);

            viewContainer.Focusable = true;

            LayerListView.DataContext = ViewModel.LayerList;

            ViewModel.LayerListView = LayerListView;
            ViewModel.SetObjectTreeView(ObjTree);


            PreviewKeyDown += OnPreviewKeyDown;

            KeyDown += onKeyDown;
            KeyUp += onKeyUp;

            SlsectModePanel.DataContext = ViewModel;
            FigurePanel.DataContext = ViewModel;

            InitTextCommand();

            textBlockXYZ.DataContext = ViewModel.FreqChangedInfo;
            textBlockXYZ2.DataContext = ViewModel.FreqChangedInfo;


            ViewModel.InteractOut = mInteractionOut;

            AddLayerButton.Click += ViewModel.ButtonClicked;
            RemoveLayerButton.Click += ViewModel.ButtonClicked;
            RunTextCommandButton.Click += RunTextCommandButtonClicked;

            ViewModePanel.DataContext = ViewModel;

            SnapMenu.DataContext = ViewModel;

            ToolBar1.DataContext = ViewModel;

            MyConsole.SelectionChanged += MyConsole_SelectionChanged;

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;

            MyConsole.KeyUp += ListMessage_KeyUp;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
        }

        private void ListMessage_KeyUp(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C)
            {
                string copyString = MyConsole.GetStringAll();

                Clipboard.SetDataObject(copyString, true);
                KeyHandled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.X)
            {
                MyConsole.Clear();
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            ViewModel.SaveSettings();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.LoadSettings();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MenuItemClicked(sender, e);
        }

        private void DebugCommand(String s)
        {
            ViewModel.DebugCommand(s);
        }

        private void MyConsole_SelectionChanged(object sender, EventArgs e)
        {
            List<string> lines = MyConsole.GetSelectedStrings();
            ViewModel.MessageSelected(lines);
        }

        #region TextCommand
        private void InitTextCommand()
        {
            ViewModel.SetupTextCommandView(textCommand);

            textCommand.Determine += TextCommand_Determine;
        }

        public void RunTextCommandButtonClicked(object sender, RoutedEventArgs e)
        {
            var s = textCommand.Text;
            if (s.Length > 0)
            {
                ViewModel.TextCommand(s);
                textCommand.History.Add(s);
            }
        }

        private void TextCommand_Determine(object sender, AutoCompleteTextBox.TextEventArgs e)
        {
            var s = e.Text;

            textCommand.Text = "";

            if (s.Length > 0)
            {
                ViewModel.TextCommand(s);
                textCommand.History.Add(s);
                viewContainer.Focus();
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
