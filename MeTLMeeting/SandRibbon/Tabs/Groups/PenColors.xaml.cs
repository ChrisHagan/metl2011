using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Practices.Composite.Presentation.Commands;
using Microsoft.Win32;
using SandRibbon;
using SandRibbon.Components.Interfaces;
using SandRibbonInterop.MeTLStanzas;
using System.ComponentModel;
using Divelements.SandRibbon;

namespace SandRibbon.Tabs.Groups
{
    public class CurrentColourValues : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        //Private Obscured fields
        private Color colour;
        private double Hue;
        private double Saturation;
        private double Value;
        private string Hex;
        private int Red;
        private int Green;
        private int Blue;
        private int Alpha;
        private double PenSize;
        private bool isHighlighter;

        private bool internalUpdating = false;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        //public accessor fields
        private DrawingAttributes currentdrawingattributes;
        public DrawingAttributes currentDrawingAttributes
        {
            get { return currentdrawingattributes; }
            set
            {
                currentdrawingattributes = value;
                Commands.SetDrawingAttributes.Execute(value);
                internalUpdating = true;
                currentColor = value.Color;
                CurrentR = value.Color.R;
                CurrentG = value.Color.G;
                CurrentB = value.Color.B;
                CurrentA = value.Color.A;
                updateHSV();
                CurrentHex = value.Color.ToString();
                CurrentPenSize = value.Height;
                CurrentlyIsHighlighter = value.IsHighlighter;
                internalUpdating = false;
            }
        }
        public Color currentColor
        {
            get { return colour; }
            set
            {
                if (colour != value)
                {
                    colour = value;
                    if (!internalUpdating)
                    {
                        var newDrawingAttributes = currentdrawingattributes.Clone();
                        newDrawingAttributes.Color = value;
                        currentDrawingAttributes = newDrawingAttributes;
                    }
                }
            }
        }
        public string CurrentHex
        {
            get { return Hex; }
            set
            {
                if (Hex != value)
                {
                    Hex = value;
                    OnPropertyChanged("CurrentHex");
                }
            }
        }
        public double CurrentPenSize
        {
            get { return PenSize; }
            set
            {
                if (PenSize != value)
                {
                    PenSize = value;
                    OnPropertyChanged("CurrentPenSize");
                    if (!internalUpdating)
                    {
                        var newDrawingAttributes = currentdrawingattributes.Clone();
                        newDrawingAttributes.Height = value;
                        newDrawingAttributes.Width = value;
                        currentDrawingAttributes = newDrawingAttributes;
                    }
                }
            }
        }
        public bool CurrentlyIsHighlighter
        {
            get { return isHighlighter; }
            set
            {
                if (isHighlighter != value)
                {
                    isHighlighter = value;
                    OnPropertyChanged("CurrentlyIsHighlighter");
                    if (!internalUpdating)
                    {
                        var newDrawingAttributes = currentdrawingattributes.Clone();
                        newDrawingAttributes.IsHighlighter = value;
                        currentDrawingAttributes = newDrawingAttributes;
                    }
                }
            }
        }

        public double CurrentH
        {
            get { return Hue; }
            set
            {
                if (Hue != value)
                {
                    Hue = value;
                    OnPropertyChanged("CurrentH");
                    if (!internalUpdating)
                    {
                        updateRGB();
                    }
                }
            }
        }
        public double CurrentS
        {
            get { return Saturation; }
            set
            {
                if (Saturation != value)
                {
                    Saturation = value;
                    OnPropertyChanged("CurrentS");
                    if (!internalUpdating)
                    {
                        updateRGB();
                    }
                }
            }
        }
        public double CurrentV
        {
            get
            {
                return Value;
            }
            set
            {
                if (Value != value)
                {
                    Value = value;
                    OnPropertyChanged("CurrentV");
                    if (!internalUpdating)
                    {
                        updateRGB();
                    }
                }
            }
        }

