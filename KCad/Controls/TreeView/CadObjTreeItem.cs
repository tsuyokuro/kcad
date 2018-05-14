﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCad
{
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

        public int GetLevel()
        {
            int i = 0;

            CadObjTreeItem parent = Parent;


            while (parent != null)
            {
                i++;
                parent = parent.Parent;
            }

            return i;
        }


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

            if (mChildren != null && IsExpand)
            {
                for (int i = 0; i < mChildren.Count; i++)
                {
                    var item = mChildren[i];
                    cnt += item.GetTotalCount();
                }
            }

            return cnt;
        }

        public bool ForEach(Func<CadObjTreeItem, bool> func)
        {
            if (!func(this))
            {
                return false;
            }

            if (!IsExpand)
            {
                return true;
            }

            if (mChildren == null)
            {
                return true;
            }

            int i;
            for (i = 0; i < mChildren.Count; i++)
            {
                CadObjTreeItem item = mChildren[i];

                if (!item.ForEach(func))
                {
                    return false;
                }
            }

            return true;
        }

        public bool ForEach(Func<CadObjTreeItem, int, bool> func, int level)
        {
            if (!func(this, level))
            {
                return false;
            }

            if (!IsExpand)
            {
                return true;
            }

            if (mChildren == null)
            {
                return true;
            }

            int i;
            for (i = 0; i < mChildren.Count; i++)
            {
                CadObjTreeItem item = mChildren[i];

                if (!item.ForEach(func, level + 1))
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
