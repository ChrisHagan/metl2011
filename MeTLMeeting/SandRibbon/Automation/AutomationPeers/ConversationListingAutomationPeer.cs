using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using SandRibbonInterop.Interfaces;

namespace SandRibbon.Automation.AutomationPeers
{
    public class ConversationListingAutomationPeer : FrameworkElementAutomationPeer, ISelectionProvider
    {
        private string label;
        public ConversationListingAutomationPeer(IConversationListing parent, string label)
            : base((FrameworkElement)parent)
        {
            this.label = label;
        }
        protected override string GetAutomationIdCore()
        {
            return string.Format("conversationList{0}", label);
        }
        protected override string GetClassNameCore()
        {
            return base.Owner.GetType().Name;
        }
        protected override bool HasKeyboardFocusCore()
        {
            return true;
        }
        public bool CanSelectMultiple
        {
            get { return false; }
        }
        public IRawElementProviderSimple[] GetSelection()
        {
            return ((IConversationListing)base.Owner).List().Select(c=>new ConversationTitle((IConversationListing)base.Owner,c)).ToArray();
        }
        public bool IsSelectionRequired
        {
            get { return false; }
        }
    }
    class ConversationTitle : ISelectionItemProvider, IRawElementProviderSimple
    {
        private string title;
        private IConversationListing parent;
        public ConversationTitle(IConversationListing parent, string title)
        {
            this.title = title;
            this.parent = parent;
        }
        public void AddToSelection()
        {
            throw new NotImplementedException();
        }
        public bool IsSelected
        {
            get { throw new NotImplementedException(); }
        }
        public void RemoveFromSelection()
        {
            throw new NotImplementedException();
        }
        public void Select()
        {
            throw new NotImplementedException();
        }
        public IRawElementProviderSimple SelectionContainer
        {
            get { throw new NotImplementedException(); }
        }
        public object GetPatternProvider(int patternId)
        {
            throw new NotImplementedException();
        }
        public object GetPropertyValue(int propertyId)
        {
            throw new NotImplementedException();
        }
        public IRawElementProviderSimple HostRawElementProvider
        {
            get { throw new NotImplementedException(); }
        }
        public ProviderOptions ProviderOptions
        {
            get { throw new NotImplementedException(); }
        }
    }
}