        public int CurrentR
        {
            get
            {
                return Red;
            }
            set
            {
                if (Red != value)
                {
                    Red = value;
                    OnPropertyChanged("CurrentR");
                    OnPropertyChanged("CurrentlyIsHighlighter");
                    if (!internalUpdating)
                    {
                        var newDrawingAttributes = currentdrawingattributes.Clone();
                        var newColor = newDrawingAttributes.Color;
                        newColor.R = System.Convert.ToByte(value);
                        newDrawingAttributes.Color = newColor;
                        currentDrawingAttributes = newDrawingAttributes;
                    }
                }
            }
        }
        public int CurrentG
        {
            get { return Green; }
            set
            {
                if (Green != value)
                {
                    Green = value;
                    OnPropertyChanged("CurrentG");
                    if (!internalUpdating)
                    {
                        var newDrawingAttributes = currentdrawingattributes.Clone();
                        var newColor = newDrawingAttributes.Color;
                        newColor.G = System.Convert.ToByte(value);
                        newDrawingAttributes.Color = newColor;
                        currentDrawingAttributes = newDrawingAttributes;
                    }
                }
            }
        }
        public int CurrentB
        {
            get { return Blue; }
            set
            {
                if (Blue != value)
                {
                    Blue = value;
                    OnPropertyChanged("CurrentB");
                    if (!internalUpdating)
                    {
                        var newDrawingAttributes = currentdrawingattributes.Clone();
                        var newColor = newDrawingAttributes.Color;
                        newColor.B = System.Convert.ToByte(value);
                        newDrawingAttributes.Color = newColor;
                        currentDrawingAttributes = newDrawingAttributes;
                    }
                }
            }
        }
        public int CurrentA
        {
            get { return Alpha; }
            set
            {
                if (Alpha != value)
                {
                    Alpha = value;
                    OnPropertyChanged("CurrentA");
                    if (!internalUpdating)
                    {
                        var newDrawingAttributes = currentdrawingattributes.Clone();
                        var newColor = newDrawingAttributes.Color;
                        newColor.A = System.Convert.ToByte(value);
                        newDrawingAttributes.Color = newColor;
                        currentDrawingAttributes = newDrawingAttributes;
                    }
                }
            }
        }
        private void updateRGB()
        {
            int tempR;
            int tempG;
            int tempB;
            new ColourTools().HsvToRgb(CurrentH, CurrentS, CurrentV, out tempR, out tempG, out tempB);
            CurrentR = tempR;
            CurrentG = tempG;
            CurrentB = tempB;
        }
        private void updateHSV()
        {
            double tempH;
            double tempS;
            double tempV;
            new ColourTools().RgbToHsv(CurrentR, CurrentG, CurrentB, out tempH, out tempS, out tempV);
            CurrentH = tempH;
            CurrentS = tempS;
            CurrentV = tempV;
        }
    }
    public partial class PenColors : RibbonGroup, IPencilCaseDisplay
    {
        public static Half halfOfParent = new Half();
        public ObservableCollection<DrawingAttributesEntry> previouslySelectedDrawingAttributes = new ObservableCollection<DrawingAttributesEntry>();
        private int nextAvailableSpot;
        public string preferredTab = "PenTools";
        public DelegateCommand<TargettedStroke> colorChanged;
        public bool ShouldNotUpdateHSV;
        public bool ShouldNotUpdateRGB;
        public CurrentColourValues currentColourValues = new CurrentColourValues();


        public PenColors()
        {
            InitializeComponent();

            this.DataContext = currentColourValues;
            PenSizeSlider.Value = 1;
            nextAvailableSpot = 0;
            previousColors.ItemsSource = previouslySelectedDrawingAttributes;
            colorChanged = new DelegateCommand<TargettedStroke>((targettedStroke) => updatePreviousDrawingAttributes(targettedStroke.stroke.DrawingAttributes));
            Commands.LoggedIn.RegisterCommand(new DelegateCommand<object>((_obj => Enable())));
            Commands.SendStroke.RegisterCommand(colorChanged);
            Commands.ReportDrawingAttributes.RegisterCommand(new DelegateCommand<DrawingAttributes>((drawingAttributes => receiveDrawingAttributesChanged(drawingAttributes))));
            SetupPreviousColoursWithDefaults();
            Commands.SetLayer.RegisterCommand(new DelegateCommand<string>(updateToolBox));
        }
        private void updateToolBox(string layer)
        {
            if (layer == "Sketch")
                Visibility = Visibility.Visible;
            else
                Visibility = Visibility.Collapsed;
        }
        private void SetupPreviousColoursWithDefaults()
        {
            currentColourValues.currentDrawingAttributes = new DrawingAttributes()
                                                               {
                                                                   Color = Colors.Black,
                                                                   IsHighlighter = false,
                                                                   Height = 1,
                                                                   Width = 1
                                                               };
            updatePreviousDrawingAttributes(new DrawingAttributes()
                                                {
                                                    Color = Colors.Red,
                                                    IsHighlighter = false,
                                                    Height = 2,
                                                    Width = 2
                                                });
            updatePreviousDrawingAttributes(new DrawingAttributes()
            {
                Color = Colors.Black,
                IsHighlighter = false,
                Height = 1,
                Width = 1
            });
            updatePreviousDrawingAttributes(new DrawingAttributes()
            {
                Color = Colors.Blue,
                IsHighlighter = false,
                Height = 3,
                Width = 3
            });
            updatePreviousDrawingAttributes(new DrawingAttributes()
            {
                Color = Colors.Yellow,
                IsHighlighter = true,
                Height = 25,
                Width = 25
            });
        }
        private void receiveDrawingAttributesChanged(DrawingAttributes arrivingDrawingAttributes)
        {
            if (currentColourValues.currentDrawingAttributes != arrivingDrawingAttributes)
                currentColourValues.currentDrawingAttributes = arrivingDrawingAttributes;
        }
        private void SetPenSizeByButton(object sender, RoutedEventArgs e)
        {
            var newPenSize = ((SandRibbonInterop.Button)sender).Tag.ToString();
            currentColourValues.CurrentPenSize = Double.Parse(newPenSize);
        }
        private void SwitchImage(object sender, RoutedEventArgs e)
        {
            var newImageChoice = ((SandRibbonInterop.Button)sender).Tag.ToString();
            Uri uri;
            switch (newImageChoice)
            {
                case "Custom":
                    var openDialogue = new OpenFileDialog();
                    openDialogue.Title = "Select Picture for Colour Chooser";
                    openDialogue.Filter = "Portable Network Graphic Files (*.png)|*.png|JPEG Files (*.jpg)|*.jpg|Bitmap Files (*.bmp)|*.bmp|All files (*.*)|*.*";
                    openDialogue.CheckFileExists = true;
                    openDialogue.FilterIndex = 2;
                    openDialogue.Multiselect = false;
                    openDialogue.RestoreDirectory = true;
                    openDialogue.ShowDialog();
                    while (openDialogue.FileName == null)
                    {
                    }
                    if (openDialogue.FileName == null || openDialogue.FileName == "")
                        return;
                    uri = new Uri(openDialogue.FileName);
                    break;
                case "ColourBars":
                    uri = new Uri("http://drawkward.adm.monash.edu/ColorBars.png", UriKind.Absolute);
                    break;
                case "Spectrum":
                    uri = new Uri("http://drawkward.adm.monash.edu/Spectrum.png", UriKind.Absolute);
                    break;
                default:
                    return;
            }
            try
            {
                var bi = new BitmapImage(uri);
                ImageSource imgSource = bi;
                ColourPicker.Source = imgSource;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Import Picture: " + ex);
            }
        }
        private void setCurrentColour()
        {
            /*var newColor = new Color();
            newColor.R = System.Convert.ToByte(CurrentR);
            newColor.G = System.Convert.ToByte(CurrentG);
            newColor.A = System.Convert.ToByte(CurrentA);
            newColor.B = System.Convert.ToByte(CurrentB);
            currentColor = newColor;
        */
        }
        private void updatePreviousDrawingAttributes(DrawingAttributes attributes)
        {
            int nextAvailableSpot = 0;
            if (previouslySelectedDrawingAttributes.Select(c => c.Attributes).Contains(attributes)) return;
            const int maxSizeOfQuickLaunchColors = 4;
            if (previouslySelectedDrawingAttributes.Count >= maxSizeOfQuickLaunchColors)
            {
                previouslySelectedDrawingAttributes.RemoveAt(maxSizeOfQuickLaunchColors - 1);
                nextAvailableSpot = (nextAvailableSpot) % maxSizeOfQuickLaunchColors;
            }
            previouslySelectedDrawingAttributes.Insert(nextAvailableSpot,
                new DrawingAttributesEntry()
                    {
                        Attributes = attributes,
                        ColorName = attributes.Color.ToString() + ":" + attributes.Height.ToString() + ":" + attributes.IsHighlighter.ToString(),
                        ColorValue = attributes.Color,
                        XAMLColorName = attributes.Color.ToString(),
                        IsHighlighter = attributes.IsHighlighter,
                        PenSize = attributes.Width
                    });
        }
        public void Disable()
        {
            Dispatcher.Invoke((Action)delegate
                                           {
                                               this.PopupOpenButton.IsEnabled = false;
                                               this.previousColors.IsEnabled = false;
                                               this.Effect = new BlurEffect();
                                           });
        }
        public void Enable()
        {
            Dispatcher.Invoke((Action)delegate
                                           {
                                               this.PopupOpenButton.IsEnabled = true;
                                               this.previousColors.IsEnabled = true;
                                               this.Effect = null;
                                           });
        }
        private void ChangeColor(object sender, RoutedEventArgs e)
        {
            var colorName = ((SandRibbonInterop.Button)sender).Tag.ToString();
            var drawingAttributes = (DrawingAttributes)previouslySelectedDrawingAttributes.Where(c => c.ColorName == colorName).Select(c => c.Attributes).Single();
            Commands.SetDrawingAttributes.Execute(drawingAttributes);
            e.Handled = true;
        }
        private void ClosePopup(object sender, RoutedEventArgs e)
        {
            RichInkOptionsPopup.IsOpen = false;
        }
        private void OpenPopup(object sender, RoutedEventArgs e)
        {
            RichInkOptionsPopup.IsOpen = true;
        }
        private void StickyPopup(object sender, RoutedEventArgs e)
        {
            if
            (RichInkOptionsPopup.StaysOpen)
            {
                RichInkOptionsPopup.StaysOpen = false;
                PinIn.Visibility = Visibility.Collapsed;
                PinOut.Visibility = Visibility.Visible;
            }
            else
            {
                RichInkOptionsPopup.StaysOpen = true;
                PinIn.Visibility = Visibility.Visible;
                PinOut.Visibility = Visibility.Collapsed;
            }
        }
        private void ColourPickerMouseUp(object sender, MouseEventArgs e)
        {
            var HPos = ((e.GetPosition(ColourPicker).X) / (ColourPicker.ActualWidth)) * ((BitmapSource)(ColourPicker.Source)).PixelWidth;
            var VPos = ((e.GetPosition(ColourPicker).Y) / (ColourPicker.ActualHeight)) * ((BitmapSource)(ColourPicker.Source)).PixelHeight;
            currentColourValues.currentColor = PickColor(HPos, VPos);
        }
        private void ColourPickerMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var HPos = ((e.GetPosition(ColourPicker).X) / (ColourPicker.ActualWidth)) * ((BitmapSource)(ColourPicker.Source)).PixelWidth;
                var VPos = ((e.GetPosition(ColourPicker).Y) / (ColourPicker.ActualHeight)) * ((BitmapSource)(ColourPicker.Source)).PixelHeight;
                currentColourValues.currentColor = PickColor(HPos, VPos);
            }
        }
        private void ColourPickerMouseDown(object sender, MouseEventArgs e)
        {
            var HPos = ((e.GetPosition(ColourPicker).X) / (ColourPicker.ActualWidth)) * ((BitmapSource)(ColourPicker.Source)).PixelWidth;
            var VPos = ((e.GetPosition(ColourPicker).Y) / (ColourPicker.ActualHeight)) * ((BitmapSource)(ColourPicker.Source)).PixelHeight;
            currentColourValues.currentColor = PickColor(HPos, VPos);
        }
        private Color PickColor(double x, double y)
        {
            BitmapSource bitmapSource = ColourPicker.Source as BitmapSource;
            if (bitmapSource != null)
            {
                var cb = new CroppedBitmap(bitmapSource,
                                                     new Int32Rect((int)Convert.ToInt32(x),
                                                                   (int)Convert.ToInt32(y), 1, 1));
                var pixels = new byte[4];
                try
                {
                    cb.CopyPixels(pixels, 4, 0);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                return Color.FromRgb(pixels[2], pixels[1], pixels[0]);
            }
            var emptyColor = new Color();
            emptyColor.A = 255;
            emptyColor.R = 0;
            emptyColor.G = 0;
            emptyColor.B = 0;
            return emptyColor;
        }

        public class DrawingAttributesEntry
        {
            private bool internalupdate = false;
            private DrawingAttributes attributes;
            public DrawingAttributes Attributes
            {
                get { return attributes; }
                set
                {
                    attributes = value;
                    internalupdate = true;
                    IsHighlighter = value.IsHighlighter;
                    PenSize = value.Height;
                    ColorValue = value.Color;
                    XAMLColorName = value.Color.ToString();
                    ColorName = value.Color.ToString() + ":" + value.Height.ToString() + ":" + value.IsHighlighter.ToString();
                    internalupdate = false;
                }
            }
            private string colorname;
            public string ColorName
            {
                get { return colorname; }
                set
                {
                    colorname = value;
                    if (internalupdate)
                        return;
                    internalupdate = true;
                    attributes = new DrawingAttributes()
                                     {
                                         Color = ColorValue,
                                         Height = PenSize,
                                         IsHighlighter = ishighlighter,
                                         Width = PenSize
                                     };
                    internalupdate = false;
                }
            }
            private string xamlcolorname;
            public string XAMLColorName { get; set; }
            private Color colorvalue { get; set; }
            public Color ColorValue
            {
                get { return colorvalue; }
                set
                {
                    colorvalue = value;
                    if (internalupdate)
                        return;
                    internalupdate = true;
                    attributes = new DrawingAttributes()
                    {
                        Color = value,
                        Height = PenSize,
                        IsHighlighter = ishighlighter,
                        Width = PenSize
                    };
                    internalupdate = false;
                }
            }
            private double pensize { get; set; }
            public double PenSize
            {
                get { return pensize; }
                set
                {
                    pensize = value;
                    if (internalupdate)
                        return;
                    internalupdate = true;
                    attributes = new DrawingAttributes()
                    {
                        Color = ColorValue,
                        Height = value,
                        IsHighlighter = ishighlighter,
                        Width = value
                    };
                    internalupdate = false;
                }
            }
            private bool ishighlighter;
            public bool IsHighlighter
            {
                get { return ishighlighter; }
                set
                {
                    ishighlighter = value;
                    if (internalupdate)
                        return;
                    internalupdate = true;
                    attributes = new DrawingAttributes()
                    {
                        Color = ColorValue,
                        Height = PenSize,
                        IsHighlighter = value,
                        Width = PenSize
                    };
                    internalupdate = false;
                }
            }
        }

        private void SetHighlighter(object sender, RoutedEventArgs e)
        {
            if (((ToggleButton)sender).IsChecked == true)
                Commands.SetHighlighterMode.Execute(true);
            else Commands.SetHighlighterMode.Execute(false);
        }
    }
    public class ColourTools
    {
        public void HsvToRgb(double h, double S, double V, out int r, out int g, out int b)
        {
            double H = h;
            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };
            double R, G, B;
            if (V <= 0)
            { R = G = B = 0; }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {
                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;
                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;
                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;
                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;
                    default:
                        R = G = B = V;
                        break;
                }
            }
            r = Clamp((int)(R * 255.0));
            g = Clamp((int)(G * 255.0));
            b = Clamp((int)(B * 255.0));
        }
        int Clamp(int i)
        {
            if (i < 0) return 0;
            if (i > 255) return 255;
            return i;
        }
        public void RgbToHsv(int R, int G, int B, out double h, out double s, out double v)
        {
            double max = R;
            var dominantColour = "Red";
            if (G > R)
            {
                max = G;
                dominantColour = "Green";
            }
            if (B > R && B > G)
            {
                max = B;
                dominantColour = "Blue";
            }
            if (max == 0)
            {
                h = 0;
                s = 0;
                v = 1;
                return;
            }
            double min = R;
            if (G < R)
                min = G;
            if (B < R && B < G)
                min = B;
            if (min == 255)
            {
                h = 0;
                s = 0;
                v = 0;
                return;
            }
            double delta = max - min;
            double Hue;
            if (delta == 0)
                Hue = 0;
            else Hue = 0 + (((G - B) / delta) * 60);
            if (dominantColour == "Green")
                if (delta == 0)
                    Hue = 120;
                else Hue = 120 + (((B - R) / delta) * 60);
            if (dominantColour == "Blue")
                if (delta == 0)
                    Hue = 240;
                else Hue = 240 + (((R - G) / delta) * 60);
            while (Hue > 360)
                Hue = Hue - 360;
            while (Hue < 0)
                Hue = Hue + 360;
            h = Hue;
            if (((double)(delta) / max) < 0)
                s = 0;
            else if ((double)(delta / max) > 1)
                s = 1;
            else
                s = (double)(delta / max);
            if ((double)(max / 255) < 0)
                v = 0;
            else if ((double)(max / 255) > 1)
                v = 1;
            else
                v = (double)(max / 255);
        }
    }
    public class HexToColourConverter : IValueConverter
    {
        public ColorConverter bc;
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                var newColor = (Brush)(new BrushConverter().ConvertFromString(value.ToString()));
                return newColor;
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((Brush)value).ToString();
        }
    }
    public class HueSliderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                if ((double)value < 360 && (double)value > 0)
                    return (double)value;
                else if ((double)value > 360)
                    return 360;
                else return 0;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((double)value != null && (double)value < 360 && (double)value > 0)
            {
                return (double)value;
            }
            return 0;
        }
    }
    public class DoubleSliderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                if ((double)value < 1 && (double)value > 0)
                    return value;
                else if ((double)value > 1)
                    return 1;
                else return 0;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((double)value != null && (double)value < 256 && (double)value > 0)
            {
                return (double)value;
            }
            return 0;
        }

    }
    public class RoundingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                if ((int)value > 255)
                    return 255;
                else if ((int)value < 0)
                    return 0;
                else return value;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((double)value != null && (double)value < 256 && (double)value > 0)
            {
                double dblValue = System.Convert.ToDouble(value);
                return (int)dblValue;
            }
            return (double)0;
        }

    }
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((Visibility)value == Visibility.Visible)
            {
                return true;
            }
            return false;
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
    public class HueTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && (double)value < 360 && (double)value > 0)
            {
                return value.ToString();
            }
            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                double dblValue = System.Convert.ToDouble((string)value);
                if (dblValue < 360 && dblValue > 0)
                    return dblValue;
                else if (dblValue > 360)
                    return 360;
                else return 0;
            }
            return 0;
        }
    }
    public class AttributesToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return "";
            var attributes = (PenColors.DrawingAttributesEntry)value;
            var Pen = attributes.IsHighlighter? "highlighter":"pen";
            var size = Math.Round(attributes.PenSize, 1);
            return string.Format("A {0}, {1} {2} wide, of colour {3}.", 
                Pen,
                size.ToString(),
                size > 1?"points":"point",
                attributes.XAMLColorName);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                double dblValue = System.Convert.ToDouble((string)value);
                if (dblValue < 1 && dblValue > 0)
                    return dblValue;
                else if (dblValue > 1)
                    return 1;
                else return 0;
            }
            return 0;
        }
    }
    public class DoubleTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && (double)value < 256 && (double)value > 0)
            {
                return value.ToString();
            }
            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                double dblValue = System.Convert.ToDouble((string)value);
                if (dblValue < 1 && dblValue > 0)
                    return dblValue;
                else if (dblValue > 1)
                    return 1;
                else return 0;
            }
            return 0;
        }

    }
    public class IntTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && (int)value < 256 && (int)value > 0)
            {
                return value.ToString();
            }
            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                int dblValue = System.Convert.ToInt32((string)value);
                if (dblValue < 256 && dblValue > 0)
                    return dblValue;
                else if (dblValue > 255)
                    return 255;
                else return 0;
            }
            return 0;
        }

    }
    public class Half : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Console.WriteLine(value.GetType().Name);
            return ((Double)value) / 2;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
