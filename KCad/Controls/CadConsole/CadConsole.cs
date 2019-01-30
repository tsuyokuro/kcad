using Plotter;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace KCad
{
    public partial class CadConsoleView : FrameworkElement
    {
        protected FontFamily mFontFamily = new FontFamily("ＭＳ ゴシック");

        protected Typeface mTypeface;

        protected Brush mForeground = Brushes.Black;

        protected Brush mBackground = Brushes.White;

        protected Brush mSelectedForeground = Brushes.White;

        protected Brush mSelectedBackground = new SolidColorBrush(Color.FromRgb(0x22, 0x8B, 0x22));


        protected double mLineHeight = 14.0;

        protected double mTextSize = 10.0;

        protected double mIndentSize = 8.0;

        protected int mMaxLine = 200;

        protected int mTopIndex = 0;

        protected double mTextLeftMargin = 4.0;

        protected bool mIsLoaded = false;


        #region Properties
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

        public double LineHeight
        {
            get
            {
                return mLineHeight;
            }

            set
            {
                mLineHeight = value;
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


        public event EventHandler SelectionChanged;

        protected virtual void OnSelectionChanged(EventArgs e)
        {
            SelectionChanged?.Invoke(this, e);
        }


        protected ScrollViewer Scroll;

        protected List<TextLine> mList = new List<TextLine>();

        protected AnsiEsc Esc = new AnsiEsc();

        protected TextAttr DefaultAttr = new TextAttr();

        protected TextAttr CurrentAttr = default;

        protected Pen FocusedBorderPen = new Pen(
                new SolidColorBrush(Color.FromRgb(0x56, 0x9D, 0xE5)), 1);

        protected double CW = 1;

        protected double CH = 1;


        public CadConsoleView()
        {
            Focusable = true;

            mTypeface = new Typeface(mFontFamily, FontStyles.Normal, FontWeights.UltraLight, FontStretches.Normal);

            Loaded += CadConsoleView_Loaded;

            GotFocus += CadConsoleView_GotFocus;
            LostFocus += CadConsoleView_LostFocus;

            KeyUp += CadConsoleView_KeyUp;

            SizeChanged += CadConsoleView_SizeChanged;
        }

        private void CadConsoleView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SizeChanged -= CadConsoleView_SizeChanged;
            RecalcSize();
            SizeChanged += CadConsoleView_SizeChanged;
        }

        private void CadConsoleView_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && (e.Key == Key.C || e.Key == Key.Insert))
            {
                string copyString = GetSelectedString();

                System.Windows.Clipboard.SetDataObject(copyString, true);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.X)
            {
                Clear();
            }
        }

        private void CadConsoleView_LostFocus(object sender, RoutedEventArgs e)
        {
            CleanSelection();
            UpdateView();
        }

        private void CadConsoleView_GotFocus(object sender, RoutedEventArgs e)
        {
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

            Esc.Palette[Esc.DefaultFColor] = mForeground;
            Esc.Palette[Esc.DefaultBColor] = mBackground;

            DefaultAttr.FColor = Esc.DefaultFColor;
            DefaultAttr.BColor = Esc.DefaultBColor;

            CurrentAttr = DefaultAttr;

            FormattedText ft = GetFormattedText("A", mForeground);

            CW = ft.Width;
            CH = ft.Height;

            NewLine();

            UpdateView();
        }

        //
        // MouseDown += handler ではなく、OnMouseDownをoverrideするように
        // しないと、Focus()を呼んでもすぐにLostFocusしてしまう
        // 上手くEventを処理済みに出来ないのかもしれない
        //
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this);

            int idx = (int)(p.Y / mLineHeight);

            int col = (int)(p.X / CW);

            //DOut.pl($"line:{idx} col:{col}");

            CleanSelection();

            if (idx >= mList.Count)
            {
                UpdateView();
                return;
            }

            TextLine item = mList[idx];

            if (item == null)
            {
                UpdateView();
                return;
            }

            item.IsSelected = true;

            UpdateView();

            OnSelectionChanged(EventArgs.Empty);

            if (Focus())
            {
                e.Handled = true;
            }

            base.OnMouseDown(e);
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
            Height = mLineHeight * (double)(mList.Count);

            if (Scroll != null)
            {
                if (Height < Scroll.ActualHeight)
                {
                    Height = Scroll.ActualHeight;
                }
            }
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

            CurrentAttr = DefaultAttr;

            var line = new TextLine(DefaultAttr);
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

            TextLine line;

            line = mList[idx];

            line.Parse(s);
        }

        public void Printf(string format, params object[] args)
        {
            string s = String.Format(format, args);
            Print(s);
        }

        public void Clear()
        {
            mList.Clear();
            //RecalcSize();
            NewLine();
            UpdateView();
        }

        public List<string> GetSelectedStrings()
        {
            List<string> lines = new List<string>();

            for (int i = 0; i < mList.Count; i++)
            {
                TextLine line = mList[i];
                if (line.IsSelected)
                {
                    lines.Add(line.Data);
                }
            }

            return lines;
        }

        public string GetSelectedString()
        {
            string s = "";

            foreach (TextLine line in mList)
            {
                if (line.IsSelected)
                {
                    s += line.Data + "\n";
                }
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

            long topNumber = (long)offset / (long)mLineHeight;

            double textOffset = (mLineHeight - mTextSize) / 2.0 - 3;

            p.X = 0;
            p.Y = mLineHeight * topNumber;

            rect.X = 0;
            rect.Y = p.Y;
            rect.Width = ActualWidth;
            rect.Height = mLineHeight + 1;

            int n = (int)topNumber;

            double rangeY = offset + dispHeight;

            while (p.Y < rangeY)
            {
                if (n >= mList.Count)
                {
                    break;
                }

                TextLine item = mList[n];
                n++;

                rect.Y = p.Y;

                if (item.IsSelected)
                {
                    dc.DrawRectangle(mSelectedBackground, null, rect);
                }
                else
                {
                    dc.DrawRectangle(mBackground, null, rect);
                }

                tp = p;

                tp.X = mTextLeftMargin;
                tp.Y += textOffset;

                DrawText(dc, item, tp);

                p.Y += mLineHeight;
            }

            if (p.Y < rangeY)
            {
                Rect sr = new Rect(0, p.Y, ActualWidth, rangeY - p.Y);
                dc.DrawRectangle(mBackground, null, sr);
            }

            if (IsFocused)
            {
                Rect sr = new Rect(0, offset, ActualWidth, dispHeight);
                dc.DrawRectangle(null, FocusedBorderPen, sr);
            }
        }

        #region draw text

        protected void DrawText(DrawingContext dc, TextLine line, Point pt)
        {
            int sp = 0;

            foreach (AttrSpan attr in line.Attrs)
            {
                string s = line.Data.Substring(sp, attr.Len);
                pt = RenderText(dc, attr.Attr, s, pt, line.IsSelected);
                sp += attr.Len;
            }
        }

        protected Point RenderText(DrawingContext dc, TextAttr attr, string s, Point pt, bool selected)
        {
            Brush foreground;

            if (selected)
            {
                foreground = Esc.Palette[Esc.DefaultFColor];
            }
            else
            {
                foreground = Esc.Palette[attr.FColor];
            }

            FormattedText ft = GetFormattedText(s, foreground);

            Point pt2 = pt;
            pt2.X += ft.WidthIncludingTrailingWhitespace; // 末尾のspaceも含む幅
            pt2.Y += mLineHeight;

            Brush background;

            if (selected)
            {
                background = mSelectedBackground;
            }
            else
            {
                background = Esc.Palette[attr.BColor];
            }

            dc.DrawRectangle(background, null, new Rect(pt, pt2));

            Point tpt = pt;

            tpt.Y = pt.Y + ((pt2.Y - pt.Y) - ft.Height) / 2;

            dc.DrawText(ft, tpt);
            pt.X += ft.Width;
            return pt;
        }
        #endregion

        protected FormattedText GetFormattedText(string s, Brush brush)
        {
            FormattedText formattedText = new FormattedText(s,
                                                      System.Globalization.CultureInfo.CurrentCulture,
                                                      System.Windows.FlowDirection.LeftToRight,
                                                      mTypeface,
                                                      mTextSize,
                                                      brush,
                                                      VisualTreeHelper.GetDpi(this).PixelsPerDip);
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

            int dispCnt = (int)(dispHeight / mLineHeight);

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
