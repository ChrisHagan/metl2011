using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Canvas;
using MeTLLib.DataTypes;
using MeTLLib.Providers.Connection;
using System.Diagnostics;

namespace SandRibbon.Components.Submissions
{
    public class SubmissionBucket
    {
        public DateTime _from;
        public DateTime _to;
        public string From { get { return string.Format("From: {0}", _from.ToString()); } set { } }
        public string To { get { return string.Format("To: {0}", _to.ToString()); } set { } }
        public ObservableCollection<TargettedSubmission> submissions { get; set; }
        public int Count { get { return submissions.Count; } }
        public SubmissionBucket(DateTime from, DateTime to)
        {
            this._from = from;
            this._to = to;
            submissions = new ObservableCollection<TargettedSubmission>();
        }
    }
    public partial class ViewSubmissions : Window
    {
        public ObservableCollection<SubmissionBucket> submissionList {get; set;}
        public ViewSubmissions()
        {
            InitializeComponent();
            Commands.ReceiveScreenshotSubmission.RegisterCommandToDispatcher<TargettedSubmission>(new DelegateCommand<TargettedSubmission>(recieveSubmission));
            Commands.JoinConversation.RegisterCommandToDispatcher<string>(new DelegateCommand<string>(joinConversation));
            Commands.ShowConversationSearchBox.RegisterCommandToDispatcher(new DelegateCommand<object>(closeMe));
        }
        private void closeMe(object obj)
        {
            Close();
        }
        private void joinConversation(string obj)
        {
            this.Close();
            UpdateLayout();
        }
        private readonly long fiveMinuteTicks = new TimeSpan(0, 5, 0).Ticks; 
        public ViewSubmissions(List<TargettedSubmission> userSubmissions):this()
        {
            Trace.TraceInformation("ViewingSubmissions");
            submissionList = new ObservableCollection<SubmissionBucket>();
            var sortedSubmissions = userSubmissions.ToList().OrderBy(s => s.time);
            long start = sortedSubmissions.First().time;
            int currentBucket = 0;
            submissionList.Add(new SubmissionBucket(new DateTime(start), new DateTime(start + fiveMinuteTicks)));
            foreach(var submission in sortedSubmissions)
            {      
               if(!(start <= submission.time && submission.time <= (start + fiveMinuteTicks)))
               {
                   currentBucket++;
                   start = submission.time;
                   submissionList.Add(new SubmissionBucket(new DateTime(start), new DateTime(start + fiveMinuteTicks)));
               }
               submissionList[currentBucket].submissions.Add(submission);

            }
            submissions.SelectedIndex = 0;
            DataContext = this;
        }
        private void recieveSubmission(TargettedSubmission submission)
        {
            if(!(submissionList.Last()._from.Ticks <= submission.time && submission.time <= (submissionList.Last()._to.Ticks) + fiveMinuteTicks))
                   submissionList.Add(new SubmissionBucket(new DateTime(submission.time), new DateTime(submission.time + fiveMinuteTicks)));
            submissionList.Last().submissions.Add(submission);

        }
        private void importSubmission(object sender, ExecutedRoutedEventArgs e)
        {
            var item = submissions.SelectedItem;
            DelegateCommand<PreParser> onPreparserAvailable = null;
            onPreparserAvailable = new DelegateCommand<PreParser>((parser) =>
               {
                   Commands.PreParserAvailable.UnregisterCommand(onPreparserAvailable);
                   var image = (TargettedSubmission) item;
                   Commands.ImageDropped.ExecuteAsync(new ImageDrop { 
                         filename = image.url.ToString (), 
                         target = "presentationSpace",
                         point = new Point (0, 0),
                         position = 1

                     });
               });
            Commands.PreParserAvailable.RegisterCommand(onPreparserAvailable);
            Commands.AddSlide.ExecuteAsync(null);
            this.Close();
        }
        private void canImportSubmission(object sender, CanExecuteRoutedEventArgs e)
        {
            if (submissions != null)
            e.CanExecute = submissions.SelectedItem != null;
        }
    }
}
