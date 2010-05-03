using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SandRibbonInterop.Interfaces;
using SandRibbonObjects;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;
using SandRibbon.Providers.Structure;
using System.Windows.Automation.Peers;
using SandRibbon.Automation.AutomationPeers;

namespace SandRibbon.Components
{
    public partial class SimpleConversationSelector : UserControl, IConversationSelector, IConversationListing
    {
        public static IEnumerable<ConversationDetails> rawConversationList = new List<ConversationDetails>();
        private string me;
        public SimpleConversationSelector()
        {
            InitializeComponent();
            this.conversations.ItemsSource = new List<SandRibbonObjects.ConversationDetails>();
            Commands.CreateConversation.RegisterCommand(new DelegateCommand<ConversationDetails>((_details) => {}, doesConversationAlreadyExist));
            Commands.StartPowerPointLoad.RegisterCommand(new DelegateCommand<object>((_details) => {}, (detailsObject)=>doesConversationAlreadyExist((ConversationDetails)detailsObject)));
            Commands.ReceiveConversationListUpdate.RegisterCommand(new DelegateCommand<object>(
                    (_arg) => { List(ConversationDetailsProviderFactory.Provider.ListConversations()); }));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(
                _details=>{},
                details=>
                    rawConversationList.Where(c=>c.Title == details.Title || string.IsNullOrEmpty(details.Title)).Count() == 0));
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<SandRibbon.Utils.Connection.JabberWire.Credentials>(
                author => me = author.name));
        }
        private bool doesConversationAlreadyExist(ConversationDetails details)
        {
            if (details == null) 
                return true;
            if (details.Title.Length == 0) 
                return true;
            var currentConversations = (List<ConversationDetails>)this.conversations.ItemsSource;
            var relevantConversations = currentConversations.Where(c => c.Title == details.Title);
            bool conversationExists = relevantConversations.Count() > 0;
            return !conversationExists;
        }
        public void ListStartupConversations()
        {
            var allConversation = ConversationDetailsProviderFactory.Provider.ListConversations();
            var recentlyCreatedConversations = allConversation.OrderByDescending(c => c.Created).Reverse().Take(6).ToList();
            var recentDocs = RecentConversationProvider.loadRecentConversations().Take(6).ToList();
            var list = new List<ConversationDetails>();
            if (recentlyCreatedConversations.Count > 0)
            {
                list.Add(new SeparatorConversation("Recently Created Conversations"));
                list.AddRange(recentlyCreatedConversations);
            }
            if(recentDocs.Count > 0)
            {
                list.Add(new SeparatorConversation("Conversations you have recently been in"));
                list.AddRange(recentDocs.ToList());
            }
            
            this.conversations.ItemsSource = list;

        }
        public void List(IEnumerable<SandRibbonObjects.ConversationDetails> conversations)
        {
            Dispatcher.Invoke((Action)delegate
            {
                rawConversationList = conversations.ToList();
                var list = new List<ConversationDetails>();
                var myConversations = conversations.Where(c => c.Author == me).OrderBy(c=>c.LastAccessed.Date).Take(2).ToList();
                if (myConversations.Count() > 0)
                {
                    list.Add(new SeparatorConversation("My Conversations"));
                    list.AddRange(myConversations);
                }
                list.Add(new SeparatorConversation("Conversations I've worked in"));
                var recentConversations = RecentConversationProvider.loadRecentConversations().Where(c => c.IsValid && conversations.Contains(c)).Take(2);
                list.AddRange(recentConversations);
                var recentAuthors = list.Select(c => c.Author).Where(c => c != me).Distinct().ToList();
                foreach (var author in recentAuthors)
                {
                    var otherConversationsByThisAuthor = conversations.Where(c => c.IsValid && !list.Contains(c) && c.Author == author);
                    if (otherConversationsByThisAuthor.Count() > 0)
                    {
                        list.Add(new SeparatorConversation(string.Format("{0}'s other conversations:", author)));
                        list.AddRange(otherConversationsByThisAuthor.Take(2));
                    }
                }
                this.conversations.ItemsSource = list;
            });
        }
        public IEnumerable<string> List()
        {
            return rawConversationList.Select(c => c.Title).ToList();
        }
        private void doJoinConversation(object sender, ExecutedRoutedEventArgs e)
        {
            Commands.JoinConversation.Execute((string)e.Parameter);
        }
        private void canJoinConversation(object sender, CanExecuteRoutedEventArgs e)
        {//CommandParameter is conversation title
            e.CanExecute = Commands.JoinConversation.CanExecute((string)e.Parameter);
        }
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ConversationListingAutomationPeer(this, "selector");
        }
    }
    public class SeparatorStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is SeparatorConversation)
            {
                return (Style)((FrameworkElement)container).FindResource("separatorStyle");
            }
            return null;
        }
    }
    public class SeparatorConversation : ConversationDetails
    {
        public string Title { 
            get { return base.Title; }
            set { base.Title = value; }
        }
        public SeparatorConversation(string label)
        {
            if (label == null) label = String.Empty;
            Title = label;
        }
    }
}