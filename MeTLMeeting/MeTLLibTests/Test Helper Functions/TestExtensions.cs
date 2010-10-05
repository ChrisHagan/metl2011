using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeTLLibTests
{
    public class TestExtensions
    {
        public static TimeSpan ConditionallyDelayFor(int timeout, bool condition)
        {
            bool hasFinished = false;
            DateTime start = DateTime.Now;
            var TimeDifference = new TimeSpan();
            var t = new System.Threading.Thread(new System.Threading.ThreadStart(() =>
            {
                while (!hasFinished)
                {
                    if (start.AddMilliseconds(timeout) < DateTime.Now || condition)
                    {
                        TimeDifference = DateTime.Now - start;
                        hasFinished = true;
                    }
                }
            }));
            t.Start();
            t.Join();
            return TimeDifference;
        }
    }
}
