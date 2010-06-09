using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using SandRibbon.Components.Sandpit;
using System.Windows.Input;
using SandRibbon.Components.Pedagogicometry;
using Constants;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbonObjects;
using SandRibbon.Utils;

namespace SandRibbon.Providers
{
    public class WorkspaceStateProvider
    {
        private static readonly string WORKSPACE_DIRECTORY = "Workspace";
        private static readonly string WORKSPACE_SAVE_FILE = string.Format(@"{0}\state.xml",WORKSPACE_DIRECTORY);
        private static readonly string WORKSPACE_ROOT_ELEMENT = "workspace";
        private static readonly string WORKSPACE_PREFERENCE_ELEMENT = "preference";
        public static readonly string WORKSPACE_COMMAND_ATTRIBUTE = "command";
        public static readonly string WORKSPACE_PARAMETER_ELEMENT = "parameter";
        private static readonly IEnumerable<ICommand> workspaceCommands = new ICommand[]{
            Commands.SetPedagogyLevel,
            Commands.SetIdentity,
            Commands.MoveTo,
            Commands.JoinConversation
        };
        public static void ensureWorkspaceDirectoryExists() { 
            if (!Directory.Exists(WORKSPACE_DIRECTORY)) {
                Directory.CreateDirectory(WORKSPACE_DIRECTORY);
            }
            if (!File.Exists(WORKSPACE_SAVE_FILE)){
                new XElement(WORKSPACE_ROOT_ELEMENT).Save(WORKSPACE_SAVE_FILE);
            } 
        }
        public static void ClearSettings()
        {
            ensureWorkspaceDirectoryExists();
            try
            {
                File.Delete(WORKSPACE_SAVE_FILE);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static void RestorePreviousSettings()
        {
            App.Now("Restoring settings");
            ensureWorkspaceDirectoryExists();
            var savedWorkspace = XElement.Load(WORKSPACE_SAVE_FILE);
            foreach (var element in savedWorkspace.Descendants(WORKSPACE_PREFERENCE_ELEMENT))
            {
                var which = element.Attribute(WORKSPACE_COMMAND_ATTRIBUTE).Value;
                var param = element.Element(WORKSPACE_PARAMETER_ELEMENT);
                switch (which) { 
                    case "SetPedagogyLevel":
                        //CommandParameterProvider.parameters[Commands.SetPedagogyLevel] = (Pedagogicometer.level(Int32.Parse(param.Value)));
                        CommandParameterProvider.parameters[Commands.SetPedagogyLevel] = Pedagogicometer.level(2);
                    break;
                    case "SetIdentity":
                        var values = (Crypto.decrypt(param.Attribute("authentication").Value)).Split(':');
                        Commands.SetIdentity.Execute(
                            new SandRibbon.Utils.Connection.JabberWire.Credentials{
                                name=values[0],
                                password=values[1],
                                authorizedGroups=new AuthorisationProvider().getEligibleGroups(values[0],values[1])
                            });
                    break;
                    case "JoinConversation":
                        Commands.JoinConversation.Execute(param.Attribute("conversation").Value);
                    break;
                        /*
                    case "MoveTo":
                        DelegateCommand<object> onJoin = null;
                        onJoin = new DelegateCommand<object>((details) =>
                        {
                            Commands.UpdateConversationDetails.UnregisterCommand(onJoin);
                            var slide = Int32.Parse(param.Attribute("slide").Value);
                            try
                            {
                                if (Globals.slide != slide)
                                    Commands.MoveTo.Execute(slide);
                            }
                            catch (NotSetException)
                            {
                                Commands.MoveTo.Execute(slide);
                            }
                        });
                        Commands.UpdateConversationDetails.RegisterCommand(onJoin);
                    break;
                         */
                }
            }
            App.Now("Finished restoring settings");
        }
        public static void SaveCurrentSettings() { 
            ensureWorkspaceDirectoryExists();
            var doc = XElement.Load(WORKSPACE_SAVE_FILE);
            foreach (var command in workspaceCommands)
            {
                XElement commandState = null;
                var commandName = Commands.which(command);
                    var currentState = doc.Descendants(WORKSPACE_PREFERENCE_ELEMENT).Where(e => 
                        e.Attribute(WORKSPACE_COMMAND_ATTRIBUTE).Value == commandName);
                if(currentState.Count() == 1)
                    commandState = currentState.Single();
                else
                {
                    commandState = new XElement(WORKSPACE_PREFERENCE_ELEMENT);
                    doc.Add(commandState);
                }
                commandState.RemoveAll();
                commandState.SetAttributeValue(WORKSPACE_COMMAND_ATTRIBUTE, commandName);
                try
                {
                    switch (commandName)
                    {
                        case "SetPedagogyLevel":
                            commandState.Add(new XElement(WORKSPACE_PARAMETER_ELEMENT, Globals.pedagogy.code));
                            break;
                        case "SetIdentity":
                            commandState.Add(new XElement(WORKSPACE_PARAMETER_ELEMENT,
                                new XAttribute("authentication", Crypto.encrypt(string.Format(@"{0}:{1}", Globals.credentials.name, Globals.credentials.password)))));
                            break;
                        case "JoinConversation":
                            commandState.Add(new XElement(WORKSPACE_PARAMETER_ELEMENT,
                                new XAttribute("conversation", Globals.conversationDetails.Jid)));
                            break;
                        case "MoveTo":
                            commandState.Add(new XElement(WORKSPACE_PARAMETER_ELEMENT,
                                new XAttribute("slide", Globals.slide)));
                            break;
                    }
                }
                catch (NotSetException) {
                    commandState.Remove();
                }
            }
            doc.Save(WORKSPACE_SAVE_FILE);
        }
    }
}
