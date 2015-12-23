using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Practices.Composite.Presentation.Commands;
using MeTLLib.DataTypes;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using SandRibbon.Pages;
using MeTLLib.Providers.Connection;
using System.Linq;
using System.Diagnostics;
using SandRibbon.Pages.Collaboration;

namespace SandRibbon.Components.Submissions
{
    public class DisplayableSubmission : DependencyObject
    {
        public BitmapImage Image { get; set; }
        public string Author { get; set; }
        public string Message { get; set; }
        public string Date { get; set; }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public Uri Url { get; internal set; }
        public string Source { get; internal set; }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(DisplayableSubmission), new PropertyMetadata(false));
    }
    public partial class ViewSubmissions : Page
    {
        public SlideAwarePage rootPage;
        public ObservableCollection<DisplayableSubmission> submissions { get; set; } = new ObservableCollection<DisplayableSubmission>();
        public ViewSubmissions(SlideAwarePage rootPage)
        {
            InitializeComponent();
            this.rootPage = rootPage;
            Submissions.DataContext = submissions;
            var receiveLiveScreenshot = new DelegateCommand<TargettedSubmission>(recieveSubmission);
            var displaySubmissions = new DelegateCommand<object>(importAllSubmissionsInBucket, canImportSubmission);
            Loaded += delegate
            {
                Commands.ReceiveScreenshotSubmission.RegisterCommand(receiveLiveScreenshot);
                Commands.ImportSubmissions.RegisterCommand(displaySubmissions);
                rootPage.NetworkController.client.historyProvider.Retrieve<PreParser>(delegate { }, delegate { }, parser =>
                {
                    foreach (var s in parser.submissions)
                    {
                        submissions.Add(load(s));
                    }
                }, rootPage.ConversationDetails.Jid);
            };
            Unloaded += delegate
            {
                Commands.ReceiveScreenshotSubmission.UnregisterCommand(receiveLiveScreenshot);
                Commands.ImportSubmissions.UnregisterCommand(displaySubmissions);
            };
        }

        private DisplayableSubmission load(TargettedSubmission submission)
        {
            var uri = rootPage.NetworkController.config.getResource(submission.url);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = new System.IO.MemoryStream(rootPage.NetworkController.client.resourceProvider.secureGetData(uri));
            bitmap.EndInit();
            return new DisplayableSubmission
            {
                Image = bitmap,
                Source=submission.url,
                Url = uri,
                Author = submission.author,
                Message = submission.title,
                Date = submission.timestamp.ToString()
            };
        }

        private void recieveSubmission(TargettedSubmission submission)
        {
            if (String.IsNullOrEmpty(submission.target) || submission.target != "submission") return;
            Dispatcher.adopt(delegate
            {
                submissions.Add(load(submission));
            });
        }

        private void importAllSubmissionsInBucket(object obj)
        {
            rootPage.NetworkController.client.JoinRoom(rootPage.Slide.id.ToString());
            var items = Submissions.SelectedItems;
            var imagesToDrop = new List<ImageDrop>();
            var height = 0;
            foreach (var elem in items)
            {
                var item = elem as DisplayableSubmission;           
                rootPage.NetworkController.client.SendImage(new TargettedImage(
                    rootPage.Slide.id,
                    rootPage.NetworkController.credentials.name,
                    "presentationSpace",
                    Privacy.Public,                  
                    item.Source,
                    0,
                    height,
                    item.Image.Width,
                    item.Image.Height,
                    item.Source,
                    -1L
                    ));                
                height += 540;
            }
            NavigationService.Navigate(new RibbonCollaborationPage(
                rootPage.UserGlobalState, 
                rootPage.UserServerState, 
                rootPage.UserConversationState, 
                rootPage.ConversationState, 
                rootPage.UserSlideState, 
                rootPage.NetworkController, 
                rootPage.ConversationDetails, 
                rootPage.Slide));
        }
        private bool canImportSubmission(object obj)
        {
            return submissions.Any(s => s.IsSelected);
        }

        private void Submissions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var submission = e.AddedItems[0] as DisplayableSubmission;
                preview.Source = submission.Image;
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            Trace.TraceInformation(String.Join(",", submissions.Select(s => s.IsSelected.ToString())));
            Commands.RequerySuggested(Commands.ImportSubmissions);
        }
    }
}