using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using SandRibbon;


namespace PluginInterfaceManager
{
    public partial class PluginMain : UserControl
    {
        public static RoutedCommand PluginFinalized = new RoutedCommand();
        public PluginMain()
        {
            InitializeComponent();
            PopulatePluginList();
        }
        private void PopulatePluginList()
        {
            var PluginList = new ObservableCollection<PluginEntry>();
            var manifestFileName = getPluginManifestPath();
            if (!File.Exists(manifestFileName))
            {
                Console.WriteLine("Cannot find the manifest file");
                return;
            }
            XDocument manifestDocument = XDocument.Load(manifestFileName);
            foreach(var plugin in manifestDocument.Descendants("plugins").Single().Descendants())
            {
                PluginList.Add(new PluginEntry
                                    {
                                        PluginName = plugin.Name.LocalName, 
                                        Enabled = Boolean.Parse(plugin.Attribute("Enabled").Value)
                                    });
            }
            pluginListing.ItemsSource = PluginList;

        }

        private void manifestExists(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = File.Exists(getPluginManifestPath());
        }


        private void UpdatePluginManifest(object sender, ExecutedRoutedEventArgs e)
        {
            var manifestFileName = getPluginManifestPath();

            var newManifest = new XDocument(new XElement("plugins"));
            foreach(var plugin in pluginListing.ItemsSource)
            {
                newManifest.Root.Add(
                    new XElement(((PluginEntry) plugin).PluginName,
                                 new XAttribute("Enabled", ((PluginEntry) plugin).Enabled)));

            }
            newManifest.Save(manifestFileName);
            MessageBox.Show("Closing MeTL.  Your new plugin configuration will be active on restart.");
            Commands.CloseApplication.Execute(null,this);
        }

        private static string getPluginManifestPath()
        {
            var pluginsPath = @".\plugins\";
            return pluginsPath + "manifest.xml";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PluginPopup.IsOpen = true;
        }
    }
    public class PluginEntry: DependencyObject
    {
        public string PluginName { get; set;}
        public bool Enabled { get; set;} 
    }
}





