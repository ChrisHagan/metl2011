using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SandRibbon.Components
{
    public enum ClipboardAction
    {
        Cut,
        Copy,
        Paste
    }

    public interface IClipboardHandler
    {
        bool CanHandleClipboardPaste();
        bool CanHandleClipboardCopy();
        bool CanHandleClipboardCut();

        void OnClipboardPaste();
        void OnClipboardCopy();
        void OnClipboardCut();
    }
    public class ClipboardManager
    {
        class Handler
        {
            public ClipboardAction ActionType;
            public Action ClipboardAction;
            public Func<bool> CanHandle;
        }

        private List<Handler> registeredHandlers = new List<Handler>();

        public void RegisterHandler(ClipboardAction action, Action onClipboardAction, Func<bool> canHandle)
        {
            registeredHandlers.Add(new Handler() { ActionType = action, ClipboardAction = onClipboardAction, CanHandle = canHandle });
        }

        public void ClearAllHandlers()
        {
            registeredHandlers.Clear();
        }

        public void OnClipboardAction(ClipboardAction action)
        {
            foreach (var handler in registeredHandlers.Where((byAction) => byAction.ActionType == action))
            {
                if (handler.CanHandle())
                    handler.ClipboardAction();
            }
        }
    }
}
