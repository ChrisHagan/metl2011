using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SandRibbon.Utils
{
    class LocalFileProvider
    {
        public static string getUserFolder(string suffix)
        {
            return getUserFolder(new string[] { suffix });
        }
        public static string getUserFolder(string[] suffix)
        {
            var tmp = "\\";
            foreach (string s in suffix.ToList())
            {
                tmp += s;
                tmp += "\\";
            }
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MonashMeTL" + tmp;
        }
    }
}
