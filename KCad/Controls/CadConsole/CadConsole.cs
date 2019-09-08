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

        protected Brush mSelectedBackground = Brushes.White;

        protected double mSelectedBackgroundOpacity = 0.2;

        protected double mLineHeight = 14.0;

        protected double mTextSize = 10.0;

        protected double mIndentSize = 8.0;

        protected int mMaxLine = 200;

        protected int mTopIndex = 0;

        protected double mTextLeftMargin = 4.0;

        protected bool mIsLoaded = false;

        public Action<string> Posting = s => { };

        #region Properties
        public Brush Background
        {
            get => mBackground;
            set => mBackground = value;
        }

        public Brush Foreground
        {
            get => mForeground;
            set => mForeground = value;
        }

        public Brush SelectedBackground
        {
            get => mSelectedBackground;
            set => mSelectedBackground = value;
        }

        public double SelectedBackgroundOpacity
        {
            get => mSelectedBackgroundOpacity;
            set => mSelectedBackgroundOpacity = value;
        }

        public double TextSize
        {
            get => mTextSize;
            set => mTextSize = value;
        }

        public double TextLeftMargin
        {
            get => mTextLeftMargin;
            set
            {
                mTextLeftMargin = value;
                UpdateView();
            }
        }

        public double LineHeight
        {
            get => mLineHeight;
            set => mLineHeight = value;
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

            get => mMaxLine;
        }

        public FontFamily FontFamily
        {
            get => mFontFamily;
            set => mFontFamily = value;
        }

        #endregion

        protected ScrollViewer Scroll;

        protected List<TextLine> mList = new List<TextLine>();

        protected AnsiEsc Esc = new AnsiEsc();

        protected TextAttr DefaultAttr = new TextAttr();

        protected TextAttr CurrentAttr = default;

        protected Pen FocusedBorderPen = new Pen(
                new SolidColorBrush(Color.FromArgb(0xff, 0x56, 0x9D, 0xE5)), 1.5);

        protected double CW = 1;

        protected double CH = 1;

        private TextRange RawSel = new TextRange();
        private TextRange Sel = new TextRange();
        private bool Selecting = false;

        public CadConsoleView()
        {
            Focusable = true;

            Loaded += CadConsoleView_Loaded;

            GotFocus += CadConsoleView_GotFocus;
            LostFocus += CadConsoleView_LostFocus;

            KeyUp += CadConsoleView_KeyUp;

            SizeChanged += CadConsoleView_SizeChanged;

            RawSel.Reset();
            Sel.Reset();
        }

        private void CadConsoleView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SizeChanged -= CadConsoleView_SizeChanged;
            RecalcSize();
            SizeChanged += CadConsoleView_SizeChanged;
        }

        private void CadConsoleView_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.C || e.Key == Key.Insert)
                {
                    string copyString = GetSelectedString();
                    Clipboard.SetDataObject(copyString, true);
                }
                else if (e.Key == Key.D)
                {
                    string postString = GetSelectedString();

                    if (postString != null && postString.Length > 0)
                    {
                        Posting(postString);
                    }
                }
                else if (e.Key == Key.X)
                {
                    Clear();
                }
            }
        }

        private void CadConsoleView_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateView();
        }

        private void CadConsoleView_GotFocus(object sender, RoutedEventArgs e)
        {
        }

        private void CadConsoleView_Loaded(object sender, RoutedEventArgs e)
        {
            mIsLoaded = true;

            mTypeface = new Typeface(mFontFamily, FontStyles.Normal, FontWeights.UltraLight, FontStretches.Normal);

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

            TextPos tp = PointToTextPos(p);

            Sel.Reset();

            RawSel.Start(tp.Row, tp.Col);
            Selecting = true;

            UpdateView();

            if (Focus())
            {
                e.Handled = true;
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            Selecting = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (Selecting)
            {
                Point p = e.GetPosition(this);

                TextPos tp = PointToTextPos(p);

                RawSel.End(tp.Row, tp.Col);

                Sel = TextRange.Naormalized(RawSel);

                InvalidateVisual();

                //DOut.pl($"sr:{Sel.SP.Row} sc:{Sel.SP.Col} - er:{Sel.EP.Row} ec:{Sel.EP.Col}");
            }
        }

        protected TextPos PointToTextPos(Point p)
        {
            TextPos tp = new TextPos();

            int row = (int)(p.Y / mLineHeight);
            int col = (int)((p.X - mTextLeftMargin) / CW);

            row = Math.Min(row, mList.Count - 1);

            if (row < 0)
            {
                row = 0;
            }

            tp.Row = row;
            tp.Col = col;

            return tp;
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
            if (Dispatcher.CheckAccess())
            {
                HandleClear();
            }
            else
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    HandleClear();
                }));
            }
        }

        private void HandleClear()
        {
            mList.Clear();
            //RecalcSize();
            NewLine();
            UpdateView();
        }

        public string GetSelectedString()
        {
            string s = "";

            if (!Sel.IsValid)
            {
                return s;
            }

            TextSpan tr;

            int i = Sel.SP.Row;

            int end = Sel.EP.Row;

            for (; i <= end; i++)
            {
                TextLine item = mList[i];
                int strLen = item.Data.Length;

                tr = Sel.GetRowSpan(i, strLen);

                if (tr.Len > 0)
                {
                    int len = Math.Min(strLen - tr.Start, tr.Len);
                    s += mList[i].Data.Substring(tr.Start, len);
                }

                if (i < end)
                {
                    s += "\n";
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
            Rect rect = default(Rect);

            long topNumber = (long)offset / (long)mLineHeight;

            double textOffset = (mLineHeight - mTextSize) / 2.0 - 3;

            p.X = 0;
            p.Y = mLineHeight * topNumber;

            Point tp;

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

                dc.DrawRectangle(mBackground, null, rect);

                tp = p;

                tp.X = mTextLeftMargin;
                tp.Y += textOffset;

                DrawText(dc, item, tp, n - 1);

                DrawSelectedRange(dc, n - 1);

                p.Y += mLineHeight;
            }

            if (p.Y < rangeY)
            {
                Rect sr = new Rect(0, p.Y, ActualWidth, rangeY - p.Y);
                dc.DrawRectangle(mBackground, null, sr);
            }

            if (IsFocused)
            {
                Rect sr = new Rect(0, offset + 1, ActualWidth, dispHeight-1);
                dc.DrawRectangle(null, FocusedBorderPen, sr);
            }
        }

        #region draw text

        protected void DrawText(DrawingContext dc, TextLine line, Point pt, int row)
        {
            int sp = 0;
            foreach (AttrSpan attr in line.Attrs)
            {
                string s = line.Data.Substring(sp, attr.Len);
                pt = RenderText(dc, attr.Attr, s, pt, row, sp);
                sp += attr.Len;
            }
        }

        protected void DrawSelectedRange(DrawingContext dc, int row)
        {
            if (Sel.IsValid && row >= Sel.SP.Row && row <= Sel.EP.Row)
            {
                Rect r = new Rect(mTextLeftMargin, row * mLineHeight, 0, mLineHeight);

                TextSpan ts = Sel.GetRowSpan(row, mList[row].Data.Length);

                r.X = ts.Start * CW + mTextLeftMargin;
                r.Width = ts.Len * CW;

                dc.PushOpacity(mSelectedBackgroundOpacity);
                dc.DrawRectangle(mSelectedBackground, null, r);
                dc.Pop();
            }
        }

        protected Point RenderText(
            DrawingContext dc, TextAttr attr, string s, Point pt, int row, int col)
        {
            Brush foreground;

            foreground = Esc.Palette[attr.FColor];

            FormattedText ft = GetFormattedText(s, foreground);

            Point pt2 = pt;
            pt2.X += ft.WidthIncludingTrailingWhitespace; // 末尾のspaceも含む幅
            pt2.Y += mLineHeight;

            Brush background;

            background = Esc.Palette[attr.BColor];

            dc.DrawRectangle(background, null, new Rect(pt, pt2));

            Point tpt = pt;

            tpt.Y = pt.Y + (mLineHeight - ft.Height) / 2;

            dc.DrawText(ft, tpt);
            pt.X += ft.Width;
            return pt;
        }
        #endregion

        protected FormattedText GetFormattedText(string s, Brush brush)
        {
            FormattedText formattedText = new FormattedText(s,
                                                      System.Globalization.CultureInfo.CurrentCulture,
                                                      FlowDirection.LeftToRight,
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

        private void UpdateView()
        {
            if (mIsLoaded)
            {
                InvalidateVisual();
            }
        }
    }
}
