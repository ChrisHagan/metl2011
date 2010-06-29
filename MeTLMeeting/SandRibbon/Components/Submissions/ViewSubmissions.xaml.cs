using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Canvas;
using SandRibbon.Utils.Connection;
using SandRibbonInterop.MeTLStanzas;

namespace SandRibbon.Components.Submissions
{
    /// <summary>
    /// Interaction logic for ViewSubmissions.xaml
    /// </summary>
    public partial class ViewSubmissions : Window
    {
        public ObservableCollection<TargettedSubmission> submissionList = new ObservableCollection<TargettedSubmission>();
        public ViewSubmissions()
        {
            InitializeComponent();
            Commands.ReceiveScreenshotSubmission.RegisterCommand(new DelegateCommand<TargettedSubmission>(recieveSubmission));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<object>(joinConversation));
        }

        private void joinConversation(object obj)
        {
            submissionList = new ObservableCollection<TargettedSubmission>();
        }

        public ViewSubmissions(List<TargettedSubmission> userSubmissions):this()
        {
            foreach (var list in userSubmissions)
                submissionList.Add(list);
            submissions.ItemsSource= submissionList;
            
        }
        private void recieveSubmission(TargettedSubmission submission)
        {
            submissionList.Add(submission);
        }
        private bool IsParserNotEmpty(PreParser parser)
        {
            return (parser.images.Count > 0
                || parser.ink.Count > 0
                || parser.text.Count > 0
                || parser.videos.Count > 0
                || parser.bubbleList.Count > 0
                || parser.autoshapes.Count > 0);
        }
        private bool isParserPrivate(PreParser parser)
        {
            if (parser.ink.Where(s => s.privacy == "private").Count() > 0)
                return true;
            if (parser.text.Where(s => s.Value.privacy == "private").Count() > 0)
                return true;
            if (parser.images.Where(s => s.Value.privacy == "private").Count() > 0)
                return true;
            if (parser.videos.Where(s => s.Value.privacy == "private").Count() > 0)
                return true;
            if (parser.autoshapes.Where(s => s.Value.privacy == "private").Count() > 0)
                return true;
            return false;
        }
        private void importSubmission(object sender, ExecutedRoutedEventArgs e)
        {
            var item = submissions.SelectedItem;
            DelegateCommand<PreParser> onPreparserAvailable = null;
            onPreparserAvailable = new DelegateCommand<PreParser>((parser) =>
               {
                   Commands.PreParserAvailable.UnregisterCommand(onPreparserAvailable);
                   var image = (TargettedSubmission) item;
                   Commands.ImageDropped.Execute(new ImageDrop { 
                         filename = image.url.ToString (), 
                         target = "presentationSpace",
                         point = new Point (0, 0),
                         position = 1

                     });
               });
            Commands.PreParserAvailable.RegisterCommand(onPreparserAvailable);
            Commands.AddSlide.Execute(null);
            this.Close();
        }

        private void canImportSubmission(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = submissions.SelectedItem != null;
        }
    }
}
