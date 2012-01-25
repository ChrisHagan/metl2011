using System;
using System.Threading;
using MeTLLib.Utilities;

namespace SandRibbon.Components.Utility
{
    public static class GlobalTimers
    {
        public static Timer SyncTimer = null;
        private static Action currentAction;
        private static int currentSlide = 0;
        private static object locker = new object();
        public static void SetSyncTimer(Action timedAction, int slide)
        {
            using (DdMonitor.Lock(locker))
            {
                syncActive = true;
                currentSlide = slide;
                currentAction = timedAction;
            }
            if(SyncTimer == null)
            {
                SyncTimer = new Timer(delegate
                                          {
                                              try
                                              {
                                                  using (DdMonitor.Lock(locker))
                                                  {
                                                      if (currentAction != null)
                                                          currentAction();
                                                      syncActive = false;
                                                      currentSlide = 0;
                                                  }
                                              }
                                              catch (Exception)
                                              {
                                              }
                                              finally
                                              {
                                                  SyncTimer = null;
                                              }
                                          },null, 500, Timeout.Infinite );}
        }
        public static void stopTimer()
        {
            if (1 == Interlocked.Increment(ref syncTimerChangeCounter))
            {
                if (SyncTimer != null)
                {
                    SyncTimer.Dispose();
                    SyncTimer = null;
                }
                Interlocked.Exchange(ref syncTimerChangeCounter, 0);
            }
        }
        public static void ExecuteSync()
        {
            using(DdMonitor.Lock(locker))
            {
                if(syncActive)
                {
                    currentAction();
                    syncActive = false;
                }
            }
        }
        public static int getSlide()
        {
            return currentSlide;
        }
        private static int syncTimerChangeCounter = 0;
        private static bool syncActive;
        public static void resetSyncTimer()
        {
            if (1 == Interlocked.Increment(ref syncTimerChangeCounter))
            {
                if (SyncTimer != null)
                {
                    SyncTimer.Change(500, Timeout.Infinite);
                }
                Interlocked.Exchange(ref syncTimerChangeCounter, 0);
            }
        }

    }
}
