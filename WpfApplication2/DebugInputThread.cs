using System;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using System.Windows;

namespace WpfApplication2
{
    public class DebugInputThread
    {
        public delegate void LineArrived(string s);

        private LineArrived mLineArrived;

        private Thread mThread;

        private bool mContinue;

        Dispatcher dispatcher = Application.Current.Dispatcher;

        public DebugInputThread(LineArrived lineArrived)
        {
            mLineArrived = lineArrived;
        }

        public void start()
        {
            if (mThread != null)
            {
                return;
            }

            mContinue = true;

            mThread = new Thread(run);
            mThread.Start();
        }

        public void stop()
        {
            if (mThread == null)
            {
                return;
            }

            mContinue = false;

            mThread.Join();
            mThread = null;
        }

        #pragma warning disable 168
        private void run()
        {
            while (mContinue)
            {
                string s = "";

                try
                {
                    Console.Write("> ");
                    s = Console.ReadLine();
                }
                catch (IOException e)
                {
                    break;
                }

                if (s != null && s.Length == 0)
                {
                    continue;
                }

                // Run on ui thread
                dispatcher.Invoke(mLineArrived, s);
            }
        }
        #pragma warning restore 0168
    }
}
