using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using SandRibbon.Components.Pedagogicometry;

namespace SandRibbon.Components.Sandpit
{
    public interface PedagogicallyVariable
    {
        bool CanSetLevel(PedagogyLevel level);
        bool SetLevel(PedagogyLevel level);
    }
    public partial class Pedagogicometer : UserControl
    {
        private static IEnumerable<PedagogyLevel> allPedagogies = null;
        private static Pedagogicometer instance;
        public Pedagogicometer()
        {
            InitializeComponent();
            instance = this;
            allPedagogies = new[] { 
                new PedagogyLevel{ code = PedagogyCode.Whiteboard,                  label= "Whiteboard" },
                new PedagogyLevel{ code = PedagogyCode.SurveyRespondent,            label= "Survey Respondent" },
                new PedagogyLevel{ code = PedagogyCode.ResponsivePresentation,      label= "Responsive Presentation" },
                new PedagogyLevel{ code = PedagogyCode.CollaborativePresentation,   label= "Collaborative Presentation" },
                new PedagogyLevel{ code = PedagogyCode.CrowdsourcedConversation,    label= "Crowdsourced Conversation" }};
            pedagogies.ItemsSource = allPedagogies;
        }
        public static void SetDefaultPedagogyLevel()
        {
            instance.pedagogies.SelectedItem = allPedagogies.ElementAt(2);
        }
        public static void SetPedagogyLevel(PedagogyLevel level)
        {
            if (Commands.SetPedagogyLevel.CanExecute(level))
            {
                instance.pedagogies.SelectedItem = level;
            }
        }
        private static void doSetPedagogyLevel(PedagogyLevel level)
        {
            Commands.SetPedagogyLevel.ExecuteAsync(level);
        }
        public static void SetPedagogyLevel(PedagogyCode code)
        {
            SetPedagogyLevel(level(code));
        }
        public static PedagogyLevel level(PedagogyCode level)
        {
            return allPedagogies.Where(p => p.code == level).Single();
        }
        private void pedagogies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            doSetPedagogyLevel((PedagogyLevel)e.AddedItems[0]);
        }
    }
}