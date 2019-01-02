using Plotter;
using Plotter.Controller;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace KCad
{
    public partial class MainWindow : Window
    {
        public PlotterViewModel ViewModel;

        private bool KeyHandled = false;

        public MainWindow()
        {
            InitializeComponent();

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


            BtnCloseWindow.Click += (sender, e) => { Close(); };
            BtnMinWindow.Click += (sender, e) => { this.WindowState = WindowState.Minimized; };
            BtnMaxWindow.Click += (sender, e) => { this.WindowState = WindowState.Maximized; };
            BtnRestWindow.Click += (sender, e) => { this.WindowState = WindowState.Normal; };

            StateChanged += MainWindow_StateChanged;

            PopupMessage.CustomPopupPlacementCallback = placeMessagePopup;
        }

        public CustomPopupPlacement[] placeMessagePopup(Size popupSize,
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
            ImageSource di = null;

            switch (type)
            {
                case PlotterObserver.MessageType.INFO:
                    di = (ImageSource)TryFindResource("infoIconDrawingImage");
                    break;
                case PlotterObserver.MessageType.INPUT:
                    di = (ImageSource)TryFindResource("inputIconDrawingImage");
                    break;
                case PlotterObserver.MessageType.ERROR:
                    di = (ImageSource)TryFindResource("errorIconDrawingImage");
                    break;
            }

            if (di == null)
            {
                di = (ImageSource)TryFindResource("infoIconDrawingImage");
            }

            return di;
        }


        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            switch (WindowState)
            {
                case WindowState.Maximized:
                    Task.Run(() =>
                    {
                        Thread.Sleep(50);
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
