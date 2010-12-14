using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Microsoft.Practices.Composite.Presentation.Commands;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Xml.Linq;
using MeTLLib.Providers;

namespace MeTLLib
{
    static class CompositeCommandExtensions
    {
        public static object lastValue(this CompositeCommand command)
        {
            if (CommandParameterProvider.parameters.ContainsKey(command))
                return CommandParameterProvider.parameters[command];
            throw new NotSetException(Commands.which(command));
        }
    }
    class NotSetException : Exception
    {
        public NotSetException(string command)
            : base(command)
        {

        }
    }
    class Commands
    {
        #region CommandInstatiation
        public static CompositeCommand WakeUpBoards = new CompositeCommand();
        public static CompositeCommand SleepBoards = new CompositeCommand();
        public static CompositeCommand MirrorVideo = new CompositeCommand();
        public static CompositeCommand VideoMirrorRefreshRectangle = new CompositeCommand();
        public static CompositeCommand SendWakeUp = new CompositeCommand();
        public static CompositeCommand SendSleep = new CompositeCommand();
        public static CompositeCommand ReceiveWakeUp = new CompositeCommand();
        public static CompositeCommand ReceiveSleep = new CompositeCommand();
        public static CompositeCommand SendMoveBoardToSlide = new CompositeCommand();
        public static CompositeCommand ReceiveMoveBoardToSlide = new CompositeCommand();
        public static CompositeCommand CloseBoardManager = new CompositeCommand();
        public static CompositeCommand SendPing = new CompositeCommand();
        public static CompositeCommand ReceivePong = new CompositeCommand();
        public static CompositeCommand ServersDown = new CompositeCommand();
        public static CompositeCommand NotImplementedYet = new CompositeCommand();
        public static CompositeCommand SendWormMove = new CompositeCommand();
        public static CompositeCommand ReceiveWormMove = new CompositeCommand();
        public static CompositeCommand SyncedMoveRequested = new CompositeCommand();
        public static CompositeCommand SendSyncMove = new CompositeCommand();
        public static CompositeCommand SendDirtyConversationDetails = new CompositeCommand();
        public static CompositeCommand UpdateConversationDetails = new CompositeCommand();
        public static CompositeCommand AllContentSent = new CompositeCommand();
        #endregion
        Commands()
        {
            NotImplementedYet.RegisterCommand(new DelegateCommand<object>((_param) => { }, (_param) => false));
        }
        public static int HandlerCount
        {
            get
            {
                return all.Aggregate(0, (acc, item) => acc += item.RegisteredCommands.Count());
            }
        }
        private static List<ICommand> staticHandlers = new List<ICommand>();
        public static void AllStaticCommandsAreRegistered()
        {
            foreach (var command in all)
            {
                foreach (var handler in command.RegisteredCommands)
                    staticHandlers.Add(handler);
            }
        }
        private static IEnumerable<CompositeCommand> all
        {
            get
            {
                return typeof(Commands).GetFields()
                    .Where(p => p.FieldType == typeof(CompositeCommand))
                    .Select(f => (CompositeCommand)f.GetValue(null));
            }
        }
        public static IEnumerable<ICommand> allHandlers()
        {
            var handlers = new List<ICommand>();
            foreach (var command in all)
                foreach (var handler in command.RegisteredCommands)
                    handlers.Add(handler);
            return handlers.ToList();
        }
        public static void UnregisterAllCommands()
        {
            foreach (var command in all)
                foreach (var handler in command.RegisteredCommands)
                    if (!staticHandlers.Contains(handler))
                        command.UnregisterCommand(handler);
        }
        public static string which(ICommand command)
        {
            foreach (var field in typeof(Commands).GetFields())
                if (field.GetValue(null) == command)
                    return field.Name;
            return "Not a member of commands";
        }
        public static CompositeCommand called(string name)
        {
            return (CompositeCommand)typeof(Commands).GetField(name).GetValue(null);
        }
        public static void RequerySuggested()
        {
            RequerySuggested(all.ToArray());
        }
        public static void RequerySuggested(params CompositeCommand[] commands)
        {
            foreach (var command in commands)
                Requery(command);
        }
        private static void Requery(CompositeCommand command)
        {
            if (command.RegisteredCommands.Count() > 0)
            {
                var delegateCommand = command.RegisteredCommands[0];
                delegateCommand.GetType().InvokeMember("RaiseCanExecuteChanged", BindingFlags.InvokeMethod, null, delegateCommand, new object[] { });
            }
        }
    }
}
