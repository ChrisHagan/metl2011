using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using MeTLLib;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Automation.AutomationPeers;
using SandRibbon.Providers;
using SandRibbon.Providers.Structure;
using SandRibbonInterop.Interfaces;
//using SandRibbonObjects;
using MeTLLib.DataTypes;
using SandRibbon.Utils;

namespace SandRibbon.Components
{
    public partial class SimpleConversationSelector : UserControl, IConversationSelector, IConversationListing
    {
        public static IEnumerable<ConversationDetails> rawConversationList = new List<ConversationDetails>();
        public static IEnumerable<ConversationDetails> recentConversations = new List<ConversationDetails>();
        public SimpleConversationSelector()
        {
            InitializeComponent();
            this.conversations.ItemsSource = new List<ConversationDetails>();
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(joinConversation));
            Commands.UpdateForeignConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            Commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            RedrawList(null);
        }

        private void joinConversation(string jid)
        {
            var details = ClientFactory.Connection().DetailsOf(jid);
            details.LastAccessed = DateTime.Now;
            if (recentConversations.Where(c => c.Jid == jid).Count() > 0)
                recentConversations.Where(c => c.Jid == jid).First().LastAccessed = details.LastAccessed;
            else
            {
                recentConversations = recentConversations.Concat(new[] {details});
            }
            RecentConversationProvider.addRecentConversation(details, Globals.me);
            conversations.ItemsSource = recentConversations.OrderByDescending(c => c.LastAccessed).Take(6);
        }
        private void UpdateConversationDetails(ConversationDetails details)
        {
            if (recentConversations.Where(c => c.Jid == details.Jid).Count() == 0) return;
            if (details.Subject == "Deleted")
            {
                recentConversations.ToList().Remove(recentConversations.Where(c => c.Jid == details.Jid).First());
            }
            recentConversations.Where(c => c.Jid == details.Jid).First().Title = details.Title;
            conversations.ItemsSource = recentConversations.Take(6);
        }

        private void RedrawList(object _unused)
        {
            Dispatcher.adoptAsync(() =>
            {
                var potentialConversations = RecentConversationProvider.loadRecentConversations();
                if (potentialConversations != null && potentialConversations.Count() > 0)
                {
                    recentConversations = potentialConversations.Where(c => c.IsValid && c.Subject != "Deleted");
                    conversations.ItemsSource = recentConversations.Take(6);
                }
            });
        }
        private void doJoinConversation(object sender, ExecutedRoutedEventArgs e)
        {
            Commands.JoinConversation.ExecuteAsync((string)e.Parameter);
        }
        private void canJoinConversation(object sender, CanExecuteRoutedEventArgs e)
        {//CommandParameter is conversation title
            e.CanExecute = Commands.JoinConversation.CanExecute((string)e.Parameter);
        }
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ConversationListingAutomationPeer(this, "selector");
        }

        public void List(IEnumerable<ConversationDetails> conversations)
        {
        }

        public IEnumerable<string> List()
        {
            return new List<string>();
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
        public string Title
        {
            get { return base.Title; }
            set { base.Title = value; }
        }
        public SeparatorConversation(string label)
            : base(label == null ? "" : label, "", "", new List<Slide>(), new Permissions("", false, false, false), "")
        {
            if (label == null) label = String.Empty;
            Title = label;
        }
    }
}