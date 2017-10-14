using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Plotter
{
    public class CadObjectItem : INotifyPropertyChanged
    {
        public enum ItemType
        {
            NONE,
            FIGURE,
            POINT,
        }

        public ItemType Type = ItemType.NONE;

        public CadFigure Figure;

        public int PointIndex;

        public ObservableCollection<CadObjectItem> mChildren;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<CadObjectItem> Children
        {
            get
            {
                return mChildren;
            }
        }
        public string Text
        {
            get
            {
                String s = "Unknown";

                if (Type == ItemType.NONE)
                {

                }
                else if (Type == ItemType.FIGURE)
                {
                    if (Figure != null)
                    {
                        s = Figure.ID.ToString() + ":" + Figure.Type.ToString();
                    }
                }
                else if (Type == ItemType.POINT)
                {
                    if (Figure != null)
                    {
                        CadVector v = Figure.PointList[PointIndex];

                        s = "[" + PointIndex.ToString() + "] " +
                            v.x.ToString() + "," + v.y.ToString() + "," + v.z.ToString();
                    }
                }

                return s;
            }
        }


        bool mIsChecked = false;

        public bool IsChecked
        {
            set
            {
                if (value != mIsChecked)
                {
                    mIsChecked = value;
                    OnPropertyChanged("IsChecked");
                    CheckedChildren(mIsChecked);
                }
            }

            get
            {
                return mIsChecked;
            }
        }

        bool mIsSelected = false;
        public bool IsSelected
        {
            set
            {
                if (value != mIsSelected)
                {
                    mIsSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
            get
            {
                return mIsSelected;
            }
        }

        bool mIsExpanded = false;
        public bool IsExpanded
        {
            set
            {
                if (value != mIsExpanded)
                {
                    mIsExpanded = value;
                    OnPropertyChanged("mIsExpanded");
                }
            }
            get
            {
                return mIsExpanded;
            }
        }

        public static CadObjectItem CreateFigure(CadFigure fig)
        {
            CadObjectItem item = new CadObjectItem();

            item.Type = ItemType.FIGURE;
            item.Figure = fig;

            if (item.Figure.ChildList != null)
            {
                item.mChildren = new ObservableCollection<CadObjectItem>();

                foreach (CadFigure child in item.Figure.ChildList)
                {
                    item.mChildren.Add( CreateFigure(child) );
                }
            }

            int pcnt = item.Figure.PointList.Count;

            if (pcnt > 0)
            {
                for (int i = 0; i < pcnt; i++)
                {
                    item.AddChild(CreatePoint(item.Figure, i));
                }

                /*
                bool allSelected = true;

                for (int i = 0; i < pcnt; i++)
                {
                    if (!item.Children[i].IsChecked)
                    {
                        allSelected = false;
                        break;
                    }
                }

                item.IsChecked = allSelected;
                */
            }
            return item;
        }

        public static CadObjectItem CreatePoint(CadFigure fig, int pointIndex)
        {
            CadObjectItem item = new CadObjectItem();

            item.Type = ItemType.POINT;
            item.Figure = fig;
            item.PointIndex = pointIndex;

            CadVector v = item.Figure.GetPointAt(pointIndex);

            item.IsChecked = v.Selected;

            return item;
        }

        public void CheckedChildren(bool check)
        {
            if (mChildren == null)
            {
                return;
            }

            for (int i=0; i<mChildren.Count; i++)
            {
                mChildren[i].IsChecked = check;
            }
        }

        public void AddChild(CadObjectItem item)
        {
            mChildren.Add(item);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }
    }
}
