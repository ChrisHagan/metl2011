using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MeTLLib.Providers.Connection;
using MeTLLib.DataTypes;
using System.Windows.Data;
using MeTLLib;
using SandRibbon.Utils;
using Size = System.Windows.Size;

namespace SandRibbon.Providers
{
    public class ThumbnailProvider
    {
        public static void getConversationThumbnails(string jid)
        {
            var details = MeTLLib.ClientFactory.Connection().DetailsOf(jid);
            foreach (var slide in details.Slides)
                getSlideThumbnail(details.Jid, slide.id);
        }
        public static void getSlideThumbnails(string jid, int[] ids)
        {
            foreach(var id in ids)
                getSlideThumbnail(jid, id);
        }
        public static void getSlideThumbnail(string jid, int id)
        {
            var host = ClientFactory.Connection().server.host.Split('.').First();
            var url = string.Format("http://radar.adm.monash.edu:9000/application/snapshot?server={0}&slide={1}", host, id);
            var sourceBytes = new WebClient {Credentials = new NetworkCredential("exampleUsername", "examplePassword")}.DownloadData(url);
            File.WriteAllBytes(PowerPointLoader.getThumbnailPath(jid, id), sourceBytes); 
        }

        public static SlideToThumbConverter SlideToThumb = new SlideToThumbConverter();
        public class SlideToThumbConverter : IValueConverter {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                var val = get((Slide)value);
                return val;
            }
            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return value;
            }
        }
        public static ImageBrush get(Slide slide)
        {
            var bitmap = new BitmapImage();
            try
            {
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(PowerPointLoader.getThumbnailPath(Globals.location.activeConversation, slide.id));
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.EndInit();
                bitmap.Freeze();
            }
            catch (Exception)
            {
                App.Now("Error in loading a thumbnail. boourns");
            }
            var image = new ImageBrush(bitmap);
            return image;
        }
        
    }
}