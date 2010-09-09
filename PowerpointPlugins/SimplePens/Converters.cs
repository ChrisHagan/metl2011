using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace PowerpointJabber
{
    class Converters
    {
        public static BoolToVisibilityConverter boolToVisibilityConverter = new BoolToVisibilityConverter();
        public static ReverseBoolToVisibilityConverter reverseBoolToVisibilityConverter = new ReverseBoolToVisibilityConverter();

        public class BoolToVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return (bool)value ? Visibility.Visible : Visibility.Collapsed;
            }
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
        public class ReverseBoolToVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if ((bool)value)
                {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if ((Visibility)value == Visibility.Visible)
                {
                    return false;
                }
                return true;
            }
        }
    
    }
}
