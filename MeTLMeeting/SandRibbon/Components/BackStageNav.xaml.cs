using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;
using MeTLLib.DataTypes;
using UserControl = System.Windows.Controls.UserControl;
using System.Diagnostics;

namespace SandRibbon.Components
{
    public partial class BackStageNav : UserControl
    {
        public BackStageNav()
        {
            InitializeComponent();
            Commands.ShowConversationSearchBox.RegisterCommandToDispatcher(new DelegateCommand<object>(ShowConversationSearchBox));
            Commands.UpdateForeignConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<MeTLLib.DataTypes.ConversationDetails>(UpdateConversationDetails));
            Commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<MeTLLib.DataTypes.ConversationDetails>(UpdateConversationDetails));
        }
        private void setMyConversationVisibility()
        {
            mine.Visibility = MeTLLib.ClientFactory.Connection().ConversationsFor(Globals.me, SearchConversationDetails.DEFAULT_MAX_SEARCH_RESULTS).ToList().Where(c => c.Author == Globals.me && c.Subject.ToLower() != "deleted").Count() > 0 ? Visibility.Visible : Visibility.Collapsed;
            if (mine.Visibility == Visibility.Collapsed)
                find.IsChecked = true;
        }
        private void UpdateConversationDetails(ConversationDetails details)
        {
            if (details.IsEmpty) return;

            if (details.isDeleted && details.IsJidEqual(Globals.location.activeConversation))
            {
                Trace.TraceInformation(string.Format("CSB:::BackStageNav::UpdateConversationDetails: DetailsJid[{0}] ActiveConversation[{1}]", details.Jid, Globals.location.activeConversation));
                current.Visibility = Visibility.Collapsed;
                currentConversation.Visibility = Visibility.Collapsed;
                separator2.Visibility = Visibility.Collapsed;
            }
            //setMyConversationVisibility();
        }
        private void ShowConversationSearchBox(object mode)
        {
            if (String.IsNullOrEmpty(Globals.location.activeConversation))
            {
                Trace.TraceInformation(string.Format("CSB:::BackStageNav::ShowConversationSearchBox: ActiveConversation[{0}]", Globals.location.activeConversation));
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
            //setMyConversationVisibility();
            App.mark("Conversation Search is open for business");
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
        
        private void HelpCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
          e.CanExecute = true;
          e.Handled = true;
        }
    }
}
