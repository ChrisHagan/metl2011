using MeTLLib.DataTypes;
using SandRibbon.Components;
using System;
using System.Globalization;
using System.Windows.Data;

namespace SandRibbon.Pages.Conversations.Views
{
    public class ThumbnailSourceConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var controller = values[0] as NetworkController;
            var slide = values[1] as Slide;
            return controller.config.thumbnailUri(slide.id.ToString());
        }
        
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
