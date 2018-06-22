using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Plotter.Controller;
using System;
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
using System.Xml;
using ICSharpCode.AvalonEdit.Search;

namespace KCad
{
    /// <summary>
    /// EditorWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class EditorWindow : Window
    {
        ScriptEnvironment ScriptEnv;

        SearchPanel mSearchPanel;

        public EditorWindow(ScriptEnvironment scriptEnvironment)
        {
            InitializeComponent();

            ScriptEnv = scriptEnvironment;

            using (var reader = new XmlTextReader("Resources\\Python-Mode.xshd"))
            {
                textEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }

            mSearchPanel = SearchPanel.Install(textEditor);

            BtnRun.Click += BtnRun_Click;
        }

        private void BtnRun_Click(object sender, RoutedEventArgs e)
        {
            string s = textEditor.Text;

            BtnRun.IsEnabled = false;

            ScriptEnv.runScriptAsync(s, onScriptEnd);
        }

        private void onScriptEnd()
        {
            BtnRun.IsEnabled = true;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement))
            {
                return;
            }

            FrameworkElement element = (FrameworkElement)sender;

            if (element.Tag.ToString() == "load_script")
            {
                System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
                ofd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                ofd.Filter = "Python files|*.py";
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    textEditor.Load(ofd.FileName);
                }
            }
            else if (element.Tag.ToString() == "save_script")
            {
                System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
                sfd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                sfd.Filter = "Python files|*.py";

                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    textEditor.Save(sfd.FileName);
                }
            }
            else if (element.Tag.ToString() == "search_text")
            {
                mSearchPanel.Open();
            }
        }
    }
}
