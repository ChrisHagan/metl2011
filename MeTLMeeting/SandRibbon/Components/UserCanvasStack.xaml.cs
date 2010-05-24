using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Sandpit;
using SandRibbon.Components.Utility;
using SandRibbon.Providers;
using SandRibbonInterop;
using SandRibbonInterop.MeTLStanzas;
using SandRibbonObjects;
using System.Windows.Controls;

namespace SandRibbon.Components
{
    public partial class UserCanvasStack
    {
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
            Commands.ReceiveNewBubble.RegisterCommand(new DelegateCommand<TargettedBubbleContext>(
                ReceiveNewBubble));
        }
        public void ReceiveNewBubble(TargettedBubbleContext context) {
            if(context.target != handwriting.target) return;
            Console.WriteLine("Received Bubble");
            var bubble = getBubble(context);
            Dispatcher.BeginInvoke((Action) delegate
                                    {
                                        if (bubble != null)
                                        {
                                            var adornerLayer = AdornerLayer.GetAdornerLayer(this);
                                            adornerLayer.Add(UIAdorner.InCanvas(this, bubble, bubble.position));
                                        }
                                    });
        }
        private ThoughtBubble getBubble(TargettedBubbleContext bubble)
        {
            if (Globals.conversationDetails == null) return null;
            var thoughtBubble = new ThoughtBubble();
            Dispatcher.Invoke((Action) delegate
               {
                        var ids = bubble.context.Select(c => c.id);
                        var relevantStrokes = getStrokesRelevantTo(ids);
                        var relevantChildren = getChildrenRelevantTo(ids);
                        if (relevantChildren.Count > 0 || relevantStrokes.Count > 0)
                        {
                            thoughtBubble = new ThoughtBubble
                                                    {
                                                        childContext = relevantChildren,
                                                        strokeContext = relevantStrokes,
                                                        parent = bubble.slide,
                                                        conversation = Globals.conversationDetails.Jid,
                                                        room = bubble.thoughtSlide
                                                    };
                            thoughtBubble.relocate();
                            thoughtBubble.enterBubble();

                        }
               });
            return thoughtBubble.conversation != null ? thoughtBubble : null;
        }
        private List<Stroke> getStrokesRelevantTo(IEnumerable<String> ids)
        {
            return handwriting.Strokes.Where(s=>ids.Contains(s.startingSum().ToString())).ToList();
        }
        private List<FrameworkElement> getChildrenRelevantTo(IEnumerable<String> ids)
        {
            var elements = images.Children.ToList();
            elements.AddRange(text.Children.ToList());
            return elements.ToList().Select(c => 
                ((FrameworkElement)c)).Where(c => c.Tag != null && ids.Contains(c.Tag.ToString()))
                .ToList();
        }
        private void loggedIn(SandRibbon.Utils.Connection.JabberWire.Credentials identity)
        {
            handwriting.Enable();
            Commands.SetTutorialVisibility.Execute(System.Windows.Visibility.Visible);
        }
        private void UpdateConversationDetails(ConversationDetails details)
        {
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
            return details.Author == Globals.me ? true : false;
        }
        private bool canStudentPublish(ConversationDetails details)
        {
            return details.Permissions.studentCanPublish;
        }
        public void MoveTo(int slide)
        {
            Flush();
        }
        public void SetEditable(bool canEdit)
        {
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