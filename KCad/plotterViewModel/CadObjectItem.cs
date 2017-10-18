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
            NODE,
            ROOT,
        }

        ItemsContext mContext;

        public ItemsContext Context
        {
            get
            {
                return mContext;
            }
        }

        public ItemType Type = ItemType.NONE;

        public CadFigure Figure;

        public int PointIndex;

        public ObservableCollection<CadObjectItem> mChildren;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name;

        public IReadOnlyList<CadObjectItem> Children
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
                else if (Type == ItemType.NODE)
                {
                    s = Name;
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
                            v.x.ToString() + ", " + v.y.ToString() + ", " + v.z.ToString();
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
                    CheckedChange(value);
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

        public static CadObjectItem CreateRoot(string name, ItemsContext context)
        {
            CadObjectItem item = new CadObjectItem();
            item.Type = ItemType.ROOT;
            item.Name = name;
            item.mChildren = new ObservableCollection<CadObjectItem>();
            return item;
        }

        public static CadObjectItem CreateNode(string name, ItemsContext context)
        {
            CadObjectItem item = new CadObjectItem();
            item.Type = ItemType.NODE;
            item.Name = name;
            item.mChildren = new ObservableCollection<CadObjectItem>();
            item.mContext = context;
            return item;
        }

        public static CadObjectItem CreateFigure(CadFigure fig, ItemsContext context)
        {
            CadObjectItem item = new CadObjectItem();

            item.Type = ItemType.FIGURE;
            item.Figure = fig;
            item.mContext = context;


            if (item.Figure.ChildList != null)
            {
                item.mChildren = new ObservableCollection<CadObjectItem>();

                foreach (CadFigure child in item.Figure.ChildList)
                {
                    item.AddChild( CreateFigure(child, context) );
                }
            }

            int pcnt = item.Figure.PointList.Count;

            if (pcnt > 0)
            {
                for (int i = 0; i < pcnt; i++)
                {
                    item.AddChild( CreatePoint(item.Figure, i, context) );
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

        public static CadObjectItem CreatePoint(CadFigure fig, int pointIndex, ItemsContext context)
        {
            CadObjectItem item = new CadObjectItem();

            item.Type = ItemType.POINT;
            item.Figure = fig;
            item.PointIndex = pointIndex;
            item.mContext = context;

            CadVector v = item.Figure.GetPointAt(pointIndex);

            item.IsChecked = v.Selected;

            return item;
        }

        private void CheckedChange(bool value, bool notify=true)
        {
            mIsChecked = value;

            if (Type == ItemType.POINT)
            {
                CadVector p = Figure.PointList[PointIndex];
                p.Selected = value;

                Figure.PointList[PointIndex] = p;
            }

            OnPropertyChanged("IsChecked");
            CheckedChildren(value);

            if (Context != null && notify)
            {
                Context.HandleItemChanged(this);
            }
        }

        public void CheckedChildren(bool check)
        {
            if (mChildren == null)
            {
                return;
            }

            for (int i=0; i<mChildren.Count; i++)
            {
                mChildren[i].CheckedChange(check, false);
            }
        }

        public void ClearChildren()
        {
            if (mChildren == null)
            {
                return;
            }

            for (int i = 0; i < mChildren.Count; i++)
            {
                mChildren[i].ClearChildren();
                mChildren[i].Dispose();
            }

            mChildren.Clear();
        }

        public void Dispose()
        {
            if (Context != null)
            {
                if (Type == ItemType.FIGURE)
                {
                    Context.FigureIDSet.Remove(Figure.ID);
                }
            }
        }

        public void AddChild(CadObjectItem item)
        {
            //item.setContext(mContext);

            if (Context != null)
            {
                if (item.Type == ItemType.FIGURE)
                {
                    Context.FigureIDSet.Add(item.Figure.ID);
                }
            }

            mChildren.Add(item);
        }

        public void ExpandAll(bool expand)
        {
            IsExpanded = expand;

            if (mChildren != null)
            {
                for (int i = 0; i < mChildren.Count; i++)
                {
                    mChildren[i].ExpandAll(expand);
                }
            }
        }

        public void Update()
        {
            if (Type == ItemType.FIGURE)
            {
                if (mChildren != null)
                {
                    CadObjectItem tmp = CreateFigure(Figure, mContext);
                    mChildren.Clear();

                    for (int i=0;i<tmp.mChildren.Count; i++)
                    {
                        mChildren.Add(tmp.mChildren[i]);
                    }
                }
            }
            else if (Type == ItemType.POINT)
            {
                if (Figure != null)
                {
                    mIsChecked = Figure.PointList[PointIndex].Selected;
                    OnPropertyChanged("Text");
                    OnPropertyChanged("IsChecked");
                }
            }

            if (mChildren != null)
            {
                for (int i=0;i<mChildren.Count;i++)
                {
                    mChildren[i].Update();
                }
            }
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

        private void setContext(ItemsContext context)
        {
            mContext = context;
            setContextToChildren(context);
        }

        private void setContextToChildren(ItemsContext context)
        {
            if (mChildren == null)
            {
                return;
            }

            for (int i = 0; i < mChildren.Count; i++)
            {
                mChildren[i].setContext(context);
            }
        }

        public class ItemsContext
        {
            public HashSet<uint> FigureIDSet = new HashSet<uint>();

            public delegate void ItemChanged(CadObjectItem item);
            public ItemChanged HandleItemChanged = ( a => { } );
        }
    }
}
