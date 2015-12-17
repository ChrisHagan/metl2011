using System;
using System.Timers;
using MeTLLib.Utilities;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Diagnostics;

namespace SandRibbon.Components.Utility
{
    class ChangeSlideTimedAction : TimedAction<Stack<Action>>
    {
        protected override void AddAction(Action timedAction)
        {
            timedActions.Push(timedAction);
        }

        protected override Action GetTimedAction()
        {
            if (timedActions.Count == 0)
                return null;

            var timedAction = timedActions.Pop();
            // always want the latest, and disregard the old requests
            timedActions.Clear();

            return timedAction;
        }
    }

    public static class GlobalTimers
    {
        private static ChangeSlideTimedAction timedActions;

        static GlobalTimers()
        {
            timedActions = new ChangeSlideTimedAction();
            Commands.ShuttingDown.RegisterCommand(new DelegateCommand<object>((_unused) => ShutdownTimers()));
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