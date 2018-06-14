using System;
using System.Collections.Generic;
using System.Linq;
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
    public class AnsiEsc
    {
        public const string ESC = "\x1b[";

        public const string BALCK = "30";
        public const string RED = "31";
        public const string GREEN = "32";
        public const string YELLOW = "33";
        public const string BLUE = "34";
        public const string MAGENTA = "35";
        public const string CYAN = "36";
        public const string WHITE = "37";
        public const string DEF_COLOR = "39";

        public const string BBALCK = "90";
        public const string BRED = "91";
        public const string BGREEN = "92";
        public const string BYELLOW = "93";
        public const string BBLUE = "94";
        public const string BMAGENTA = "95";
        public const string BCYAN = "96";
        public const string BWHITE = "97";


        public const string ABalck = ESC + "30m";
        public const string ARed = ESC + "31m";
        public const string AGreen = ESC + "32m";
        public const string AYellow = ESC + "33m";
        public const string ABlue = ESC + "34m";
        public const string AMagenta = ESC + "35m";
        public const string ACyan = ESC + "36m";
        public const string AWhite = ESC + "37m";
        public const string DefColor = ESC + "39m";

        public const string BBalck = ESC + "90m";
        public const string BRed = ESC + "91m";
        public const string BGreen = ESC + "92m";
        public const string BYellow = ESC + "93m";
        public const string BBlue = ESC + "94m";
        public const string BMagenta = ESC + "95m";
        public const string BCyan = ESC + "96m";
        public const string BWhite = ESC + "97m";

        public Dictionary<string, Brush> Palette;

        public Brush DefaultColor = Brushes.LightGray;

        public AnsiEsc()
        {
            Palette = new Dictionary<string, Brush>()
            {
                {"30m", Brushes.Black},
                {"31m", Brushes.MediumVioletRed},
                {"32m", Brushes.SeaGreen},
                {"33m", Brushes.Goldenrod},
                {"34m", Brushes.SteelBlue},
                {"35m", Brushes.DarkMagenta},
                {"36m", Brushes.DarkCyan},
                {"37m", Brushes.LightGray},

                {"39m", Brushes.LightGray},

                {"90m", Brushes.Black},
                {"91m", Brushes.LightCoral},
                {"92m", Brushes.SpringGreen},
                {"93m", Brushes.Yellow},
                {"94m", Brushes.CornflowerBlue},
                {"95m", Brushes.MediumOrchid},
                {"96m", Brushes.Turquoise},
                {"97m", Brushes.White},
            };
        }
    }

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

        AnsiEsc Esc = new AnsiEsc();

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

            Esc.DefaultColor = mForeground;
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
        }

        protected void DrawText(DrawingContext dc, ListItem item, Point pt)
        {
            FormattedText ft;

            string s = item.Data;

            string[] ss = s.Split('\x1b');

            Brush br;

            if (item.IsSelected)
            {
                br = mSelectedForeground;
            }
            else
            {
                //br = mForeground;
                br = Esc.DefaultColor;
            }

            Brush cbr = br;

            string ps;

            for (int i=0; i<ss.Length; i++)
            {
                s = ss[i];

                if (s.Length <= 0)
                {
                    continue;
                }


                if (i>0)
                {
                    if (s.StartsWith("["))
                    {
                        string c = s.Substring(1, 3);
                        ps = s.Substring(4);

                        cbr = Esc.Palette[c];

                        if (cbr == null)
                        {
                            cbr = br;
                        }
                    }
                    else
                    {
                        ps = s;
                    }
                }
                else
                {
                    ps = s;
                }

                ft = GetText(ps, cbr);
                dc.DrawText(ft, pt);
                pt.X += ft.Width;
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
