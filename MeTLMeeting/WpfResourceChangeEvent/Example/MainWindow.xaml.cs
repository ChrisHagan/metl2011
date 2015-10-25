using System.Windows;
using Itschwabing.Libraries.ResourceChangeEvent;

namespace Example
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LargeFont_Click(object sender, RoutedEventArgs e)
        {
            Resources["VariableFontSize"] = 20d;
        }

        private void SmallFont_Click(object sender, RoutedEventArgs e)
        {
            Resources["VariableFontSize"] = 8d;
        }

        private void OnResourceChanged_VariableFontSize(object sender, ResourceChangeEventArgs e)
        {
            MessageBox.Show("This is a popup triggered by an event handler that subscribed to ResourceChangeEventBehavior ResourceChanged event!!");
        }
    }
}
