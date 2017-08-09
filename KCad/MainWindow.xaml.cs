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

        private DebugInputThread InputThread;

        //private ObservableCollection<string> messageList = new ObservableCollection<string>();

        private PlotterController.Interaction mInteractionOut = new PlotterController.Interaction();

        private bool KeyHandled = false;

        public MainWindow()
        {
            InitializeComponent();

            if (App.GetCurrent().InputThread != null)
            {
                App.GetCurrent().InputThread.OnLineArrived = DebugCommand;
            }

            ViewModel = new PlotterViewModel(this, viewContainer);

            viewContainer.Focusable = true;

            LayerListView.DataContext = ViewModel.LayerList;

            ViewModel.LayerListView = LayerListView;

            PreviewKeyDown += ViewModel.perviewKeyDown;

            KeyDown += onKeyDown;
            KeyUp += onKeyUp;

            SlsectModePanel.DataContext = ViewModel;
            FigurePanel.DataContext = ViewModel;

            InitTextCommand();

            textBlockXYZ.DataContext = ViewModel.FreqChangedInfo;
            textBlockXYZ2.DataContext = ViewModel.FreqChangedInfo;

            mInteractionOut.println = MessageOut;

            ViewModel.InteractOut = mInteractionOut;

            AddLayerButton.Click += ViewModel.ButtonClicked;
            RemoveLayerButton.Click += ViewModel.ButtonClicked;
            RunTextCommandButton.Click += RunTextCommandButtonClicked;

            ViewModePanel.DataContext = ViewModel;

            SnapMenu.DataContext = ViewModel;

            listMessage.SelectionChanged += ListMessage_SelectionChanged;

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;

            listMessage.KeyUp += ListMessage_KeyUp;
        }

        private void ListMessage_KeyUp(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C)
            {
                string copyString = "";

                foreach (MessageLine line in this.listMessage.Items)
                {
                    copyString += line.Content + "\n";
                }

                Clipboard.SetDataObject(copyString, true);
                KeyHandled = true;
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


        private void ListMessage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<string> lines = new List<string>();


            for (int i = 0; i < listMessage.SelectedItems.Count; i++)
            {
                MessageLine line = (MessageLine)listMessage.SelectedItems[i];
                lines.Add(line.Content.ToString());
            }

            ViewModel.MessageSelected(lines);
        }


        #region Message出力
        public class MessageLine : ListBoxItem
        {
            public MessageLine(string s)
            {
                Content = s;
                // 行ごとに色を変えるならこれ
                //Content = new Run(s) { Foreground = Brushes.Cyan };

                // 複数個所に色をつけるなら以下で行ける
                /*
                TextBlock tb = new TextBlock();
                tb.Inlines.Add(new Run(s) { Foreground = Brushes.Cyan });
                tb.Inlines.Add(new Run(" test") { Foreground = Brushes.Green});
                Content = tb;
                */
            }
        }

        private void MessageOut(string s)
        {
            if (listMessage.Items.Count > 30)
            {
                listMessage.Items.RemoveAt(0);
            }

            var line = new MessageLine(s);
            listMessage.Items.Add(line);

            Object obj = listMessage.Items[listMessage.Items.Count - 1];

            listMessage.ScrollIntoView(obj);
        }
        #endregion

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


            if (!textCommand.IsFocused && !listMessage.IsFocused)
            {
               ViewModel.OnKeyUp(sender, e);
            }
        }
        #endregion
    }
}
