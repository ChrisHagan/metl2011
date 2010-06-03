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

        private void importSubmission(object sender, ExecutedRoutedEventArgs e)
        {
            var item = submissions.SelectedItem;
            DelegateCommand<object> onPreparserAvailable = null;
            onPreparserAvailable = new DelegateCommand<object>((_obj) =>
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
        }

        private void canImportSubmission(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = submissions.SelectedItem != null;
        }
    }
}
