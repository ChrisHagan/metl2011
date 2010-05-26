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

namespace SandRibbon.Components.Sandpit
{
    public interface PedagogicallyVariable {
        bool CanSetLevel(PedagogyLevel level);
        bool SetLevel(PedagogyLevel level);
    }
    public partial class Pedagogicometer : UserControl
    {
        private static IEnumerable<PedagogyLevel> allPedagogies = null;
        public static PedagogyLevel level;
        private static List<PedagogicallyVariable> variants = new List<PedagogicallyVariable>();
        private static Pedagogicometer instance;
        public Pedagogicometer()
        {
            InitializeComponent();
            instance = this;
            var i = 0;
            allPedagogies = new[] { 
                new PedagogyLevel{ code = i++, label= "Whiteboard" },
                new PedagogyLevel{ code = i++, label= "Powerpoint" },
                new PedagogyLevel{ code = i++, label= "CP3" },
                new PedagogyLevel{ code = i++, label= "MeTL" },
                new PedagogyLevel{ code = i++, label= "EdgeMeTL" }};
            pedagogies.ItemsSource = allPedagogies;
        }
        public static void SetDefaultPedagogyLevel() 
        {
            instance.pedagogies.SelectedItem = allPedagogies.ElementAt(0);
        }
        public static void RegisterVariant(PedagogicallyVariable variant) {
            variants.Add(variant);
        }
        public void SetPedagogyLevel(PedagogyLevel level) 
        {
            Pedagogicometer.level = level;
            foreach (var variant in variants)
                if (variant.CanSetLevel(level))
                    variant.SetLevel(level);
        }
        private void pedagogies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var level = (PedagogyLevel)e.AddedItems[0];
            if (Commands.SetPedagogyLevel.CanExecute(level))
                SetPedagogyLevel(level);
        }
    }
}