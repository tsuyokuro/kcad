using Plotter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace KCad
{
    public partial class CadConsoleView : FrameworkElement
    {
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

        protected List<TextLine> mList = new List<TextLine>();

        protected AnsiEsc Esc = new AnsiEsc();

        protected TextAttr DefaultAttr = new TextAttr();

        protected TextAttr CurrentAttr = default;

        protected Pen FocusedBorderPen = new Pen(
                new SolidColorBrush(Color.FromRgb(0x56, 0x9D, 0xE5)), 1);

        public CadConsoleView()
        {
            Focusable = true;

            mFontFamily = new FontFamily("メイリオ");
            mTypeface = new Typeface(mFontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

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
            DebugOut.println("CadConsoleView_KeyUp");

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

            Esc.SelPalette[Esc.DefaultFColor] = mSelectedForeground;
            Esc.SelPalette[Esc.DefaultBColor] = mSelectedBackground;


            DefaultAttr.FColor = Esc.DefaultFColor;
            DefaultAttr.BColor = Esc.DefaultBColor;

            CurrentAttr = DefaultAttr;

            //RecalcSize();
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

            int idx = (int)(p.Y / mItemHeight);

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
            Height = mItemHeight * (double)(mList.Count);

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

        /*
        public string GetStringAll()
        {
            string s = "";

            foreach (ListItem line in mList)
            {
                // エスケープシーケンスを取り除く
                string ps = Regex.Replace(line.Data, "\x1b\\[[0-9]+m", "");
                s += ps + "\n";
            }

            return s;
        }

        // エスケープシーケンスを取り除かない版
        public string GetStringAllRaw()
        {
            string s = "";

            foreach (ListItem line in mList)
            {
                s += line.Data + "\n";
            }

            return s;
        }
        */

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

                TextLine item = mList[n];
                n++;

                FormattedText ft;

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

                p.Y += mItemHeight;
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
        /*
        public void DrawText(DrawingContext dc, string s, Point pt, bool selected)
        {
            TextAttr attr = DefaultAttr;

            StringBuilder sb = new StringBuilder();

            int state = 0;

            int x = 0;

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '\x1b')
                {
                    state = 1;
                    pt = RenderText(dc, attr, sb.ToString(), pt, selected);
                    sb.Clear();
                    continue;
                }

                switch (state)
                {
                    case 0:
                        sb.Append(s[i]);
                        break;
                    case 1:
                        if (s[i] == '[')
                        {
                            state = 2;
                        }
                        break;
                    case 2:
                        if (s[i] >= '0' && s[i] <= '9')
                        {
                            state = 3;
                            x = s[i] - '0';
                        }
                        else if (s[i] == 'm')
                        {
                            if (x == 0)
                            {
                                attr.BColor = 0;
                                attr.FColor = 7;
                            }

                            state = 0;
                        }
                        else
                        {
                            sb.Append(s[i]);
                            state = 0;
                        }
                        break;
                    case 3:
                        if (s[i] >= '0' && s[i] <= '9')
                        {
                            x = x * 10 + (s[i] - '0');
                        }
                        else if (s[i] == 'm')
                        {
                            if (x == 0)
                            {
                                attr.BColor = 0;
                                attr.FColor = 7;
                            }
                            else if (x >= 30 && x <= 37) // front std
                            {
                                attr.FColor = (byte)(x - 30);
                            }
                            else if (x >= 40 && x <= 47) // back std
                            {
                                attr.BColor = (byte)(x - 40);
                            }
                            else if (x >= 90 && x <= 97) // front strong
                            {
                                attr.FColor = (byte)(x - 90 + 8);
                            }
                            else if (x >= 100 && x <= 107) // back std
                            {
                                attr.BColor = (byte)(x - 100 + 8);
                            }
                            state = 0;
                        }
                        else
                        {
                            sb.Append(s[i]);
                            state = 0;
                        }

                        break;
                }
            }

            if (sb.Length > 0)
            {
                pt = RenderText(dc, attr, sb.ToString(), pt, selected);
                sb.Clear();
            }
        }
        */

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
            Brush fgb;

            if (selected)
            {
                fgb = Esc.SelPalette[attr.FColor];
            }
            else
            {
                fgb = Esc.Palette[attr.FColor];
            }

            FormattedText ft = GetFormattedText(s, fgb);

            Point pt2 = pt;
            pt2.X += ft.Width;
            pt2.Y += ft.Height;

            Brush bgb;

            if (selected)
            {
                bgb = Esc.SelPalette[attr.BColor];
            }
            else
            {
                bgb = Esc.Palette[attr.BColor];
            }

            dc.DrawRectangle(bgb, null, new Rect(pt, pt2));

            dc.DrawText(ft, pt);
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
