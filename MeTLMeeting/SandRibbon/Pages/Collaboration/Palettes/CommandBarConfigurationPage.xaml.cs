using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

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
            TopBar = new Bar(5)
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Rows = 1,
                    Columns = 8
                };
            Bars.ItemsSource = new[] {
                TopBar
            };
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
        public int Rows { get; set; }
        public int Columns { get; set; }
        public HorizontalAlignment HorizontalAlignment { get; set; }
        public VerticalAlignment VerticalAlignment { get; set; }
        public ObservableCollection<Macro> Macros { get; set; }
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