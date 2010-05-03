using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using SandRibbon.Components;
using SandRibbon.Components.Utility;
using SandRibbon.Utils.Connection;
using SandRibbonInterop;

namespace SandRibbon.Quizzing
{
    public partial class AuthorAQuiz : UserControl
    {
        public int quizSlide;
        private ObservableCollection<Option> options = new ObservableCollection<Option>();
        private SandRibbon.Utils.Connection.JabberWire.UserInformation info;
        public AuthorAQuiz(SandRibbon.Utils.Connection.JabberWire.UserInformation info, int quizSlide)
        {
            InitializeComponent();
            this.info = info;
            this.quizSlide = quizSlide;
            canvasStack.SetIdentity(info, true);
            Loaded += loaded;
        }
        public void Submit()
        {
            var path = "quizSample.png";
            var width = (int)printingDummy.ActualWidth;
            var height = (int)printingDummy.ActualHeight;
            new PrintingHost().saveCanvasToDisk(
                printingDummy,
                path,
                width, height, width, height);
            var hostedFileName = ResourceUploader.uploadResource(info.credentials.name, path);
            Commands.SendQuiz.Execute(new QuizDetails{
                author=info.credentials.name, 
                optionCount=options.Count(),
                quizPath=hostedFileName,
                returnSlide=info.location.currentSlide,
                targetSlide=quizSlide,
                target="presentationSpace"
            });
            Cancel();
        }
        private void loaded(object sender, RoutedEventArgs e)
        {
            var adorner = AdornerLayer.GetAdornerLayer(this);
            adorner.Add(new UIAdorner(this, new AuthorAQuizInstructions()));
            adorner.Add(new UIAdorner(this, new AuthorAQuizSampleAnswers().SetOptions(options.ToList())));
            adorner.Add(new UIAdorner(this, new AuthorAQuizControls(this)));
            canvasStack.handwriting.DefaultPenAttributes();
        }
        public void Cancel()
        {
            ((Panel)Parent).Children.Remove(this);
        }
        private void addOption()
        {
            int index = options.Count()+1;
            options.Add(new Option {index=index});
        }
        public AuthorAQuiz SetQuestionCount(int count)
        {
            for (int i = 0; i < count; i++)
                addOption();
            return this;
        }
    }
    public class Option
    {
        public int index{ get; set;}
    }
}
