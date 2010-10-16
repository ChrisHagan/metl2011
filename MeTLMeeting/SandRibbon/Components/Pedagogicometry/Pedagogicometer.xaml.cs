using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SandRibbon.Components.Pedagogicometry;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;
using System.Windows.Threading;
using SandRibbon.Components.Utility;
using SandRibbon;

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
            var i = 0;
            allPedagogies = new[] { 
                new PedagogyLevel{ code = i++, label= "Whiteboard" },
                new PedagogyLevel{ code = i++, label= "Survey Respondent" },
                new PedagogyLevel{ code = i++, label= "Responsive Presentation" },
                new PedagogyLevel{ code = i++, label= "Collaborative Presentation" },
                new PedagogyLevel{ code = i++, label= "Crowdsourced Conversation" }};
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
        public static void SetPedagogyLevel(int code)
        {
            SetPedagogyLevel(level(code));
        }
        public static PedagogyLevel level(int level)
        {
            return allPedagogies.Where(p => p.code == level).Single();
        }
        private void pedagogies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            doSetPedagogyLevel((PedagogyLevel)e.AddedItems[0]);
        }
    }
}