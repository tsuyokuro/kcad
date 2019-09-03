using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Search;
using Plotter.Controller;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace KCad
{
    /// <summary>
    /// EditorWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class EditorWindow : Window
    {
        ScriptEnvironment ScriptEnv;

        SearchPanel mSearchPanel;

        private CompletionWindow mCompletionWindow;

        private string FileName = null;

        private bool Modified = false;

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

            textEditor.TextArea.TextEntered += TextArea_TextEntered;

            textEditor.TextArea.TextEntering += TextArea_TextEntering;

            textEditor.TextChanged += TextEditor_TextChanged;

            textEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;

            mSearchPanel.MarkerBrush = Brushes.SteelBlue;

            PreviewKeyUp += EditorWindow_PreviewKeyUp;

            ShowRowCol();
        }

        private void EditorWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
            {
                if (FileName != null)
                {
                    textEditor.Save(FileName);
                    UpdateTitle(false, true);
                }
                else
                {
                    SaveWithDialog();
                }
            }
        }

        private void Caret_PositionChanged(object sender, EventArgs e)
        {
            ShowRowCol();
        }

        private void ShowRowCol()
        {
            LblCaretPos.Content =
                textEditor.TextArea.Caret.Position.Line.ToString() +
                "," +
                textEditor.TextArea.Caret.Position.Column.ToString();
        }

        private void UpdateTitle(bool modified, bool force)
        {
            if (modified == Modified && !force)
            {
                return;
            }

            Modified = modified;

            string s = Modified ? "* " : "";

            if (FileName != null)
            {
                s += FileName;
            }
            else
            {
                s += "Script Editor";
            }

            this.Title = s;
        }

        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && mCompletionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    mCompletionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }

        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            int offset = textEditor.TextArea.Caret.Offset;

            WordData wd = getDocumentWord(offset - 1);

            //DebugOut.println(w.StartPos.ToString() + " " + w.Word);

            showCompletionWindow(wd);
        }

        private void TextEditor_TextChanged(object sender, EventArgs e)
        {
            UpdateTitle(true, false);
        }

        private void showCompletionWindow(WordData wd)
        {
            if (wd.Word.Length < 3)
            {
                return;
            }

            List<MyCompletionData> list = new List<MyCompletionData>();

            foreach (var str in ScriptEnv.AutoCompleteList)
            {
                string text = str as String;

                if (text.Contains(wd.Word))
                {
                    list.Add(new MyCompletionData(text, wd));
                }
            }

            if (list.Count > 0)
            {
                mCompletionWindow = new CompletionWindow(textEditor.TextArea);
                IList<ICompletionData> data = mCompletionWindow.CompletionList.CompletionData;

                foreach (MyCompletionData cd in list)
                {
                    data.Add(cd);
                }

                mCompletionWindow.Show();
                mCompletionWindow.Closed += delegate
                {
                    mCompletionWindow = null;
                };
            }
            else
            {
                if (mCompletionWindow != null)
                {
                    mCompletionWindow.Close();
                }
            }
        }

        private WordData getDocumentWord(int pos)
        {
            int p = pos;
            int sp = p;
            string s = "";

            while (p >= 0)
            {
                char c = textEditor.TextArea.Document.GetCharAt(p);

                if (Char.IsLetterOrDigit(c))
                {
                    p--;
                }
                else
                {
                    break;
                }
            }

            sp = p + 1;

            p = sp;

            while (p < textEditor.TextArea.Document.TextLength)
            {
                char c = textEditor.TextArea.Document.GetCharAt(p);

                if (Char.IsLetterOrDigit(c))
                {
                    s += c;
                    p++;
                }
                else
                {
                    break;
                }
            }

            WordData ret = default(WordData);

            ret.StartPos = sp;
            ret.Word = s;

            return ret;
        }

        private void BtnRun_Click(object sender, RoutedEventArgs e)
        {
            string s = textEditor.Text;

            BtnRun.IsEnabled = false;

            var callback = new ScriptEnvironment.RunCallback();

            callback.OnStart = () =>
            {
                BtnRun.IsEnabled = false;
            };

            callback.OnEnd = () =>
            {
                BtnRun.IsEnabled = true;
            };

            ScriptEnv.RunScriptAsync(s, callback);
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
                LoadWithDialog();
            }
            else if (element.Tag.ToString() == "save_script")
            {
                SaveWithDialog();
            }
            else if (element.Tag.ToString() == "search_text")
            {
                mSearchPanel.Open();
            }
            else if (element.Tag.ToString() == "copy_text")
            {
                textEditor.Copy();
            }
            else if (element.Tag.ToString() == "paste_text")
            {
                textEditor.Paste();
            }
        }

        public void LoadWithDialog()
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            ofd.Filter = "Python files|*.py";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textEditor.Load(ofd.FileName);
                FileName = ofd.FileName;
                UpdateTitle(false, true);
            }
        }

        public void SaveWithDialog()
        {
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            sfd.Filter = "Python files|*.py";

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textEditor.Save(sfd.FileName);
                FileName = sfd.FileName;
                UpdateTitle(false, true);
            }
        }
    }

    public struct WordData
    {
        public int StartPos;
        public string Word;
    }

    public class MyCompletionData : ICompletionData
    {
        //入力候補一覧に表示される内容
        public object Content
        {
            get
            {
                return Text;
            }
            set
            {
            }
        }

        public object Description { get; set; }

        // Item icon
        public ImageSource Image { get; set; }

        public double Priority { get; set; }

        public string Text { get; set; }

        public WordData mWordData;

        public MyCompletionData(string text, WordData wd)
        {
            Text = text;
            mWordData = wd;
        }

        //アイテム選択後の処理
        public void Complete(
            TextArea textArea,
            ISegment completionSegment,
            EventArgs insertionRequestEventArgs
            )
        {
            //textArea.Document.Replace(completionSegment, Text);

            textArea.Document.Replace(mWordData.StartPos, mWordData.Word.Length, Text);
        }
    }
}
