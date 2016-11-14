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

namespace WpfApplication2
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
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

            ViewModel = new PlotterViewModel(plotterView1);

            PreviewKeyDown += ViewModel.perviewKeyDown;

            SlsectModePanel.DataContext = ViewModel;
            FigurePanel.DataContext = ViewModel;

            textCommand.KeyDown += textCommand_KeyDown;

            ViewModel.InteractOut.print = MessageOut;
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

        private void MessageOut(string s)
        {
            listMessage.Items.Add(s);

            var peer = ItemsControlAutomationPeer.CreatePeerForElement(this.listMessage);
            var scrollProvider = peer.GetPattern(PatternInterface.Scroll) as IScrollProvider;
            scrollProvider.SetScrollPercent(scrollProvider.HorizontalScrollPercent, 100.0);
        }
    }
}
