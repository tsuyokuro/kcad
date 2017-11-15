using KCad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KCad.CadObjectTreeView;

namespace Plotter
{
    public abstract class CadObjTreeItem : ICadObjectTreeItem
    {
        public bool IsExpand
        {
            get; set;
        }

        public virtual ICadObjectTreeItem Parent
        {
            get; set;
        }

        public virtual bool IsChecked
        {
            get
            {
                return false;
            }

            set
            {
            }
        }

        public virtual string Text
        {
            get
            {
                return "----";
            }
        }

        protected List<ICadObjectTreeItem> mChildren;

        public List<ICadObjectTreeItem> Children
        {
            get
            {
                return mChildren;
            }
        }

        public virtual void Add(ICadObjectTreeItem item)
        {
            if (mChildren == null)
            {
                mChildren = new List<ICadObjectTreeItem>();
            }

            item.Parent = this;

            mChildren.Add(item);
        }

        public virtual int GetTotalCount()
        {
            int cnt = 1;

            if (mChildren != null)
            {
                for (int i = 0; i < mChildren.Count; i++)
                {
                    var item = mChildren[i];
                    cnt += item.GetTotalCount();
                }
            }

            return cnt;
        }

        public bool ForEach(TreeWalker walker)
        {
            if (!walker(this))
            {
                return false;
            }

            if (mChildren == null)
            {
                return true;
            }

            int i;
            for (i = 0; i < mChildren.Count; i++)
            {
                ICadObjectTreeItem item = mChildren[i];

                if (!item.ForEach(walker))
                {
                    return false;
                }
            }

            return true;
        }

        public bool ForEach(TreeWalkerLv walker, int level)
        {
            if (!walker(this , level))
            {
                return false;
            }

            if (mChildren == null)
            {
                return true;
            }

            int i;
            for (i = 0; i < mChildren.Count; i++)
            {
                ICadObjectTreeItem item = mChildren[i];

                if (!item.ForEach(walker, level+1))
                {
                    return false;
                }
            }

            return true;
        }

        public ICadObjectTreeItem GetAt(int n)
        {
            int i = 0;

            ICadObjectTreeItem ret = null;

            ForEach(item =>
            {
                if (n == i)
                {
                    ret = item;
                    return false;
                }

                i++;
                return true;
            });

            return ret;
        }
    }

    class CadLayerTreeItem : CadObjTreeItem
    {
        public CadLayer Layer;

        public override bool IsChecked
        {
            get
            {
                return false;
            }

            set
            {
            }
        }

        public override string Text
        {
            get
            {
                return "LAYER:" + Layer.Name;
            }
        }

        public CadLayerTreeItem(CadLayer layer)
        {
            Layer = layer;
            foreach (CadFigure fig in Layer.FigureList)
            {
                ICadObjectTreeItem item = new CadFigTreeItem(fig);
                Add(item);
            };
        }
    }

    class CadFigTreeItem : CadObjTreeItem
    {
        public CadFigure Fig;

        public override bool IsChecked
        {
            get
            {
                return false;
            }

            set
            {
            }
        }

        public override string Text
        {
            get
            {
                return "FIG:" + Fig.ID.ToString();
            }
        }

        public CadFigTreeItem(CadFigure fig)
        {
            Fig = fig;

            int idx = 0;

            Fig.PointList.ForEach(p =>
            {
                CadPointTreeItem pi = new CadPointTreeItem(fig, idx);
                Add(pi);
                idx++;
            });

            Fig.ChildList.ForEach(c =>
            {
                CadFigTreeItem pi = new CadFigTreeItem(c);
                Add(pi);
            });
        }
    }

    class CadPointTreeItem : CadObjTreeItem
    {
        public CadFigure Fig;
        public int Index;

        public override bool IsChecked
        {
            get
            {
                if (Index >=0 && Index < Fig.PointCount)
                {
                    return Fig.GetPointAt(Index).Selected;
                }

                return false;
            }

            set
            {
                CadVector v = Fig.GetPointAt(Index);
                v.Selected = value;
                Fig.SetPointAt(Index, v);
            }
        }

        public override string Text
        {
            get
            {
                CadVector v;

                if (Index >= 0 && Index < Fig.PointCount)
                {
                    v = Fig.GetPointAt(Index);

                    return v.x.ToString("F2") + "," + v.y.ToString("F2") + "," + v.z.ToString("F2");
                }

                return "removed";
            }
        }

        public CadPointTreeItem(CadFigure fig, int idx)
        {
            Fig = fig;
            Index = idx;
        }
    }
}
