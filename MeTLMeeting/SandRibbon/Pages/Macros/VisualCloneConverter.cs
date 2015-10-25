using SandRibbon.Pages.Collaboration.Palettes;
using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace SandRibbon.Pages.Macros
{
    public class IconControl : ContentControl
    {
        private static VisualCloneConverter clone = new VisualCloneConverter();
        private static DynamicResourceConverter resource = new DynamicResourceConverter();
        static IconControl()
        {
            ContentControl.ContentProperty.OverrideMetadata(typeof(IconControl), new FrameworkPropertyMetadata(
                null,
                null,
                new CoerceValueCallback((d, baseValue) =>
                {
                    return clone.Convert(resource.Convert(baseValue,null,null, null), null, null, null);
                })));
        }
    }
    public class VisualCloneConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visual = value as Visual;
            object clone;
            using (var stream = new MemoryStream())
            {
                XamlWriter.Save(visual, stream);
                stream.Seek(0, SeekOrigin.Begin);
                clone = XamlReader.Load(stream);
            }
            return clone as Visual;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
