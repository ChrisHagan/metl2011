using System;
using System.Timers;
using MeTLLib.Utilities;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Diagnostics;

namespace SandRibbon.Components.Utility
{
    class TimedAction
    {
        readonly object lockObj = new object();
        const int SYNC_DELAY = 500;
        System.Timers.Timer worker;
        Stack<Action> timedActions = new Stack<Action>();

        public TimedAction()
        {
            worker = new System.Timers.Timer();
            worker.Interval = SYNC_DELAY;
            worker.AutoReset = true;
            worker.Elapsed += new ElapsedEventHandler(ExecuteTimedAction);
        }

        public void Shutdown()
        {
            worker.Stop();
        }

        public void Add(Action timedAction)
        {
            lock (lockObj)
            {
                timedActions.Push(timedAction);   
                if (!worker.Enabled)
                    worker.Start();
            }
        }
        public void ChangeTimers(int delay)
        {
            worker.Interval = delay;
        }

        public void ResetTimer()
        {
            worker.Interval = SYNC_DELAY;
        }

        void ExecuteTimedAction(object sender, ElapsedEventArgs e)
        {
            Action timedAction = null;
            lock (lockObj)
            {
                if (timedActions.Count == 0)
                    return;
                timedAction = timedActions.Pop();
                // always want the latest, and disregard the old requests
                timedActions.Clear();
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

    public static class GlobalTimers
    {
        private static TimedAction timedActions;

        static GlobalTimers()
        {
            timedActions = new TimedAction();
            Commands.LeaveAllRooms.RegisterCommand(new DelegateCommand<object>((_unused) => ShutdownTimers()));
        }

        static void ShutdownTimers()
        {
            if (timedActions!= null)
            {
                timedActions.Shutdown();
            }
        }

        public static void ExecuteSync()
        {
            timedActions.ChangeTimers(30);
        }
        
        public static void ResetSyncTimer()
        {
            timedActions.ResetTimer();
        }

        public static void SetSyncTimer(Action action)
        {
            timedActions.Add(action);
        }
    }
}