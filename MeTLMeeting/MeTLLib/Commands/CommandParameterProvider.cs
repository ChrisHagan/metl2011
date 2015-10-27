using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace MeTLLib
{
    class CommandParameterProvider
    {
        public static Dictionary<CompositeCommand, object> parameters = new Dictionary<CompositeCommand, object>();
        public CommandParameterProvider(ClientCommands commands) 
        {
            RegisterToAllCommands(commands);
        }
        public void RegisterToAllCommands(ClientCommands commands)
        {
            var applicationCommands = typeof(ClientCommands)
                .GetFields()
                .Where(f=>f.FieldType == typeof(CompositeCommand))
                .Select(f=>f.GetValue(commands))
                .Select(o=>(CompositeCommand)o)
                .ToList();
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