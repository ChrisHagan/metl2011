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
using System.IO;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbonObjects;
using SandRibbon.Providers;

namespace SandRibbon.Components
{
    public partial class SlideNavigationControls : UserControl
    {
        public SlideNavigationControls()
        {
            InitializeComponent();
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            Commands.SetSync.RegisterCommand(new DelegateCommand<bool>(SetSync));
            Commands.SetSync.Execute(false);
        }
        private void UpdateConversationDetails(ConversationDetails details)
        {
            Dispatcher.adopt(delegate
            {
                nav.Visibility = Visibility.Visible;
                if (details.Author == Globals.me)
                {
                    Commands.SetSync.Execute(true);
                    addSlideButton.Visibility = Visibility.Visible;
                    syncButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    addSlideButton.Visibility = Visibility.Collapsed;
                    syncButton.Visibility = Visibility.Visible;
                }
            });
        }
        private void SetSync(bool sync)
        { 
            var synced = new Uri(Directory.GetCurrentDirectory() + "\\Resources\\SyncRed.png");
            var deSynced = new Uri(Directory.GetCurrentDirectory() + "\\Resources\\SyncGreen.png");
            BitmapImage source;
            if(Globals.synched)
            {
                source = new BitmapImage(synced);
                try
                {
                    var teacherSlide = (int)Globals.teacherSlide;
                    if (Globals.location.availableSlides.Contains(teacherSlide))
                        Commands.MoveTo.Execute((int)Globals.teacherSlide);
                }
                catch (NotSetException){ }
            }
            else
            {
                source = new BitmapImage(deSynced);
            }
            syncButton.Icon = source;
        }
        private void toggleSync(object sender, RoutedEventArgs e)
        {
            Commands.SetSync.Execute(!Globals.synched);
        }
    }
}
