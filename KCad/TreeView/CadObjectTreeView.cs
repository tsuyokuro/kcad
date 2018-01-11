﻿using System;
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
    public delegate bool TreeWalker(ICadObjectTreeItem item);
    public delegate bool TreeWalkerLv(ICadObjectTreeItem item, int level);

    public interface ICadObjectTreeItem
    {
        ICadObjectTreeItem Parent { get; set; }

        bool IsExpand { get; set; }
        bool IsChecked { get; set; }

        string Text { get; }

        List<ICadObjectTreeItem> Children { get; }
        int GetTotalCount();
        void Add(ICadObjectTreeItem item);

        bool ForEach(TreeWalker walker);
        bool ForEach(TreeWalkerLv walker, int level);

        ICadObjectTreeItem GetAt(int n);
    }

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


        protected ICadObjectTreeItem mRoot;

        public ICadObjectTreeItem Root
        {
            get { return mRoot; }
            set
            {
                AttachRoot(value);
            }
        }

        protected ScrollViewer Scroll;

        protected FontFamily mFontFamily;

        protected Typeface mTypeface;

        protected Brush mForeground = Brushes.Black;

        protected Brush mBackground = Brushes.White;

        protected Brush mCheckedForeground = Brushes.Black;

        protected Brush mCheckedBackground = new SolidColorBrush(Color.FromRgb(120,160,0));


        protected double mItemHeight = 20.0;

        protected double mTextSize = 14.0;

        protected double mIndentSize = 8.0;

        public CadObjectTreeView()
        {
            mFontFamily = new FontFamily("Arial");
            mTypeface = new Typeface(mFontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

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
        }


        protected void CadObjectTree_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this);

            int idx = (int)(p.Y / mItemHeight);

            if (!ShowRoot)
            {
                idx++;
            }

            ICadObjectTreeItem item = mRoot.GetAt(idx);

            if (item == null)
            {
                return;
            }


            item.IsChecked = item.IsChecked == false;

            OnCheckChanged(EventArgs.Empty);

            InvalidateVisual();
        }

        public void SetVPos(int pos)
        {
            Scroll.ScrollToVerticalOffset(pos * mItemHeight);
        }

        public int Find(Func<ICadObjectTreeItem, bool> comp)
        {
            int idx = -1;
            int cnt = 0;

            mRoot.ForEach((item) =>
            {
                if (!ShowRoot && item == mRoot)
                {
                    return true;
                }

                if (comp(item))
                {
                    idx = cnt;
                    return false;
                }

                cnt++;
                return true;
            });

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

                if (item.IsChecked)
                {
                    ft = GetText(item.Text, mCheckedForeground);
                    dc.DrawRectangle(mCheckedBackground, null, rect);
                }
                else
                {
                    ft = GetText(item.Text, mForeground);
                    dc.DrawRectangle(mBackground, null, rect);
                }

                p.X = mIndentSize * (level - topLevel);

                tp = p;

                tp.Y += textOffset;

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


        public void AttachRoot(ICadObjectTreeItem root)
        {
            mRoot = root;

            int tc = mRoot.GetTotalCount();

            Height = mItemHeight * (double)(tc + 2);
        }

        public void Redraw()
        {
            InvalidateVisual();
        }
    }
}