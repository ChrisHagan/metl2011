using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace SandRibbon.Utils
{
    public class UndoHistory
    {
        private static long millisecondToTicks = 10000; 
        private class HistoricalAction
        {
            public Action undo;
            public Action redo;
            public long time;
            public HistoricalAction(Action undo, Action redo, long time)
            {
                this.undo = undo;
                this.redo = redo;
                this.time = time;
            }

            internal bool Merge(HistoricalAction newAction)
            {
                var action = this;
                var undo = action.undo;
                var redo = action.redo;
                Console.WriteLine(string.Format("{0} <= {1} <= {2}", action.time, newAction.time, (newAction.time + (30 * millisecondToTicks)))); 
                if (newAction.time >= action.time && newAction.time <= (action.time + (10 * millisecondToTicks)))
                {
                    this.undo = () => { undo(); newAction.undo(); };
                    this.redo = () => { redo(); newAction.redo(); };
                    this.time = newAction.time;
                    Console.WriteLine("Merged");
                    return true;
                }
                return false;
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
            var merged = false;
            var queueingTime = DateTime.Now.Ticks;
            var newAction =(new HistoricalAction(undo,redo, queueingTime)); 
            if (undoQueue[currentSlide].Count > 0)
            {
                var last = undoQueue[currentSlide].Peek();
                merged = last.Merge(newAction);
            }
            if(!merged)
                undoQueue[currentSlide].Push(newAction);
            Console.WriteLine(string.Format("undo queue has {0} items", undoQueue[currentSlide].Count()));
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
