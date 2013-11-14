using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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
            return getUserFile(suffix, "");
        }
        private static string ensureDirectories(string basePath,string[] path){
            return path.Aggregate(basePath, (acc, item) =>
            {
                var currentItem = acc + "\\" + item;
                if (!Directory.Exists(currentItem))
                {
                    Directory.CreateDirectory(currentItem);
                }
                return currentItem;
            });
        }
        public static string getUserFile(string[] path, string filename)
        {
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var autoAdd = new string[]{"MonashMeTL"};
            var finalPath = ensureDirectories(basePath,autoAdd.Concat(path).ToArray()) + "\\" + filename;
            return finalPath;
        }
    }
}
