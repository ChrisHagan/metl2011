using MeTLLib;
using MeTLLib.DataTypes;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Linq;
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
        public int slide { get; set; }
        public int index { get; set; }
        public LocatedActivity(string name, int slide, int activityCount, int voices)
        {
            this.name = name;
            this.slide = slide;
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
        public ConversationOverviewPage(UserGlobalState _userGlobal, UserServerState _userServer, UserConversationState _userConv, NetworkController _networkController, ConversationDetails presentationPath)
        {
            UserGlobalState = _userGlobal;
            UserServerState = _userServer;
            UserConversationState = _userConv;
            ConversationDetails = presentationPath;
            NetworkController = _networkController;
            InitializeComponent();
            DataContext = conversation = new ReticulatedConversation
            {
                networkController = NetworkController,
                PresentationPath = presentationPath,
                RelatedMaterial = new List<string>{}.Select(jid => NetworkController.client.DetailsOf(jid)).ToList()

            };
            conversation.CalculateLocations();            
            processing.Maximum = conversation.Locations.Count;
            conversation.LocationAnalyzed += () => Dispatcher.adopt(delegate
            {
                processing.Value++;
            });
            var t = new System.Threading.ThreadStart(() =>
            {
                conversation.AnalyzeLocations();
            });
            new System.Threading.Thread(t).Start();
        }

        private void SlideSelected(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            var slide = element.DataContext as VmSlide;
            //Commands.MoveToCollaborationPage.Execute(slide.Slide.id);
            var userSlide = new UserSlideState();
            NavigationService.Navigate(new RibbonCollaborationPage(UserGlobalState, UserServerState, UserConversationState, new ConversationState(), userSlide, NetworkController, slide.Details, slide.Slide));            
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
