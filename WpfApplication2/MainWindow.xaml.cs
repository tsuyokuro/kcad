using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplication2
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            plotterView1.MouseMove += pic1_MouseMove;
            plotterView1.BackColor = System.Drawing.Color.AliceBlue;
        }

        private void pic1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Graphics g = plotterView1.CreateGraphics();
        }


        private void preview_key_down(object sender, KeyEventArgs e)
        {

        }
    }
}
