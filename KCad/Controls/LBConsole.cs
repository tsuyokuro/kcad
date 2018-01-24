using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace KCad
{
    public class LBConsole
    {
        private int mMaxLine = 200;

        public int MaxLine
        {
            set
            {
                mMaxLine = value;

                while (mListBox.Items.Count > mMaxLine)
                {
                    mListBox.Items.RemoveAt(0);
                }
            }

            get
            {
                return mMaxLine;
            }
        }

        private ListBox mListBox;

        public LBConsole(ListBox listBox, int maxLine)
        {
            mListBox = listBox;
            mMaxLine = maxLine;
        }

        public class MessageLine : ListBoxItem
        {
            public MessageLine()
            {
                Content = "";
            }

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

            public void SetText(string s)
            {
                Content = s;
            }

            public string GetText()
            {
                return (string)Content;
            }
        }

        private object ExitFrames(object obj)
        {
            ((DispatcherFrame)obj).Continue = false;
            return null;
        }

        private void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            var callback = new DispatcherOperationCallback(ExitFrames);
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, callback, frame);
            Dispatcher.PushFrame(frame);
        }

        public void PrintLn(string s)
        {
            Print(s);

            NewLine();
        }

        public void Print(string s)
        {
            string[] lines = s.Split('\n');

            int i = 0;
            for (;i<lines.Length-1;i++)
            {
                AppendString(lines[i]);
                NewLine();
            }

            AppendString(lines[i]);
        }

        private void NewLine()
        {
            while (mListBox.Items.Count > mMaxLine)
            {
                mListBox.Items.RemoveAt(0);
            }

            var line = new MessageLine();
            mListBox.Items.Add(line);

            ScrollToLast();
            DoEvents();
        }

        private void AppendString(string s)
        {
            int idx = mListBox.Items.Count - 1;

            MessageLine line;

            if (idx < 0)
            {
                line = new MessageLine();
                mListBox.Items.Add(line);
            }
            else
            {
                line = (MessageLine)mListBox.Items[idx];
            }

            line.SetText(line.GetText() + s);
        }

        public void Printf(string format, params object[] args)
        {
            string s = String.Format(format, args);
            Print(s);
        }

        public List<string> GetSelectedStrings()
        {
            List<string> lines = new List<string>();

            for (int i = 0; i < mListBox.SelectedItems.Count; i++)
            {
                MessageLine line = (MessageLine)mListBox.SelectedItems[i];
                lines.Add(line.Content.ToString());
            }

            return lines;
        }

        public string GetStringAll()
        {
            string s = "";

            foreach (MessageLine line in mListBox.Items)
            {
                s += line.Content + "\n";
            }

            return s;
        }

        public void ScrollToLast()
        {
            Object obj = mListBox.Items[mListBox.Items.Count - 1];
            mListBox.ScrollIntoView(obj);
        }

        public void Clear()
        {
            mListBox.Items.Clear();
        }
    }
}
