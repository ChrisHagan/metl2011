using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Composite.Presentation.Commands;
using MeTLLib.DataTypes;
using System.Windows.Media.Imaging;
using System.Linq;

namespace SandRibbon.Components.Submissions
{
    public class DisplayableSubmission
    {
        public BitmapImage image { get; set; }
        public string authorName { get; set; }
    }
    public partial class ViewSubmissions : Window
    {
        public ObservableCollection<DisplayableSubmission> submissions { get; set; }        
        public NetworkController controller { get; protected set; }
        public ViewSubmissions(NetworkController _controller, List<TargettedSubmission> userSubmissions)
        {
            InitializeComponent();
            controller = _controller;
            submissions = new ObservableCollection<DisplayableSubmission>(userSubmissions.Select(load));
            Submissions.ItemsSource = submissions;
            Commands.ReceiveScreenshotSubmission.RegisterCommand(new DelegateCommand<TargettedSubmission>(recieveSubmission));
            Commands.JoiningConversation.RegisterCommand(new DelegateCommand<string>(close));
            Commands.ShowConversationSearchBox.RegisterCommand(new DelegateCommand<object>(close));
        }

        private DisplayableSubmission load(TargettedSubmission submission) {
            var uri = controller.config.getResource(submission.url);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = new System.IO.MemoryStream(controller.client.resourceProvider.secureGetData(uri));
            bitmap.EndInit();
            return new DisplayableSubmission
            {
                image = bitmap,
                authorName = submission.author
            };
        }    

        private void close(object obj)
        {
            Dispatcher.adopt(delegate
            {
                Close();
            });
        }
        private void recieveSubmission(TargettedSubmission submission)
        {
            if (String.IsNullOrEmpty(submission.target) || submission.target != "submission") return;
            Dispatcher.adopt(delegate
            {
                submissions.Add(load(submission));
            });
        }

        private void importAllSubmissionsInBucket(object sender, ExecutedRoutedEventArgs e)
        {
            var items = Submissions.SelectedItems;
            var imagesToDrop = new List<ImageDrop>();
            var height = 0;
            foreach (var elem in items)
            {
                var image = (TargettedSubmission)elem;
                imagesToDrop.Add(new ImageDrop
                {
                    Filename = image.url.ToString(),
                    Target = "presentationSpace",
                    Point = new Point(0, height),
                    Position = 1,
                    OverridePoint = true
                });
                height += 540;
            }
            Commands.ImagesDropped.Execute(imagesToDrop);
        }
        private void canImportSubmission(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Submissions.SelectedItems.Count > 0;
        }
    }
}