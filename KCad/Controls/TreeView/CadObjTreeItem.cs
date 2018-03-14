using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCad
{
    public delegate bool TreeWalker(CadObjTreeItem item);
    public delegate bool TreeWalkerLv(CadObjTreeItem item, int level);

    public abstract class CadObjTreeItem
    {
        public bool IsExpand
        {
            get; set;
        }

        public virtual CadObjTreeItem Parent
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

        protected List<CadObjTreeItem> mChildren;

        public List<CadObjTreeItem> Children
        {
            get
            {
                return mChildren;
            }
        }

        public virtual void Add(CadObjTreeItem item)
        {
            if (mChildren == null)
            {
                mChildren = new List<CadObjTreeItem>();
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
                CadObjTreeItem item = mChildren[i];

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
                CadObjTreeItem item = mChildren[i];

                if (!item.ForEach(walker, level + 1))
                {
                    return false;
                }
            }

            return true;
        }

        public CadObjTreeItem GetAt(int n)
        {
            int i = 0;

            CadObjTreeItem ret = null;

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
