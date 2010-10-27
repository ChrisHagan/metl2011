using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SandRibbon.Utils;
using MeTLLib.DataTypes;

namespace SandRibbon.Providers
{
    public class ThumbnailProvider
    {
        public static ImageBrush get(Slide slide)
        {
            App.Now("Loading thumbnail for {0}", slide.id);
            return new ImageBrush(new BitmapImage(new Uri(
                string.Format("http://spacecaps.adm.monash.edu.au:8080/thumb/{0}?width={1}&height={2}",
                slide.id, 720, 540))));
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
            }
            catch (Exception e)
            {
                Logger.Log("Loaded cached image failed on " + uri);
            }
            return bi;
        }
        public static void ClearThumbnails()
        {
        }
    }
}