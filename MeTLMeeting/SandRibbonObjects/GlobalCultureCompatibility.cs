using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SandRibbonObjects
{
    public class DateTimeFactory
    {
        private static System.Globalization.CultureInfo currentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-AU");
        public static System.DateTime DateTime()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
            return new System.DateTime();
        }

        public static System.DateTime Parse(string s)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
            return System.DateTime.Parse(s);
        }
        public static DateTime FromMilis(long milis)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(milis);
        }
        public static System.DateTime Now()
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
                // I got a security permissions here once.  MSCorLib of course.
            }
            catch(Exception)
            {
            }
            return System.DateTime.Now;
        }
        public override string ToString()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
            return base.ToString();
        }
    }
}
