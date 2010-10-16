using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SandRibbon.Components
{
    class PrintOptions : DependencyObject{
        public bool IncludePrivateContent
        {
            get { return (bool)GetValue(IncludePrivateContentProperty); }
            set { SetValue(IncludePrivateContentProperty, value); }
        }
        public static readonly DependencyProperty IncludePrivateContentProperty =
            DependencyProperty.Register("IncludePrivateContent", typeof(bool), typeof(PrintOptions), new UIPropertyMetadata(false));
    }
    public partial class PrintDialog : Window
    {
        public PrintDialog()
        {
            InitializeComponent();
            DataContext = new PrintOptions();
        }
        private void startPrinting_Click(object sender, RoutedEventArgs e)
        {
            var options = (PrintOptions)((FrameworkElement)sender).DataContext;
            if (options.IncludePrivateContent)
                Commands.PrintConversationHandout.ExecuteAsync(null);
            else
                Commands.PrintConversation.ExecuteAsync(null);
        }
    }
}
