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
            /*
            var directory = Directory.GetCurrentDirectory();
            var unknownSlidePath = directory + "\\Resources\\slide_Not_Loaded.png";
            var path = string.Format(@"{0}\thumbs\{1}\{2}.png", directory, Globals.me, slide.id);
            var serverPath = string.Format(@"https://{0}:1188/Resource{1}/thumbs", Constants.JabberWire.SERVER, slide.id);
            var serverFile = string.Format(@"{0}/{1}.png", serverPath, slide.id);
            ImageSource thumbnailSource;
            if (File.Exists(path))
                thumbnailSource = loadedCachedImage(path);
            else
            {
                if (HttpResourceProvider.exists(serverFile))
                    thumbnailSource = loadedCachedImage(serverFile);
                else
                    thumbnailSource = loadedCachedImage(unknownSlidePath);
            }
            var imageBrush = new ImageBrush(thumbnailSource);
            return imageBrush;
             */
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