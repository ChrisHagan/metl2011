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
using SandRibbon.Providers;

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
                    Commands.SetDrawingAttributes.Execute(value);
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
    public partial class PenColors : RibbonGroup, IPencilCaseDisplay
    {
        public static Half halfOfParent = new Half();
        public ObservableCollection<PenColors.DrawingAttributesEntry> previouslySelectedDrawingAttributes = new ObservableCollection<PenColors.DrawingAttributesEntry>();
        public string preferredTab = "PenTools";
        public bool ShouldNotUpdateHSV;
        public bool ShouldNotUpdateRGB;
        public CurrentColourValues currentColourValues = new CurrentColourValues();

        public Brush[] simpleColourSet = new Brush[] {
            Brushes.White,Brushes.LightPink,Brushes.PaleGreen,Brushes.PaleTurquoise,Brushes.PaleVioletRed,Brushes.LightYellow,
            Brushes.LightGray,Brushes.Pink,Brushes.LightGreen,Brushes.LightBlue,Brushes.Violet,Brushes.Yellow,
            Brushes.DarkGray,Brushes.Red,Brushes.Green,Brushes.Blue,Brushes.Purple,Brushes.Orange,
            Brushes.Black,Brushes.DarkRed,Brushes.DarkGreen,Brushes.DarkBlue,Brushes.Maroon,Brushes.OrangeRed    
        };
        public double[] simpleSizeSet = new double[]{
            1,2,3,5,10,25
        };

        private DrawingAttributes[] defaultDrawingAttributes = new DrawingAttributes[] {
            new DrawingAttributes{Color = Colors.Black, IsHighlighter = false, Height = 1,Width = 1},
            new DrawingAttributes{Color = Colors.Red, IsHighlighter = false, Height = 2,Width = 2},
            new DrawingAttributes{Color = Colors.Blue, IsHighlighter = false, Height = 2,Width = 2},
            new DrawingAttributes{Color = Colors.Green, IsHighlighter = false, Height = 3,Width = 3},
            new DrawingAttributes{Color = Colors.Cyan, IsHighlighter = true, Height = 10,Width = 10},
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

            this.DataContext = currentColourValues;
            Commands.EnablePens.RegisterCommand(new DelegateCommand<object>((_unused => Enable()), mustBeInConversation));
            Commands.DisablePens.RegisterCommand(new DelegateCommand<object>((_unused => Disable())));
            Commands.ReportStrokeAttributes.RegisterCommand(new DelegateCommand<DrawingAttributes>((DrawingAttributes) => updatePreviousDrawingAttributes(DrawingAttributes)));
            Commands.ReportDrawingAttributes.RegisterCommand(new DelegateCommand<DrawingAttributes>((drawingAttributes => receiveDrawingAttributesChanged(drawingAttributes))));
            SetupPreviousColoursWithDefaults();
            Commands.SetLayer.RegisterCommand(new DelegateCommand<string>(updateToolBox));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<object>(setDefaults));
            Commands.SetDrawingAttributes.RegisterCommand(new DelegateCommand<object>(setDefaults));
        }
        private void setDefaults(object obj)
        {

            Console.WriteLine("Pen"); 
            Draw.IsChecked = true;
            Commands.SetInkCanvasMode.Execute("Ink");
        }
        private bool mustBeInConversation(object _unused)
        {
            try
            {
                return (Globals.location != null && Globals.location.activeConversation != null);
            }
            catch (NotSetException e)
            {
                return false;
            }
        }
        private void updateToolBox(string layer)
        {
            if (layer == "Sketch")
                Visibility = Visibility.Visible;
            else
                Visibility = Visibility.Collapsed;
        }
        private void updatePreviousDrawingAttributes(DrawingAttributes attributes)
        {
            int nextAvailableSpot = 0;
            if (previouslySelectedDrawingAttributes.Select(c => c.Attributes).Contains(attributes)) return;
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
        private void receiveDrawingAttributesChanged(DrawingAttributes arrivingDrawingAttributes)
        {
            if (currentColourValues.currentDrawingAttributes != arrivingDrawingAttributes)
                currentColourValues.currentDrawingAttributes = arrivingDrawingAttributes;
        }
        public void Disable()
        {
            Dispatcher.Invoke((Action)delegate
                                           {
                                               this.Effect = new BlurEffect();
                                           });
        }
        public void Enable()
        {
            Dispatcher.Invoke((Action)delegate
                                           {
                                               this.Effect = null;
                                           });
        }
        private void ChangeColorFromPreset(object sender, RoutedEventArgs e)
        {
            var IndexNumber = Int32.Parse(((System.Windows.Controls.Button)sender).Tag.ToString());
            var drawingAttributes = (DrawingAttributes)(((DrawingAttributesEntry)(defaultColours.Items[IndexNumber])).Attributes);
            Commands.SetDrawingAttributes.Execute(drawingAttributes);
            e.Handled = true;
        }
        private void OpenColourSettingPopup(object sender, RoutedEventArgs e)
        {
            var newBrush = new SolidColorBrush();
            ColourSettingPopup.Tag = ((System.Windows.Controls.Button)sender).Tag.ToString();
            newBrush.Color = ((DrawingAttributes)defaultDrawingAttributes[Int32.Parse(((System.Windows.Controls.Button)sender).Tag.ToString())]).Color;
            ColourSettingPopupDefaultColour.Fill = newBrush;
            ColourSettingPopup.IsOpen = true;
            ColourChooser.ItemsSource = simpleColourSet;
            SizeChooser.ItemsSource = simpleSizeSet;
        }
        private void ChangeColour(object sender, RoutedEventArgs e)
        {
            var Brush = ((Brush)((System.Windows.Controls.Button)sender).Background).ToString();
            var Color = (Color)ColorConverter.ConvertFromString(Brush);
            var PresetToUpdate = Int32.Parse(ColourSettingPopup.Tag.ToString());
            ((DrawingAttributesEntry)defaultColours.Items[PresetToUpdate]).ColorValue = Color;
            ((DrawingAttributesEntry)defaultColours.Items[PresetToUpdate]).XAMLColorName = Color.ToString();
            defaultColours.Items.Refresh();
            InvokeAlteredPreset(PresetToUpdate);
        }
        private void InvokeAlteredPreset(int index)
        {
            var drawingAttributes = (DrawingAttributes)(((DrawingAttributesEntry)(defaultColours.Items[index])).Attributes);
            Commands.SetDrawingAttributes.Execute(drawingAttributes);
            ColourSettingPopup.IsOpen = false;
        }
        private void ResetToDefault(object sender, RoutedEventArgs e)
        {
            var PresetToUpdate = Int32.Parse(ColourSettingPopup.Tag.ToString());
            var DrawingAttributes = ((DrawingAttributes)defaultDrawingAttributes[PresetToUpdate]);
            ((DrawingAttributesEntry)(defaultColours.Items[PresetToUpdate])).Attributes = DrawingAttributes;
            defaultColours.Items.Refresh();
            InvokeAlteredPreset(PresetToUpdate);
        }
        private void ChangeSize(object sender, RoutedEventArgs e)
        {
            var newSize = (double)Double.Parse(((System.Windows.Controls.Button)sender).Tag.ToString());
            var PresetToUpdate = Int32.Parse(ColourSettingPopup.Tag.ToString());
            ((DrawingAttributesEntry)defaultColours.Items[PresetToUpdate]).PenSize = newSize;
            defaultColours.Items.Refresh();
            InvokeAlteredPreset(PresetToUpdate);
        }

        public class DrawingAttributesEntry
        {
            public int Index { get; set; }
            private bool internalupdate = false;
            private DrawingAttributes attributes;

            public StrokeCollection BrushPreviewStroke
            {
                get
                {
                    return new StrokeCollection(new[]{new Stroke(
                        new StylusPointCollection(
                        new StylusPoint[]{new StylusPoint(3.60613397901533,34.1392520522955,0.01568628f),new StylusPoint(3.48345439870863,34.1457383196514,0.09411765f),new StylusPoint(3.30266343825665,34.2170872605655,0.1764706f),new StylusPoint(3.10250201775625,34.3078950035472,0.2352941f),new StylusPoint(2.90879741727198,34.4246478159522,0.2784314f),new StylusPoint(2.70863599677159,34.5803182324921,0.3098039f),new StylusPoint(2.53430185633575,34.7295023816763,0.3411765f),new StylusPoint(2.35351089588377,34.9111178676396,0.3607843f),new StylusPoint(2.19209039548022,35.1186784230262,0.3764706f),new StylusPoint(2.06295399515739,35.3521840478362,0.3960784f),new StylusPoint(1.99192897497982,35.5986622073579,0.4117647f),new StylusPoint(1.94673123486682,35.8710854363028,0.427451f),new StylusPoint(1.95318805488297,36.1305361305361,0.4431373f),new StylusPoint(1.97255851493139,36.4159318941928,0.4627451f),new StylusPoint(2.04358353510896,36.7013276578494,0.4823529f),new StylusPoint(2.14689265536723,37.0061822235735,0.5019608f),new StylusPoint(2.3276836158192,37.298064254586,0.5176471f),new StylusPoint(2.54075867635189,37.5834600182426,0.5294118f),new StylusPoint(2.79903147699758,37.8364244451201,0.5411765f),new StylusPoint(3.07021791767554,38.0634438025742,0.5490196f),new StylusPoint(3.3866020984665,38.2256004864701,0.5568628f),new StylusPoint(3.72235673930589,38.3553258335867,0.5647059f),new StylusPoint(4.10976594027441,38.4072159724334,0.5686275f),new StylusPoint(4.49717514124293,38.3942434377217,0.572549f),new StylusPoint(4.87812752219532,38.2969494273842,0.5803922f),new StylusPoint(5.24616626311541,38.1412790108442,0.5921569f),new StylusPoint(5.61420500403551,37.8948008513226,0.6039216f),new StylusPoint(5.96933010492332,37.6029188203101,0.6156863f),new StylusPoint(6.33736884584341,37.2396878483835,0.6352941f),new StylusPoint(6.6731234866828,36.8505118070335,0.654902f),new StylusPoint(6.99596448748991,36.4159318941928,0.6745098f),new StylusPoint(7.29943502824859,35.9618931792845,0.6941177f),new StylusPoint(7.61581920903954,35.4300192561062,0.7098039f),new StylusPoint(7.91928974979822,34.8722002635046,0.7254902f),new StylusPoint(8.25504439063761,34.2495185973447,0.7372549f),new StylusPoint(8.56497175141242,33.6203506638289,0.7490196f),new StylusPoint(8.86198547215496,32.9392925914665,0.7568628f),new StylusPoint(9.16545601291364,32.2193169149691,0.7647059f),new StylusPoint(9.4818401937046,31.4409648322692,0.7764706f),new StylusPoint(9.78531073446327,30.6496402148576,0.7843137f),new StylusPoint(10.0887812752219,29.8388567953785,0.7921569f),new StylusPoint(10.3728813559322,29.008614573832,0.8039216f),new StylusPoint(10.637610976594,28.1329684807946,0.8117647f),new StylusPoint(10.9023405972559,27.2313773183338,0.8235294f),new StylusPoint(11.17998385795,26.3232998885173,0.8313726f),new StylusPoint(11.4382566585956,25.3892773892774,0.8431373f),new StylusPoint(11.6900726392252,24.43579608797,0.8509804f),new StylusPoint(11.9483454398709,23.4433971825276,0.8588235f),new StylusPoint(12.2066182405165,22.4445120097294,0.8666667f),new StylusPoint(12.4713478611784,21.452113104287,0.8745098f),new StylusPoint(12.7619047619048,20.4532279314888,0.8823529f),new StylusPoint(13.0718321226796,19.4413702239789,0.8862745f),new StylusPoint(13.4011299435028,18.4230262491132,0.8901961f),new StylusPoint(13.7627118644068,17.4176548089592,0.8941177f),new StylusPoint(14.1630347054076,16.4057971014493,0.9019608f),new StylusPoint(14.5827280064568,15.3809668592277,0.9019608f),new StylusPoint(15.0217917675545,14.3755954190737,0.9098039f),new StylusPoint(15.499596448749,13.3831965136313,0.9137255f),new StylusPoint(16.0484261501211,12.4037701429006,0.9215686f),new StylusPoint(16.63599677159,11.456775108949,0.9254902f),new StylusPoint(17.2558514931396,10.49032127293,0.9333333f),new StylusPoint(17.9144471347861,9.58224384311341,0.9372549f),new StylusPoint(18.5988700564972,8.73902908685517,0.945098f),new StylusPoint(19.3155770782889,7.99310834093443,0.9529412f),new StylusPoint(20.1291364003228,7.32502280328367,0.9568627f),new StylusPoint(20.9943502824859,6.74774500861457,0.9607843f),new StylusPoint(21.8918482647296,6.29370629370629,0.9607843f),new StylusPoint(22.8345439870863,5.96290665855883,0.9647059f),new StylusPoint(23.8288942695722,5.76831863788385,0.9647059f),new StylusPoint(24.8555286521388,5.6775108949022,0.9686275f),new StylusPoint(25.8886198547215,5.70994223168136,0.9647059f),new StylusPoint(26.9023405972559,5.87209891557718,0.9568627f),new StylusPoint(27.8579499596449,6.13803587716631,0.9529412f),new StylusPoint(28.7360774818402,6.54667072058376,0.9490196f),new StylusPoint(29.4850686037127,7.09800344582953,0.945098f),new StylusPoint(30.09200968523,7.75311644876862,0.9372549f),new StylusPoint(30.5504439063761,8.51200972940103,0.9294118f),new StylusPoint(30.8732849071832,9.38765582243843,0.9215686f),new StylusPoint(31.0799031476997,10.3216783216783,0.9137255f),new StylusPoint(31.1509281678773,11.3140772271207,0.9058824f),new StylusPoint(31.0540758676352,12.3518800040539,0.8941177f),new StylusPoint(30.8603712671509,13.3702239789196,0.8862745f),new StylusPoint(30.5827280064568,14.3885679537853,0.8705882f),new StylusPoint(30.2534301856336,15.3809668592277,0.854902f),new StylusPoint(29.8466505246166,16.3344481605351,0.8431373f),new StylusPoint(29.4205004035512,17.2036079862167,0.827451f),new StylusPoint(28.9362389023406,18.0143914056958,0.8117647f),new StylusPoint(28.4519774011299,18.7149082801257,0.7960784f),new StylusPoint(27.9612590799031,19.3311036789298,0.7843137f),new StylusPoint(27.4576271186441,19.824059997973,0.7686275f),new StylusPoint(26.9475383373688,20.1937772372555,0.7529412f),new StylusPoint(26.4374495560936,20.4402553967771,0.7450981f),new StylusPoint(25.9209039548022,20.5959258133171,0.7372549f),new StylusPoint(25.3849878934625,20.6737610215871,0.7333333f),new StylusPoint(24.7974172719935,20.6932198236546,0.7294118f),new StylusPoint(24.2098466505246,20.6413296848079,0.7254902f),new StylusPoint(23.6158192090395,20.5570082091821,0.7254902f),new StylusPoint(23.0088781275222,20.4272828620655,0.7254902f),new StylusPoint(22.3825665859564,20.2716124455255,0.7254902f),new StylusPoint(21.7627118644068,20.0770244248505,0.7254902f),new StylusPoint(21.1557707828894,19.8175737306172,0.7254902f),new StylusPoint(20.5811138014528,19.4997466301814,0.7254902f),new StylusPoint(20.0258272800646,19.0781392520523,0.7254902f),new StylusPoint(19.4834543987086,18.5268065268065,0.7254902f),new StylusPoint(19.0508474576271,17.8522347217999,0.7254902f),new StylusPoint(18.908797417272,16.9311847572717,0.682353f),new StylusPoint(19.0508474576271,15.6923076923077,0.5294118f),}),attributes)});
                }
            }
            public StrokeCollection HighlighterPreviewStroke
            {
                get
                {
                    return new StrokeCollection(new[]{new Stroke(
                        new StylusPointCollection(
                        new StylusPoint[]{new StylusPoint(6.22681666159927,7.0863599677159,0.02745098f),new StylusPoint(6.20087159217594,7.00242130750605,0.1490196f),new StylusPoint(6.20087159217594,7.00242130750605,0.2705882f),new StylusPoint(6.19438532482011,6.89911218724778,0.3607843f),new StylusPoint(6.18789905746427,6.75706214689265,0.4392157f),new StylusPoint(6.2333029289551,6.5956416464891,0.5019608f),new StylusPoint(6.2527617310226,6.33091202582728,0.5529412f),new StylusPoint(6.34356947400426,6.07909604519774,0.5882353f),new StylusPoint(6.42789094963008,5.76916868442292,0.6156863f),new StylusPoint(6.57707509881423,5.46569814366424,0.6392157f),new StylusPoint(6.75220431742171,5.12994350282486,0.6509804f),new StylusPoint(7.00516874429918,4.83938660209847,0.6705883f),new StylusPoint(7.25813317117665,4.5682001614205,0.682353f),new StylusPoint(7.57596027161244,4.35512510088781,0.6941177f),new StylusPoint(7.91324617411574,4.17433414043584,0.7058824f),new StylusPoint(8.30242221546569,4.04519774011299,0.7137255f),new StylusPoint(8.71105705888315,3.94188861985472,0.7254902f),new StylusPoint(9.13266443701226,3.88377723970944,0.7333333f),new StylusPoint(9.54129928042971,3.88377723970944,0.7411765f),new StylusPoint(9.9758791932705,3.92897497982244,0.7529412f),new StylusPoint(10.4364041755346,4.01291364003228,0.7607843f),new StylusPoint(10.9228742272221,4.16787732041969,0.7686275f),new StylusPoint(11.3769129421303,4.38095238095238,0.7843137f),new StylusPoint(11.7985203202595,4.68442292171106,0.7960784f),new StylusPoint(12.1682375595419,5.06537530266344,0.8078431f),new StylusPoint(12.512009729401,5.54963680387409,0.8235294f),new StylusPoint(12.7779466909902,6.11783696529459,0.8313726f),new StylusPoint(12.9919935137326,6.75060532687651,0.8431373f),new StylusPoint(13.1152325934935,7.43502824858757,0.8509804f),new StylusPoint(13.1606364649843,8.17110573042776,0.8627451f),new StylusPoint(13.134691395561,8.94592413236481,0.8627451f),new StylusPoint(13.0503699199351,9.727199354318,0.8666667f),new StylusPoint(12.9271308401743,10.5084745762712,0.8666667f),new StylusPoint(12.7714604236343,11.2639225181598,0.8627451f),new StylusPoint(12.5833586703152,12,0.8588235f),new StylusPoint(12.3498530455052,12.7425343018563,0.854902f),new StylusPoint(12.0904023512719,13.5044390637611,0.854902f),new StylusPoint(11.8114928549711,14.2663438256659,0.8509804f),new StylusPoint(11.506638289247,15.0411622276029,0.8509804f),new StylusPoint(11.2147562582345,15.8547215496368,0.8470588f),new StylusPoint(10.9163879598662,16.681194511703,0.8470588f),new StylusPoint(10.6180196614979,17.5399515738499,0.8470588f),new StylusPoint(10.3391101651971,18.4180790960452,0.8470588f),new StylusPoint(10.0602006688963,19.3284907183212,0.8470588f),new StylusPoint(9.77480490523969,20.2647296206618,0.8470588f),new StylusPoint(9.46346407215972,21.2332526230831,0.8509804f),new StylusPoint(9.17158204114726,22.2146892655367,0.8470588f),new StylusPoint(8.8602412080673,23.2090395480226,0.8470588f),new StylusPoint(8.51646903820817,24.2356739305892,0.8470588f),new StylusPoint(8.15972433363738,25.2687651331719,0.8470588f),new StylusPoint(7.78352082699909,26.3083131557708,0.8470588f),new StylusPoint(7.4267761224283,27.3155770782889,0.8470588f),new StylusPoint(7.06354515050167,28.2841000807103,0.8431373f),new StylusPoint(6.73274551535421,29.2138821630347,0.8431373f),new StylusPoint(6.40194588020675,30.0984665052462,0.8392157f),new StylusPoint(6.08411877977095,30.9184826472962,0.8392157f),new StylusPoint(5.81169555082599,31.6481033091203,0.8352941f),new StylusPoint(5.55224485659268,32.3002421307506,0.8352941f),new StylusPoint(5.32522549913854,32.907183212268,0.8313726f),new StylusPoint(5.0982061416844,33.4882970137207,0.8313726f),new StylusPoint(4.89064558629776,33.9790153349475,0.8313726f),new StylusPoint(4.72848890240195,34.3728813559322,0.8313726f),new StylusPoint(4.59227728792946,34.6763518966909,0.8313726f),new StylusPoint(4.49498327759197,34.8506860371267,0.8313726f),new StylusPoint(4.49498327759197,34.8506860371267,0.8313726f),new StylusPoint(4.49498327759197,34.8506860371267,0.8313726f),new StylusPoint(4.49498327759197,34.5407586763519,0.827451f),new StylusPoint(4.59876355528529,34.1275221953188,0.8313726f),new StylusPoint(4.76092023918111,33.5657788539144,0.8313726f),new StylusPoint(4.99442586399108,32.8684422921711,0.8352941f),new StylusPoint(5.32522549913854,32.0484261501211,0.8352941f),new StylusPoint(5.74034660991183,31.1315577078289,0.8392157f),new StylusPoint(6.22033039424344,30.1242937853107,0.8431373f),new StylusPoint(6.77814938684504,29.0524616626312,0.8509804f),new StylusPoint(7.39434478564913,27.954802259887,0.854902f),new StylusPoint(8.10783419479072,26.8054882970137,0.8588235f),new StylusPoint(8.89915881220229,25.6755447941889,0.8627451f),new StylusPoint(9.74237356846052,24.5778853914447,0.8627451f),new StylusPoint(10.6050471267863,23.5383373688458,0.8666667f),new StylusPoint(11.4806932198237,22.589184826473,0.8666667f),new StylusPoint(12.3757981149285,21.7691686844229,0.8666667f),new StylusPoint(13.2644167426776,21.0653753026634,0.8666667f),new StylusPoint(14.1530353704267,20.4519774011299,0.8666667f),new StylusPoint(15.0092226613966,19.9289749798224,0.8666667f),new StylusPoint(15.7940610114523,19.4963680387409,0.8627451f),new StylusPoint(16.5334954900172,19.1670702179177,0.8627451f),new StylusPoint(17.2469848991588,18.9152542372881,0.8666667f),new StylusPoint(17.9085841694537,18.7409200968523,0.8666667f),new StylusPoint(18.518293300902,18.6440677966102,0.8666667f),new StylusPoint(19.0890848282153,18.6246973365617,0.8666667f),new StylusPoint(19.6079862166819,18.7086359967716,0.8666667f),new StylusPoint(20.0879700010135,18.9217110573043,0.8705882f),new StylusPoint(20.5160636464984,19.2187247780468,0.8705882f),new StylusPoint(20.8728083510692,19.6061339790153,0.8705882f),new StylusPoint(21.1582041147259,20.0710250201776,0.8745098f),new StylusPoint(21.3657646701125,20.5746569814366,0.8745098f),new StylusPoint(21.4825174825175,21.0976594027441,0.8784314f),new StylusPoint(21.5344076213641,21.6400322841001,0.8784314f),new StylusPoint(21.5149488192966,22.1694915254237,0.8784314f),new StylusPoint(21.424141076315,22.6924939467312,0.8823529f),new StylusPoint(21.268470659775,23.2025827280065,0.8823529f),new StylusPoint(21.0803689064559,23.6997578692494,0.8823529f),new StylusPoint(20.8274044795784,24.1969330104923,0.8823529f),new StylusPoint(20.5614675179893,24.6941081517353,0.8823529f),new StylusPoint(20.2825580216885,25.1912832929782,0.8823529f),new StylusPoint(19.990675990676,25.6884584342211,0.8823529f),new StylusPoint(19.7377115637985,26.1727199354318,0.8784314f),new StylusPoint(19.4652883348536,26.637610976594,0.8784314f),new StylusPoint(19.2317827100436,27.089588377724,0.8745098f),new StylusPoint(18.9982770852336,27.5157384987893,0.8745098f),new StylusPoint(18.7972027972028,27.9031476997579,0.8705882f),new StylusPoint(18.6155873112395,28.2776432606941,0.8666667f),new StylusPoint(18.4534306273437,28.6327683615819,0.8666667f),new StylusPoint(18.2977602108037,28.9749798224374,0.8627451f),new StylusPoint(18.129117259552,29.3042776432607,0.8588235f),new StylusPoint(17.9799331103679,29.6271186440678,0.854902f),new StylusPoint(17.8566940306071,29.9370460048426,0.8509804f),new StylusPoint(17.7464274855579,30.2405165456013,0.8470588f),new StylusPoint(17.6556197425763,30.5569007263923,0.8431373f),new StylusPoint(17.5777845343063,30.8603712671509,0.8392157f),new StylusPoint(17.5129218607479,31.1573849878935,0.8392157f),new StylusPoint(17.474004256613,31.4479418886199,0.8352941f),new StylusPoint(17.4675179892571,31.7449556093624,0.8313726f),new StylusPoint(17.4675179892571,32.0548829701372,0.827451f),new StylusPoint(17.4869767913246,32.3583535108959,0.827451f),new StylusPoint(17.5388669301713,32.6553672316384,0.8196079f),new StylusPoint(17.6426472078646,32.9265536723164,0.8156863f),new StylusPoint(17.7594000202696,33.1783696529459,0.8117647f),new StylusPoint(17.9085841694537,33.4043583535109,0.8078431f),new StylusPoint(18.090199655417,33.5980629539952,0.8f),new StylusPoint(18.2912739434479,33.7659402744148,0.7960784f),new StylusPoint(18.5247795682578,33.8757062146893,0.7882353f),new StylusPoint(18.7647714604236,33.9661016949153,0.7843137f),new StylusPoint(18.9853045505219,34.0371267150928,0.7803922f),new StylusPoint(19.1928651059086,34.0952380952381,0.7764706f),new StylusPoint(19.3874531265836,34.1404358353511,0.772549f),new StylusPoint(19.5950136819702,34.1791767554479,0.7686275f),new StylusPoint(19.7831154352893,34.1985472154964,0.7686275f),new StylusPoint(19.9647309212527,34.2114608555287,0.7647059f),new StylusPoint(20.1333738725043,34.2050040355125,0.7647059f),new StylusPoint(20.3085030911118,34.1920903954802,0.7647059f),new StylusPoint(20.4966048444309,34.1662631154157,0.7647059f),new StylusPoint(20.6847065977501,34.1146085552865,0.7647059f),new StylusPoint(20.8792946184251,34.0500403551251,0.7647059f),new StylusPoint(21.0544238370325,33.9596448748991,0.7607843f),new StylusPoint(21.22955305564,33.8563357546408,0.7607843f),new StylusPoint(21.4046822742475,33.7336561743341,0.7568628f),new StylusPoint(21.5538664234316,33.591606133979,0.7568628f),new StylusPoint(21.69656430526,33.410815173527,0.7529412f),new StylusPoint(21.8262896523766,33.1912832929782,0.7490196f),new StylusPoint(21.9819600689166,32.9652945924132,0.654902f),new StylusPoint(22.0792540792541,32.6489104116223,0.4980392f),new StylusPoint(22.2024931590149,32.2679580306699,0.3372549f),new StylusPoint(22.3581635755549,31.8224374495561,0.172549f),new StylusPoint(22.5332927941624,31.2929782082324,0.01568628f),}),new DrawingAttributes{Color=Colors.Black,Height=1,Width=1,IsHighlighter=false})});
                }
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
    public class colourContrastConverter : IValueConverter
    {
        public ColorConverter bc;
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                var nc = (Color)(ColorConverter.ConvertFromString(value.ToString()));
                if (nc.R + nc.G + nc.B > 381)
                    return Brushes.Black;
                //var newColor = (Brush)(new BrushConverter().ConvertFromString(value.ToString()));
                else return Brushes.White;
                //return newColor;
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
            var Pen = attributes.IsHighlighter ? "highlighter" : "pen";
            var size = Math.Round(attributes.PenSize, 1);
            return string.Format("A {0}, {1} {2} wide, of colour {3}.",
                Pen,
                size.ToString(),
                size > 1 ? "points" : "point",
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
