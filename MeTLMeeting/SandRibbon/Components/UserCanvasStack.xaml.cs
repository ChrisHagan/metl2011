using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Documents;
using System.Windows.Ink;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Sandpit;
using SandRibbon.Components.Utility;
using SandRibbon.Providers;
using SandRibbonObjects;
using System.Windows.Controls;
using MeTLLib.DataTypes;

namespace SandRibbon.Components
{
    public partial class UserCanvasStack
    {
        public Grid stack;
        //private ClipboardManager clipboardManager = new ClipboardManager();

        public UserCanvasStack()
        {
            InitializeComponent();
            stack = canvasStack;
            handwriting.Disable();
            Commands.SetLayer.RegisterCommandToDispatcher<string>(new DelegateCommand<string>(SetLayer));
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<MeTLLib.DataTypes.Credentials>(loggedIn));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<MeTLLib.DataTypes.ConversationDetails>(UpdateConversationDetails));
            Commands.MoveTo.RegisterCommandToDispatcher(new DelegateCommand<object>(MoveTo));
            //Commands.ClipboardManager.RegisterCommand(new DelegateCommand<ClipboardAction>((action) => clipboardManager.OnClipboardAction(action)));

            /*clipboardManager.RegisterHandler(ClipboardAction.Paste, text.OnClipboardPaste, text.CanHandleClipboardPaste);
            clipboardManager.RegisterHandler(ClipboardAction.Paste, handwriting.OnClipboardPaste, handwriting.CanHandleClipboardPaste);
            clipboardManager.RegisterHandler(ClipboardAction.Paste, images.OnClipboardPaste, images.CanHandleClipboardPaste);*/
        }
        private List<Stroke> getStrokesRelevantTo(IEnumerable<String> ids)
        {
            return handwriting.Strokes.Where(s => ids.Contains(s.startingSum().ToString())).ToList();
        }
        private List<FrameworkElement> getChildrenRelevantTo(IEnumerable<String> ids)
        {
            var elements = images.Children.ToList();
            elements.AddRange(text.Children.ToList());
            return elements.ToList().Select(c =>
                ((FrameworkElement)c)).Where(c => c.Tag != null && ids.Contains(c.Tag.ToString()))
                .ToList();
        }
        private void loggedIn(MeTLLib.DataTypes.Credentials identity)
        {
            handwriting.Enable();
        }
        private void UpdateConversationDetails(MeTLLib.DataTypes.ConversationDetails details)
        {
            if (ConversationDetails.Empty.Equals(details)) return;
            Dispatcher.adoptAsync(delegate
            {
                var editingMode = isAuthor(details) || canStudentPublish(details);
                handwriting.SetCanEdit(editingMode);
                text.SetCanEdit(editingMode);
                images.SetCanEdit(editingMode);
            });
        }
        private bool isAuthor(MeTLLib.DataTypes.ConversationDetails details)
        {
            return details.Author == Globals.me ? true : false;
        }
        private bool canStudentPublish(MeTLLib.DataTypes.ConversationDetails details)
        {
            return details.Permissions.studentCanPublish;
        }
        public void SetEditable(bool canEdit)
        {
            handwriting.SetCanEdit(canEdit);
            images.SetCanEdit(canEdit);
            text.SetCanEdit(canEdit);
        }
        private void SetLayer(string newLayer)
        {
            if (handwriting.me.ToLower() == "projector") return;
            UIElement currentCanvas;
            switch (newLayer)
            {
                case "Text":
                    currentCanvas = text;
                    canvasStack.Children.Remove(text);
                    break;
                case "View":
                    currentCanvas = viewCanvas;
                    canvasStack.Children.Remove(viewCanvas);
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
            foreach (var layer in canvasStack.Children)
            {
                ((UIElement)layer).Opacity = .8;
            }
            if (currentCanvas == null) return;
            currentCanvas.Opacity = 1.0;
            canvasStack.Children.Add(currentCanvas);
        }
        private void MoveTo(object _unused)
        {
            UIElement elem = canvasStack.Children[canvasStack.Children.Count - 1];
            elem.Focus();
        }

        public void Flush()
        {
            ClearAdorners();
            handwriting.FlushStrokes();
            images.FlushImages();
            text.FlushText();
            viewCanvas.FlushDimensions();
        }
        protected void ClearAdorners()
        {
            Dispatcher.adopt((Action)delegate
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (adornerLayer == null) return;
                var adorners = adornerLayer.GetAdorners(this);
                if (adorners != null)
                    foreach (var adorner in adorners)
                        adornerLayer.Remove(adorner);
            });
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
        public UserCanvasStackAutomationPeer(UserCanvasStack stack)
            : base(stack)
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