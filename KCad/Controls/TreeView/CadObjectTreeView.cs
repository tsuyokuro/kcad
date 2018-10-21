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
    public class CadObjectTreeView : FrameworkElement
    {
        static CadObjectTreeView()
        {
        }

        #region Event
        public event EventHandler CheckChanged;

        protected virtual void OnCheckChanged(EventArgs e)
        {
            if (CheckChanged != null)
            {
                CheckChanged(this, e);
            }
        }
        #endregion

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

        public Brush CheckedBackground
        {
            get
            {
                return mCheckedBackground;
            }
            set
            {
                mCheckedBackground = value;
            }
        }

        public Brush CheckedForeground
        {
            get
            {
                return mCheckedForeground;
            }
            set
            {
                mCheckedForeground = value;
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

        public bool ShowRoot
        {
            get; set;
        } = false;


        protected CadObjTreeItem mRoot;

        public CadObjTreeItem Root
        {
            get { return mRoot; }
            set
            {
                AttachRoot(value);
            }
        }

        protected ScrollViewer Scroll;

        //protected FontFamily mFontFamily;

        protected Typeface mTypeface;

        protected Typeface mPartsTypeface;


        protected Brush mForeground = Brushes.Black;

        protected Brush mBackground = Brushes.White;

        protected Brush mCheckedForeground = Brushes.White;

        protected Brush mCheckedBackground = new SolidColorBrush(Color.FromRgb(0x22,0x8B,0x22));


        protected double mItemHeight = 20.0;

        protected double mTextSize = 16.0;

        protected double mIndentSize = 12.0;

        protected double mSmallIndentSize = 4.0;


        FormattedText mExpand;

        FormattedText mContract;

        public CadObjectTreeView()
        {
            FontFamily font;

            //font = new FontFamily("ＭＳ ゴシック");
            font = new FontFamily("Consolas");
            mTypeface = new Typeface(font, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

            font = new FontFamily("Marlett");   // WindowsのCloseボタン等の部品がFontになったもの
            mPartsTypeface = new Typeface(font, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

            Loaded += CadObjectTree_Loaded;
            MouseDown += CadObjectTree_MouseDown;
        }

        protected void CadObjectTree_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement parent = (FrameworkElement)Parent;

            if (parent is ScrollViewer)
            {
                Scroll = (ScrollViewer)parent;
            }

            if (Scroll != null)
            {
                Scroll.ScrollChanged += Scroll_ScrollChanged;
            }

            CreateParts();
        }


        protected void CadObjectTree_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this);
            
            int idx = (int)(p.Y / mItemHeight);

            if (!ShowRoot)
            {
                idx++;
            }

            CadObjTreeItem item = mRoot.GetAt(idx);

            if (item == null)
            {
                return;
            }

            int level = item.GetLevel();

            if (item.Children != null)
            {
                if (p.X > (level) * mIndentSize)
                {
                    item.IsChecked = item.IsChecked == false;
                    OnCheckChanged(EventArgs.Empty);
                }
                else
                {
                    item.IsExpand = item.IsExpand == false;
                    RecalcSize();
                }
            }
            else
            {
                item.IsChecked = item.IsChecked == false;
                OnCheckChanged(EventArgs.Empty);
            }

            InvalidateVisual();
        }

        public void SetVPos(int pos)
        {
            if (Dispatcher.CheckAccess())
            {
                Scroll.ScrollToVerticalOffset(pos * mItemHeight);
            }
            else
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    Scroll.ScrollToVerticalOffset(pos * mItemHeight);
                }));
            }
        }

        public int Find(Func<CadObjTreeItem, bool> comp)
        {
            int idx = -1;
            int cnt = 0;

            mRoot.ForEach((item) =>
            {
                if (comp(item))
                {
                    idx = cnt;
                    return false;
                }

                cnt++;
                return true;
            });

            if (!ShowRoot && idx>=0)
            {
                idx--;
            }

            return idx;
        }

        protected void Scroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            Size size = RenderSize;

            double offset = 0;

            double dispHeight = ActualHeight;

            if (Scroll != null)
            {
                offset = Scroll.VerticalOffset;
                dispHeight = Scroll.ActualHeight;
            }

            if (mRoot == null)
            {
                return;
            }

            double textOffset = (mItemHeight - mTextSize) / 2.0;

            Point p = default(Point);
            Point tp = default(Point);
            Rect rect = default(Rect);

            Point mp = default(Point);

            long topNumber = (long)offset / (long)mItemHeight;

            if (!ShowRoot)
            {
                topNumber++;
            }

            p.X = 0;
            p.Y = mItemHeight * topNumber;

            if (ShowRoot)
            {
                p.Y = mItemHeight * topNumber;
            }
            else
            {
                p.Y = mItemHeight * (topNumber - 1);
            }

            rect.X = 0;
            rect.Y = p.Y;
            rect.Width = ActualWidth;
            rect.Height = mItemHeight + 1;

            long skip = topNumber;

            double rangeY = offset + dispHeight;

            int topLevel = 0;

            if (!ShowRoot)
            {
                topLevel = 1;
            }

            mRoot.ForEach((item, level) =>
            {
                skip--;
                if (skip >= 0)
                {
                    return true;
                }

                FormattedText ft;

                rect.Y = p.Y;

                Brush fbrush = item.getForeColor();

                Brush bbrush = item.getBackColor();

                if (item.IsChecked)
                {
                    fbrush = fbrush ?? mCheckedForeground;
                    bbrush = bbrush ?? mCheckedBackground;
                }
                else
                {
                    fbrush = fbrush ?? mForeground;
                    bbrush = bbrush ?? mBackground;
                }

                ft = GetText(item.Text, fbrush);
                dc.DrawRectangle(bbrush, null, rect);

                if (item.Children != null)
                {
                    p.X = mIndentSize * (level - topLevel) + mIndentSize;
                }
                else
                {
                    p.X = mIndentSize * (level - topLevel) + mSmallIndentSize;
                }

                tp = p;

                tp.Y += textOffset;

                if (item.Children != null)
                {
                    mp = tp;
                    mp.X -= mIndentSize;
                    //mp.X += 4;

                    if (item.IsExpand)
                    {
                        dc.DrawText(mContract, mp);
                    }
                    else
                    {
                        dc.DrawText(mExpand, mp);
                    }
                }

                dc.DrawText(ft, tp);

                p.Y += mItemHeight;

                if (p.Y < rangeY)
                {
                    return true;
                }

                return false;
            }, 0);

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

        protected FormattedText GetText(string s, Brush brush, Typeface typeFace)
        {
            FormattedText formattedText = new FormattedText(s,
                                                      System.Globalization.CultureInfo.CurrentCulture,
                                                      System.Windows.FlowDirection.LeftToRight,
                                                      typeFace,
                                                      mTextSize,
                                                      brush);
            return formattedText;
        }

        protected void CreateParts()
        {
            mExpand = GetText("4", mForeground, mPartsTypeface);
            mContract = GetText("6", mForeground, mPartsTypeface);
        }

        public void AttachRoot(CadObjTreeItem root)
        {
            mRoot = root;

            mRoot.IsExpand = true;

            RecalcSize();
        }

        private void RecalcSize()
        {
            int tc = mRoot.GetTotalCount();
            Height = mItemHeight * (double)(tc + 2);
        }

        public void Redraw()
        {
            InvalidateVisual();
        }
    }
}
