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

namespace SandRibbon.Components.Sandpit
{
    public interface PedagogicallyVariable {
        bool CanSetLevel(PedagogyLevel level);
        bool SetLevel(PedagogyLevel level);
    }
    public partial class Pedagogicometer : UserControl
    {
        public static PedagogyLevel level;
        private static List<PedagogicallyVariable> variants = new List<PedagogicallyVariable>();
        public Pedagogicometer()
        {
            InitializeComponent();
            var i = 0;
            pedagogies.ItemsSource = new[] { 
                new PedagogyLevel{ code = i++, label= "Whiteboard" },
                new PedagogyLevel{ code = i++, label= "Powerpoint" },
                new PedagogyLevel{ code = i++, label= "CP3" },
                new PedagogyLevel{ code = i++, label= "MeTL" },
                new PedagogyLevel{ code = i++, label= "EdgeMeTL" }
            };
        }
        public static void RegisterVariant(PedagogicallyVariable variant) {
            variants.Add(variant);
        }
        public static void SetPedagogyLevel(PedagogyLevel level) 
        {
            Pedagogicometer.level = level;
            foreach (var variant in variants)
                if (variant.CanSetLevel(level))
                    variant.SetLevel(level);
        }
        private void PedagogyLevelClicked(object sender, RoutedEventArgs e)
        {
            var desiredLevel = (PedagogyLevel)((FrameworkElement)sender).DataContext;
            SetPedagogyLevel(desiredLevel);
        }
    }
}