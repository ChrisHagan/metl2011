using MeTLLib;
using MeTLLib.DataTypes;
using MeTLLib.Providers.Connection;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using SandRibbon.Components;
using System;
using System.Windows.Navigation;
using SandRibbon.Providers;

namespace SandRibbon.Pages.Analytics
{
    public partial class SlideReplayPage : Page, SlideAwarePage
    {
        public Slide slide { get; protected set; }
        List<TargettedElement> elements = new List<TargettedElement>();
        int pointer = 0;
        public ConversationDetails details { get; protected set; }
        public NetworkController controller { get; protected set; }
        public SlideReplayPage(NetworkController _controller, ConversationDetails _details, Slide _slide)
        {
            InitializeComponent();
            this.slide = slide;
            details = _details;
            controller = _controller;
            loadParser();
        }
        public void loadParser()
        {
            controller.client.historyProvider.Retrieve<PreParser>(
                                        null,
                                        null,
                                        (parser) =>
                                        {
                                            var unsorted = new List<TargettedElement>();
                                            unsorted.AddRange(parser.ink);
                                            unsorted.AddRange(parser.text.Values);
                                            unsorted.AddRange(parser.images.Values);
                                            elements = unsorted.OrderBy(e => e.timestamp).ToList();
                                            shuttle.Maximum = elements.Count;
                                            shuttle.ValueChanged += Shuttle_ValueChanged;
                                        },
                                        slide.id.ToString());
        }

        private void Shuttle_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {                 
        }

        public Slide getSlide()
        {
            return slide;
        }

        public UserSlideState getUserSlideState()
        {
            throw new NotImplementedException();
        }

        public ConversationDetails getDetails()
        {
            return details;
        }

        public UserConversationState getUserConversationState()
        {
            throw new NotImplementedException();
        }

        public NetworkController getNetworkController()
        {
            return controller;
        }

        public UserServerState getUserServerState()
        {
            throw new NotImplementedException();
        }

        public UserGlobalState getUserGlobalState()
        {
            throw new NotImplementedException();
        }

        public NavigationService getNavigationService()
        {
            return NavigationService;
        }
    }
}
