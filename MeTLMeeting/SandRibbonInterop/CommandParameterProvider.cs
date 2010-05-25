using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Composite.Presentation.Commands;
namespace SandRibbon.Providers
{
    public class CommandParameterProvider
    {
        public static Dictionary<CompositeCommand, object> parameters = new Dictionary<CompositeCommand, object>();
        static CommandParameterProvider() 
        {
            RegisterToAllCommands();
        }
        public static void RegisterToAllCommands()
        {
            var ignoreCommands = new[] {
                Commands.ReceiveWormMove, 
                Commands.SendWormMove
            };
            var applicationCommands = typeof(Commands)
                .GetFields()
                .Where(f=>f.FieldType == typeof(CompositeCommand))
                .Select(f=>f.GetValue(null))
                .Select(o=>(CompositeCommand)o)
                .Where(hcc=>!ignoreCommands.Contains(hcc)).ToList();
            foreach(var command in applicationCommands)
            {
                var thisCommand = command;
                command.RegisterCommand(new DelegateCommand<object>(
                    parameter =>
                        parameters[thisCommand] = parameter
                ));
            }
        }
    }
}