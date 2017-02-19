using Plotter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;

namespace KCad
{
    public partial class MainWindow : Window
    {
        public PlotterViewModel ViewModel;

        private DebugInputThread InputThread;

        private ObservableCollection<string> messageList = new ObservableCollection<string>();


        public MainWindow()
        {
            InitializeComponent();

            InputThread = new DebugInputThread(debugCommand);
            InputThread.start();

            ViewModel = new PlotterViewModel(viewContainer);

            viewContainer.Focusable = true;

            LayerListView.DataContext = ViewModel.LayerList;

            ViewModel.LayerListView = LayerListView;

            PreviewKeyDown += ViewModel.perviewKeyDown;

            KeyDown += onKeyDown;
            KeyUp += onKeyUp;

            SlsectModePanel.DataContext = ViewModel;
            FigurePanel.DataContext = ViewModel;

            textCommand.KeyDown += textCommand_KeyDown;
            textCommand.KeyUp += textCommand_KeyUp;

            textBlockXYZ.DataContext = ViewModel.FreqChangedInfo;


            ViewModel.InteractOut.print = MessageOut;

            AddLayerButton.Click += ViewModel.ButtonClicked;
            RemoveLayerButton.Click += ViewModel.ButtonClicked;

            ViewModePanel.DataContext = ViewModel;
        }

        private void textCommand_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void textCommand_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var s = textCommand.Text;
                if (s.Length > 0)
                {
                    ViewModel.textCommand(s);
                }

                viewContainer.Focus();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuitem = (MenuItem)sender;
            var tag = menuitem.Tag.ToString();
            ViewModel.menuCommand(tag);
        }

        private void debugCommand(String s)
        {
            ViewModel.debugCommand(s);
        }


        // In the case of the String class Eqauls() returns true
        // even for different instances when the contents are the same.
        // Therefore, ListBox.ScrollIntoView() does not work properly.
        // So, we wrap string for suppressing this behavior.
        public class MessageLine
        {
            private string Line;
            public MessageLine(string s)
            {
                Line = s;
            }

            override public String ToString()
            {
                return Line;
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

        public void ButtonClicked(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            ViewModel.ButtonClicked(sender, e);
        }

        #region "Key handling"
        private void onKeyDown(object sender, KeyEventArgs e)
        {
            if (!textCommand.IsFocused)
            {
                ViewModel.onKeyDown(sender, e);
            }
        }

        private void onKeyUp(object sender, KeyEventArgs e)
        {
            if (!textCommand.IsFocused)
            {
                ViewModel.onKeyUp(sender, e);
            }
        }
        #endregion
    }
}
