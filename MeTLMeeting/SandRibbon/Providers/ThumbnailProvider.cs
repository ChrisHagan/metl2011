using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SandRibbon.Utils;

namespace SandRibbon.Providers
{
    public class ThumbnailProvider
    {
        public static ImageBrush get(int id)
        {
            var directory = Directory.GetCurrentDirectory();
            var unknownSlidePath = directory + "\\Resources\\slide_Not_Loaded.png";
            var path = string.Format(@"{0}\thumbs\{1}\{2}.png", directory, Globals.me, id);
            ImageSource thumbnailSource;
            if (File.Exists(path))
                thumbnailSource = loadedCachedImage(path);
            else
                thumbnailSource = loadedCachedImage(unknownSlidePath);
            return new ImageBrush(thumbnailSource);
        }
        private static BitmapImage loadedCachedImage(string uri)
        {
            BitmapImage bi = new BitmapImage();
            try
            {
                bi.BeginInit();
                bi.UriSource = new Uri(uri);
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bi.EndInit();
                bi.Freeze();
            }
            catch (Exception e)
            {
                Logger.Log("Loaded cached image failed on "+uri);
            }
            return bi;
        }
    }
}