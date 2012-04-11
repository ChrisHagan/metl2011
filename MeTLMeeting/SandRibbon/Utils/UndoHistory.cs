using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components;

namespace SandRibbon.Utils
{
    public partial class UndoHistory
    {
        public class HistoricalAction
        {
            public Action undo;
            public Action redo;
            public long time;
            public string description;

            public HistoricalAction(Action undo, Action redo, long time, string description)
            {
                this.undo = undo;
                this.redo = redo;
                this.time = time;
                this.description = description;
            }
        }
        private static Dictionary<int, Stack<HistoricalAction>> undoQueue = new Dictionary<int,Stack<HistoricalAction>>();
        private static Dictionary<int, Stack<HistoricalAction>> redoQueue = new Dictionary<int,Stack<HistoricalAction>>();
        private static int currentSlide;
        private static UndoHistoryVisualiser visualiser; 
        static UndoHistory()
        {
            Commands.Undo.RegisterCommand(new DelegateCommand<object>(Undo, CanUndo));
            Commands.Redo.RegisterCommand(new DelegateCommand<object>(Redo, CanRedo));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>(
                i =>
                {
                    currentSlide = i;
                    RaiseQueryHistoryChanged();
                    visualiser.ClearViews();
                }
            ));

            visualiser = new UndoHistoryVisualiser();
        }
        public static void Queue(Action undo, Action redo, String description, bool changeContentVisibility = false)
        {
#if TOGGLE_CONTENT
            if (!changeContentVisibility)
            {
                var setMineVisible = Commands.SetContentVisibility.IsInitialised ? (ContentVisibilityEnum)Commands.SetContentVisibility.LastValue() : ContentVisibilityEnum.NoneVisible | ContentVisibilityEnum.MineVisible;
                // TODO: Change this to a different command that doesn't add the visibility state to the undo queue
                Commands.SetContentVisibility.Execute(setMineVisible);
            }
#endif

            foreach(var queue in new[]{undoQueue, redoQueue})
                if(!queue.ContainsKey(currentSlide)) 
                    queue.Add(currentSlide, new Stack<HistoricalAction>());
            
            var newAction = new HistoricalAction(undo,redo, DateTime.Now.Ticks, description); 
            undoQueue[currentSlide].Push(newAction);
            visualiser.UpdateUndoView(undoQueue[currentSlide]);

            RaiseQueryHistoryChanged();
        }
        private static void RaiseQueryHistoryChanged()
        {
            Commands.RequerySuggested(Commands.Undo, Commands.Redo);
        }
        private static bool CanUndo(object _param) 
        { 
            return undoQueue.ContainsKey(currentSlide) && undoQueue[currentSlide].Count() > 0; 
        }

        internal static void Undo(object param)
        {
            if (CanUndo(param))
            {
                var head = undoQueue[currentSlide].Pop();
                visualiser.UpdateUndoView(undoQueue[currentSlide]);
                head.undo.Invoke();
                redoQueue[currentSlide].Push(head);
                visualiser.UpdateRedoView(redoQueue[currentSlide]);
                RaiseQueryHistoryChanged();
            }
        }
        private static bool CanRedo(object _param)
        {
            return redoQueue.ContainsKey(currentSlide) && redoQueue[currentSlide].Count() > 0;
        }
        private static void Redo(object param)
        {
            if (CanRedo(param))
            {
                var head = redoQueue[currentSlide].Pop();
                visualiser.UpdateRedoView(redoQueue[currentSlide]);
                head.redo.Invoke();
                undoQueue[currentSlide].Push(head);
                visualiser.UpdateUndoView(undoQueue[currentSlide]);
                RaiseQueryHistoryChanged();
            }
        }

    }
}
