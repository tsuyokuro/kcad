﻿using Plotter;
using Plotter.Controller;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace KCad
{
    public partial class MainWindow : Window
    {
        public PlotterViewModel ViewModel;

        private bool KeyHandled = false;

        private ImageSource[] PopupMessageIcons = new ImageSource[3];

        public MainWindow()
        {
            InitializeComponent();

            SetupDebugConsole();
            SetupInteractionConsole();

            ViewModel = new PlotterViewModel(this);

            viewContainer.Focusable = true;

            ViewModel.LayerListView = LayerListView;
            ViewModel.ObjectTreeView = ObjTree;

            ViewModel.SetupTextCommandView(textCommand);
            textCommand.Determine += TextCommand_OnDetermine;

            KeyDown += onKeyDown;
            KeyUp += onKeyUp;

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;

            RunTextCommandButton.Click += RunTextCommandButtonClicked;

            SetupDataContext();

            InitWindowChrome();

            InitPopup();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DOut.pl("sc");
        }

        private void SetupDebugConsole()
        {
            if (App.UseConsole)
            {
                // DOutの出力はデフォルトでConsoleになっているので、UseConsoleの場合は、
                // あらためて設定する必要はない
                DOut.pl("DOut's output setting is Console");
            }
            else
            {
                DOut.PrintFunc = MyConsole.Print;
                DOut.PrintLnFunc = MyConsole.PrintLn;
                DOut.FormatPrintFunc = MyConsole.Printf;
            }
        }

        private void SetupInteractionConsole()
        {
            ItConsole.PrintFunc = MyConsole.Print;
            ItConsole.PrintLnFunc = MyConsole.PrintLn;
            ItConsole.FormatPrintFunc = MyConsole.Printf;
            ItConsole.clear = MyConsole.Clear;
        }

        private void SetupDataContext()
        {
            LayerListView.DataContext = ViewModel.LayerList;

            SlsectModePanel.DataContext = ViewModel;
            FigurePanel.DataContext = ViewModel;

            textBlockXYZ.DataContext = ViewModel.CursorPosVM;
            textBlockXYZ2.DataContext = ViewModel.CursorPosVM;

            ViewModePanel.DataContext = ViewModel;

            SnapMenu.DataContext = ViewModel.Settings;

            DrawOptionMenu.DataContext = ViewModel.Settings;

            ToolBar1.DataContext = ViewModel.Settings;

            TreeViewToolBar.DataContext = ViewModel.Settings;
        }

        private void InitWindowChrome()
        {
            BtnCloseWindow.Click += (sender, e) => { Close(); };
            BtnMinWindow.Click += (sender, e) => { this.WindowState = WindowState.Minimized; };
            BtnMaxWindow.Click += (sender, e) => { this.WindowState = WindowState.Maximized; };
            BtnRestWindow.Click += (sender, e) => { this.WindowState = WindowState.Normal; };

            StateChanged += MainWindow_StateChanged;
        }

        private void InitPopup()
        {
            InitPopupMessageIcons();
            PopupMessage.CustomPopupPlacementCallback = PlaceMessagePopup;
        }

        private void InitPopupMessageIcons()
        {
            PopupMessageIcons[(int)PlotterObserver.MessageType.INFO] =
                (ImageSource)TryFindResource("infoIconDrawingImage");

            PopupMessageIcons[(int)PlotterObserver.MessageType.INPUT] =
                (ImageSource)TryFindResource("inputIconDrawingImage");

            PopupMessageIcons[(int)PlotterObserver.MessageType.ERROR] =
                (ImageSource)TryFindResource("errorIconDrawingImage");
        }

        public CustomPopupPlacement[] PlaceMessagePopup(Size popupSize,
                                           Size targetSize,
                                           Point offset)
        {
            Point p = new Point(targetSize.Width - popupSize.Width - 8, 8);

            CustomPopupPlacement placement1 =
                new CustomPopupPlacement(p, PopupPrimaryAxis.Horizontal);

            CustomPopupPlacement[] ttplaces =
                    new CustomPopupPlacement[] { placement1 };
            return ttplaces;
        }

        public void OpenPopupMessage(string text, PlotterObserver.MessageType messageType)
        {
            Application.Current.Dispatcher.Invoke(() => {
                PopupMessageIcon.Source = SelectPopupMessageIcon(messageType);

                PopupMessageText.Text = text;
                PopupMessage.IsOpen = true;
            }); 
        }

        public void ClosePopupMessage()
        {
            if (Application.Current.Dispatcher.Thread.ManagedThreadId ==
                System.Threading.Thread.CurrentThread.ManagedThreadId)
            {
                PopupMessage.IsOpen = false;
                return;
            }

            Application.Current.Dispatcher.Invoke(() => {
                PopupMessage.IsOpen = false;
            });
        }

        ImageSource SelectPopupMessageIcon(PlotterObserver.MessageType type)
        {
            return PopupMessageIcons[(int)type];
        }


        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            switch (WindowState)
            {
                case WindowState.Maximized:
                    Task.Run(() =>
                    {
                        Thread.Sleep(10);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            LayoutRoot.Margin = new Thickness(9);
                        });
                    });

                    break;
                default:
                    LayoutRoot.Margin = new Thickness(0);
                    break;
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            ViewModel.Close();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var hsrc = HwndSource.FromVisual(this) as HwndSource;
            hsrc.AddHook(WndProc);


            ViewModel.Open();

            System.Drawing.Color c = ViewModel.CurrentDC.Tools.BrushColor(DrawTools.BRUSH_BACKGROUND);
            viewRoot.Background = new SolidColorBrush(Color.FromRgb(c.R, c.G, c.B));
        }

        private void Command_Clicked(object sender, RoutedEventArgs e)
        {
            Control control = (Control)sender;

            String command = control.Tag.ToString();

            ViewModel.ExecCommand(command);
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

        private void TextCommand_OnDetermine(object sender, AutoCompleteTextBox.TextEventArgs e)
        {
            var s = e.Text;

            textCommand.Text = "";

            if (s.Length > 0)
            {
                ViewModel.TextCommand(s);
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

        public void SetMainView(IPlotterView view)
        {
            viewContainer.Child = view.FormsControl;
        }

        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WinAPI.WM_ENTERSIZEMOVE:
                    {
                        MainWindow wnd = (MainWindow)Application.Current.MainWindow;
                        wnd.viewContainer.Visibility = Visibility.Hidden;
                    }
                    break;
                case WinAPI.WM_EXITSIZEMOVE:
                    {
                        MainWindow wnd = (MainWindow)Application.Current.MainWindow;
                        wnd.viewContainer.Visibility = Visibility.Visible;

                        ViewModel.Redraw();
                    }
                    break;
            }
            return IntPtr.Zero;
        }
    }
}
