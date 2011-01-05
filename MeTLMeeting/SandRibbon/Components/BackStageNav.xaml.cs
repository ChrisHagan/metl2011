using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;
using MeTLLib.DataTypes;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace SandRibbon.Components
{
    public partial class BackStageNav : UserControl
    {
        public BackStageNav()
        {
            InitializeComponent();
            Commands.ShowConversationSearchBox.RegisterCommandToDispatcher(new DelegateCommand<object>(ShowConversationSearchBox));
            Commands.UpdateForeignConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<MeTLLib.DataTypes.ConversationDetails>(updateDetails));
            Commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<MeTLLib.DataTypes.ConversationDetails>(updateDetails));
            setMyConversationVisibility();
        }
        private void setMyConversationVisibility()
        {
            mine.Visibility = MeTLLib.ClientFactory.Connection().AvailableConversations.ToList().Where(c => c.Author == Globals.me && c.Subject.ToLower() != "deleted").Count() > 0 ? Visibility.Visible : Visibility.Collapsed;
            if (mine.Visibility == Visibility.Collapsed)
                find.IsChecked = true;
        }
        private void updateDetails(ConversationDetails details)
        {
            if (details.Subject.ToLower() == "deleted" && details.Jid == Globals.location.activeConversation)
            {
                current.Visibility = Visibility.Collapsed;
                currentConversation.Visibility = Visibility.Collapsed;
                separator2.Visibility = Visibility.Collapsed;
            }
            setMyConversationVisibility();
        }
        private void ShowConversationSearchBox(object mode)
        {
            if (String.IsNullOrEmpty(Globals.location.activeConversation))
            {
               current.Visibility = Visibility.Collapsed;
                currentConversation.Visibility = Visibility.Collapsed;
                separator2.Visibility = Visibility.Collapsed;
            }
            else { 
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
        private void openCorrectTab(string mode) {
            if ("MyConversations" == mode)
                openMyConversations();
            else
                openFindConversations();
        }
        public string currentMode { 
            get{
                return new[]{mine,find,currentConversation}.Aggregate(mine, (acc, item) =>
                                                                     {
                                                                         if (true == item.IsChecked)
                                                                             return item;
                                                                         return acc;
                                                                     }).Name;
            }
            set
            {
                var elements = new[] {mine, find, currentConversation};
                foreach (var button in elements)
                    if (button.Name == value)
                        button.IsChecked = true;
               

            }
        }
        private void mode_Checked(object sender, RoutedEventArgs e) {
            var mode = ((FrameworkElement)sender).Name;
            Commands.BackstageModeChanged.ExecuteAsync(mode);
        }
        private void current_Click(object sender, RoutedEventArgs e){
            Commands.HideConversationSearchBox.Execute(null);
        }

        private void deleteConversations(object sender, RoutedEventArgs e)
        {
            if (Globals.me.ToLower() != "sajames")
            {
                MessageBox.Show("sorry this functionality is only for stupid people");
                return;
            }
            var result = MessageBox.Show("Are you sure you want to delete all your conversations Stuart?!?", "Delete Conversations", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                foreach (var conversation in MeTLLib.ClientFactory.Connection().AvailableConversations.Where(c => c.Author == Globals.me))
                {
                    if (conversation.Author == Globals.me)
                    {
                        conversation.Subject = "Deleted";
                        MeTLLib.ClientFactory.Connection().UpdateConversationDetails(conversation);
                    }
                }
            }
        }
    }
}
