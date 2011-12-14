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
using System.Windows.Data;
using MeTLLib;
using System.ComponentModel;

namespace SandRibbon.Components.Submissions
{
    public class SubmissionBucket : INotifyPropertyChanged
    {
        private DateTime _from;
        private DateTime _to;
        private bool _allSubmissionsBucket;
        private readonly ObservableCollection<TargettedSubmission> _submissions = new ObservableCollection<TargettedSubmission>();

        public string From { get { return string.Format("From: {0}", _from.ToString()); } }
        public string To { get { return string.Format("To: {0}", _to.ToString()); } }
        public long ToInTicks { get { return _to.Ticks; } }
        public long FromInTicks { get { return _from.Ticks; } }
        public bool IsAllSubmissionsBucket // Flag to signify that all submissions will be added to this bucket
        { 
            get
            {
                return _allSubmissionsBucket;
            } 
            set
            {
                if (value != _allSubmissionsBucket)
                {
                    _allSubmissionsBucket = value;
                    NotifyPropertyChanged("IsAllSubmissionsBucket");
                }
            }
        }
        public ObservableCollection<TargettedSubmission> submissions { get { return _submissions; } }

        public int Count { get { return submissions.Count; } }
    
        #region Helper methods
        public void UpdateToIfRequired(DateTime toTime)
        {
            if (IsAllSubmissionsBucket && toTime.Ticks > ToInTicks)
            {
                _to = toTime;
                NotifyPropertyChanged("To");
            }
        }

        public bool TimeInBucketRange(long ticks)
        {
            return ticks >= FromInTicks && ticks <= ToInTicks;
        }
        #endregion
    
        public SubmissionBucket(DateTime from, DateTime to)
        {
            _from = from;
            _to = to;
            _allSubmissionsBucket = false;
            _submissions.CollectionChanged += (_sender, _args) => { NotifyPropertyChanged("submissions"); };
        }

        #region INotifyPropertyChanged members
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
    public partial class ViewSubmissions : Window
    {
        public ObservableCollection<SubmissionBucket> submissionList {get; set;}
        private CollectionViewSource submissionsView;
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
            submissionsView = FindResource("sortedSubmissionsView") as CollectionViewSource;
            submissionList = new ObservableWithPropertiesCollection<SubmissionBucket>();
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

            CreateAllBucket(sortedSubmissions);

            submissions.SelectedIndex = 0;
            DataContext = this;
        }
        
        /// <remark>
        /// CreateAllBucket must be called after adding all of the submissions into the submissionList 
        /// </remark>
        /// <param name="listOrderedBySubmissionTime">Ordered by submission time list</param>
        private void CreateAllBucket(IOrderedEnumerable<TargettedSubmission> listOrderedBySubmissionTime)
        {
            // add a bucket for all of the submissions at the end
            var allBucket = new SubmissionBucket(new DateTime(FirstSubmissionTime()), new DateTime(LastSubmissionTime()));
            var allBucketSubmissionList = allBucket.submissions;
            foreach (var submission in listOrderedBySubmissionTime)
            {
                allBucketSubmissionList.Add(submission);
            }
            allBucket.IsAllSubmissionsBucket = true;
            submissionList.Add(allBucket);
        }
        
        private long FirstSubmissionTime()
        {
            return submissionList.First().FromInTicks;
        }

        private long LastSubmissionTime()
        {
            return submissionList.Last().ToInTicks;
        }

        private void UpdateAllBucket(TargettedSubmission submission)
        {
            var allBucket = submissionList.Last();
            Debug.Assert(allBucket.IsAllSubmissionsBucket);

            allBucket.UpdateToIfRequired(new DateTime(submission.time + fiveMinuteTicks));
            allBucket.submissions.Add(submission);
            submissionsView.View.Refresh();
        }

        private void recieveSubmission(TargettedSubmission submission)
        {
            var addedToBucket = false;

            // find the bucket the submission belongs to
            foreach (var bucket in submissionList)
            {
                if (!bucket.IsAllSubmissionsBucket && bucket.TimeInBucketRange(submission.time))
                {
                    bucket.submissions.Add(submission);
                    addedToBucket = true;
                    break;
                }
            }

            if (!addedToBucket)
            {
                var bucket = new SubmissionBucket(new DateTime(submission.time), new DateTime(submission.time + fiveMinuteTicks));
                bucket.submissions.Add(submission);
                submissionList.Insert(submissionList.Count - 2, bucket); // zero-based index. Count - 1 is last item, Count - 2 is second last.
            }

            UpdateAllBucket(submission);
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
                         Filename = image.url.ToString (), 
                         Target = "presentationSpace",
                         Point = new Point (0, 0),
                         Position = 1

                     });
               });
            Commands.PreParserAvailable.RegisterCommand(onPreparserAvailable);
            Commands.AddSlide.ExecuteAsync(null);
            this.Close();
        }
        private void importAllSubmissionsInBucket(object sender, ExecutedRoutedEventArgs e)
        {
             var items = submissions.Items;
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
                               Position = 1
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
            if (submissions != null)
            e.CanExecute = submissions.SelectedItem != null;
        }
    }
}
