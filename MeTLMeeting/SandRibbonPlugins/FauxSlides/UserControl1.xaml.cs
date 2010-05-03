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
using SandRibbon;
using SandRibbon.Components.Interfaces;
using SandRibbon.Providers.Structure;

namespace FauxSlides
{
    public partial class PluginMain : UserControl, ISlideDisplay
    {
        public string preferredTab = "Navigate";
        public PluginMain()
        {
            InitializeComponent();
        }
        private void slides_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Commands.MoveTo.Execute(e.AddedItems[0], this);
        }
        public void Display(ConversationDetails details)
        {
            slides.ItemsSource = details.SlideIds;
        }
    }
}
