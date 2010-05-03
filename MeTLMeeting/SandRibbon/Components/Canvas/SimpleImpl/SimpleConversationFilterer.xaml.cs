using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SandRibbonObjects;
using SandRibbon.Providers.Structure;
using Microsoft.Practices.Composite.Presentation.Commands;
using System;
using SandRibbonInterop.Interfaces;
using SandRibbon.Automation.AutomationPeers;
using System.Windows.Automation.Peers;

namespace SandRibbon.Components.SimpleImpl
{
    public partial class SimpleConversationFilterer : IConversationListing
    {
        private List<ConversationDetails> conversationList;
        private List<ConversationDetails> backup;
        public SimpleConversationFilterer()
        {
            InitializeComponent();
            direction.ItemsSource = new List<string> { "Ascending", "Descending" };
            direction.SelectedIndex = 0;
            sorter.ItemsSource = new List<string> { "Date", "Author", "Title" };
            sorter.SelectedIndex = 2;
            Commands.ReceiveConversationListUpdate.RegisterCommand(new DelegateCommand<object>(
                    (_arg) => { updateList(ConversationDetailsProviderFactory.Provider.ListConversations()); }));
        }
        public void FocusSearch()
        {
            textSorter.Focus();
        }
        private void updateList(IEnumerable<ConversationDetails> list)
        {
            doList(list);
        }
        public void List(IEnumerable<ConversationDetails> list)
        {
            if (conversationList == null)
            doList(list);
        }
        public IEnumerable<string> List()
        {
            return conversationList.Select(c => c.Title).ToList();
        }
        public void doList(IEnumerable<ConversationDetails> list)
        {
            Dispatcher.Invoke((Action)delegate
            {
                backup = list.Where(c => c.Title != null && c.Author != null).ToList();
                conversationList = backup;
                this.conversations.ItemsSource = conversationList;
                sort();
            });
        }
        private void resortConversation(object sender, SelectionChangedEventArgs e)
        {
            sort();
        }
        private void sort()
        {
            if (conversationList == null) return;
            List<ConversationDetails> temp = conversationList;
            switch (sorter.SelectedItem as string)
            {
                case "Date":
                    temp = (from conv in conversationList orderby conv.Created.Ticks select conv).ToList();
                    break;
                case "Author":
                    temp = (from conv in conversationList orderby conv.Author select conv).ToList();
                    break;
                case "Title":
                    temp = (from conv in conversationList orderby conv.Title select conv).ToList();
                    break;
                default:
                    temp = backup;
                    break;
            }
            if (direction.SelectedIndex != 0) temp.Reverse();
            conversationList = temp;
            conversations.ItemsSource = conversationList;
        }
        private void filterValues(object sender, TextChangedEventArgs e)
        {
            var selectedFilter = textSorter.Text.ToLower();
            if(selectedFilter.Count() == 0)
            {
                conversationList = backup;
                sort();
                return;
            }
            var temp = conversationList;
            conversationList = (from conv in temp
                                where conv.Title.ToLower().Contains(selectedFilter) 
                                || conv.Author.ToLower().Contains(selectedFilter)
                                || conv.Tag.ToLower().Contains(selectedFilter)
                                select conv).ToList();
            conversations.ItemsSource = conversationList;
            sort();
        }
        private void CommandBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            Commands.JoinConversation.Execute((string)e.Parameter);
        }
        private void CommandBinding_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Commands.JoinConversation.CanExecute((string)e.Parameter);
        }
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ConversationListingAutomationPeer(this, "selector");
        }
    }
}
