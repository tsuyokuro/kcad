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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace KCad
{
    class CadConsoleView : FrameworkElement
    {
        public class ListItem
        {
            public bool IsSelected = false;
            public string Data = "";
        }

        #region Properties
        protected FontFamily mFontFamily;

        protected Typeface mTypeface;

        protected Brush mForeground = Brushes.Black;

        protected Brush mBackground = Brushes.White;

        protected Brush mSelectedForeground = Brushes.White;

        protected Brush mSelectedBackground = new SolidColorBrush(Color.FromRgb(0x22, 0x8B, 0x22));


        protected double mItemHeight = 14.0;

        protected double mTextSize = 10.0;

        protected double mIndentSize = 8.0;

        protected int mMaxLine = 200;

        protected int mTopIndex = 0;

        protected double mTextLeftMargin = 4.0;

        protected bool mIsLoaded = false;

        public Brush Background
        {
            get
            {
                return mBackground;
            }
            set
            {
                mBackground = value;
            }
        }

        public Brush Foreground
        {
            get
            {
                return mForeground;
            }
            set
            {
                mForeground = value;
            }
        }

        public Brush SelectedBackground
        {
            get
            {
                return mSelectedBackground;
            }
            set
            {
                mSelectedBackground = value;
            }
        }

        public Brush SelectedForeground
        {
            get
            {
                return mSelectedForeground;
            }
            set
            {
                mSelectedForeground = value;
            }
        }

        public double TextSize
        {
            get
            {
                return mTextSize;
            }

            set
            {
                mTextSize = value;
            }
        }

        public double TextLeftMargin
        {
            get
            {
                return mTextLeftMargin;
            }
            set
            {
                mTextLeftMargin = value;
                UpdateView();
            }
        }

        public double ItemHeight
        {
            get
            {
                return mItemHeight;
            }

            set
            {
                mItemHeight = value;
            }
        }

        public int MaxLine
        {
            set
            {
                mMaxLine = value;

                if (mList.Count > mMaxLine)
                {
                    mList.RemoveRange(0, mList.Count - mMaxLine);
                }
            }

            get
            {
                return mMaxLine;
            }
        }

        #endregion

        #region Event
        public event EventHandler SelectionChanged;

        protected virtual void OnSelectionChanged(EventArgs e)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, e);
            }
        }
        #endregion

        protected ScrollViewer Scroll;

        protected List<ListItem> mList = new List<ListItem>();

        public CadConsoleView()
        {
            mFontFamily = new FontFamily("メイリオ");
            mTypeface = new Typeface(mFontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

            Loaded += CadConsoleView_Loaded;

            MouseDown += CadConsoleView_MouseDown;
        }

        private void CadConsoleView_Loaded(object sender, RoutedEventArgs e)
        {
            mIsLoaded = true;

            FrameworkElement parent = (FrameworkElement)Parent;

            if (parent is ScrollViewer)
            {
                Scroll = (ScrollViewer)parent;
            }

            if (Scroll != null)
            {
                Scroll.ScrollChanged += Scroll_ScrollChanged;
            }

            RecalcSize();
        }

        private void CadConsoleView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this);

            int idx = (int)(p.Y / mItemHeight);

            if (idx >= mList.Count)
            {
                return;
            }

            ListItem item = mList[idx];

            if (item == null)
            {
                return;
            }

            CleanSelection();

            item.IsSelected = true;

            UpdateView();

            OnSelectionChanged(EventArgs.Empty);
        }

        private void CleanSelection()
        {
            int i = 0;
            for (; i < mList.Count; i++)
            {
                mList[i].IsSelected = false;
            }
        }

        private void Scroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            InvalidateVisual();
        }

        private void RecalcSize()
        {
            Height = mItemHeight * (double)(mList.Count);
        }


        public void PrintLn(string s)
        {
            if (Dispatcher.CheckAccess())
            {
                Print(s);
                NewLine();
            }
            else
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    Print(s);
                    NewLine();
                }));
            }
        }

        public void Print(string s)
        {
            if (Dispatcher.CheckAccess())
            {
                PrintString(s);
            }
            else
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    PrintString(s);
                }));
            }
        }

        private void PrintString(string s)
        {
            string[] lines = s.Split('\n');

            int i = 0;
            for (; i < lines.Length - 1; i++)
            {
                AppendString(lines[i]);
                NewLine();
            }

            AppendString(lines[i]);
            UpdateView();
        }

        private void NewLine()
        {
            int prevCnt = mList.Count;

            var line = new ListItem();
            mList.Add(line);

            while (mList.Count > mMaxLine)
            {
                mList.RemoveAt(0);
            }

            if (prevCnt != mList.Count)
            {
                RecalcSize();
            }

            ScrollToEnd();
        }

        private void AppendString(string s)
        {
            int idx = mList.Count - 1;

            ListItem line;

            if (idx < 0)
            {
                line = new ListItem();
                mList.Add(line);
            }
            else
            {
                line = mList[idx];
            }

            line.Data += s;
        }

        public void Printf(string format, params object[] args)
        {
            string s = String.Format(format, args);
            Print(s);
        }

        public void Clear()
        {
            mList.Clear();
            RecalcSize();
            UpdateView();
        }

        public List<string> GetSelectedStrings()
        {
            List<string> lines = new List<string>();

            for (int i = 0; i < mList.Count; i++)
            {
                ListItem line = mList[i];
                if (line.IsSelected)
                {
                    lines.Add(line.Data);
                }
            }

            return lines;
        }

        public string GetStringAll()
        {
            string s = "";

            foreach (ListItem line in mList)
            {
                s += line.Data + "\n";
            }

            return s;
        }

        protected override void OnRender(DrawingContext dc)
        {
            double offset = 0;

            double dispHeight = ActualHeight;

            if (Scroll != null)
            {
                offset = Scroll.VerticalOffset;
                dispHeight = Scroll.ActualHeight;
            }

            Point p = default(Point);
            Point tp = default(Point);
            Rect rect = default(Rect);

            long topNumber = (long)offset / (long)mItemHeight;

            double textOffset = (mItemHeight - mTextSize) / 2.0 - 3;

            p.X = 0;
            p.Y = mItemHeight * topNumber;

            rect.X = 0;
            rect.Y = p.Y;
            rect.Width = ActualWidth;
            rect.Height = mItemHeight + 1;

            int n = (int)topNumber;

            double rangeY = offset + dispHeight;

            while (p.Y < rangeY)
            {
                if (n >= mList.Count)
                {
                    break;
                }

                ListItem item = mList[n];
                n++;

                FormattedText ft;

                rect.Y = p.Y;

                if (item.IsSelected)
                {
                    ft = GetText(item.Data, mSelectedForeground);
                    dc.DrawRectangle(mSelectedBackground, null, rect);
                }
                else
                {
                    ft = GetText(item.Data, mForeground);
                    dc.DrawRectangle(mBackground, null, rect);
                }

                tp = p;

                tp.X = mTextLeftMargin;
                tp.Y += textOffset;

                dc.DrawText(ft, tp);

                p.Y += mItemHeight;
            }

            if (p.Y < rangeY)
            {
                Rect sr = new Rect(0, p.Y, ActualWidth, rangeY - p.Y);
                dc.DrawRectangle(mBackground, null, sr);
            }
        }

        protected FormattedText GetText(string s, Brush brush)
        {
            FormattedText formattedText = new FormattedText(s,
                                                      System.Globalization.CultureInfo.CurrentCulture,
                                                      System.Windows.FlowDirection.LeftToRight,
                                                      mTypeface,
                                                      mTextSize,
                                                      brush);
            return formattedText;
        }

        public void ScrollToEnd()
        {
            if (Scroll == null)
            {
                return;
            }

            Scroll.ScrollToEnd();
        }


        private int GetTopIndexForEnd()
        {
            double offset = 0;

            double dispHeight = ActualHeight;

            if (Scroll != null)
            {
                offset = Scroll.VerticalOffset;
                dispHeight = Scroll.ActualHeight;
            }

            int dispCnt = (int)(dispHeight / mItemHeight);

            int topIndex = mList.Count - dispCnt;

            if (topIndex < 0)
            {
                topIndex = 0;
            }

            return topIndex;
        }

        private void UpdateView()
        {
            if (mIsLoaded)
            {
                InvalidateVisual();
            }
        }
    }
}
