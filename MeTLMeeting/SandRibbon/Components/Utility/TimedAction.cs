using System;
using System.Collections.Generic;
using System.Timers;
using System.Collections;
using System.Diagnostics;

namespace SandRibbon.Components.Utility
{
    public abstract class TimedAction<T> where T : IEnumerable<Action>, new()
    {
        readonly object lockObj = new object();
        const int DEFAULT_SYNC_DELAY = 500;
        System.Timers.Timer worker;
        protected T timedActions = new T();

        void CreateTimer()
        {
            worker = new System.Timers.Timer();
            worker.Interval = DEFAULT_SYNC_DELAY;
            worker.AutoReset = true;
            worker.Elapsed += new ElapsedEventHandler(ExecuteTimedAction);
        }

        public TimedAction()
        {
            CreateTimer();
        }

        public void Shutdown()
        {
            worker.Stop();
        }
        private int numHits = 0;
        public void Add(Action timedAction)
        {
            Debug.WriteLine(string.Format("Adding timed action Hits{0}", ++numHits));
            lock (lockObj)
            {
                AddAction(timedAction);
                if (!worker.Enabled)
                {
                    worker.Start();
                }
            }
        }

        protected abstract void AddAction(Action timedAction);

        public void ChangeTimers(int delay)
        {
            worker.Interval = delay;
        }

        public void ResetTimer()
        {
            worker.Interval = DEFAULT_SYNC_DELAY;
        }

        protected abstract Action GetTimedAction();

        void ExecuteTimedAction(object sender, ElapsedEventArgs e)
        {
            Action timedAction = null;
            lock (lockObj)
            {
                timedAction = GetTimedAction();
            }

            if (timedAction == null)
                return;

            try
            {
                timedAction();
            }
            catch (Exception)
            {
            }
        }
    }
}
