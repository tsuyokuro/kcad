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

        private PlotterView plotterView1;

        private PlotterViewGL plotterViewGL1;

        public MainWindow()
        {
            InitializeComponent();

            InputThread = new DebugInputThread(debugCommand);
            InputThread.start();

            plotterView1 = new PlotterView();

            plotterViewGL1 = PlotterViewGL.Create();

            viewContainer.Child = plotterView1;

            ViewModel = new PlotterViewModel(plotterView1);

            LayerListView.DataContext = ViewModel.LayerList;

            ViewModel.LayerListView = LayerListView;

            PreviewKeyDown += ViewModel.perviewKeyDown;

            KeyDown += onKeyDown;
            KeyUp += onKeyUp;

            SlsectModePanel.DataContext = ViewModel;
            FigurePanel.DataContext = ViewModel;

            textCommand.KeyDown += textCommand_KeyDown;

            textBlockXYZ.DataContext = ViewModel.FreqChangedInfo;


            ViewModel.InteractOut.print = MessageOut;

            AddLayerButton.Click += ViewModel.ButtonClicked;
            RemoveLayerButton.Click += ViewModel.ButtonClicked;

            btnAxisXY.Click += ButtonClicked;
            btnAxisXZ.Click += ButtonClicked;
            btnAxisZY.Click += ButtonClicked;
            btnAxisXYZ.Click += ButtonClicked;
        }

        private void textCommand_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var s = textCommand.Text;
                if (s.Length > 0)
                {
                    ViewModel.textCommand(s);
                }

                plotterView1.Focus();
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
            switch (btn.Tag.ToString())
            {
                case "axis_xy":
                    viewContainer.Child = plotterView1;
                    ViewModel.ButtonClicked(sender, e);
                    break;

                case "axis_xz":
                    viewContainer.Child = plotterView1;
                    ViewModel.ButtonClicked(sender, e);
                    break;

                case "axis_zy":
                    viewContainer.Child = plotterView1;
                    ViewModel.ButtonClicked(sender, e);
                    break;

                case "axis_xyz":
                    viewContainer.Child = plotterViewGL1;
                    break;
            }

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
