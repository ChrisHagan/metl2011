using MeTLLib;
using MeTLLib.DataTypes;
using MeTLLib.Providers.Connection;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;

namespace SandRibbon.Pages.Analytics
{
    public partial class SlideReplayPage : Page
    {
        Slide slide;
        List<TargettedElement> elements = new List<TargettedElement>();
        int pointer = 0;
        public SlideReplayPage(Slide slide)
        {
            InitializeComponent();
            this.slide = slide;
            loadParser();
        }
        public void loadParser()
        {
            App.controller.client.historyProvider.Retrieve<PreParser>(
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
    }
}
