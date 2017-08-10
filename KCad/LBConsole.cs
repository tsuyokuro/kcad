using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace KCad
{
    public class LBConsole
    {
        private int mMaxLine = 30;

        public int MaxLine
        {
            set
            {
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
            MaxLine = maxLine;
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

        public void PrintLn(string s)
        {
            Print(s);

            while (mListBox.Items.Count > MaxLine)
            {
                mListBox.Items.RemoveAt(0);
            }

            var line = new MessageLine();
            mListBox.Items.Add(line);

            PrintScrollToLast();
        }

        public void Print(string s)
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

            PrintScrollToLast();
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

        public void PrintScrollToLast()
        {
            Object obj = mListBox.Items[mListBox.Items.Count - 1];
            mListBox.ScrollIntoView(obj);
        }
    }
}
