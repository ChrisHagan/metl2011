using Itschwabing.Libraries.ResourceChangeEvent;
using SandRibbon.Pages.Collaboration.Palettes;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SandRibbon.Pages.Collaboration
{
    public partial class CastBarFrame
    {
        public CastBarFrame()
        {
            InitializeComponent();
        }

        private void ContentControl_Drop(object sender, DragEventArgs e)
        {//Receive a Macro and replace the backing Macro set in the global profile
            var resourceKey = e.Data.GetData("Macro") as string;
            var slot = sender as ContentControl;
            var macro = slot.DataContext as Macro;
            macro.ResourceKey = resourceKey;
        }

        private void ButtonWidthChanged(object sender, Itschwabing.Libraries.ResourceChangeEvent.ResourceChangeEventArgs e)
        {
            var behaviour = sender as ResourceChangeEventBehavior;
            var element = behaviour.GetAssociatedObject();
            if (element == null) return;
            var context = element.DataContext as Bar;
            if (context.Orientation == Orientation.Vertical)
            {
                var width = (Double)e.NewValue;
                element.Width = width;
            }
        }

        private void ButtonHeightChanged(object sender, Itschwabing.Libraries.ResourceChangeEvent.ResourceChangeEventArgs e)
        {
            var behaviour = sender as ResourceChangeEventBehavior;
            var element = behaviour.GetAssociatedObject();
            if (element == null) return;
            var context = element.DataContext as Bar;
            if (context.Orientation == Orientation.Horizontal)
            {
                var height = (Double)e.NewValue;
                element.Height = height;
            }
        }

    }
}
