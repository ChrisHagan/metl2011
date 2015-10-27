using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;
using MeTLLib.DataTypes;
using MeTLLib;

namespace SandRibbon.Components
{
    public partial class SlideNavigationControls : UserControl
    {
        protected MeTLLib.MetlConfiguration backend;

        public SlideNavigationControls()
        {
            InitializeComponent();
            this.PreviewKeyDown += KeyPressed;
            App.getContextFor(backend).controller.commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            AppCommands.SetSync.RegisterCommand(new DelegateCommand<bool>(SetSync));
            AppCommands.SetSync.Execute(false);
        }

        private void KeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.PageUp || e.Key == Key.Up)
            {
                if(AppCommands.MoveToPrevious.CanExecute(null))
                  AppCommands.MoveToPrevious.Execute(null);
                e.Handled = true;
            }
            if (e.Key == Key.PageDown || e.Key == Key.Down)
            {
                if(AppCommands.MoveToNext.CanExecute(null))
                  AppCommands.MoveToNext.Execute(null);
                e.Handled = true;
            }
        }

        private void UpdateConversationDetails(ConversationDetails details)
        {
            if (ConversationDetails.Empty.Equals(details)) return;
            Dispatcher.adopt(delegate
            {
                nav.Visibility = Visibility.Visible;
                if (details.Author == App.getContextFor(backend).controller.creds.name)
                {
                    AppCommands.SetSync.Execute(true);
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
                    if (Globals.location.availableSlides.Contains(teacherSlide) && !Globals.isAuthor(App.getContextFor(backend).controller.creds.name))
                        App.getContextFor(backend).controller.commands.MoveToCollaborationPage.Execute((int)Globals.teacherSlide);
                }
                catch (NotSetException){ }
            }
            else
            {
                source = new BitmapImage(deSynced);
            }
            //Dispatcher.adoptAsync(()=>syncButton.Icon = source);
        }
        private void toggleSync(object sender, RoutedEventArgs e)
        {
            var synch = !Globals.synched;
            System.Diagnostics.Trace.TraceInformation("ManuallySynched {0}", synch);
            AppCommands.SetSync.Execute(synch);
        }

    }
}
