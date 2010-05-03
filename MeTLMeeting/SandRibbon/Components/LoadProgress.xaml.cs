using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SandRibbon.Components
{
    public class ProgressItem : DependencyObject
    {
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(ProgressItem), new UIPropertyMetadata("?"));
        public bool Done
        {
            get { return (bool)GetValue(DoneProperty); }
            set { SetValue(DoneProperty, value); }
        }
        public static readonly DependencyProperty DoneProperty =
            DependencyProperty.Register("Done", typeof(bool), typeof(ProgressItem), new UIPropertyMetadata(false));
        public double Proportion
        {
            get { return (double)GetValue(ProportionProperty); }
            set { SetValue(ProportionProperty, value); }
        }
        public static readonly DependencyProperty ProportionProperty =
            DependencyProperty.Register("Proportion", typeof(double), typeof(ProgressItem), new UIPropertyMetadata(1.0));


    }
    public partial class LoadProgress : UserControl
    {
        public static List<ProgressItem> ITEMS = new List<ProgressItem>();
        public static int ITEM_COUNT
        {
            get { 
                return ITEMS.Count(); }
        }
        public LoadProgress()
        {
            InitializeComponent();
        }
        public void Show(int upTo, int of)
        {
            if (ITEMS.Count() != of)
            {
                ITEMS = Enumerable.Range(0, of).Select(i => new ProgressItem { Label = i.ToString() }).ToList();
                foreach (var item in ITEMS)
                    item.Proportion = 1.0 / of;
                progress.ItemsSource = ITEMS;
            }
            if (upTo >= of-1)
            {
                Visibility = Visibility.Collapsed;
                return;
            }
            Visibility = Visibility.Visible;
            for (int i = 0; i <= upTo; i++)
                ITEMS.ElementAt(i).Done = true;
            for (int i = upTo+1; i < ITEMS.Count(); i++)
                ITEMS.ElementAt(i).Done = false;
        }
    }
}
