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
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;
using MeTLLib.DataTypes;

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
        }
        private void updateDetails(ConversationDetails details)
        {
            if (details.Subject.ToLower() == "deleted" && details.Jid == Globals.location.activeConversation)
            {
                current.Visibility = Visibility.Collapsed;
                currentConversation.Visibility = Visibility.Collapsed;
                separator2.Visibility = Visibility.Collapsed;
                if(currentConversation.IsChecked == true)
                    all.IsChecked = true;
            }
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
        private void openAllConversations()
        {
            all.IsChecked = true;
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
                return new[]{help,mine,all, find, currentConversation}.Aggregate(all, (acc, item) =>
                                                                     {
                                                                         if (true == item.IsChecked)
                                                                             return item;
                                                                         return acc;
                                                                     }).Name;
            }
            set
            {
                var elements = new[] {help, mine, all, find, currentConversation};
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
    }
}
