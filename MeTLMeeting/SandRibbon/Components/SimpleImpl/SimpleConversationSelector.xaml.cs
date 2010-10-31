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
//using SandRibbonObjects;
using MeTLLib.DataTypes;
using SandRibbon.Utils;

namespace SandRibbon.Components
{
    public partial class SimpleConversationSelector : UserControl, IConversationSelector, IConversationListing
    {
        public static IEnumerable<ConversationDetails> rawConversationList = new List<ConversationDetails>();
        public SimpleConversationSelector()
        {
            InitializeComponent();
            this.conversations.ItemsSource = new List<ConversationDetails>();
            Commands.CreateConversation.RegisterCommand(new DelegateCommand<object>((_details) => {}, doesConversationAlreadyExist));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<object>(RedrawList));
            Commands.UpdateForeignConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(updateForeignConversationDetails, canUpdateForeignConversationDetails));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            RedrawList(null);
        }
        private void UpdateConversationDetails(ConversationDetails details) { 
            if(details.Subject == "Deleted")
                RedrawList(null);
        } 
        private void updateForeignConversationDetails(ConversationDetails _obj)
        {
            RedrawList(null);
        }
        private bool canUpdateForeignConversationDetails(ConversationDetails details)
        {
            return rawConversationList.Where(c=>c.Title == details.Title || string.IsNullOrEmpty(details.Title)).Count() == 0;
        }
        private bool doesConversationAlreadyExist(object obj)
        {
            if (!(obj is ConversationDetails))
                return true;
            var details = (ConversationDetails)obj;
            if (details == null) 
                return true;
            if (details.Title.Length == 0) 
                return true;
            var currentConversations = MeTLLib.ClientFactory.Connection().AvailableConversations;
            bool conversationExists = currentConversations.Any(c=> c.Title.Equals(details.Title));
            Logger.Log(currentConversations.Aggregate("", (acc,item)=>acc+" "+item.Title));
            Logger.Log(string.Format("[{0}] already exists: {1}", details.Title, conversationExists));
            return !conversationExists;
        }
        private void RedrawList(object _unused)
        {
            Dispatcher.adopt(() =>
            {
                this.conversations.ItemsSource =
                    RecentConversationProvider.loadRecentConversations()
                    .Where(c => c.IsValid && c.Subject != "Deleted")
                    .Take(6);
            });
        }
        public void List(IEnumerable<ConversationDetails> conversations)
        {
            Dispatcher.adopt((Action)delegate
            {
                rawConversationList = conversations.ToList();
                var list = new List<ConversationDetails>();
                var myConversations = conversations.Where(c => c.Author == Globals.me).OrderBy(c => c.LastAccessed.Date).Reverse().Take(2).ToList();
                if (myConversations.Count() > 0)
                {
                    list.Add(new SeparatorConversation("My Conversations"));
                    list.AddRange(myConversations);
                }
                list.Add(new SeparatorConversation("Conversations I've worked in"));
                var recentConversations = RecentConversationProvider.loadRecentConversations().Where(c => c.IsValid && conversations.Contains(c)).Reverse().Take(2);
                list.AddRange(recentConversations);
                var recentAuthors = list.Select(c => c.Author).Where(c => c != Globals.me).Distinct().ToList();
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
            });
        }
        public IEnumerable<string> List()
        {
            return rawConversationList.Select(c => c.Title).ToList();
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
        public SeparatorConversation(string label) : base(label == null?"":label,"","",new List<Slide>(),new Permissions("",false,false,false),"")
        {
            if (label == null) label = String.Empty;
            Title = label;
        }
    }
}