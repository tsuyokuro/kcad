using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Search;
using Plotter;
using Plotter.Controller;
using Plotter.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;

namespace KCad
{
    public partial class EditorWindow : Window
    {
        ScriptEnvironment ScriptEnv;

        SearchPanel mSearchPanel;

        private CompletionWindow mCompletionWindow;

        private string FileName = null;

        private bool Modified = false;

        private SolidColorBrush CornerBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x2d, 0x2d, 0x2d));

        private const string FileFilter = "Python files|*.py";

        private HashSet<int> BreakPoints;

        public EditorWindow(ScriptEnvironment scriptEnvironment)
        {
            InitializeComponent();

            ScriptEnv = scriptEnvironment;

            SetupHighlightForPython();

            mSearchPanel = SearchPanel.Install(textEditor);

            BtnRun.Click += BtnRun_Click;

            BtnStop.Click += BtnStop_Click;

            textEditor.TextArea.TextEntered += TextArea_TextEntered;

            textEditor.TextArea.TextEntering += TextArea_TextEntering;

            textEditor.TextChanged += TextEditor_TextChanged;

            textEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;

            mSearchPanel.MarkerBrush = Brushes.SteelBlue;

            PreviewKeyUp += EditorWindow_PreviewKeyUp;

            BreakPoints = new HashSet<int>();

            BreakPointMargin breakPointMargin = new BreakPointMargin(BreakPoints);

            textEditor.TextArea.LeftMargins.Insert(0, breakPointMargin);

            textEditor.Loaded += TextEditor_Loaded;

            ShowRowCol();
        }

        private void TextEditor_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollViewer scrollViewer = (ScrollViewer)textEditor.Template.FindName("PART_ScrollViewer", textEditor);

            Rectangle corner = ((Rectangle)scrollViewer.Template.FindName("Corner", scrollViewer));

            corner.Fill = CornerBrush;
        }

        private void SetupHighlightForPython()
        {
            using (var reader = new XmlTextReader("Resources\\Python-Mode.xshd"))
            {
                textEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }
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

            //DOut.pl(wd.StartPos.ToString() + " " + wd.Word);

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

                mCompletionWindow.Width = 512;
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

                if (Char.IsLetterOrDigit(c) || c == '-' || c == '_')
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

                if (Char.IsLetterOrDigit(c) || c == '-' || c == '_')
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

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            ScriptEnv.CancelScript();
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

        private bool IsVaridDir(string path)
        {
            if (path == null)
            {
                return false;
            }

            if (path.Length == 0)
            {
                return false;
            }

            return Directory.Exists(path);
        }

        public void LoadWithDialog()
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();

            if (IsVaridDir(SettingsHolder.Settings.LastScriptDir))
            {
                ofd.InitialDirectory = SettingsHolder.Settings.LastScriptDir;
            }
            else
            {
                ofd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            }

            ofd.Filter = FileFilter;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SettingsHolder.Settings.LastScriptDir = System.IO.Path.GetDirectoryName(ofd.FileName);

                textEditor.Load(ofd.FileName);
                FileName = ofd.FileName;
                UpdateTitle(false, true);
            }
        }

        public void SaveWithDialog()
        {
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();

            if (FileName != null && FileName.Length > 0)
            {
                sfd.InitialDirectory = System.IO.Path.GetDirectoryName(FileName);
            }
            else if (IsVaridDir(SettingsHolder.Settings.LastScriptDir))
            {
                sfd.InitialDirectory = SettingsHolder.Settings.LastScriptDir;
            }
            else
            {
                sfd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            }

            sfd.Filter = FileFilter;

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SettingsHolder.Settings.LastScriptDir = System.IO.Path.GetDirectoryName(sfd.FileName);

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

    public class BreakPointMargin : AbstractMargin
    {
        private const int margin = 20;

        private HashSet<int> BreakPoints;

        private TextArea textArea;

        public BreakPointMargin(HashSet<int> bps)
        {
            BreakPoints = bps;
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(margin, 0);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            TextView textView = this.TextView;
            Size renderSize = this.RenderSize;
            if (textView != null && textView.VisualLinesValid)
            {
                foreach (VisualLine line in textView.VisualLines)
                {
                    int lineNumber = line.FirstDocumentLine.LineNumber;
                    if (BreakPoints.Contains(lineNumber))
                    {
                        double y = line.GetTextLineVisualYPosition(line.TextLines[0], VisualYPosition.TextTop);
                        y -= textView.VerticalOffset;

                        double x = (renderSize.Width - 8) / 2;

                        y = (line.Height - 8) / 2 + y; 

                        drawingContext.DrawRectangle(Brushes.Red, null, new Rect(x, y, 8, 8));
                    }
                }
            }
        }

        protected override void OnTextViewChanged(TextView oldTextView, TextView newTextView)
        {
            if (oldTextView != null)
            {
                oldTextView.VisualLinesChanged -= TextViewVisualLinesChanged;
            }
            base.OnTextViewChanged(oldTextView, newTextView);
            if (newTextView != null)
            {
                newTextView.VisualLinesChanged += TextViewVisualLinesChanged;

                // find the text area belonging to the new text view
                textArea = newTextView.Services.GetService(typeof(TextArea)) as TextArea;
            }
            else
            {
                textArea = null;
            }
            InvalidateVisual();
        }

        void TextViewVisualLinesChanged(object sender, EventArgs e)
        {
            InvalidateVisual();
        }
    }
}

