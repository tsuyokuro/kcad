using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace Plotter
{
    public abstract class MessageHandler
    {
        public class Message
        {
            public int What;
            public Object Obj;
            public int Arg1;
            public int Arg2;
            public long ExpireTime;

            public void clean()
            {
                What = 0;
                Obj = null;
                Arg1 = 0;
                Arg2 = 0;
                ExpireTime = 0;
            }

            public Message(int what)
            {
                What = what;
            }

            public Message() { }


            public new String ToString()
            {
                return "Message What=" + What.ToString();
            }
        }

        private Task Looper;

        private bool ContinueLoop;

        private FlexBlockingQueue<Message> Messages;

        private List<Message> DelayedMessages;

        private FlexBlockingQueue<Message> FreeMessages;

        private int QueueSize = 5;

        private System.Timers.Timer CheckTimer;

        private Object LockObj = new Object();

        public MessageHandler(int maxMessage)
        {
            QueueSize = maxMessage;

            Messages = new FlexBlockingQueue<Message>(QueueSize);
            FreeMessages = new FlexBlockingQueue<Message>(QueueSize);

            DelayedMessages = new List<Message>();

            for (int i = 0; i < QueueSize; i++)
            {
                FreeMessages.Push(new Message());
            }

            CheckTimer = new System.Timers.Timer();
            CheckTimer.Elapsed += new ElapsedEventHandler(OnElapsed_TimersTimer);

            //CheckTimer = new System.Threading.Timer(TimerCallback);

            Looper = new Task(loop);
        }

        public Message ObtainMessage()
        {
            Message msg = FreeMessages.Pop();
            msg.clean();
            return msg;
        }

        public void SendMessage(Message msg)
        {
            lock (LockObj)
            {
                Messages.Push(msg);
            }
        }

        private long GetCurrentMilliSec()
        {
            return DateTime.Now.Ticks / 10000;
        }


        public void SendMessage(Message msg, int delay)
        {
            //Console.WriteLine("SendMessage cnt=" + FreeMessages.Count.ToString());

            lock (LockObj)
            {
                msg.ExpireTime = GetCurrentMilliSec() + delay;
                DelayedMessages.Add(msg);
                UpdateTimer();
            }
        }

        public abstract void HandleMessage(Message msg);

        public void loop()
        {
            while (ContinueLoop)
            {
                Message msg = Messages.Pop();

                //Console.WriteLine("HandleMessage " + GetCurrentMilliSec().ToString());

                HandleMessage(msg);

                FreeMessages.Push(msg);
            }
        }

        public void stop()
        {
            ContinueLoop = false;
        }

        public void start()
        {
            ContinueLoop = true;
            Looper.Start();
        }

        private void TimerCallback(object state)
        {
            UpdateTimer();
        }

        void OnElapsed_TimersTimer(object sender, ElapsedEventArgs e)
        {
            CheckTimer.Stop();
            UpdateTimer();
        }

        private void UpdateTimer()
        {
            lock (LockObj)
            {
                long now = GetCurrentMilliSec();

                long minDt = long.MaxValue;

                if (DelayedMessages.Count > 0)
                {
                    foreach (Message msg in DelayedMessages)
                    {
                        if (msg.ExpireTime == 0)
                        {
                            continue;
                        }

                        long t = msg.ExpireTime - now;

                        if (t <= 0)
                        {
                            msg.ExpireTime = 0;
                            Messages.Push(msg);
                        }
                        else
                        {
                            if (t < minDt)
                            {
                                minDt = t;
                            }
                        }
                    }
                }
                DelayedMessages.RemoveAll(m => m.ExpireTime == 0);

                if (minDt != long.MaxValue)
                {
                    //CheckTimer.Change(minDt, Timeout.Infinite);

                    CheckTimer.Stop();
                    CheckTimer.Interval = minDt;
                    CheckTimer.Start();
                }
            }
        }

        public void RemoveAll(Predicate<Message> match)
        {
            lock (LockObj)
            {
                Messages.RemoveAll(match, Removed);

                for (int i = DelayedMessages.Count - 1; i >= 0; i--)
                {
                    Message item = DelayedMessages[i];

                    if (match(item))
                    {
                        DelayedMessages.RemoveAt(i);
                        Removed(item);
                    }
                }
            }
        }

        public void RemoveAll(int what)
        {
            RemoveAll(m => m.What == what);

        }

        private void Removed(Message msg)
        {
            FreeMessages.Push(msg);
        }
    }
}
