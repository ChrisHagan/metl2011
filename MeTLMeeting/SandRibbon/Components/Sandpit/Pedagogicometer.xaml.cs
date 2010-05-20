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

namespace SandRibbon.Components.Sandpit
{
    public class PedagogyLevel { }
    public interface PedagogicallyVariable {
        bool CanSetLevel(PedagogyLevel level);
        bool SetLevel(PedagogyLevel level);
    }
    public enum Pedagogies { 
        INSTANT_WHITEBOARD=0,
        SIMPLE_GROUP=1,
        CP3=2,
        METL=3,
        EDGE_METL=4
    }
    public partial class Pedagogicometer : UserControl
    {
        public static PedagogyLevel level;
        private static Dictionary<Pedagogies, PedagogyLevel> pedagogyLevels = new Dictionary<Pedagogies, PedagogyLevel>();
        private static List<PedagogicallyVariable> variants = new List<PedagogicallyVariable>();
        public Pedagogicometer()
        {
            InitializeComponent();
            foreach (var pedagogy in Enum.GetValues(typeof(Pedagogies)))
                pedagogyLevels[(Pedagogies)pedagogy] = new PedagogyLevel();
            slider.Maximum = Enum.GetValues(typeof(Pedagogies)).Length-1;
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
        public void PedagogicometerValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) 
        {
            var newLevel = (Pedagogies)Enum.GetValues(typeof(Pedagogies)).GetValue((int)e.NewValue);
            SetPedagogyLevel(pedagogyLevels[newLevel]);
        }
    }
}
