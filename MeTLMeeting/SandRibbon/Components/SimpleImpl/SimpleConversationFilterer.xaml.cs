using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Automation.AutomationPeers;
using SandRibbon.Providers.Structure;
using SandRibbonInterop.Interfaces;
using SandRibbonObjects;
using SandRibbon.Utils.Connection;

namespace SandRibbon.Components.SimpleImpl
{
    public partial class SimpleConversationFilterer : IConversationListing
    {
        private List<ConversationDetails> conversationList;
        private List<ConversationDetails> backup;
        public SimpleConversationFilterer()
        {
            InitializeComponent();
            textSorter.PreviewKeyDown += textSorter_KeyDown;
            conversations.PreviewKeyDown += new KeyEventHandler(conversation_KeyDown);
            direction.ItemsSource = new List<string> { "Ascending", "Descending" };
            direction.SelectedIndex = 0;
            sorter.ItemsSource = new List<string> { "Date", "Author", "Title" };
            sorter.SelectedIndex = 2;
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            Commands.UpdateForeignConversationDetails.RegisterCommand( new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
        }
        private void UpdateConversationDetails(ConversationDetails details)
        {
            var doUpdate = (Action) delegate
                                        {
                                            List(ConversationDetailsProviderFactory.Provider.ListConversations());
                                            filterConversationListing();
                                        };
            if (Thread.CurrentThread != Dispatcher.Thread)
                Dispatcher.BeginInvoke(doUpdate);
            else
                doUpdate();
        }
        private void conversation_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                var currentConversation = (ConversationDetails) conversations.SelectedItem;
                if(currentConversation == null) return;
                Commands.JoinConversation.Execute(currentConversation.Jid);
            }
        }
        private void textSorter_KeyDown(object sender, KeyEventArgs e)
        {
            if (conversationList == null) return;
            if(Keyboard.FocusedElement != textSorter) return;
            if(e.Key == Key.Down)
            {
                conversations.Focus();
                conversations.SelectedIndex = 0;
            }
            else if(e.Key == Key.Enter)
            {
                if(conversationList.Count != 1) return;
                Commands.JoinConversation.Execute(conversationList.First().Jid);
            }
        }
        public void FocusSearch()
        {
            if (conversationList == null) 
                List(ConversationDetailsProviderFactory.Provider.ListConversations());        
            textSorter.Focus();
        }
        public IEnumerable<string> List()
        {
            return conversationList.Select(c => c.Title).ToList();
        }
        public void List(IEnumerable<ConversationDetails> list)
        {
            var doWork = (Action)delegate
            {
                backup = list.Where(c => c.Title != null && c.Author != null).ToList();
                conversationList = backup;
                this.conversations.ItemsSource = conversationList;
                sort();
            };
            if (Thread.CurrentThread != Dispatcher.Thread)
                Dispatcher.BeginInvoke(doWork);
            else
                doWork();
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
            filterConversationListing();
        }

        private void filterConversationListing()
        {
            if (conversationList == null) return;
            var selectedFilter = textSorter.Text.ToLower();
            if(selectedFilter.Count() == 0)
            {
                conversationList = backup;
                sort();
                return;
            }
            conversationList = (from conv in backup 
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
            return new ConversationListingAutomationPeer(this, "filterer");
        }
    }
}
