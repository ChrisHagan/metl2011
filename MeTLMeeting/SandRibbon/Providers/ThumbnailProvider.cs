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

namespace SandRibbon.Providers
{
    public class ThumbnailProvider
    {
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
            App.Now("Loading thumbnail for {0}", slide.id);
            return new ImageBrush(new BitmapImage(new Uri(
                string.Format("http://spacecaps.adm.monash.edu.au:8080/thumb/{0}?width={1}&height={2}",
                slide.id, 720, 540))));
        }
    }
}