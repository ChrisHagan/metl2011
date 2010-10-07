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
        public static bool comparedCollection<T>(List<T> collection1, List<T> collection2)
        {
            var results = new Dictionary<int, KeyValuePair<bool, KeyValuePair<T, T>>>();
            for (int a = 0; a < collection1.Count; a++)
            {
                if (collection1[a].Equals(collection2[a]))
                    results.Add(a, new KeyValuePair<bool, KeyValuePair<T, T>>(true, new KeyValuePair<T, T>(collection1[a], collection2[a])));
                else
                    results.Add(a, new KeyValuePair<bool, KeyValuePair<T, T>>(true, new KeyValuePair<T, T>(collection1[a], collection2[a])));
            }
            return !(results.Any(s => s.Value.Key == false));
        }
    }
}
