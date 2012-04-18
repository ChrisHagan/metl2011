using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeTLLib.Providers
{
    class DateTimeFactory
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

        /// <summary>
        /// Try to parse the s param as a DateTime except not throw an exception if the conversion failed.
        /// </summary>
        /// <param name="s">String containing a date and time to convert</param>
        /// <returns>The successfully parsed s as a DateTime, otherwise DateTimeFactory.Now()</returns>
        public static System.DateTime TryParse(string s)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
            var parsedDateTime = new DateTime();
            if (System.DateTime.TryParse(s, out parsedDateTime))
            {
                return parsedDateTime;
            }

            return Now();
        }

        public static System.DateTime ParseFromTicks(string s)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
            return new DateTime(Convert.ToInt64(s));
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
