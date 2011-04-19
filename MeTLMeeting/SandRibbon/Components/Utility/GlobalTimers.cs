using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SandRibbon.Providers;
using SandRibbon.Utils;

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
                                              catch (Exception e)
                                              {

                                              }
                                              SyncTimer = null;
                                          },null, 500, Timeout.Infinite );
        }
        public static int getSlide()
        {
            return currentSlide;
        }
        private static object resetLocker = new object();
        public static void resetSyncTimer()
        {
            if(SyncTimer != null)
            {
                lock(resetLocker)
                    SyncTimer.Change(500, Timeout.Infinite);
            }
        }

    }
}
