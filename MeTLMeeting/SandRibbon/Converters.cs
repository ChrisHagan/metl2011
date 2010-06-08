using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SandRibbon.Providers;
using SandRibbonInterop;
using SandRibbonInterop.MeTLStanzas;
using SandRibbonObjects;
using System.Collections.Generic;
using System.Linq;
using SandRibbon.Components;
using System.Windows.Controls;

namespace SandRibbon
{
    public class Converters
    {
        public static milisecondsToTimeConverter milisecondsToTimeConverter = new milisecondsToTimeConverter();
        public static videoMediaElementToMediaElementConverter videoMediaElementToMediaElementConverter = new videoMediaElementToMediaElementConverter();
        public static videoTimeSpanToDoubleSecondsConverter videoTimeSpanToDoubleSecondsConverter = new videoTimeSpanToDoubleSecondsConverter();
        public static videoDurationToDoubleConverter videoDurationToDoubleConverter = new videoDurationToDoubleConverter();
        public static DebugConverter debugConverter = new DebugConverter();
        public static ConversationNameExtractor conversationNameExtractor = new ConversationNameExtractor();
        public static ConversationTooltipExtractor conversationTooltipExtractor = new ConversationTooltipExtractor();
        public static ServerStatusAsVisibility serverStatusToVisibility = new ServerStatusAsVisibility();
        public static ServerStatusAsBackgroundColor serverStatus = new ServerStatusAsBackgroundColor();
        public static ServerStatusAsString serverStatusString = new ServerStatusAsString();
        public static ServerStatusAsHealthColour serverStatusHealthColour = new ServerStatusAsHealthColour();
        public static HalfValueConverter half = new HalfValueConverter();
        public static Fraction fraction = new Fraction();
        public static ProgressAsColor progressColor = new ProgressAsColor();
        public static MultiplyConverter multiply = new MultiplyConverter();
        public static DivideConverter divide = new DivideConverter();
        public static RandomConverter random = new RandomConverter();
        public static StringToIntConverter parseInt = new StringToIntConverter();
        public static QuizPositionConverter quizPositionConverter = new QuizPositionConverter();
        public static ConversationDateConverter DateTimeConverter = new ConversationDateConverter();
        public static ColorToBrushConverter ColorToBrushConverter = new ColorToBrushConverter();
        public static BracketingConverter BracketingConverter = new BracketingConverter();
        public static ConvertStringToImageSource ConvertStringToImageSource = new ConvertStringToImageSource();
        public static ExtractUrlAndConvertConverter ExtractUrlAndConvertConverter = new ExtractUrlAndConvertConverter();
        public static IndexInThisCollectionConverter IndexInThisCollectionConverter = new IndexInThisCollectionConverter();
    }
    public class IndexInThisCollectionConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Any(v => v == DependencyProperty.UnsetValue)) return 0;
            var items = ((ItemsControl)values[1]).Items;
            var number = items.IndexOf(values[0]);
            return number;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("You shouldn't be converting an IndexInThisCollection back to anything");
        }
    }
    public class ExtractUrlAndConvertConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if(value == null) return value;
            var converter = new ConvertStringToImageSource();
            return converter.Convert(((TargettedSubmission)value).url, null,null, null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
    public class BracketingConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Format("({0})", value);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
    public class ConvertStringToImageSource: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var uri = new Uri(value.ToString(), UriKind.RelativeOrAbsolute);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = uri;
            bitmap.EndInit();
            return bitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
    public class ColorToBrushConverter : IValueConverter {
        private static readonly int INCREMENT = 20;
        private static Color modify(Func<byte,byte> action, Color color) {
            return new Color
            {
                A = color.A,
                R = action(color.R),
                G = action(color.G),
                B = action(color.B)
            };
        }
        private static Color highlightColor(Color color) { 
            return modify(i=>(byte)Math.Min(255,i+INCREMENT*4), color);
        }
        private static Color shadowColor(Color color) { 
            return modify(i=>(byte)Math.Max(0,i-INCREMENT), color);
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var baseColor = (Color)value;
            return new LinearGradientBrush
            {
                GradientStops = new GradientStopCollection {
                        new GradientStop(shadowColor(baseColor), 0), 
                        new GradientStop(baseColor, 0.2), 
                        new GradientStop(highlightColor(baseColor), 0.65)
                    },
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0)
            };
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
    public class RandomConverter : IValueConverter
    {
        static Random random = new Random();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null)
                return random.Next(Int32.Parse((string)parameter));
            return random.Next(5);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class DebugConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Console.WriteLine(value);
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    public class QuizPositionConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var list = ((ObservableCollection<QuizQuestion>)values[1]);
            return string.Format("Quiz: {0}", list.IndexOf((QuizQuestion)values[0]) + 1);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
    public class ConversationNameExtractor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value is ConversationDetails)
                return ((ConversationDetails)value).Title;
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    public class ConversationDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value is DateTime)
                return ((DateTime)value).ToString();
            else
                return DateTime.Now.ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    public class ConversationTooltipExtractor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value is ConversationDetails)
                return RecentConversationProvider.DisplayNameFor((ConversationDetails)value);
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    public class ServerStatusAsBackgroundColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                var brush = new RadialGradientBrush { Center = new Point(0.5, 0.5), RadiusX = 0.5, RadiusY = 0.5 };
                brush.GradientStops.Add(new GradientStop { Color = Colors.White, Offset = 0 });
                brush.GradientStops.Add(new GradientStop { Color = Colors.DarkGray, Offset = 0.8 });
                brush.GradientStops.Add(new GradientStop { Color = Colors.Black, Offset = 1 });
                return brush;
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ServerStatusAsHealthColour : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return Brushes.Green;
            }
            return Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ServerStatusAsVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ServerStatusAsString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return "Online";
            }
            return "Offline";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ProgressAsColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return Brushes.Green;
            }
            return Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class Fraction : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string)
                return ((double)value) / Double.Parse((string)parameter);
            return ((double)value) / (int)parameter;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class HalfValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values,
                              Type targetType,
                              object parameter,
                              CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                throw new ArgumentException(
                    "HalfValueConverter expects 2 double values to be passed" +
                    " in this order -> totalWidth, width",
                    "values");
            }

            double totalWidth = (double)values[0];
            double width = (double)values[1];
            return (object)((totalWidth - width) / 2);
        }
        public object[] ConvertBack(object value,
                                    Type[] targetTypes,
                                    object parameter,
                                    CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class MultiplyConverter : IMultiValueConverter
    {
        public object Convert(object[] values,
                              Type targetType,
                              object parameter,
                              CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                throw new ArgumentException("Multiplier expects 2 double values to be passed values");
            double numerator = (double)values[0];
            double denominator = (double)values[1];
            return numerator * denominator;
        }
        public object[] ConvertBack(object value,
                                    Type[] targetTypes,
                                    object parameter,
                                    CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class DivideConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                throw new ArgumentException("Divide expects 2 double values to be passed values");
            double numerator = System.Convert.ToDouble(values[0]);
            double denominator = System.Convert.ToDouble(values[1]);
            return numerator / denominator;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class TitleHydrator : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new ConversationDetails
            {
                Title = (string)values[0],
                Tag = (string)values[1],
                Subject = (string)values[2], // ST***
                Permissions = (Permissions)values[3]
            };
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class LabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.Format("{0} {1}", value, parameter);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class videoTimeSpanToDoubleSecondsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            var me = ((TimeSpan)value);
            return (double)(me.Hours * 3600) + (me.Minutes * 60) + (me.Seconds);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class videoDurationToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            var meDuration = ((Duration)value);
            TimeSpan me;
            if (meDuration.HasTimeSpan)
                me = meDuration.TimeSpan;
            else me = new TimeSpan(0,0,0);    
            return (double)(me.Hours * 3600) + (me.Minutes * 60) + (me.Seconds);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class milisecondsToTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            double Totalmilis = (double)value;
            int Seconds = System.Convert.ToInt32(Totalmilis / 1000);
            int Minutes = System.Convert.ToInt32(Seconds / 60);
            int Hours = System.Convert.ToInt32(Minutes / 60);
            return new TimeSpan(Hours,Minutes,Seconds).ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class videoMediaElementToMediaElementConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            return ((FrameworkElement)value);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class StringToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Int32.Parse((string)value);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}