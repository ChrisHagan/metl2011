using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;

namespace SandRibbon.Components.SimpleImpl
{
    /// <summary>
    /// Interaction logic for FeatureListing.xaml
    /// </summary>
    public partial class FeatureListing : UserControl
    {
        public static RoutedCommand FeatureFinalized = new RoutedCommand();
        public FeatureListing()
        {
            InitializeComponent();
            PopulateFeatureList();
        }
        ObservableCollection<FeatureEntry> FeatureList = new ObservableCollection<FeatureEntry>();
        private void PopulateFeatureList()
        {
            var manifestFileName = getFeatureManifestPath();
            if (!File.Exists(manifestFileName))
            {
                App.auditor.log("Cannot find the manifest file");
                return;
            }
            XDocument manifestDocument = XDocument.Load(manifestFileName);
            foreach(var feature in manifestDocument.Descendants("features").Single().Descendants())
            {
                Brush status = Boolean.Parse(feature.Attribute("Finalized").Value) ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
                FeatureList.Add(new FeatureEntry
                                    {
                                        FeatureName= feature.Name.LocalName.Replace('_', ' '), 
                                        Finalized = Boolean.Parse(feature.Attribute("Finalized").Value),
                                        FeatureStatus = status
                                    });
            }
            featureListing.ItemsSource = FeatureList;
        }
        private void manifestExists(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = File.Exists(getFeatureManifestPath());
        }
        private void UpdateFeatureListing(object sender, ExecutedRoutedEventArgs e)
        {
            var manifestFileName = getFeatureManifestPath();

            var newManifest = new XDocument(new XElement("features"));
            if (featureListing.ItemsSource != null)
            {
                foreach (var feature in featureListing.ItemsSource)
                {
                    var name = ((FeatureEntry) feature).FeatureName;
                    var finalized = ((FeatureEntry) feature).Finalized;
                    updateCollection(name, finalized);
                    newManifest.Root.Add(
                        new XElement(name.Replace(' ', '_'),
                                     new XAttribute("Finalized", ((FeatureEntry) feature).Finalized)));
                }
            }
            if(newFeature.Text != "")
            {
                var newFeatureName = newFeature.Text.Replace(' ', '_');
                newManifest.Root.Add(
                    new XElement(newFeatureName,
                                 new XAttribute("Finalized", false)));
                FeatureList.Add(new FeatureEntry
                                    {
                                        FeatureName = newFeature.Text,
                                        Finalized = false,
                                        FeatureStatus = Brushes.Red
                                    });
                newFeature.Text = "";
            }
            newManifest.Save(manifestFileName);
        }
        private void updateCollection(string name, bool finalized)
        {
            foreach(var feature in FeatureList)
            {
                if(feature.FeatureName == name)
                {
                    feature.FeatureName = name.Replace('_', ' ');
                    feature.Finalized = finalized;
                    feature.FeatureStatus = finalized ? Brushes.Green : Brushes.Red;
                }
            }
        }
        private static string getFeatureManifestPath()
        {
            var featurePath = @"..\..\feature\";
            if (!Directory.Exists(featurePath))
                Directory.CreateDirectory(featurePath);
            return featurePath  + "manifest.xml";
        }

        private void alwaysTrue(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
    }
    public class FeatureEntry: DependencyObject
    {
        public string FeatureName { get; set;}
        public bool Finalized { get; set;}
        public Brush FeatureStatus { get; set; }
    }

}
