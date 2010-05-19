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
    public partial class Pedagogicometer : UserControl
    {
        public Pedagogicometer()
        {
            InitializeComponent();
        }
        public static void RegisterVariant(PedagogicallyVariable variant) { 
        }
    }
}
