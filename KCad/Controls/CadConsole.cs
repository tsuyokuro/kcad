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

        public const string Reset = ESC + "0m";

        public const string Balck = ESC + "30m";
        public const string Red = ESC + "31m";
        public const string Green = ESC + "32m";
        public const string Yellow = ESC + "33m";
        public const string Blue = ESC + "34m";
        public const string Magenta = ESC + "35m";
        public const string Cyan = ESC + "36m";
        public const string White = ESC + "37m";

        public const string BBalck = ESC + "90m";
        public const string BRed = ESC + "91m";
        public const string BGreen = ESC + "92m";
        public const string BYellow = ESC + "93m";
        public const string BBlue = ESC + "94m";
        public const string BMagenta = ESC + "95m";
        public const string BCyan = ESC + "96m";
        public const string BWhite = ESC + "97m";


        public const string BalckB = ESC + "40m";
        public const string RedB = ESC + "41m";
        public const string GreenB = ESC + "42m";
        public const string YellowB = ESC + "43m";
        public const string BlueB = ESC + "44m";
        public const string MagentaB = ESC + "45m";
        public const string CyanB = ESC + "46m";
        public const string WhiteB = ESC + "47m";

        public const string BBalckB = ESC + "100m";
        public const string BRedB = ESC + "101m";
        public const string BGreenB = ESC + "102m";
        public const string BYellowB = ESC + "103m";
        public const string BBlueB = ESC + "104m";
        public const string BMagentaB = ESC + "105m";
        public const string BCyanB = ESC + "106m";
        public const string BWhiteB = ESC + "107m";


        public Brush[] Palette;
        public Brush[] SelPalette;

        public int DefaultFColor = 7;
        public int DefaultBColor = 0;

        public AnsiEsc()
        {
            Palette = new Brush[16];

            Palette[0] = Brushes.Black;
            Palette[1] = Brushes.MediumVioletRed;
            Palette[2] = Brushes.SeaGreen;
            Palette[3] = Brushes.Goldenrod;
            Palette[4] = Brushes.SteelBlue;
            Palette[5] = Brushes.DarkMagenta;
            Palette[6] = Brushes.DarkCyan;
            Palette[7] = Brushes.LightGray;

            Palette[8] = Brushes.Black;
            Palette[9] = Brushes.LightCoral;
            Palette[10] = Brushes.SpringGreen;
            Palette[11] = Brushes.Yellow;
            Palette[12] = Brushes.CornflowerBlue;
            Palette[13] = Brushes.MediumOrchid;
            Palette[14] = Brushes.Turquoise;
            Palette[15] = Brushes.White;



            SelPalette = new Brush[16];

            float b = 1.4f;

            SelPalette[0] = Brigahtness(Palette[0], b);
            SelPalette[1] = Brigahtness(Palette[1], b);
            SelPalette[2] = Brigahtness(Palette[2], b);
            SelPalette[3] = Brigahtness(Palette[3], b);
            SelPalette[4] = Brigahtness(Palette[4], b);
            SelPalette[5] = Brigahtness(Palette[5], b);
            SelPalette[6] = Brigahtness(Palette[6], b);
            SelPalette[7] = Brigahtness(Palette[7], b);

            SelPalette[8] = Brigahtness(Palette[8], b);
            SelPalette[9] = Brigahtness(Palette[9], b);
            SelPalette[10] = Brigahtness(Palette[10], b);
            SelPalette[11] = Brigahtness(Palette[11], b);
            SelPalette[12] = Brigahtness(Palette[12], b);
            SelPalette[13] = Brigahtness(Palette[13], b);
            SelPalette[14] = Brigahtness(Palette[14], b);
            SelPalette[15] = Brigahtness(Palette[15], b);
        }

        Brush Brigahtness(Brush brush, float a)
        {
            if (!(brush is SolidColorBrush))
            {
                return brush;
            }

            SolidColorBrush src = (SolidColorBrush)brush;
            Color c = src.Color;

            RGB rgb = new RGB(c.R/255f, c.G/255f, c.B/255f);

            rgb = ColorUtil.Brightness(rgb, a);

            Color cc = Color.FromArgb(0xff, (byte)(rgb.R * 255f), (byte)(rgb.G * 255f), (byte)(rgb.B * 255f));

            return new SolidColorBrush(cc);
        }
    }

    struct TextAttr
    {
        public int FColor;
        public int BColor;
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

        TextAttr DefaultAttr = new TextAttr();

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

            Esc.Palette[Esc.DefaultFColor] = mForeground;
            Esc.Palette[Esc.DefaultBColor] = mBackground;

            Esc.SelPalette[Esc.DefaultFColor] = mSelectedForeground;
            Esc.SelPalette[Esc.DefaultBColor] = mSelectedBackground;


            DefaultAttr.FColor = Esc.DefaultFColor;
            DefaultAttr.BColor = Esc.DefaultBColor;
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

                DrawText(dc, item.Data, tp, item.IsSelected);

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

        #region draw text
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
                                attr.FColor = x - 30;
                            }
                            else if (x >= 40 && x <= 47) // back std
                            {
                                attr.BColor = x - 40;
                            }
                            else if (x >= 90 && x <= 97) // front strong
                            {
                                attr.FColor = x - 90 + 8;
                            }
                            else if (x >= 100 && x <= 107) // back std
                            {
                                attr.BColor = x - 100 + 8;
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

        public Point RenderText(DrawingContext dc, TextAttr attr, string s, Point pt, bool selected)
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

            FormattedText ft = GetText(s, fgb);

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
