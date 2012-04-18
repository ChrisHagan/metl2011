using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace MeTLLib
{
    public static class XElementExtensions
    {
        public static string ValueOrDefault(this XElement xelement, string defaultValue)
        {
            if (xelement == null)
            {
                return defaultValue;
            }

            return xelement.Value;
        }
    }
}
