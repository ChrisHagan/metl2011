using System;
using System.Threading;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Documents;
using System.Windows.Input;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbonObjects;
using System.Windows.Controls;

namespace SandRibbon.Components
{
    public partial class UserCanvasStack
    {
        private string me;
        private int currentSlide;
        private string currentConversation;
        private ConversationDetails details;
        public Grid stack;
        public UserCanvasStack()
        {
            InitializeComponent();
            stack = canvasStack;
            handwriting.Disable();
            Commands.SetLayer.RegisterCommand(new DelegateCommand<string>(setTopLayer));
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<SandRibbon.Utils.Connection.JabberWire.Credentials>(loggedIn));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>(MoveTo));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            Commands.SetLayer.Execute("Sketch");
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(JoinConversation));
        }
        private void JoinConversation(string jid)
        {
            currentConversation = jid;
        }
        private void loggedIn(SandRibbon.Utils.Connection.JabberWire.Credentials identity)
        {
            me = identity.name;
            handwriting.Enable();
            Commands.SetTutorialVisibility.Execute(System.Windows.Visibility.Visible);
        }
        private void UpdateConversationDetails(ConversationDetails details)
        {
           if (details.Jid != currentConversation) return;
           this.details = details;
           Dispatcher.BeginInvoke((Action) delegate
           {
               var editingMode = isAuthor(details) || canStudentPublish(details);
               handwriting.SetCanEdit(editingMode);
               text.SetCanEdit(editingMode);
               images.SetCanEdit(editingMode);
           });
        }
        private bool isAuthor(ConversationDetails details)
        {
            return details.Author == me ? true : false;
        }
        private bool canStudentPublish(ConversationDetails details)
        {
            return details.Permissions.studentCanPublish;
        }
        public void MoveTo(int slide)
        {
            Flush();
            currentSlide = slide;
        }
        public void SetIdentity(SandRibbon.Utils.Connection.JabberWire.UserInformation info, bool canEdit)
        {
            me = info.credentials.name;
            if (info.location == null)
                info.location = new SandRibbon.Utils.Connection.JabberWire.Location { currentSlide = currentSlide };
            handwriting.SetIdentity(info);
            images.SetIdentity(info);
            text.SetIdentity(info);
            handwriting.SetCanEdit(canEdit);
            images.SetCanEdit(canEdit);
            text.SetCanEdit(canEdit);
        }
        private void setTopLayer(string newLayer)
        {
            UIElement currentCanvas;
            switch (newLayer)
            {
                case "Text":
                    currentCanvas = text;
                    canvasStack.Children.Remove(text);
                    break;
                case "Insert":
                    currentCanvas = images;
                    canvasStack.Children.Remove(images);
                    break;
                default: //default is to always have the inkcanvas hittable
                    currentCanvas = handwriting;
                    canvasStack.Children.Remove(handwriting);
                    break;
            }
            foreach(var layer in canvasStack.Children)
            {
                ((UIElement) layer).Opacity = .8;    
            }
            if (currentCanvas == null) return;
            currentCanvas.Opacity = 1.0;
            canvasStack.Children.Add(currentCanvas);
        }
        public void Flush()
        {
            ClearAdorners();
            handwriting.FlushStrokes();
            images.FlushImages();
            text.FlushText();
        }
        protected void ClearAdorners()
        {
            var doClear = (Action)delegate
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (adornerLayer == null) return;
                var adorners = adornerLayer.GetAdorners(this);
                if (adorners != null)
                    foreach (var adorner in adorners)
                        adornerLayer.Remove(adorner);
            };
            if (Thread.CurrentThread != Dispatcher.Thread)
                Dispatcher.Invoke(doClear);
            else
                doClear();
        }
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new UserCanvasStackAutomationPeer(this);
        }
    }
    class UserCanvasStackAutomationPeer : FrameworkElementAutomationPeer
    {
        private static string AUTOMATION_ID = "UserCanvasStack";
        private string target { get { return UserCanvasStack().handwriting.target; } }
        public UserCanvasStackAutomationPeer(UserCanvasStack stack) : base(stack)
        {
        }
        public UserCanvasStack UserCanvasStack()
        {
            return (UserCanvasStack)base.Owner;
        }
        protected override string GetAutomationIdCore()
        {
            return target;
        }
        protected override string GetClassNameCore()
        {
            return AUTOMATION_ID;
        }
        protected override bool HasKeyboardFocusCore()
        {
            return true;
        }
    }
}