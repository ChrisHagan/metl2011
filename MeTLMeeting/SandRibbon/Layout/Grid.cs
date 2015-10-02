using System.Windows;
using System.Windows.Controls;

namespace SandRibbon.Layout
{

    public class GridLayout
    {
        /// <summary>
        /// Handles property changed event for the ItemsPerRow property, constructing
        /// the required ItemsPerRow elements on the grid which this property is attached to.
        /// </summary>
        private static void OnItemsPerRowPropertyChanged(DependencyObject d,
                            DependencyPropertyChangedEventArgs e)
        {
            Grid grid = d as Grid;
            int itemsPerRow = (int)e.NewValue;

            // construct the required row definitions
            grid.LayoutUpdated += (s, e2) =>
            {
                var childCount = grid.Children.Count;

                // add the required number of row definitions
                int rowsToAdd = (childCount - grid.RowDefinitions.Count) / itemsPerRow;
                for (int row = 0; row < rowsToAdd; row++)
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height=GridLength.Auto});
                }

                // set the row property for each chid
                for (int i = 0; i < childCount; i++)
                {
                    var child = grid.Children[i] as FrameworkElement;
                    Grid.SetRow(child, i / itemsPerRow);
                }
            };
        }
        /// <summary>
        /// Identifies the ItemsPerRow attached property. 
        /// </summary>
        public static readonly DependencyProperty ItemsPerRowProperty =
            DependencyProperty.RegisterAttached("ItemsPerRow", typeof(int), typeof(GridLayout),
                new PropertyMetadata(0, new PropertyChangedCallback(OnItemsPerRowPropertyChanged)));

        /// <summary>
        /// Gets the value of the ItemsPerRow property
        /// </summary>
        public static int GetItemsPerRow(DependencyObject d)
        {
            return (int)d.GetValue(ItemsPerRowProperty);
        }

        /// <summary>
        /// Sets the value of the ItemsPerRow property
        /// </summary>
        public static void SetItemsPerRow(DependencyObject d, int value)
        {
            d.SetValue(ItemsPerRowProperty, value);
        }
    }
}
