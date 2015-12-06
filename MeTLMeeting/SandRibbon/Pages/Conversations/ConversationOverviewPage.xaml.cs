using MeTLLib.DataTypes;
using System.Windows.Navigation;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System;
using SandRibbon.Pages.Conversations.Models;
using SandRibbon.Components;
using System.Windows.Controls;
using SandRibbon.Pages.Collaboration.Models;

namespace SandRibbon.Pages.Collaboration
{
    public class LocatedActivity : DependencyObject
    {
        public string name { get; set; }
        public Slide slide { get; set; }
        public int index { get; set; }
        public LocatedActivity(string name, Slide slide, int activityCount, int voices)
        {
            this.name = name;
            this.slide = slide;
            this.index = slide.index;
            this.activityCount = activityCount;
            this.voices = voices;            
        }

        public int activityCount
        {
            get { return (int)GetValue(activityCountProperty); }
            set { SetValue(activityCountProperty, value); }
        }        
        public static readonly DependencyProperty activityCountProperty =
            DependencyProperty.Register("activityCount", typeof(int), typeof(LocatedActivity), new PropertyMetadata(0));

        public int voices
        {
            get { return (int)GetValue(voicesProperty); }
            set { SetValue(voicesProperty, value); }
        }
        
        public static readonly DependencyProperty voicesProperty =
            DependencyProperty.Register("voices", typeof(int), typeof(LocatedActivity), new PropertyMetadata(0));
    };
    public partial class ConversationOverviewPage : Page
    {        
        public ConversationOverviewPage()
        {
            InitializeComponent();
        }
        private void SlideSelected(object sender, RoutedEventArgs e)
        {
            var rootPage = DataContext as DataContextRoot;
            var element = sender as FrameworkElement;
            var activity = element.DataContext as LocatedActivity;
            var slide = activity.slide;
            rootPage.ConversationState.Slide = slide;
            NavigationService.Navigate(new RibbonCollaborationPage(
                rootPage.UserGlobalState, 
                rootPage.UserServerState, 
                rootPage.UserConversationState, 
                rootPage.ConversationState, 
                rootPage.NetworkController));            
        }
    }    
    public class GridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double val = (double)value;
            GridLength gridLength = new GridLength(val);

            return gridLength;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GridLength val = (GridLength)value;

            return val.Value;
        }
    }
}
