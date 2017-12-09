using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCad
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
            if (!walker(this, level))
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

                if (!item.ForEach(walker, level + 1))
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
}
