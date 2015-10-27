using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;
using MeTLLib.DataTypes;
using UserControl = System.Windows.Controls.UserControl;
using System.Diagnostics;
using MeTLLib;

namespace SandRibbon.Components
{
    public partial class BackStageNav : UserControl
    {
        public MetlConfiguration backend;
        public BackStageNav()
        {
            InitializeComponent();
            AppCommands.ShowConversationSearchBox.RegisterCommandToDispatcher(new DelegateCommand<object>(ShowConversationSearchBox));
            App.getContextFor(backend).controller.commands.UpdateForeignConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<MeTLLib.DataTypes.ConversationDetails>(UpdateConversationDetails));
            App.getContextFor(backend).controller.commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<MeTLLib.DataTypes.ConversationDetails>(UpdateConversationDetails));
        }
        private void setMyConversationVisibility()
        {
            mine.Visibility = App.getContextFor(backend).controller.client.ConversationsFor(App.getContextFor(backend).controller.creds.name, SearchConversationDetails.DEFAULT_MAX_SEARCH_RESULTS).ToList().Where(c => c.Author == App.getContextFor(backend).controller.creds.name && c.Subject.ToLower() != "deleted").Count() > 0 ? Visibility.Visible : Visibility.Collapsed;
            if (mine.Visibility == Visibility.Collapsed)
                find.IsChecked = true;
        }
        private void UpdateConversationDetails(ConversationDetails details)
        {
            if (String.IsNullOrEmpty(Globals.location.activeConversation))
            {
                current.Visibility = Visibility.Collapsed;
                currentConversation.Visibility = Visibility.Collapsed;
                separator2.Visibility = Visibility.Collapsed;
            }
            if (details.IsEmpty) return;

            // if the conversation we're participating in has been deleted or we're no longer in the listed permission group 
            if (details.IsJidEqual(Globals.location.activeConversation))
            {
                if (details.isDeleted || (!details.UserHasPermission(App.getContextFor(backend).controller.creds)))
                {
                    current.Visibility = Visibility.Collapsed;
                    currentConversation.Visibility = Visibility.Collapsed;
                    separator2.Visibility = Visibility.Collapsed;
                    AppCommands.ShowConversationSearchBox.Execute("find");
                }
            }
            //setMyConversationVisibility();
        }
        private void ShowConversationSearchBox(object mode)
        {
            if (String.IsNullOrEmpty(Globals.location.activeConversation))
            {
                current.Visibility = Visibility.Collapsed;
                currentConversation.Visibility = Visibility.Collapsed;
                separator2.Visibility = Visibility.Collapsed;
            }
            else
            {
                current.Visibility = Visibility.Visible;
                currentConversation.Visibility = Visibility.Visible;
                separator2.Visibility = Visibility.Visible;
            }
            openCorrectTab((string)mode);            
        }
        private void openMyConversations()
        {
            mine.IsChecked = true;
        }
        private void openFindConversations()
        {
            Dispatcher.adoptAsync(() =>
            find.IsChecked = true);
        }
        private void openCorrectTab(string mode)
        {
            if ("MyConversations" == mode)
                openMyConversations();
            else
                openFindConversations();
        }
        public string currentMode
        {
            get
            {
                return new[] { mine, find, currentConversation }.Aggregate(mine, (acc, item) =>
                                                                          {
                                                                              if (true == item.IsChecked)
                                                                                  return item;
                                                                              return acc;
                                                                          }).Name;
            }
            set
            {
                var elements = new[] { mine, find, currentConversation };
                foreach (var button in elements)
                    if (button.Name == value)
                        button.IsChecked = true;
            }
        }
        private void mode_Checked(object sender, RoutedEventArgs e)
        {
            var mode = ((FrameworkElement)sender).Name;
            AppCommands.BackstageModeChanged.ExecuteAsync(mode);
        }
        private void current_Click(object sender, RoutedEventArgs e)
        {
            AppCommands.HideConversationSearchBox.Execute(null);
        }

        private void HelpCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
    }
}
