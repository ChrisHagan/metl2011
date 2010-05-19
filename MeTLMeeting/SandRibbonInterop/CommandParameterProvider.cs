using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace SandRibbon.Providers
{
    public class CommandParameterProvider
    {
        private static object instanceLock = new object();
        private static CommandParameterProvider instance;
        public Dictionary<CompositeCommand, object> parameters = new Dictionary<CompositeCommand, object>();
        public static CommandParameterProvider provider(){
            lock (instanceLock)
                if (instance == null)
                    instance = new CommandParameterProvider();
            return instance;
        } 
        public CommandParameterProvider() 
        { 
            foreach(var command in typeof(Commands)
                .GetFields()
                .Where(f=>f.FieldType == typeof(CompositeCommand))
                .Select(f=>f.GetValue(typeof(Commands)))
                .Select(o=>(CompositeCommand)o))
            {
                command.RegisterCommand(new DelegateCommand<object>(
                    parameter=>
                        parameters[command] = parameter
                ));
            }
        }
    }
}
