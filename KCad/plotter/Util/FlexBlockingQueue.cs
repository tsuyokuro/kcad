using System;
using System.Collections.Generic;
using System.Threading;

namespace Plotter
{
    class FlexBlockingQueue<T>
    {
        private readonly List<T> Queue = new List<T>();
        private readonly int Max;

        private bool Closing = false;

        public FlexBlockingQueue(int maxSize)
        {
            Max = maxSize;
        }

        public void Close()
        {
            Closing = true;
            Monitor.PulseAll(Queue);
        }

        public void Push(T item)
        {
            lock (Queue)
            {
                while (Queue.Count >= Max)
                {
                    if (Closing)
                    {
                        return;
                    }

                    Monitor.Wait(Queue);
                }

                Queue.Add(item);

                if (Queue.Count == 1)
                {
                    Monitor.PulseAll(Queue);
                }
            }
        }
        public T Pop()
        {
            lock (Queue)
            {
                while (Queue.Count == 0)
                {
                    if (Closing)
                    {
                        return default(T);
                    }

                    Monitor.Wait(Queue);
                }

                T item = Queue[0];
                Queue.RemoveAt(0);

                if (Queue.Count == Max - 1)
                {
                    Monitor.PulseAll(Queue);
                }

                return item;
            }
        }

        public int RemoveAll(Predicate<T> match)
        {
            int rc;

            lock (Queue)
            {
                rc = Queue.RemoveAll(match);

                if (Queue.Count == Max - 1)
                {
                    Monitor.PulseAll(Queue);
                }
            }

            return rc;
        }

    }
}
