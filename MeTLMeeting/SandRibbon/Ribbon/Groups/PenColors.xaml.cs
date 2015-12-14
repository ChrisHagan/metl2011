using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Interfaces;
using System.ComponentModel;
using SandRibbon.Providers;
using System.Diagnostics;
using SandRibbon.Utils;

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
                if (currentDrawingAttributes != null && currentDrawingAttributes.Color != value.Color)
                {
                    currentdrawingattributes = value;
                    Commands.SetDrawingAttributes.ExecuteAsync(value);
                }
                else currentdrawingattributes = value;
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

    public enum PenMode
    {
        Draw,
        Select,
        Erase
    }

    public class PenColorsUIState
    {
        public DrawingAttributes CurrentDrawingAttributes;
        public CurrentColourValues CurrentColorValues;
        public PenMode CurrentPenMode;
        public int CurrentSelectedColor;
        public int CurrentSelectedPen;
        public int CurrentSelectedSize;
        public string CurrentChosenColor;
    }

    public partial class PenColors : UserControl, IPencilCaseDisplay
    {
        public static Half halfOfParent = new Half();
        public ObservableCollection<PenColors.DrawingAttributesEntry> previouslySelectedDrawingAttributes = new ObservableCollection<PenColors.DrawingAttributesEntry>();
        public string preferredTab = "PenTools";
        public bool ShouldNotUpdateHSV;
        public bool ShouldNotUpdateRGB;
        public DrawingAttributes currentAttributes;

        private CurrentColourValues _currentColourValues;
        public CurrentColourValues currentColourValues
        {
            get
            {
                if (_currentColourValues == null)
                {
                    _currentColourValues = new CurrentColourValues();
                }
                return _currentColourValues;
            }
        }

        private Brush[] _simpleColourSet;
        public Brush[] simpleColourSet
        {
            get
            {
                if (_simpleColourSet == null)
                {
                    _simpleColourSet = new Brush[]
                    {
                        new SolidColorBrush(Colors.White), new SolidColorBrush(Colors.LightPink),new SolidColorBrush(Colors.PaleGreen),
                        new SolidColorBrush(Colors.Cyan), new SolidColorBrush(Colors.PaleVioletRed), new SolidColorBrush(Colors.LightYellow),
                        new SolidColorBrush(Colors.LightGray), new SolidColorBrush(Colors.Pink), new SolidColorBrush(Colors.LightGreen),
                        new SolidColorBrush(Colors.LightBlue), new SolidColorBrush(Colors.Violet), new SolidColorBrush(Colors.Yellow),
                        new SolidColorBrush(Colors.DarkGray), new SolidColorBrush(Colors.Red), new SolidColorBrush(Colors.Green),
                        new SolidColorBrush(Colors.Blue), new SolidColorBrush(Colors.Purple), new SolidColorBrush(Colors.Orange),
                        new SolidColorBrush(Colors.Black), new SolidColorBrush(Colors.DarkRed), new SolidColorBrush(Colors.DarkGreen),
                        new SolidColorBrush(Colors.DarkBlue), new SolidColorBrush(Colors.Maroon), new SolidColorBrush(Colors.OrangeRed)
                    };
                }
                return _simpleColourSet;
            }
            set
            {
                _simpleColourSet = value;
            }
        }

        private double[] _simpleSizeSet;
        public double[] simpleSizeSet
        {
            get
            {
                if (_simpleSizeSet == null)
                {
                    _simpleSizeSet = new double[]
                    {
                        1,2,3,5,10,25
                    };
                }
                return _simpleSizeSet;
            }
        }

        private DrawingAttributes[] defaultDrawingAttributes = new DrawingAttributes[] {
            new DrawingAttributes{Color = Colors.Black, IsHighlighter = false, Height = 1,Width = 1},
            new DrawingAttributes{Color = Colors.Red, IsHighlighter = false, Height = 2,Width = 2},
            new DrawingAttributes{Color = Colors.Blue, IsHighlighter = false, Height = 2,Width = 2},
            new DrawingAttributes{Color = Colors.Green, IsHighlighter = false, Height = 3,Width = 3},
            new DrawingAttributes{Color = Colors.Cyan, IsHighlighter = true, Height = 25,Width = 25},
            new DrawingAttributes{Color = Colors.Yellow, IsHighlighter = true, Height = 25,Width = 25}
        };
        private DrawingAttributes[] usefulDrawingAttributes = new DrawingAttributes[] {
            new DrawingAttributes{Color = Colors.Black, IsHighlighter = false, Height = 1,Width = 1},
            new DrawingAttributes{Color = Colors.Red, IsHighlighter = false, Height = 1,Width = 1},
            new DrawingAttributes{Color = Colors.Blue, IsHighlighter = false, Height = 1,Width = 1},
            new DrawingAttributes{Color = Colors.Cyan, IsHighlighter = true, Height = 25,Width = 25},
            new DrawingAttributes{Color = Colors.Yellow, IsHighlighter = true, Height = 25,Width = 25}
        };

        public PenColors()
        {
            InitializeComponent();

            //this.DataContext = currentColourValues;
            this.DataContext = this;
            SetupPreviousColoursWithDefaults();
            Commands.SetInkCanvasMode.RegisterCommand(new DelegateCommand<string>(SetInkCanvasMode));
            Commands.SetLayer.RegisterCommand(new DelegateCommand<string>(SetLayer));
            Commands.JoiningConversation.RegisterCommand(new DelegateCommand<object>(JoinConversation));
            Commands.SetDrawingAttributes.RegisterCommand(new DelegateCommand<object>(SetDrawingAttributes));

            Commands.SaveUIState.RegisterCommand(new DelegateCommand<object>(SaveUIState));
            Commands.RestoreUIState.RegisterCommand(new DelegateCommand<object>(RestoreUIState));

            InvokeAlteredPreset(2);
        }

        private void SaveUIState(object parameter)
        {
            Dispatcher.adopt(delegate
            {

                // save selected pen, size and color   
                var saveState = new PenColorsUIState();

                saveState.CurrentDrawingAttributes = currentAttributes;
                saveState.CurrentColorValues = currentColourValues;

                var penMode = PenMode.Draw;
                if (drawRadio.IsChecked ?? false)
                    penMode = PenMode.Draw;
                if (selectRadio.IsChecked ?? false)
                    penMode = PenMode.Select;
                if (eraseRadio.IsChecked ?? false)
                    penMode = PenMode.Erase;

                saveState.CurrentPenMode = penMode;
                saveState.CurrentSelectedColor = ColourChooser.SelectedIndex;
                saveState.CurrentSelectedSize = SizeChooser.SelectedIndex;
                saveState.CurrentSelectedPen = defaultColours.SelectedIndex;
                saveState.CurrentChosenColor = ColourSettingPopup.Tag as string;

                Globals.StoredUIState.PenColorsUIState = saveState;
            });
        }

        private void RestoreUIState(object parameter)
        {
            Dispatcher.adopt(delegate
            {

                // restore saved state
                var saveState = Globals.StoredUIState.PenColorsUIState;

                var penMode = "Ink";
                if (saveState != null)
                {
                    currentAttributes = saveState.CurrentDrawingAttributes;
                    _currentColourValues = saveState.CurrentColorValues;

                    switch (saveState.CurrentPenMode)
                    {
                        case PenMode.Draw:
                            penMode = "Ink";
                            drawRadio.IsChecked = true;
                            break;

                        case PenMode.Select:
                            penMode = "Select";
                            selectRadio.IsChecked = true;
                            break;

                        case PenMode.Erase:
                            penMode = "EraseByStroke";
                            eraseRadio.IsChecked = true;
                            break;

                        default:
                            penMode = "Ink";
                            drawRadio.IsChecked = true;
                            break;
                    }

                    ColourChooser.SelectedIndex = saveState.CurrentSelectedColor;
                    SizeChooser.SelectedIndex = saveState.CurrentSelectedSize;
                    defaultColours.SelectedIndex = saveState.CurrentSelectedPen;
                    ColourSettingPopup.Tag = saveState.CurrentChosenColor;

                    //ChangeColour(ColourChooser, null);
                    //ChangeColorFromPreset(defaultColours, null);

                    Commands.SetInkCanvasMode.ExecuteAsync(penMode);
                    Commands.SetDrawingAttributes.ExecuteAsync(currentAttributes);
                }
            });
        }

        private void checkDraw()
        {
            Dispatcher.adopt(delegate
            {
                drawRadio.IsChecked = true;
            });
            Commands.SetInkCanvasMode.ExecuteAsync("Ink");
        }
        private void JoinConversation(object obj)
        {
            checkDraw();
        }
        private void SetDrawingAttributes(object obj)
        {
            checkDraw();
        }
        private void SetInkCanvasMode(string mode)
        {
            if (mode != "Ink")
            {
                Dispatcher.adopt(delegate
                {

                    defaultColours.SelectedIndex = -1;
                });
            }
        }
        private void SetLayer(string layer)
        {
            if (layer == "Sketch")
            {
                Commands.SetDrawingAttributes.ExecuteAsync(currentAttributes);
            }
            else
                Dispatcher.adopt(delegate
                {

                    Visibility = Visibility.Collapsed;
                });
        }
        private void updatePreviousDrawingAttributes(DrawingAttributes attributes)
        {
            int nextAvailableSpot = 0;
            if (previouslySelectedDrawingAttributes.Select(c => c.Attributes).ToList().Contains(attributes)) return;
            previouslySelectedDrawingAttributes.Insert(nextAvailableSpot,
                new PenColors.DrawingAttributesEntry()
                {
                    Attributes = attributes,
                    ColorName = attributes.Color.ToString() + ":" + attributes.Height.ToString() + ":" + attributes.IsHighlighter.ToString(),
                    ColorValue = attributes.Color,
                    XAMLColorName = attributes.Color.ToString(),
                    IsHighlighter = attributes.IsHighlighter,
                    PenSize = attributes.Width
                });
        }
        private void SetupPreviousColoursWithDefaults()
        {
            int i = 0;
            foreach (DrawingAttributes color in defaultDrawingAttributes)
            {
                updatePreviousDrawingAttributes(color);
                defaultColours.Items.Add(new DrawingAttributesEntry
                {
                    Attributes = color,
                    ColorName = color.Color.ToString() + ":" + color.Height.ToString() + ":" + color.IsHighlighter.ToString(),
                    ColorValue = color.Color,
                    XAMLColorName = color.Color.ToString(),
                    IsHighlighter = color.IsHighlighter,
                    PenSize = color.Width,
                    Index = i
                });
                i++;
            }
            foreach (DrawingAttributes color in usefulDrawingAttributes)
            {
                updatePreviousDrawingAttributes(color);
            }
            currentColourValues.currentDrawingAttributes = defaultDrawingAttributes[0];
        }
        /*private void receiveDrawingAttributesChanged(DrawingAttributes arrivingDrawingAttributes)
        {
            if (currentColourValues.currentDrawingAttributes != arrivingDrawingAttributes)
                currentColourValues.currentDrawingAttributes = arrivingDrawingAttributes;
        }*/
        public void SetPens(bool enabled)
        {
            if (enabled)
                Enable();
            else
                Disable();
        }
        public void Disable()
        {
            Dispatcher.adopt((Action)delegate
                                           {
                                               this.Effect = new BlurEffect();
                                           });
        }
        public void Enable()
        {
            Dispatcher.adopt((Action)delegate
                                           {
                                               this.Effect = null;
                                           });
        }
        private void ChangeColorFromPreset(object sender, SelectionChangedEventArgs e)
        {
            var listBox = ((ListBox)sender);
            if (listBox.SelectedItem != null)
            {
                var IndexNumber = listBox.Items.IndexOf(listBox.SelectedItem);
                var drawingAttributes = (DrawingAttributes)(((DrawingAttributesEntry)(defaultColours.Items[IndexNumber])).Attributes);
                currentAttributes = drawingAttributes;
                Commands.SetDrawingAttributes.ExecuteAsync(drawingAttributes);
                var msg = String.Format("Pen selected, Pen {0}, Colour {1}, Size {2}, isHighlighter {3}", IndexNumber.ToString(), drawingAttributes.Color.ToString(), drawingAttributes.Height.ToString(), drawingAttributes.IsHighlighter.ToString());
                Trace.TraceInformation(msg);
                e.Handled = true;
            }
        }
        private bool OpeningPopup;

        private void OpenColourSettingPopup(object sender, RoutedEventArgs e)
        {
            Trace.TraceInformation("OpenedCustomColorDialog");
            var newBrush = new SolidColorBrush();
            var AttributeNumber = Int32.Parse(((System.Windows.Controls.Button)sender).Tag.ToString());
            InvokeAlteredPreset(AttributeNumber);
            ColourSettingPopup.Tag = AttributeNumber.ToString();
            var Attributes = ((DrawingAttributes)defaultDrawingAttributes[AttributeNumber]);
            var PopupAttributes = defaultColours.Items[AttributeNumber];
            newBrush.Color = Attributes.Color;
            ColourSettingPopupDefaultColour.Fill = newBrush;
            ColourSettingPopupDefaultSize.Height = Attributes.Height;
            ColourSettingPopup.IsOpen = true;
            //ColourChooser.ItemsSource = simpleColourSet;
            //SizeChooser.ItemsSource = simpleSizeSet;
            OpeningPopup = true;
            foreach (double item in SizeChooser.Items)
            {
                if (item == ((DrawingAttributesEntry)PopupAttributes).Attributes.Height)
                {
                    SizeChooser.SelectedItem = item;
                }
            }
            foreach (SolidColorBrush item in ColourChooser.Items)
            {
                if (item.Color.ToString() == ((DrawingAttributesEntry)PopupAttributes).Attributes.Color.ToString())
                {
                    ColourChooser.SelectedItem = item;
                }
            }
            OpeningPopup = false;
            Trace.TraceInformation("Pen modifier popup open, Pen {0}", AttributeNumber.ToString());
        }
        private void ChangeColour(object sender, RoutedEventArgs e)
        {
            if (OpeningPopup) return;
            var Brush = ((Brush)((ListBox)sender).SelectedItem).ToString();
            var Color = (Color)ColorConverter.ConvertFromString(Brush);
            var PresetToUpdate = Int32.Parse(ColourSettingPopup.Tag.ToString());
            ((DrawingAttributesEntry)defaultColours.Items[PresetToUpdate]).ColorValue = Color;
            ((DrawingAttributesEntry)defaultColours.Items[PresetToUpdate]).XAMLColorName = Color.ToString();
            defaultColours.Items.Refresh();
            InvokeAlteredPreset(PresetToUpdate);
            Trace.TraceInformation("Pen changed colour, Pen {0}, newColour {1}", PresetToUpdate.ToString(), Color.ToString());
        }
        private void InvokeAlteredPreset(int index)
        {
            defaultColours.SelectedItem = defaultColours.Items[index];
            var drawingAttributes = (DrawingAttributes)(((DrawingAttributesEntry)(defaultColours.Items[index])).Attributes);
            currentAttributes = drawingAttributes;
            Commands.SetDrawingAttributes.ExecuteAsync(drawingAttributes);
            ColourSettingPopup.IsOpen = false;
        }
        private void ResetToDefault(object sender, RoutedEventArgs e)
        {
            var PresetToUpdate = Int32.Parse(ColourSettingPopup.Tag.ToString());
            var DrawingAttributes = ((DrawingAttributes)defaultDrawingAttributes[PresetToUpdate]);
            ((DrawingAttributesEntry)(defaultColours.Items[PresetToUpdate])).Attributes = DrawingAttributes;
            defaultColours.Items.Refresh();
            InvokeAlteredPreset(PresetToUpdate);
            Trace.TraceInformation("Pen Reset to Default, Pen {0}", PresetToUpdate.ToString());
        }
        private void ChangeSize(object sender, RoutedEventArgs e)
        {
            if (OpeningPopup) return;
            var newSize = ((double)((ListBox)sender).SelectedItem);
            var PresetToUpdate = Int32.Parse(ColourSettingPopup.Tag.ToString());
            ((DrawingAttributesEntry)defaultColours.Items[PresetToUpdate]).PenSize = newSize;
            defaultColours.Items.Refresh();
            InvokeAlteredPreset(PresetToUpdate);
            Trace.TraceInformation("Pen changed size, Pen {0}, newSize {1}", PresetToUpdate.ToString(), newSize.ToString());
        }
        private void SizeUp(object sender, MouseButtonEventArgs e)
        {
            var CurrentPreset = Int32.Parse(ColourSettingPopup.Tag.ToString());
            InvokeAlteredPreset(CurrentPreset);
            Trace.TraceInformation("Pen SizeUp, Pen {0}", CurrentPreset.ToString());
        }
        private void ColourUp(object sender, MouseButtonEventArgs e)
        {
            var CurrentPreset = Int32.Parse(ColourSettingPopup.Tag.ToString());
            InvokeAlteredPreset(CurrentPreset);
            Trace.TraceInformation("Pen ColourUp, Pen {0}", CurrentPreset.ToString());
        }

        public class DrawingAttributesEntry
        {
            public int Index { get; set; }
            private bool internalupdate = false;
            private DrawingAttributes attributes;

            public StrokeCollection DrawnPenPreviewStroke
            {
                get
                {
                    return new StrokeCollection(new[]{new Stroke(
new StylusPointCollection(
new StylusPoint[]{new StylusPoint(30.6666666666667,90,0.5f),new StylusPoint(32.6666666666667,91.3333333333333,0.5f),new StylusPoint(33.6666666666667,91.6666666666667,0.5f),new StylusPoint(35,92,0.5f),new StylusPoint(35.6666666666667,92.3333333333333,0.5f),new StylusPoint(36.3333333333333,92.6666666666667,0.5f),new StylusPoint(37.3333333333333,93,0.5f),new StylusPoint(38,93.3333333333333,0.5f),new StylusPoint(39,93.6666666666667,0.5f),new StylusPoint(40.3333333333333,94,0.5f),new StylusPoint(41.3333333333333,94.3333333333333,0.5f),new StylusPoint(42.6666666666667,94.3333333333333,0.5f),new StylusPoint(43.6666666666667,94.6666666666667,0.5f),new StylusPoint(45.3333333333333,95,0.5f),new StylusPoint(46.6666666666667,95.3333333333333,0.5f),new StylusPoint(48,95.3333333333333,0.5f),new StylusPoint(49.3333333333333,95.3333333333333,0.5f),new StylusPoint(51,95.6666666666667,0.5f),new StylusPoint(52.6666666666667,95.6666666666667,0.5f),new StylusPoint(54,95.3333333333333,0.5f),new StylusPoint(55.6666666666667,95.3333333333333,0.5f),new StylusPoint(57.3333333333333,95,0.5f),new StylusPoint(59,94.6666666666667,0.5f),new StylusPoint(60.6666666666667,94.3333333333333,0.5f),new StylusPoint(62.3333333333333,94,0.5f),new StylusPoint(64.3333333333333,93.3333333333333,0.5f),new StylusPoint(65.6666666666667,93,0.5f),new StylusPoint(67.3333333333333,92.3333333333333,0.5f),new StylusPoint(69,91.6666666666667,0.5f),new StylusPoint(70.6666666666667,91,0.5f),new StylusPoint(72,90.3333333333333,0.5f),new StylusPoint(73.6666666666667,89.3333333333333,0.5f),new StylusPoint(75,88.6666666666667,0.5f),new StylusPoint(76.3333333333333,87.6666666666667,0.5f),new StylusPoint(77.3333333333333,86.6666666666667,0.5f),new StylusPoint(78.6666666666667,85.6666666666667,0.5f),new StylusPoint(79.6666666666667,84.6666666666667,0.5f),new StylusPoint(80.6666666666667,83.6666666666667,0.5f),new StylusPoint(81.6666666666667,82.3333333333333,0.5f),new StylusPoint(82.6666666666667,81,0.5f),new StylusPoint(83.3333333333333,80,0.5f),new StylusPoint(84,78.6666666666667,0.5f),new StylusPoint(84.3333333333333,77.3333333333333,0.5f),new StylusPoint(85,76,0.5f),new StylusPoint(85.3333333333333,74.6666666666667,0.5f),new StylusPoint(85.6666666666667,73,0.5f),new StylusPoint(86,71.6666666666667,0.5f),new StylusPoint(86,70.3333333333333,0.5f),new StylusPoint(86,69,0.5f),new StylusPoint(86,67.6666666666667,0.5f),new StylusPoint(85.6666666666667,66.3333333333333,0.5f),new StylusPoint(85.6666666666667,65,0.5f),new StylusPoint(85.3333333333333,63.6666666666667,0.5f),new StylusPoint(85,62.3333333333333,0.5f),new StylusPoint(84.3333333333333,61,0.5f),new StylusPoint(83.6666666666667,59.6666666666667,0.5f),new StylusPoint(83,58.6666666666667,0.5f),new StylusPoint(82.3333333333333,57.3333333333333,0.5f),new StylusPoint(81.6666666666667,56,0.5f),new StylusPoint(80.6666666666667,55,0.5f),new StylusPoint(79.6666666666667,53.6666666666667,0.5f),new StylusPoint(78.6666666666667,52.6666666666667,0.5f),new StylusPoint(77.3333333333333,51.3333333333333,0.5f),new StylusPoint(76,50.3333333333333,0.5f),new StylusPoint(74.6666666666667,49.3333333333333,0.5f),new StylusPoint(73.3333333333333,48.3333333333333,0.5f),new StylusPoint(71.6666666666667,47.3333333333333,0.5f),new StylusPoint(70,46.3333333333333,0.5f),new StylusPoint(68.3333333333333,45.6666666666667,0.5f),new StylusPoint(66.6666666666667,45,0.5f),new StylusPoint(65,44.3333333333333,0.5f),new StylusPoint(63,43.6666666666667,0.5f),new StylusPoint(61.3333333333333,43.3333333333333,0.5f),new StylusPoint(59.3333333333333,43,0.5f),new StylusPoint(57.3333333333333,42.6666666666667,0.5f),new StylusPoint(55.3333333333333,42.3333333333333,0.5f),new StylusPoint(53.3333333333333,42.3333333333333,0.5f),new StylusPoint(51,42,0.5f),new StylusPoint(49,42.3333333333333,0.5f),new StylusPoint(46.6666666666667,42.3333333333333,0.5f),new StylusPoint(44.3333333333333,42.6666666666667,0.5f),new StylusPoint(42.3333333333333,43,0.5f),new StylusPoint(40,43.6666666666667,0.5f),new StylusPoint(37.6666666666667,44.3333333333333,0.5f),new StylusPoint(35.3333333333333,45,0.5f),new StylusPoint(33,46,0.5f),new StylusPoint(30.6666666666667,47.3333333333333,0.5f),new StylusPoint(28.3333333333333,48.6666666666667,0.5f),new StylusPoint(26,50,0.5f),new StylusPoint(24,51.6666666666667,0.5f),new StylusPoint(21.6666666666667,53.6666666666667,0.5f),new StylusPoint(19.3333333333333,55.3333333333333,0.5f),new StylusPoint(17,57.3333333333333,0.5f),}),attributes)});
                }
            }

            public StrokeCollection DrawnHighlighterPreviewStroke
            {
                get
                {
                    return new StrokeCollection(new[]{new Stroke(
new StylusPointCollection(
new StylusPoint[]{new StylusPoint(17.6666666666667,86,0.5f),new StylusPoint(18,87.3333333333333,0.5f),new StylusPoint(18,87.6666666666667,0.5f),new StylusPoint(18.3333333333333,87.6666666666667,0.5f),new StylusPoint(18.6666666666667,87.6666666666667,0.5f),new StylusPoint(19.3333333333333,88.3333333333333,0.5f),new StylusPoint(19.6666666666667,88.3333333333333,0.5f),new StylusPoint(20,88.6666666666667,0.5f),new StylusPoint(20.3333333333333,89,0.5f),new StylusPoint(21,89.3333333333333,0.5f),new StylusPoint(21.6666666666667,89.6666666666667,0.5f),new StylusPoint(22.3333333333333,90,0.5f),new StylusPoint(23,90.6666666666667,0.5f),new StylusPoint(23.6666666666667,91,0.5f),new StylusPoint(24.6666666666667,91.3333333333333,0.5f),new StylusPoint(25.6666666666667,91.6666666666667,0.5f),new StylusPoint(26.6666666666667,92,0.5f),new StylusPoint(27.6666666666667,92.6666666666667,0.5f),new StylusPoint(28.6666666666667,93,0.5f),new StylusPoint(30,93.6666666666667,0.5f),new StylusPoint(31.3333333333333,94,0.5f),new StylusPoint(32.3333333333333,94.6666666666667,0.5f),new StylusPoint(33.6666666666667,95,0.5f),new StylusPoint(35.3333333333333,95.6666666666667,0.5f),new StylusPoint(36.6666666666667,96,0.5f),new StylusPoint(38.3333333333333,96.3333333333333,0.5f),new StylusPoint(39.6666666666667,96.6666666666667,0.5f),new StylusPoint(41.3333333333333,97,0.5f),new StylusPoint(43,97.3333333333333,0.5f),new StylusPoint(44.6666666666667,97.6666666666667,0.5f),new StylusPoint(46.6666666666667,97.6666666666667,0.5f),new StylusPoint(48.3333333333333,97.6666666666667,0.5f),new StylusPoint(50.3333333333333,98,0.5f),new StylusPoint(52,98,0.5f),new StylusPoint(54,97.6666666666667,0.5f),new StylusPoint(56,97.6666666666667,0.5f),new StylusPoint(57.6666666666667,97.3333333333333,0.5f),new StylusPoint(59.6666666666667,97,0.5f),new StylusPoint(61.3333333333333,96.6666666666667,0.5f),new StylusPoint(69,94.3333333333333,0.5f),new StylusPoint(70.6666666666667,93.3333333333333,0.5f),new StylusPoint(74,91.6666666666667,0.5f),new StylusPoint(75.6666666666667,90.6666666666667,0.5f),new StylusPoint(77,89.3333333333333,0.5f),new StylusPoint(78.3333333333333,88,0.5f),new StylusPoint(79.6666666666667,86.6666666666667,0.5f),new StylusPoint(81,85.3333333333333,0.5f),new StylusPoint(82,84,0.5f),new StylusPoint(83.3333333333333,82.3333333333333,0.5f),new StylusPoint(84,80.6666666666667,0.5f),new StylusPoint(85,79,0.5f),new StylusPoint(85.6666666666667,77.3333333333333,0.5f),new StylusPoint(86.3333333333333,75.6666666666667,0.5f),new StylusPoint(87,73.6666666666667,0.5f),new StylusPoint(87.3333333333333,72,0.5f),new StylusPoint(87.3333333333333,70,0.5f),new StylusPoint(87.3333333333333,68,0.5f),new StylusPoint(87.3333333333333,66,0.5f),new StylusPoint(87.3333333333333,64.3333333333333,0.5f),new StylusPoint(86.6666666666667,62.3333333333333,0.5f),new StylusPoint(86.3333333333333,60.3333333333333,0.5f),new StylusPoint(85.6666666666667,58.6666666666667,0.5f),new StylusPoint(85,57,0.5f),new StylusPoint(84,55.3333333333333,0.5f),new StylusPoint(83,53.6666666666667,0.5f),new StylusPoint(82,52,0.5f),new StylusPoint(81,50.6666666666667,0.5f),new StylusPoint(79.6666666666667,49,0.5f),new StylusPoint(78.3333333333333,47.6666666666667,0.5f),new StylusPoint(76.6666666666667,46.3333333333333,0.5f),new StylusPoint(75,45,0.5f),new StylusPoint(73.3333333333333,43.6666666666667,0.5f),new StylusPoint(71.6666666666667,42.3333333333333,0.5f),new StylusPoint(69.6666666666667,41.3333333333333,0.5f),new StylusPoint(67.6666666666667,40.3333333333333,0.5f),new StylusPoint(65.6666666666667,39.3333333333333,0.5f),new StylusPoint(63.6666666666667,38.6666666666667,0.5f),new StylusPoint(61.6666666666667,37.6666666666667,0.5f),new StylusPoint(59.6666666666667,37,0.5f),new StylusPoint(57.3333333333333,36.6666666666667,0.5f),new StylusPoint(55.3333333333333,36,0.5f),new StylusPoint(53,35.6666666666667,0.5f),new StylusPoint(51,35.3333333333333,0.5f),new StylusPoint(48.6666666666667,35.3333333333333,0.5f),new StylusPoint(46.6666666666667,35,0.5f),new StylusPoint(44.3333333333333,35,0.5f),new StylusPoint(42.3333333333333,35,0.5f),new StylusPoint(40,35,0.5f),new StylusPoint(38,35.3333333333333,0.5f),new StylusPoint(35.6666666666667,35.6666666666667,0.5f),new StylusPoint(33.6666666666667,36,0.5f),new StylusPoint(31.3333333333333,36.6666666666667,0.5f),new StylusPoint(29.3333333333333,37.3333333333333,0.5f),new StylusPoint(27.3333333333333,38,0.5f),new StylusPoint(25.3333333333333,38.6666666666667,0.5f),new StylusPoint(23.6666666666667,39.6666666666667,0.5f),new StylusPoint(21.6666666666667,40.6666666666667,0.5f),new StylusPoint(19.6666666666667,42,0.5f),new StylusPoint(18,43.3333333333333,0.5f),new StylusPoint(16.3333333333333,44.6666666666667,0.5f),new StylusPoint(14.6666666666667,46,0.5f),new StylusPoint(13,47.3333333333333,0.5f),new StylusPoint(11.6666666666667,48.6666666666667,0.5f),}),attributes)});
                }
            }

            public PointCollection HighlighterPreviewPoints
            {
                get
                {
                    return new PointCollection{
                        new Point(400,0),
                        new Point(222,0),
                        new Point(167,70),
                        new Point(167,74),
                        new Point(158,74),
                        new Point(154,79),
                        new Point(130,108),
                        new Point(127,106),
                        new Point(115,123),
                        new Point(119,130),
                        new Point(125,155),
                        new Point(125,178),
                        new Point(122,210),
                        new Point(112,239),
                        new Point(98,261),
                        new Point(74,292),
                        new Point(73,296),
                        new Point(74,306),
                        new Point(60,321),
                        new Point(49,341),
                        new Point(48,345),
                        new Point(50,347),
                        new Point(86,362),
                        new Point(106,342),
                        new Point(114,336),
                        new Point(125,335),
                        new Point(163,295),
                        new Point(204,271),
                        new Point(252,261),
                        new Point(274,262),
                        new Point(282,266),
                        new Point(297,250),
                        new Point(296,249),
                        new Point(323,217),
                        new Point(322,215),
                        new Point(326,210),
                        new Point(330,209),
                        new Point(300,121),
                        new Point(400,0)
                    };
                }
            }
            public PointCollection BrushPreviewPoints
            {
                get
                {
                    return new PointCollection{
                        new Point(100,0),
                        new Point(71,0),
                        new Point(62,12),
                        new Point(62,20),
                        new Point(48,47),
                        new Point(37,65),
                        new Point(37,69),
                        new Point(31,83),
                        new Point(29,89),
                        new Point(30,90),
                        new Point(32,91),
                        new Point(37,85),
                        new Point(48,75),
                        new Point(52,75),
                        new Point(77,43),
                        new Point(91,32),
                        new Point(100,21),
                        new Point(100,0)
                    };
                }
            }
            public SolidColorBrush ColorBrush
            {
                get { return new SolidColorBrush(attributes.Color); }
            }
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
    }

}
