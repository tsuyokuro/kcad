using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    class ThreadUtil
    {
        public TaskScheduler MainThreadScheduler;

        public int MainThreadID;

        public ThreadUtil()
        {
            MainThreadScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            MainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        public void RunOnMainThread(Action action, bool wait)
        {
            if (MainThreadID == System.Threading.Thread.CurrentThread.ManagedThreadId)
            {
                action();
                return;
            }

            Task task = new Task(() =>
            {
                action();
            }
            );

            task.Start(MainThreadScheduler);

            if (wait)
            {
                task.Wait();
            }
        }
    }
}
