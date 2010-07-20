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
            var serverPath = string.Format(@"https://{0}:1188/Resource/{1}/thumbs", Constants.JabberWire.SERVER, id);
            var serverFile = string.Format(@"{0}/{1}.png", serverPath, id);
            ImageSource thumbnailSource;
            if (HttpResourceProvider.exists(serverFile))
                thumbnailSource = loadedCachedImage(serverFile);
            else
            {
                if (File.Exists(path))
                {
                    thumbnailSource = loadedCachedImage(path);
                    if (Globals.me == Globals.slides.Where(s => s.id == id).First().author)
                        SandRibbon.Utils.Connection.ResourceUploader.uploadResourceToPath(path, id.ToString() + "/thumbs", "slideThumb.png");
                }
                else
                    thumbnailSource = loadedCachedImage(unknownSlidePath);
            }
            App.Now("Thumbnail created for " + id);
            var imageBrush = new ImageBrush(thumbnailSource);
            return imageBrush;
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
                Logger.Log("Loaded cached image failed on " + uri);
            }
            return bi;
        }
    }
}