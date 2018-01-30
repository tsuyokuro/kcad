using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public class VectorList
    {
        public List<CadVector> VList = new List<CadVector>();
        public List<List<int>> CList = new List<List<int>>();

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

        public CadVector this [ int contIdx, int pIdx ]
        {
            get
            {
                return VList[CList[contIdx][pIdx]];
            }

            set
            {
                VList[CList[contIdx][pIdx]] = value;
            }
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

        public int NewCont()
        {
            CList.Add(new List<int>());
            return CList.Count - 1;
        }

        public int AddCont(List<int> cont)
        {
            CList.Add(cont);
            return CList.Count - 1;
        }

        public void AddToCont(int contIdx, int pointIdx)
        {
            CList[contIdx].Add(pointIdx);
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

        public void InsertRange(int index, IEnumerable<CadVector> collection)
        {
            VList.InsertRange(index, collection);
        }
    }
}
