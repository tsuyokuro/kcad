using System;
using System.Collections.Generic;

namespace MyCollections
{
    public class AutoArray<T>
    {
        protected T[] Tbl;

        public int Count = 0;

        public int Capacity;

        public AutoArray()
        {
            Init(8);
        }

        public AutoArray(int capa)
        {
            Init(capa);
        }

        public AutoArray(AutoArray<T> src)
        {
            Init(src.Count);
            Array.Copy(src.Tbl, Tbl, src.Count);
            Count = src.Count;
        }

        protected void Init(int capa)
        {
            Capacity = capa;
            Tbl = new T[Capacity];
            Count = 0;
        }

        public int Add(T v)
        {
            if (Count >= Tbl.Length)
            {
                Capacity = Tbl.Length * 2;
                Array.Resize<T>(ref Tbl, Capacity);
            }

            Tbl[Count] = v;
            Count++;

            return Count - 1;
        }

        public void Clear()
        {
            Count = 0;
        }

        public T this[int idx]
        {
            get
            {
                return Tbl[idx];
            }
            set
            {
                Tbl[idx] = value;
            }
        }

        public ref T Ref(int idx)
        {
            return ref Tbl[idx];
        }

        public void RemoveAt(int idx)
        {
            Array.Copy(Tbl, idx + 1, Tbl, idx, Count - (idx + 1));
            Count--;
        }

        public void ForEach(Action<T> d)
        {
            for (int i=0;i<Count; i++)
            {
                d(Tbl[i]);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            int i = 0;

            for (; i < Count; i++)
            {
                yield return Tbl[i];
            }

            yield break;
        }

        public void RemoveAll(Predicate<T> match)
        {
            int i = Count - 1;
            for (; i >= 0; i--)
            {
                if (match(Tbl[i]))
                {
                    RemoveAt(i);
                }
            }
        }

        public void AddRange(AutoArray<T> src)
        {
            int cnt = Count + src.Count;

            if (cnt >= Tbl.Length)
            {
                Capacity = cnt * 3 / 2;
                Array.Resize<T>(ref Tbl, Capacity);
            }

            Array.Copy(src.Tbl, 0, Tbl, Count, src.Count);

            Count += src.Count;
        }

        public void Insert(int idx, T val)
        {
            if (Count >= Tbl.Length)
            {
                Capacity = Tbl.Length * 2;
                Array.Resize<T>(ref Tbl, Capacity);
            }

            Array.Copy(Tbl, idx, Tbl, idx + 1, Count - idx);

            Count++;

            Tbl[idx] = val;
        }

        public void RemoveRange(int s, int cnt)
        {
            Array.Copy(Tbl, s + cnt, Tbl, s, Count - (s + cnt));
            Count -= cnt;
        }

        public void InsertRange(int idx, AutoArray<T> src)
        {
            int cnt = Count + src.Count;
            if (cnt >= Tbl.Length)
            {
                Capacity = cnt * 3 / 2;
                Array.Resize<T>(ref Tbl, Capacity);
            }

            Array.Copy(Tbl, idx, Tbl, idx + src.Count, Count - idx);
            Array.Copy(src.Tbl, 0, Tbl, idx, src.Count);

            Count += src.Count;
        }

        public T Find(Predicate<T> match)
        {
            int i = Count - 1;
            for (; i >= 0; i--)
            {
                if (match(Tbl[i]))
                {
                    return Tbl[i];
                }
            }

            return default(T);
        }
    }
}
