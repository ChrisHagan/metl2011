using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;

namespace MeTLLib.Providers
{
    class DateTimeFactory
    {
        private static CultureInfo enAUCulture = CultureInfo.GetCultureInfo("en-AU");
        private static CultureInfo enUSCulture = CultureInfo.GetCultureInfo("en-US");

        public static DateTime Parse(string s)
        {
            Thread.CurrentThread.CurrentCulture = enAUCulture;
            return DateTime.Parse(s);
        }

        /// <summary>
        /// Try to parse the s param as a DateTime except not throw an exception if the conversion failed.
        /// </summary>
        /// <param name="s">String containing a date and time to convert</param>
        /// <returns>The successfully parsed s as a DateTime, otherwise DateTimeFactory.Now()</returns>
        public static DateTime TryParse(string s)
        {
            Thread.CurrentThread.CurrentCulture = enAUCulture;
            
            var parsedDateTime = new DateTime();
            if (DateTime.TryParse(s, out parsedDateTime))
            {
                return parsedDateTime;
            }
            else
            {
                //MeTLX sends this format of date and hence the workaround
                const string dateFormat = "ddd MMM dd HH:mm:ss EST yyyy";
                if (DateTime.TryParseExact(s, dateFormat, enAUCulture, DateTimeStyles.None, out parsedDateTime))
                {
                    return parsedDateTime;
                }
                return Now();
            }
        }

        public static DateTime ParseFromTicks(string s)
        {
            Thread.CurrentThread.CurrentCulture = enAUCulture;
            return new DateTime(Convert.ToInt64(s));
        }
        public static DateTime Now()
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = enUSCulture;
                // I got a security permissions here once.  MSCorLib of course.
            }
            catch(Exception)
            {
            }
            return DateTime.Now;
        }
        public override string ToString()
        {
            Thread.CurrentThread.CurrentCulture = enAUCulture;
            return base.ToString();
        }
    }
}
