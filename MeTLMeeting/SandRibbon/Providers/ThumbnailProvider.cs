using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SandRibbon.Utils;
using MeTLLib.DataTypes;
using System.Windows.Data;
using MeTLLib;
using System.Net;
using System.Net.Cache;

namespace SandRibbon.Providers
{
    public class ThumbnailProvider
    {
        private static RequestCachePolicy bitmapRetrievePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
        public static SlideToThumbConverter SlideToThumb = new SlideToThumbConverter();
        public class SlideToThumbConverter : IValueConverter {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                var val = ThumbnailProvider.get((Slide)value);
                return val;
            }
            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return value;
            }
        }
        public static ImageBrush get(Slide slide)
        {
            var path = string.Format("http://spacecaps.adm.monash.edu.au:8080/?slide={0}&width={1}&height={2}&server={3}&invalidate={4}",
                slide.id, 180, 135, ClientFactory.Connection().server.host.Split('.').First(), Globals.isAuthor.ToString().ToLower());
            App.Now("Loading thumbnail for {0} at {1}", slide.id, path);
            return new ImageBrush(new BitmapImage(new Uri(path), bitmapRetrievePolicy));
        }
    }
}