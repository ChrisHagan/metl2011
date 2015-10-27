using Itschwabing.Libraries.ResourceChangeEvent;
using MeTLLib;
using SandRibbon.Pages.Collaboration.Models;
using SandRibbon.Pages.Collaboration.Palettes;
using SandRibbon.Providers;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SandRibbon.Pages.Collaboration
{
    public partial class GroupCollaborationPage : ServerAwarePage
    {
        
        public GroupCollaborationPage(MetlConfiguration _backend, int slide) : base(_backend)
        {
            InitializeComponent();
            DataContext = new ToolableSpaceModel(_backend)
            {
                    context = new VisibleSpaceModel {
                    Slide = slide
                },
                profile = Globals.currentProfile
            };
            ServerContext.controller.commands.MoveToCollaborationPage.Execute(slide);            
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
    public class CollaborationViewModel
    {

    }
}
