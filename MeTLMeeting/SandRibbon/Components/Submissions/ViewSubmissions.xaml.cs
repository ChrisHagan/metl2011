using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Composite.Presentation.Commands;
using MeTLLib.DataTypes;
using MeTLLib.Providers.Connection;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using SandRibbon.Providers;
using Newtonsoft.Json.Linq;
using System.Text;

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
    public partial class ViewSubmissions : Window
    {
        public ObservableCollection<DisplayableSubmission> submissions { get; set; } = new ObservableCollection<DisplayableSubmission>();
        public ViewSubmissions()
        {
            InitializeComponent();
            Submissions.DataContext = submissions;
            var receiveLiveScreenshot = new DelegateCommand<TargettedSubmission>(recieveSubmission);
            var displaySubmissions = new DelegateCommand<object>(importAllSubmissionsInBucket, canImportSubmission);
            Loaded += delegate
            {
                Commands.ReceiveScreenshotSubmission.RegisterCommand(receiveLiveScreenshot);
                Commands.ImportSubmissions.RegisterCommand(displaySubmissions);
                App.controller.client.historyProvider.Retrieve<PreParser>(delegate { }, delegate { }, parser =>
                {
                    foreach (var s in parser.submissions)
                    {
                        submissions.Add(load(s));
                    }
                }, Globals.location.activeConversation);
            };
            Unloaded += delegate
            {
                Commands.ReceiveScreenshotSubmission.UnregisterCommand(receiveLiveScreenshot);
                Commands.ImportSubmissions.UnregisterCommand(displaySubmissions);
            };
        }

        private DisplayableSubmission load(TargettedSubmission submission)
        {
            var uri = App.controller.config.getResource(submission.url);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = new System.IO.MemoryStream(App.controller.client.resourceProvider.secureGetData(uri));
            bitmap.EndInit();
            return new DisplayableSubmission
            {
                Image = bitmap,
                Source = submission.url,
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
        private void importAllSubmissionsInBucket(object o)
        {
            var items = submissions.Where(s => s.IsSelected).Select(d => d.Source).ToList();
            var url = App.controller.config.displaySubmissionOnNewSlideAtIndex(Globals.conversationDetails.Jid, Globals.slideDetails.index + 1);
            var data = new JArray(items);
            var newC = App.controller.client.resourceProvider.securePutData(url, Encoding.UTF8.GetBytes(data.ToString()));
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
