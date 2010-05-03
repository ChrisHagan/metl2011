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
        public static System.DateTime Now()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
            return System.DateTime.Now;
        }
        public string ToString()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
            return base.ToString();
        }
    }
}
