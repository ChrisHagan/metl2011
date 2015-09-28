using MeTLLib.DataTypes;
using SandRibbon.Providers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;

namespace SandRibbon.Pages.Collaboration.Palettes
{
    public partial class CommandBarConfigurationPage : Page
    {
        Bar TopBar;
        public CommandBarConfigurationPage()
        {
            InitializeComponent();
             var rangeProperties = new[] {
                new SlotConfigurer("How wide are your buttons?","ButtonWidth"),
                new SlotConfigurer("How tall are your buttons?","ButtonHeight"),
                new SlotConfigurer("How wide are your graphs?", "SensorWidth")
            };            
            sliders.ItemsSource = rangeProperties;            
            
            Bars.ItemsSource = Globals.currentProfile.castBars;
            ToolSets.ItemsSource = new[] {
                new MacroGroup {
                    Label="Freehand inking",
                    Macros=new[] {
                        new Macro("pen_red"),
                        new Macro("pen_blue"),
                        new Macro("pen_black"),
                        new Macro("pen_green")
                    }
                },
                new MacroGroup {
                    Label="Highlighters",
                    Macros=new[] {
                        new Macro("pen_yellow_highlighter"),                        
                        new Macro("pen_orange_highlighter")
                    }
                },
                new MacroGroup {
                    Label="Immediate teaching feedback",
                    Macros=new[] {
                        new Macro("worm")                        
                    }
                }
            };
            SimulateFeedback();            
        }

        private void SimulateFeedback() {
            var t = new DispatcherTimer();
            t.Interval = new System.TimeSpan(0, 0, 5);
            t.Tick += delegate {
                Commands.ReceiveStrokes.Execute(Enumerable.Range(0,new Random().Next(50)).Select(i => new TargettedStroke(
                    0,"","",Privacy.NotSet,"",0,null,0.0
                    )).ToList());
            };
            t.Start();
        }

        private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {//Package the macro you picked up
            var thumb = sender as Thumb;
            var macro = thumb.DataContext as Macro;
            var dataObject = new DataObject();
            dataObject.SetData("Macro", macro.ResourceKey);
            DragDrop.DoDragDrop(thumb, dataObject, DragDropEffects.Copy);
        }

        private void ContentControl_Drop(object sender, DragEventArgs e)
        {//Land the macro on the right slot
            var resourceKey = e.Data.GetData("Macro") as string;
            var slot = sender as ContentControl;
            slot.DataContext = new Macro(resourceKey);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var element = sender as FrameworkElement;
            var context = element.DataContext as SlotConfigurer;
            Application.Current.Resources[context.Property] = e.NewValue;
        }
        
    }
    public class MacroGroup {
        public string Label { get; set; }
        public IEnumerable<Macro> Macros { get; set; }
    }
    public class Macro
    {
        public string ResourceKey { get; set; }
        public Macro(string resourceKey) {
            ResourceKey = resourceKey;
        }
    }
    public class Bar
    {              
        public Orientation Orientation { get; set; }
        public HorizontalAlignment HorizontalAlignment { get; set; }
        public VerticalAlignment VerticalAlignment { get; set; }
        public ObservableCollection<Macro> Macros { get; set; }
        public double ScaleFactor { get; set; }
        public int Rows { get; internal set; }
        public int Columns { get; internal set; }

        public Bar(int size)
        {
            Macros = new ObservableCollection<Macro>(Enumerable.Range(0, size).Select(i => new Macro("slot")));            
        }
    }
    public class SlotConfigurer
    {
        public string DisplayLabel { get; set; }
        public string Property { get; set; }
        public SlotConfigurer(string label, string property)
        {
            this.DisplayLabel = label;
            this.Property = property;
        }
    }

    class FactorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {            
            var containerMeasure = (Double)values[0];
            var dataContext = values[1] as Bar;
            var orientation = (Orientation) Enum.Parse(typeof(Orientation),(string) parameter);
            if (dataContext.Orientation == orientation)
            {
                return containerMeasure * dataContext.ScaleFactor;
            }
            switch (dataContext.Orientation) {
                case Orientation.Horizontal: return App.Current.TryFindResource("ButtonHeight");
                case Orientation.Vertical: return (Double)App.Current.TryFindResource("ButtonWidth") + (Double)App.Current.TryFindResource("SensorWidth");
                default:return DependencyProperty.UnsetValue;
            }
        }
        
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class DynamicResourceConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            var resourceKey = value as string;
            return Application.Current.FindResource(resourceKey);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}