using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public class CadObjectItem : INotifyPropertyChanged
    {
        public CadFigure Figure;

        public ObservableCollection<CadObjectItem> mChildren;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<CadObjectItem> Children
        {
            get
            {
                return mChildren;
            }
        }

        public string ID
        {
            get
            {
                if (Figure == null)
                {
                    return "0";
                }

                return Figure.ID.ToString();
            }
        }

        public string Type
        {
            get
            {
                if (Figure == null)
                {
                    return CadFigure.Types.NONE.ToString();
                }

                return Figure.Type.ToString();
            }
        }


        bool mIsSelected = false;
        public bool IsSelected
        {
            set
            {
                mIsSelected = value;
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
                mIsExpanded = value;
            }
            get
            {
                return mIsExpanded;
            }
        }

        public CadObjectItem(CadFigure fig)
        {
            Figure = fig;
            mChildren = new ObservableCollection<CadObjectItem>();

            if (Figure.ChildList != null)
            {
                foreach (CadFigure child in Figure.ChildList)
                {
                    mChildren.Add(new CadObjectItem(child));
                }
            }
        }

        public void Add(CadObjectItem item)
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
