using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public struct PointSpec
    {
        public int ContIndex;
        public int Index;

        public PointSpec(int cidx, int idx)
        {
            ContIndex = cidx;
            Index = idx;
        }
    }


    public class VectorList
    {
        private List<CadVector> VList = null;

        public int Count
        {
            get
            {
                return VList.Count;
            }
        }

        public CadVector this [ int i ]
        {
            get
            {
                return VList[i];
            }

            set
            {
                VList[i] = value;
            }
        }

        public VectorList()
        {
            VList = new List<CadVector>();
        }

        public VectorList(VectorList src)
        {
            VList = new List<CadVector>(src.VList);
        }

        public void ForEach(Action<CadVector> d)
        {
            int i = 0;
            for (;i<VList.Count;i++)
            {
                d(VList[i]);
            }
        }

        public IEnumerator<CadVector> GetEnumerator()
        {
            return VList.GetEnumerator();
        }

        public void Add(CadVector v)
        {
            VList.Add(v);
        }

        public void RemoveAt(int i)
        {
            VList.RemoveAt(i);
        }

        public void RemoveAll(Predicate<CadVector> match)
        {
            VList.RemoveAll(match);
        }

        public void Clear()
        {
            VList.Clear();
        }

        public void AddRange(VectorList list)
        {
            VList.AddRange(list.VList);
        }

        public void Insert(int index, CadVector v)
        {
            VList.Insert(index, v);
        }

        public void RemoveRange(int s, int cnt)
        {
            VList.RemoveRange(s, cnt);
        }

        public void InsertRange(int index, VectorList vlist)
        {
            VList.InsertRange(index, vlist.VList);
        }
    }
}
