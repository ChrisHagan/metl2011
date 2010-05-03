using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml.Linq;
using Divelements.SandRibbon;

namespace SandRibbon.Utils
{
    public class PluginManager
    {
        public static List<FrameworkElement> LoadPlugins(Dictionary<string, IAddChild> pluginRoot, IAddChild defaultPluginRoot)
        {
            try
            {
                return Manifest.Load().Select(plugin => plugin.Load(pluginRoot, defaultPluginRoot)).ToList();
            }
            catch (Exception)
            {
                return new List<FrameworkElement>();
            }
        }
    }
    class Manifest
    {
        private Dictionary<string, Plugin> plugins = new Dictionary<string, Plugin>();
        public static IEnumerable<Plugin> Load()
        {
            try
            {
                var activePlugins = new List<Plugin>();
                var pluginsPath = @"plugins\";
                if (!Directory.Exists(pluginsPath))
                    Directory.CreateDirectory(pluginsPath);
                var manifestFileName = pluginsPath + "manifest.xml";
                XDocument manifestDocument;
                if (!File.Exists(manifestFileName))
                    manifestDocument = new XDocument(new XElement("plugins"));
                else
                    manifestDocument = XDocument.Load(manifestFileName);
                var newPlugins = Directory.GetDirectories(pluginsPath)
                    .Select(fullName => fullName.Split('\\')[1])
                    .Where(shortName => manifestDocument.Root.Element(shortName) == null);
                foreach (var newPlugin in newPlugins)
                    manifestDocument.Root.Add(
                        new XElement(newPlugin,
                            new XAttribute("Enabled", "true")));
                manifestDocument.Save(manifestFileName);
                return (from p in manifestDocument.Root.Elements()
                        where "true" == p.Attribute("Enabled").Value
                        select new Plugin
                        {
                            name = p.Name.LocalName,
                            enabled = p.Attribute("Enabled").Value == "true"
                        })
                       .ToList();
            }
            catch (Exception) {
                //Load testing tends to hit these resources simultaneously
                return new List<Plugin>();
            }
        }
    }
    class Plugin
    {
        public bool enabled;
        public bool loaded;
        public int version;
        public string name;
        public string root;
        public FrameworkElement Load(Dictionary<String, IAddChild> parent, IAddChild defaultPluginRoot)
        {
            root = "plugins\\" + name;
            validateThisPlugin();
            loadDependencies();
            loadMainClass();
            return insertPlugin(parent, defaultPluginRoot);
        }
        private void validateThisPlugin()
        {
            validateLibDirectoryExists();
            validateMainAssemblyExists();
            validatePluginDownMessageExists();
        }
        private void validateLibDirectoryExists()
        {
            if (!Directory.Exists(root))
                throw new InvalidPluginFormatException(name, "This plugin does not reside in a directory named " + name);
        }
        private void validateMainAssemblyExists()
        {
            if (!File.Exists(root + "\\" + name + ".dll"))
                throw new InvalidPluginFormatException(name, "This plugin does not supply a main UserControl in an assembly called <pluginName>.dll");
        }
        private void validatePluginDownMessageExists()
        {
            if (!File.Exists(root + "\\pluginDown.txt"))
                throw new InvalidPluginFormatException(name, "This plugin does not supply a file containing a support message for plugin failure called pluginDown.txt");
        }
        private void loadDependencies()
        {
            if (Directory.GetDirectories(root, "lib").Length > 0)
                foreach (var dependency in Directory.GetFiles(root + "\\lib", "*.dll"))
                {
                    try
                    {
                        System.Reflection.Assembly.LoadFrom(dependency);
                    }
                    catch (Exception e)
                    {
                        throw new InvalidPluginFormatException(name,
                                                               "This plugin has failed to load a dependency (" +
                                                               dependency + ") correctly, failing with the error: " +
                                                               e.Message);
                    }
                }
        }

        private void loadMainClass()
        {
            try
            {
                System.Reflection.Assembly.LoadFrom(root + "\\" + name + ".dll");
            }
            catch (Exception e)
            {
                throw new InvalidPluginFormatException(name, "This plugin has failed to load the assembly containing its main class correctly, failing with the error: " + e.Message);
            }
        }
        private FrameworkElement insertPlugin(Dictionary<String, IAddChild> pluginRoots, IAddChild defaultPluginRoot)
        {
            try
            {
                var assembly = System.Reflection.Assembly.LoadFrom(root + "\\" + name + ".dll");
                var pluginMain = assembly.GetType(name + ".PluginMain");
                var pluginInstance = (UserControl)pluginMain.GetConstructor(new Type[0]).Invoke(new object[0]);
                var pluginGroup = new RibbonGroup();
                if (pluginMain.GetFields().Where(f => f.Name == "preferredTab").Count() == 1)
                {
                    var preferredTab = (string)pluginMain.GetField("preferredTab").GetValue(pluginInstance);
                    if (preferredTab == "PenTools" || preferredTab == "Navigate")
                    {
                        pluginRoots[preferredTab].AddChild(pluginInstance);
                        return pluginInstance;
                    }
                    if (pluginRoots.ContainsKey(preferredTab))
                    {
                        pluginGroup.Header = name;
                        pluginGroup.Items.Add(pluginInstance);
                        pluginRoots[preferredTab].AddChild(pluginGroup);
                    }
                    else
                    {
                        throw new InvalidPluginFormatException(name, "This plugin has requested to live in a tab which does not exist: " + preferredTab + ".  Available tabs are: " + pluginRoots.Keys.Aggregate("", (key, acc) => key + " " + acc));
                    }
                }
                else
                {
                    //Uncomment these lines to make the plugin live in a RibbonGroup that will collapse at priority 9, go to small at 8 and go to medium at 7.
                    //pluginGroup.Variants.Add(new GroupVariant(pluginGroup,RibbonGroupVariant.Collapsed,9));
                    //pluginGroup.Variants.Add(new GroupVariant(pluginGroup,RibbonGroupVariant.Small,8));
                    //pluginGroup.Variants.Add(new GroupVariant(pluginGroup,RibbonGroupVariant.Medium,7));
                    pluginGroup.Header = name;
                    pluginGroup.Items.Add(pluginInstance);
                    defaultPluginRoot.AddChild(pluginGroup);
                }
                return pluginInstance;
            }
            catch (Exception e)
            {
                throw new InvalidPluginFormatException(name, "This plugin has failed to instantiate its main class (which must be named PluginMain) correctly, failing with the error: " + e.Message);
            }
        }
    }
    class InvalidPluginFormatException : Exception
    {
        public InvalidPluginFormatException(string name, string message) : base(name + ": " + message) { }
    }
}
