using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;
using MeTLLib.DataTypes;
using UserControl = System.Windows.Controls.UserControl;
using System.Diagnostics;
using SandRibbon.Pages.Collaboration;
using SandRibbon.Pages;

namespace SandRibbon.Components
{
    public partial class BackStageNav : UserControl
    {
        public SlideAwarePage rootPage { get; protected set; }
        public BackStageNav()
        {
            InitializeComponent();
            var showConversationSearchBoxCommand = new DelegateCommand<object>(ShowConversationSearchBox);
            var updateForeignConversationDetailsCommand = new DelegateCommand<MeTLLib.DataTypes.ConversationDetails>(UpdateConversationDetails);
            var updateConversationDetailsCommand = new DelegateCommand<MeTLLib.DataTypes.ConversationDetails>(UpdateConversationDetails);
            Loaded += (s, e) =>
            {
                if (rootPage == null)
                {
                    rootPage = DataContext as SlideAwarePage;
                }
                Commands.ShowConversationSearchBox.RegisterCommandToDispatcher(showConversationSearchBoxCommand);
                Commands.UpdateForeignConversationDetails.RegisterCommandToDispatcher(updateForeignConversationDetailsCommand);
                Commands.UpdateConversationDetails.RegisterCommandToDispatcher(updateConversationDetailsCommand);
            };
            Unloaded += (s, e) =>
            {
                Commands.ShowConversationSearchBox.UnregisterCommand(showConversationSearchBoxCommand);
                Commands.UpdateForeignConversationDetails.UnregisterCommand(updateForeignConversationDetailsCommand);
                Commands.UpdateConversationDetails.UnregisterCommand(updateConversationDetailsCommand);
            };
        }
        private void setMyConversationVisibility()
        {
            mine.Visibility = rootPage.NetworkController.client.ConversationsFor(rootPage.NetworkController.credentials.name, SearchConversationDetails.DEFAULT_MAX_SEARCH_RESULTS).ToList().Where(c => c.Author == rootPage.NetworkController.credentials.name && c.Subject.ToLower() != "deleted").Count() > 0 ? Visibility.Visible : Visibility.Collapsed;
            if (mine.Visibility == Visibility.Collapsed)
                find.IsChecked = true;
        }
        private void UpdateConversationDetails(ConversationDetails details)
        {
            if (String.IsNullOrEmpty(rootPage.ConversationDetails.Jid))
            {
                current.Visibility = Visibility.Collapsed;
                currentConversation.Visibility = Visibility.Collapsed;
                separator2.Visibility = Visibility.Collapsed;
            }
            if (details.IsEmpty) return;

            // if the conversation we're participating in has been deleted or we're no longer in the listed permission group 
            if (details.IsJidEqual(rootPage.ConversationDetails.Jid))
            {
                if (details.isDeleted || (!details.UserHasPermission(rootPage.NetworkController.credentials)))
                {
                    current.Visibility = Visibility.Collapsed;
                    currentConversation.Visibility = Visibility.Collapsed;
                    separator2.Visibility = Visibility.Collapsed;
                    Commands.ShowConversationSearchBox.Execute("find");
                }
            }
            //setMyConversationVisibility();
        }
        private void ShowConversationSearchBox(object mode)
        {
            if (String.IsNullOrEmpty(rootPage.ConversationDetails.Jid))
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
            Commands.BackstageModeChanged.ExecuteAsync(mode);
        }
        private void current_Click(object sender, RoutedEventArgs e)
        {
            Commands.HideConversationSearchBox.Execute(null);
        }

        private void HelpCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
    }
}
