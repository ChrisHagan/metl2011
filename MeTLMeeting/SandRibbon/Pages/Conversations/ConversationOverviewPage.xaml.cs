using MeTLLib;
using MeTLLib.DataTypes;
using MeTLLib.Providers.Connection;
using SandRibbon.Components;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Linq;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System;
using SandRibbon.Pages.Conversations.Models;

namespace SandRibbon.Pages.Collaboration
{
    public class LocatedActivity : MeTLUser
    {
        public int slide { get; set; }
        public int index { get; set; }
        public int voices { get; set; }
        public LocatedActivity(string name, int slide, int activityCount, int voiceCount) : base(name)
        {
            this.slide = slide;
            this.activityCount = activityCount;
            this.voices = voiceCount;
        }
    };
    public partial class ConversationOverviewPage : Page
    {
        ReticulatedConversation conversation;
        public ConversationOverviewPage(ConversationDetails presentationPath)
        {
            InitializeComponent();
            DataContext = conversation = new ReticulatedConversation
            {
                PresentationPath = presentationPath,
                RelatedMaterial = new List<string>{}.Select(jid => ClientFactory.Connection().DetailsOf(jid)).ToList()

            };
            conversation.CalculateLocations();            
            processing.Maximum = conversation.Locations.Count;
            conversation.LocationAnalyzed += () => processing.Value++;
            conversation.AnalyzeLocations();
        }

        private void SlideSelected(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            var slide = element.DataContext as VmSlide;
            NavigationService.Navigate(new GroupCollaborationPage(slide.Slide.id));
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
