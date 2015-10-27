using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Utils;
using MeTLLib;

namespace SandRibbon.Providers
{
    public class WorkspaceStateProvider
    {
        private static readonly string WORKSPACE_DIRECTORY = LocalFileProvider.getUserFolder("Workspace");
        private static readonly string WORKSPACE_SAVE_FILE = LocalFileProvider.getUserFile(new string[]{"Workspace"},"state.xml");
        private static readonly string WORKSPACE_ROOT_ELEMENT = "workspace";
        private static readonly string WORKSPACE_PREFERENCE_ELEMENT = "preference";
        public static readonly string WORKSPACE_COMMAND_ATTRIBUTE = "command";
        public static readonly string WORKSPACE_PARAMETER_ELEMENT = "parameter";
        private static readonly IEnumerable<CompositeCommand> workspaceCommands = new CompositeCommand[]{
            Commands.SetPedagogyLevel,
            Commands.SetIdentity,
            Commands.RegisterPowerpointSourceDirectoryPreference
        };
        public static bool savedStateExists()
        {
            if (!Directory.Exists(WORKSPACE_DIRECTORY) || !File.Exists(WORKSPACE_SAVE_FILE)) return false;
            var xml = XElement.Load(WORKSPACE_SAVE_FILE);
            return workspaceCommands.Take(2).All(c=>xml.Descendants(WORKSPACE_PREFERENCE_ELEMENT).Where(node=>{
                var preference = node.Attribute(WORKSPACE_COMMAND_ATTRIBUTE);
                return preference != null && preference.Value == Commands.which(c);
            }).Count() > 0);
        }
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
            ensureWorkspaceDirectoryExists();
            var savedWorkspace = XElement.Load(WORKSPACE_SAVE_FILE);
            foreach (var element in savedWorkspace.Descendants(WORKSPACE_PREFERENCE_ELEMENT))
            {
                var which = element.Attribute(WORKSPACE_COMMAND_ATTRIBUTE).Value;
                var param = element.Element(WORKSPACE_PARAMETER_ELEMENT);
                switch (which) { 
                    case "SetPedagogyLevel":
                        var level = ConfigurationProvider.instance.getMeTLPedagogyLevel();
                        Commands.SetPedagogyLevel.DefaultValue = level;
                    break;
                    case "RegisterPowerpointSourceDirectoryPreference":
                        Commands.RegisterPowerpointSourceDirectoryPreference.Execute(param.Value);
                    break;
                }
            }
        }
        public static void SaveCurrentSettings() {
            if (!Globals.rememberMe) return;
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
                        case "RegisterPowerpointSourceDirectoryPreference":
                            if (Globals.rememberMe)
                            {
                                if (!Commands.RegisterPowerpointSourceDirectoryPreference.IsInitialised)
                                    commandState.Remove();
                                else
                                    commandState.Add(new XElement(WORKSPACE_PARAMETER_ELEMENT, Commands.RegisterPowerpointSourceDirectoryPreference.LastValue()));
                            }
                            break;
                    }
                }
                catch (NotSetException){
                    commandState.Remove();
                }
            }
            doc.Save(WORKSPACE_SAVE_FILE);
        }
    }
}
