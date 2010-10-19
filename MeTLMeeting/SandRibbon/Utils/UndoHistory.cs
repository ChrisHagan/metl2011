using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace SandRibbon.Utils
{
    public class UndoHistory
    {
        private class HistoricalAction
        {
            public Action undo;
            public Action redo;
            public HistoricalAction(Action undo, Action redo)
            {
                this.undo = undo;
                this.redo = redo;
            }
        }
        private static Dictionary<int, Stack<HistoricalAction>> undoQueue = new Dictionary<int,Stack<HistoricalAction>>();
        private static Dictionary<int, Stack<HistoricalAction>> redoQueue = new Dictionary<int,Stack<HistoricalAction>>();
        private static int currentSlide;
        public UndoHistory()
        {
            Commands.Undo.RegisterCommand(new DelegateCommand<object>(Undo, CanUndo));
            Commands.Redo.RegisterCommand(new DelegateCommand<object>(Redo, CanRedo));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>(
                i =>
                {
                    currentSlide = i;
                    RaiseQueryHistoryChanged();
                }
            ));
        }
        public static void Queue(Action undo, Action redo)
        {
            foreach(var queue in new[]{undoQueue, redoQueue})
                if(!queue.ContainsKey(currentSlide)) 
                    queue.Add(currentSlide, new Stack<HistoricalAction>());
            
            undoQueue[currentSlide].Push(new HistoricalAction(undo,redo));
            RaiseQueryHistoryChanged();
        }
        private static void RaiseQueryHistoryChanged()
        {
            Commands.RequerySuggested(Commands.Undo, Commands.Redo);
        }
        private static bool CanUndo(object _param) 
        { 
            return undoQueue.Keys.ToList().Contains(currentSlide) && undoQueue[currentSlide].Count() > 0; 
        }
        private static void Undo(object param)
        {
            if (CanUndo(param))
            {
                var head = undoQueue[currentSlide].Pop();
                head.undo.Invoke();
                redoQueue[currentSlide].Push(head);
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
                head.redo.Invoke();
                undoQueue[currentSlide].Push(head);
                RaiseQueryHistoryChanged();
            }
        }
    }
}
