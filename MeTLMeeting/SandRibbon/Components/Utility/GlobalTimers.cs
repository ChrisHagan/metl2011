using System;
using System.Threading;

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
            lock (locker)
            {
                currentSlide = slide;
                currentAction = timedAction;
            }
            if(SyncTimer == null)
                SyncTimer = new Timer(delegate
                                          {
                                              try
                                              {
                                                  lock (locker)
                                                  {
                                                      if(currentAction != null)
                                                          currentAction();
                                                      currentSlide = 0;
                                                  }
                                              }
                                              catch (Exception)
                                              {

                                              }
                                              SyncTimer = null;
                                          },null, 500, Timeout.Infinite );
        }
        public static int getSlide()
        {
            return currentSlide;
        }
        private static int syncTimerChangeCounter = 0;
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
