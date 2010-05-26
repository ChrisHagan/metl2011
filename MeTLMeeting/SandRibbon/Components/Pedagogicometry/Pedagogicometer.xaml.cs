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
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<object>(JoinConversation));
            Commands.SetPedagogyLevel.RegisterCommand(new DelegateCommand<object>((_object) => { }, CanSetPedagogyLevel));
        }
        public bool CanSetPedagogyLevel(object _level) { 
            try
            {
                Commands.JoinConversation.lastValue();
                return true;
            }
            catch (NotSetException) {
                return false;
            }
        }
        public static void RegisterVariant(PedagogicallyVariable variant) {
            variants.Add(variant);
        }
        private void JoinConversation(object unused)
        {
            if(pedagogies.SelectedItem == null)
                pedagogies.SelectedIndex = 0;
        }
        public static void SetPedagogyLevel(PedagogyLevel level) 
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