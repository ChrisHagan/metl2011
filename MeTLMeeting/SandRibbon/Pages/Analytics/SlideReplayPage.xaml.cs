using MeTLLib.DataTypes;
using MeTLLib.Providers.Connection;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using SandRibbon.Pages.Collaboration.Models;

namespace SandRibbon.Pages.Analytics
{
    public partial class SlideReplayPage : Page
    {        
        List<TargettedElement> elements = new List<TargettedElement>();
        int pointer = 0;        
        public SlideReplayPage()
        {
            InitializeComponent();                        
            loadParser();
        }
        public void loadParser()
        {
            var rootPage = DataContext as DataContextRoot;
            rootPage.NetworkController.client.historyProvider.Retrieve<PreParser>(
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
                                        rootPage.ConversationState.Slide.id.ToString());
        }

        private void Shuttle_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {                 
        }        
    }
}
