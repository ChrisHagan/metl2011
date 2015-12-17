using MeTLLib.DataTypes;
using Microsoft.Win32;
using SandRibbon.Components;
using SandRibbon.Pages;
using SandRibbon.Pages.Collaboration;
using SandRibbon.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SandRibbon.Frame.Flyouts
{
    public class ppProgressInfo : DependencyObject
    {


        public string Category
        {
            get { return (string)GetValue(CategoryProperty); }
            set { SetValue(CategoryProperty, value); }
        }
        public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register("Category", typeof(string), typeof(ppProgressInfo), new PropertyMetadata(""));
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(ppProgressInfo), new PropertyMetadata(""));
        public int Count
        {
            get { return (int)GetValue(CountProperty); }
            set { SetValue(CountProperty, value); }
        }
        public static readonly DependencyProperty CountProperty = DependencyProperty.Register("Count", typeof(int), typeof(ppProgressInfo), new PropertyMetadata(0));
        public int Total
        {
            get { return (int)GetValue(TotalProperty); }
            set { SetValue(TotalProperty, value); }
        }
        public static readonly DependencyProperty TotalProperty = DependencyProperty.Register("Total", typeof(int), typeof(ppProgressInfo), new PropertyMetadata(0));
        public Visibility Visible {
            get { return (Visibility)GetValue(VisibleProperty); }
            set { SetValue(VisibleProperty, value); }
        }
        public static readonly DependencyProperty VisibleProperty = DependencyProperty.Register("Visible", typeof(Visibility), typeof(ppProgressInfo), new PropertyMetadata(Visibility.Collapsed));
        public ppProgressInfo(string category) {
            Category = category;
        }
        public ppProgressInfo(string category,string description,int count,int total) : this(category)
        {
            Description = description;
            Count = count;
            Total = total;
            if (count == total)
            {
                Visible = Visibility.Collapsed;
            }
            else
            {
                Visible = Visibility.Visible;
            }
        }
    }
    public partial class PowerpointLoaderFlyout : FlyoutCard
    {
        public string pptFile { get; protected set; }
        public int pptMagnification { get; protected set; }
        public NavigationService NavigationService { get; protected set; }
        public NetworkController NetworkController { get; protected set; }
        public UserGlobalState UserGlobalState { get; protected set; }
        public UserServerState UserServerState { get; protected set; }
        public PowerPointLoader loader { get; protected set; }
        public PowerpointLoaderFlyout(NavigationService navigationService, NetworkController networkController, UserGlobalState userGlobalState, UserServerState userServerState) 
        {
            NavigationService = navigationService;
            NetworkController = networkController;
            UserGlobalState = userGlobalState;
            UserServerState = userServerState;
            InitializeComponent();
            Title = "Import Powerpoint into " + NetworkController.config.name;
            loader = new PowerPointLoader(NetworkController);
            Loaded += (s, e) =>
            {
                var initialDirectory = "";
                //These variables may or may not be available in any given OS
                foreach (var path in new[] { Environment.SpecialFolder.MyDocuments, Environment.SpecialFolder.DesktopDirectory, Environment.SpecialFolder.MyComputer })
                    try
                    {
                        initialDirectory = Environment.GetFolderPath(path);
                        break;
                    }
                    catch (Exception) { }
                var fileBrowser = new OpenFileDialog
                {
                    InitialDirectory = initialDirectory,
                    Filter = "PowerPoint files (*.ppt, *.pptx)|*.ppt; *.pptx|All files (*.*)|*.*",
                    FilterIndex = 0,
                    RestoreDirectory = true,
                    Multiselect = false
                };
                var fileDialogResult = fileBrowser.ShowDialog();
                if (!String.IsNullOrEmpty(fileBrowser.FileName))
                {
                    pptFile = fileBrowser.FileName;
                }
                if (!fileDialogResult.HasValue) {
                    pptFile = null;
                    CloseFlyout();
                }
                importTypes.ItemsSource = loader.IsPowerPointInstalled() ?
                    new ObservableCollection<PowerpointImportType> { PowerpointImportType.Image, PowerpointImportType.HighDefImage, PowerpointImportType.Shapes, PowerpointImportType.ServerSideImage, PowerpointImportType.ServerSideHighDefImage, PowerpointImportType.ServerSideShapes } :
                    new ObservableCollection<PowerpointImportType> { PowerpointImportType.ServerSideImage, PowerpointImportType.ServerSideHighDefImage, PowerpointImportType.ServerSideShapes };
                
            };         
        }

        private void TypeChosen(object sender, RoutedEventArgs e)
        {
            importTypes.Visibility = Visibility.Collapsed;
            var pptImportType = (PowerpointImportType)((FrameworkElement)sender).DataContext;

            var totalProgress = new ppProgressInfo("total");
            var localProgress = new ppProgressInfo("local");
            var serverProgress = new ppProgressInfo("server");
            var slideProgress = new ppProgressInfo("slide");
            var progressMeters = new ObservableCollection<ppProgressInfo> { totalProgress, localProgress, slideProgress, serverProgress };
            progresses.ItemsSource = progressMeters;
            progresses.Visibility = Visibility.Visible;
            var progressMappings = progressMeters.ToDictionary(kv => kv.Category);
            Action<string, string, int, int> onProgress = (category, description, count, total) =>
            {
                try
                {
                    var p = progressMappings[category];
                    Dispatcher.adopt(delegate {
                        p.Count = count;
                        p.Total = total;
                        p.Description = description;
                        if (count == total)
                        {
                            p.Visible = Visibility.Collapsed;
                        }
                        else
                        {
                            p.Visible = Visibility.Visible;
                        }
                    });
                }
                catch { }
            };
            Action<ConversationDetails> onComplete = (conv) => {
                if (conv != ConversationDetails.Empty)
                {
                    Dispatcher.adopt(delegate {
                        progresses.Visibility = Visibility.Collapsed;
                        goToConversation.IsEnabled = true;
                            goToConversation.Click += (bs, be) =>
                        {
                            NavigationService.Navigate(new ConversationOverviewPage(UserGlobalState, UserServerState, new UserConversationState(), NetworkController, conv));
                            CloseFlyout();
                        };
                    });
                } else
                {
                    importTypes.Visibility = Visibility.Visible;
                }
            };
            var spec = new PowerpointSpec
            {
                File = pptFile,
                Type = pptImportType,
                Magnification = pptImportType == PowerpointImportType.HighDefImage || pptImportType == PowerpointImportType.ServerSideHighDefImage ? 3 : 1
                
            };
            var pptThread = new System.Threading.Thread(new System.Threading.ThreadStart(delegate {
                loader.UploadPowerpoint(spec, onProgress, onComplete);
            }));
            pptThread.SetApartmentState(System.Threading.ApartmentState.STA);
            pptThread.Start();
        }
    }
}
