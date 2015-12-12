using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SandRibbon.Frame.Flyouts
{
    public abstract class FlyoutCard : UserControl
    {
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(FlyoutCard), new PropertyMetadata(""));
        public bool ShowCloseButton
        {
            get { return (bool)GetValue(ShowCloseButtonProperty); }
            set { SetValue(ShowCloseButtonProperty, value); }
        }
        public static readonly DependencyProperty ShowCloseButtonProperty = DependencyProperty.Register("ShowCloseButton", typeof(bool), typeof(FlyoutCard), new PropertyMetadata(true));
        public FlyoutCard(string title, FrameworkElement content, bool showCloseButton = true) : this()
        {
            Content = content;
            Title = title;
            ShowCloseButton = showCloseButton;
            DataContext = this;
        }
        public FlyoutCard()
        {
            Template = (ControlTemplate)TryFindResource("flyoutCardControlTemplate");
            InitializeComponent();
        }
        public void CloseFlyout()
        {
            Commands.CloseFlyoutCard.Execute(this);
        }
    }
}
