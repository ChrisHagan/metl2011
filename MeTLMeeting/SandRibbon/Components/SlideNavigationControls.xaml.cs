using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;
using MeTLLib.DataTypes;
using SandRibbon.Pages.Collaboration;
using SandRibbon.Pages;
using SandRibbon.Pages.Collaboration.Models;

namespace SandRibbon.Components
{
    public partial class SlideNavigationControls : UserControl
    {        
        public SlideNavigationControls()
        {
            InitializeComponent();
            this.PreviewKeyDown += KeyPressed;            
        }

        private void KeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.PageUp || e.Key == Key.Up)
            {
                if(Commands.MoveToPrevious.CanExecute(null))
                  Commands.MoveToPrevious.Execute(null);
                e.Handled = true;
            }
            if (e.Key == Key.PageDown || e.Key == Key.Down)
            {
                if(Commands.MoveToNext.CanExecute(null))
                  Commands.MoveToNext.Execute(null);
                e.Handled = true;
            }
        }

        private void UpdateConversationDetails(ConversationDetails details)
        {
            var rootPage = DataContext as DataContextRoot;
            if (ConversationDetails.Empty.Equals(details)) return;
            Dispatcher.adopt(delegate
            {
                nav.Visibility = Visibility.Visible;
                if (details.Author == rootPage.NetworkController.credentials.name)
                {
                    Commands.SetSync.Execute(true);                    
                }                
            });
        }        
    }
}
