using MeTLLib.DataTypes;
using SandRibbon.Pages.Collaboration.Models;
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
        public CommandBarConfigurationPage()
        {
            InitializeComponent();
             var rangeProperties = new[] {
                new SlotConfigurer("How wide are your slider handles?","HorizontalThumbWidth"),
                new SlotConfigurer("How wide are your buttons?","ButtonWidth"),
                new SlotConfigurer("How tall are your buttons?","ButtonHeight"),
                new SlotConfigurer("How wide are your graphs?", "SensorWidth"),
                new SlotConfigurer("How wide are your column dividers?", "SplitterWidth")
            };            
            sliders.ItemsSource = rangeProperties;

            DataContext = new ToolableSpaceModel
            {//There is no slide context for this UI
                context = new VisibleSpaceModel(),
                profile = Globals.currentProfile
            };
            Resources["ToolSets"] = new[] {
                new MacroGroup {
                    Label="Navigation",                    
                    Macros=new[] {
                        new Macro("conversation_overview")
                    }
                },
                new MacroGroup {
                    Label="Keyboarding",
                    Macros=new[] {
                        new Macro("font_toggle_bold"),
                        new Macro("font_toggle_italic"),
                        new Macro("font_toggle_underline"),
                        new Macro("font_toggle_strikethrough"),
                        new Macro("font_size_increase"),
                        new Macro("font_size_decrease"),
                        new Macro("font_more_options")                        
                    }
                },
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
                },
                new MacroGroup {
                    Label="Social controls",                    
                    Macros=new[] {
                        new Macro("participants_toggle")
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
        {//Package whichever Macro you've picked up
            var thumb = sender as Thumb;
            var macro = thumb.DataContext as Macro;
            var dataObject = new DataObject();
            dataObject.SetData("Macro", macro.ResourceKey);
            DragDrop.DoDragDrop(thumb, dataObject, DragDropEffects.Copy);
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
    public class Macro : DependencyObject
    {        
        public string ResourceKey
        {
            get { return (string)GetValue(ResourceKeyProperty); }
            set { SetValue(ResourceKeyProperty, value); }
        }

        public static readonly DependencyProperty ResourceKeyProperty =
            DependencyProperty.Register("ResourceKey", typeof(string), typeof(Macro), new PropertyMetadata("slot"));


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

        public Bar(int size) : this(size,Enumerable.Range(0, size).Select(i => new Macro("slot")))
        {            
        }
        public Bar(int size, IEnumerable<Macro> macros) {            
            Macros = new ObservableCollection<Macro>(macros);
            for (var i = macros.Count(); i < size; i++)
            {
                Macros.Insert(i, new Macro("slot"));
            }
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
            if (values[0] == DependencyProperty.UnsetValue)
            {
                return DependencyProperty.UnsetValue;
            }
            else
            {
                var containerMeasure = (Double)values[0];
                var dataContext = values[1] as Bar;
                var orientation = (Orientation)Enum.Parse(typeof(Orientation), (string)parameter);
                if (dataContext.Orientation == orientation)
                {
                    return containerMeasure * dataContext.ScaleFactor;
                }
                switch (dataContext.Orientation)
                {
                    case Orientation.Horizontal: return App.Current.TryFindResource("ButtonHeight");
                    case Orientation.Vertical: return (Double)App.Current.TryFindResource("ButtonWidth") + (Double)App.Current.TryFindResource("SensorWidth");
                    default: return DependencyProperty.UnsetValue;
                }
            }
        }
        
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class MacroAppearanceConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            var resourceKey = value as string;
            var functionalControl = Application.Current.FindResource(resourceKey) as FrameworkElement;
            foreach (var c in LogicalTreeHelper.GetChildren(functionalControl)) {                
                if (c is Appearance)
                {
                    var a = c as Appearance;
                    return a.Clone() ;
                }
            }
            return null;         
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return value;
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