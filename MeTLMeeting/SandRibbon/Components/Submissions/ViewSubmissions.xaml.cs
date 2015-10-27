using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Composite.Presentation.Commands;
using MeTLLib.DataTypes;
using MeTLLib.Providers.Connection;

namespace SandRibbon.Components.Submissions
{
    public partial class ViewSubmissions : Window
    {
        protected MeTLLib.MetlConfiguration backend;

        public ObservableCollection<TargettedSubmission> submissions {get; set;}        
        public ViewSubmissions()
        {
            InitializeComponent();
            Submissions.ItemsSource = submissions;
            Commands.ReceiveScreenshotSubmission.RegisterCommandToDispatcher<TargettedSubmission>(new DelegateCommand<TargettedSubmission>(recieveSubmission));
            App.getContextFor(backend).controller.commands.JoinConversation.RegisterCommandToDispatcher<string>(new DelegateCommand<string>(close));
            Commands.ShowConversationSearchBox.RegisterCommandToDispatcher(new DelegateCommand<object>(close));
        }
        
        public ViewSubmissions(List<TargettedSubmission> userSubmissions):this()
        {
            submissions = new ObservableCollection<TargettedSubmission>(userSubmissions);
        }
        private void close(object obj)
        {
            Close();
        }
        private void recieveSubmission(TargettedSubmission submission)
        {
            if (!String.IsNullOrEmpty(submission.target) && submission.target != "submission")
                return;
            submissions.Add(submission);
        }
        
        private void importAllSubmissionsInBucket(object sender, ExecutedRoutedEventArgs e)
        {
            var items = Submissions.SelectedItems;
            DelegateCommand<PreParser> onPreparserAvailable = null;
            onPreparserAvailable = new DelegateCommand<PreParser>((parser) =>
               {
                   Commands.PreParserAvailable.UnregisterCommand(onPreparserAvailable);
                   var imagesToDrop = new List<ImageDrop>();
                   var height = 0;
                   foreach(var elem in items)
                   {
                       var image = (TargettedSubmission) elem;
                       imagesToDrop.Add( new ImageDrop
                           {
                               Filename = image.url.ToString(),
                               Target = "presentationSpace",
                               Point = new Point(0, height),
                               Position = 1,
                               OverridePoint = true
                           });
                       height += 540;
                   }
                   Commands.ImagesDropped.ExecuteAsync(imagesToDrop);
               });
            Commands.PreParserAvailable.RegisterCommand(onPreparserAvailable);
            Commands.AddSlide.ExecuteAsync(null);
        }
        private void canImportSubmission(object sender, CanExecuteRoutedEventArgs e)
        {            
            e.CanExecute = Submissions.SelectedItems.Count > 0;
        }
    }
}