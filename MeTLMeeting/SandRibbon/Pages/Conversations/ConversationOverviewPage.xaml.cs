using MeTLLib.DataTypes;
using System.Windows.Navigation;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System;
using SandRibbon.Pages.Conversations.Models;
using SandRibbon.Components;

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
    public partial class ConversationOverviewPage : ConversationAwarePage
    {
        public ReticulatedConversation conversation { get; protected set; }
        public ConversationOverviewPage(UserGlobalState _userGlobal, UserServerState _userServer, UserConversationState _userConv, ConversationState _conversationState, NetworkController _networkController)
        {/*Grab all the states for further threading.  Trust that bindings have already been established.  Your referrer
            knows more about possible optimisations than you do right now.*/
            UserGlobalState = _userGlobal;
            UserServerState = _userServer;
            UserConversationState = _userConv;
            ConversationState = _conversationState;
            NetworkController = _networkController;            
            DataContext = ConversationState;
            InitializeComponent();
        }

        private void SlideSelected(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            var activity = element.DataContext as LocatedActivity;
            var slide = activity.slide;
            var userSlide = new UserSlideState();            
            NavigationService.Navigate(new RibbonCollaborationPage(UserGlobalState, UserServerState, UserConversationState, ConversationState, userSlide, NetworkController, slide));            
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
