using System.Windows;
using System.Windows.Controls;

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
        public FlyoutCard()
        {
            Template = (ControlTemplate)TryFindResource("flyoutCardControlTemplate");
            DataContext = this;
        }
        public void CloseFlyout()
        {
            Commands.CloseFlyoutCard.Execute(this);
        }
    }
}
