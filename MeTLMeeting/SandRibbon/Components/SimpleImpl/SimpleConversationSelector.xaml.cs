using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Automation.AutomationPeers;
using SandRibbon.Providers;
using SandRibbon.Providers.Structure;
using SandRibbonInterop.Interfaces;
using SandRibbonObjects;
using SandRibbon.Utils;

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
            Commands.UpdateForeignConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(
                (_arg) => 
                    List(ConversationDetailsProviderFactory.Provider.ListConversations()),
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
            var currentConversations = ConversationDetailsProviderFactory.Provider.ListConversations();
            bool conversationExists = currentConversations.Any(c=> c.Title.Equals(details.Title));
            Logger.Log(currentConversations.Aggregate("", (acc,item)=>acc+" "+item.Title));
            Logger.Log(string.Format("[{0}] already exists: {1}", details.Title, conversationExists));
            return !conversationExists;
        }
        public void ListStartupConversations()
        {
            var allConversation = ConversationDetailsProviderFactory.Provider.ListConversations();
            List(allConversation);
        }
        public void ListRecentConversations()
        {
            var allConversation = ConversationDetailsProviderFactory.Provider.ListConversations();
            ListRecent(allConversation);
        }

        private void ListRecent(IEnumerable<ConversationDetails> conversations)
        {
            var doRecent = (Action) delegate
                {
                    this.conversations.ItemsSource =
                        RecentConversationProvider.loadRecentConversations()
                        .Where(c => c.IsValid && conversations.Contains(c))
                        .Reverse()
                        .Take(6);

                };
            if (Thread.CurrentThread != Dispatcher.Thread)
                Dispatcher.Invoke(doRecent);
            else
                doRecent();
        }

        public void List(IEnumerable<SandRibbonObjects.ConversationDetails> conversations)
        {
            var doList = (Action)delegate
            {
                rawConversationList = conversations.ToList();
                var list = new List<ConversationDetails>();
                var myConversations = conversations.Where(c => c.Author == me).OrderBy(c => c.LastAccessed.Date).Reverse().Take(2).ToList();
                if (myConversations.Count() > 0)
                {
                    list.Add(new SeparatorConversation("My Conversations"));
                    list.AddRange(myConversations);
                }
                list.Add(new SeparatorConversation("Conversations I've worked in"));
                var recentConversations = RecentConversationProvider.loadRecentConversations().Where(c => c.IsValid && conversations.Contains(c)).Reverse().Take(2);
                list.AddRange(recentConversations);
                var recentAuthors = list.Select(c => c.Author).Where(c => c != me).Distinct().ToList();
                foreach (var author in recentAuthors)
                {
                    var otherConversationsByThisAuthor = conversations.Where(c => c.IsValid && !list.Contains(c) && c.Author == author).Reverse();
                    if (otherConversationsByThisAuthor.Count() > 0)
                    {
                        list.Add(new SeparatorConversation(string.Format("{0}'s other conversations:", author)));
                        list.AddRange(otherConversationsByThisAuthor.Take(2));
                    }
                }
                this.conversations.ItemsSource = list;
            };
            if (Thread.CurrentThread != Dispatcher.Thread)
                Dispatcher.Invoke(doList);
            else
                doList();
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