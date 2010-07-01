using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SandRibbonInterop.MeTLStanzas;
using System.Text;

namespace Functional
{
    class Constants
    {
        public static readonly string TEST_DB = "reviver.adm.monash.edu.au";
    }
    [TestClass]
    public class SoloStudentTraversesExistingConversation
    {
        public SoloStudentTraversesExistingConversation()
        {
        }
        private TestContext testContextInstance;
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        private static AutomationElementCollection windows;
        [TestInitialize]
        public void AttachToProcess()
        {
            if(windows == null)
                windows = AutomationElement.RootElement.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.AutomationIdProperty, "ribbonWindow"));
            Assert.IsNotNull(windows, "Could not find a process named MeTL.  Have you started an instance (it can be clickonce)");
        }
        string target = "presentationSpace";
        string slide = "483401";
        XNamespace METL = "monash:metl";
        Random RANDOM = new Random();
        [TestMethod] 
        public void SUPERTEST()
        {
            LocateAndLogin();
            Thread.Sleep(2000);
            JoinConversation();
            Thread.Sleep(2000);
            StudentSync();
            InjectContent();
            TeacherAdd();
            InjectContent();
            TeacherMoveForward();
            EditConversation();
            Thread.Sleep(2000);
            for (var i = 0; i < 3; i++)
            {
                InjectContent();
                TeacherMoveForward();
            }
            Thread.Sleep(3000);
            InjectStudentStrokes();
            Thread.Sleep(2000);
            StudentSubmitScreenshot();
            Thread.Sleep(2000);
            TeacherViewSubmissions();
            Thread.Sleep(2000);
            TeacherImportSubmission();
            Thread.Sleep(2000);
            OpenQuiz();
            Thread.Sleep(1000);
            CreateQuiz();
            Thread.Sleep(2000);
            openQuizToAnswer();
            Thread.Sleep(3000);
            answerAQuiz();
        }
        [TestMethod]
        public void InjectContentAndMove()
        {
            InjectContent();
            TeacherMoveForward();
        }
        [TestMethod]
        public void ScreenshotSubmissions()
        {
            InjectStudentStrokes();
            Thread.Sleep(1000);
            StudentSubmitScreenshot();
            Thread.Sleep(1000);
            TeacherViewSubmissions();
            Thread.Sleep(1000);
            TeacherImportSubmission();
        }
        [TestMethod]
        public void QuizCreationAndAnswering()
        {
            OpenQuiz();
            Thread.Sleep(1000);
            CreateQuiz();
            Thread.Sleep(2000);
            openQuizToAnswer();
            Thread.Sleep(3000);
            answerAQuiz();
        }
        [TestMethod]
        public void TeacherConversationCreation()
        {
            LocateAndLogin();
            CreateConversation();
        }
        [TestMethod]
        private void openQuizToAnswer()
        {
            var elements = AutomationElement
                        .RootElement
                        .FindAll(TreeScope.Children, 
                                    new PropertyCondition(AutomationElement.AutomationIdProperty, 
                                    "ribbonWindow"));
            foreach(AutomationElement window in elements)
                new Quiz(window).openQuiz();
        }
        [TestMethod]
        private void answerAQuiz()
        {
            new QuizAnswer().answer();
        }
        [TestMethod]
        private void OpenQuiz()
        {
            new Quiz(windows[0]).open();
        }
        [TestMethod]
        private void CreateQuiz()
        {
            new QuizCreate().options().create();
        }

        [TestMethod]
        public void StudentSubmitScreenshot()
        {
            new Submission(windows[1]).submit();
        }
        [TestMethod]
        public void TeacherViewSubmissions()
        {
            new Submission(windows[0]).view();
        }
        [TestMethod]
        public void TeacherImportSubmission()
        {
            new SubmissionViewer(windows[0]).import();
        }
        [TestMethod]
        public void justTeacher()
        {
            InjectContent();
            TeacherAdd();
            InjectContent();
            TeacherMoveForward();
            for (var i = 0; i < 5; i++)
            {
                InjectContent();
                TeacherMoveForward();
            }
            TeacherMoveBack();
            TeacherMoveBack();
        }
        [TestMethod]
        public void StudentJoin()
        {
            var window = windows[1]; 
            var search = new ConversationSearcher(window);
            Thread.Sleep(1000);
            search.searchField("AutomatedConversation").Search();
        }
        [TestMethod]
        public void LocateAndLogin()
        {
            int userSuffix = 22;
            foreach (AutomationElement window in windows)
            {
                var name = string.Format("dhag{0}", userSuffix++);
                new Login(window).username(name).password("mon4sh2008");
                new Login((AutomationElement) window).submit();
            }
        }
        [TestMethod]
        public void TeacherMoveForward()
        {
            var window = windows[0];
            window.pause(500);
            new SlideNavigation(window).Forward();
        }
        [TestMethod]
        public void TeacherMoveBack()
        {
            var window = windows[0];
            window.pause(500);
            new SlideNavigation(window).Back();
        }
        [TestMethod]
        public void TeacherAdd()
        {
            var window = windows[0];
            window.pause(500);
            new SlideNavigation(window).Add();
        }
        [TestMethod]
        public void StudentSync()
        {
            var window = windows[1];
            window.pause(500);
            new SlideNavigation(window).Sync();
        }
        [TestMethod]
        public void CreateConversation()
        {
            var window = windows[0];
            new ApplicationPopup(window).CreateConversation()
                .title(string.Format("AutomatedConversation{0}", DateTime.Now)).createType(1)
                .powerpointType(2).file(@"C:\Users\monash\Desktop\beards.ppt").create();
        }
        [TestMethod]
        public void EditConversation()
        {
            var window = windows[0];
            new ApplicationPopup(window).EditConversation().title("AutomatedConversationEdited").update();
        }
        [TestMethod]
        public void JoinConversation()
        {
            foreach (AutomationElement window in windows)
            {
                var search = new ConversationSearcher(window);
                Thread.Sleep(1000);
                search.searchField("AutomatedConversation").Search();
            }
        }
        [TestMethod]
        public void InjectContent()
        {
            InjectStrokes();
            InjectText();
            InjectImages();
        }
        [TestMethod]
        public void InjectStrokes()
        {
            var presentationSpace = new UserCanvasStack(windows[0], "canvas");
            presentationSpace.Ink = ink("dhag22", 30,0, "public");
        }
        [TestMethod]
        public void InjectStudentStrokes()
        {
            var presentationSpace = new UserCanvasStack(windows[1], "canvas");
            presentationSpace.Ink = ink("dhag23", 100,100, "private");
        }
        [TestMethod]
        public void InjectText()
        {
            var presentationSpace = new UserCanvasStack(windows[0], "canvas");
            presentationSpace.Text = "Some TEXT";
        }
        [TestMethod]
        public void stressTestImages()
        {
            for(var i =0; i < 80; i++)
            {
                InjectImages();
            }
        }
        [TestMethod]
        public void InjectImages()
        {
            var presentationSpace = new UserCanvasStack(windows[0], "canvas");
            presentationSpace.Images = @"C:\specialMeTL\robot.jpg"; 
            //presentationSpace.Images = image("dhag22", @"http://i144.photobucket.com/albums/r181/jssst21/mortalwombatbannercopy.jpg", 80, RANDOM.Next(200));
        }
        private string image(string author, string url, int x, int y)
        {//THIS WILL NOT CONTROL PRIVACY
             return (
               new XElement("strokeCollection",
                   new XElement("message",
                       new XElement(METL + "image",
                           new XElement(METL + "height", "100"),
                           new XElement(METL + "width", "100"),
                           new XElement(METL + "x", x),
                           new XElement(METL + "y", y),
                           new XElement(METL + "source", url),
                           new XElement(METL + "author", author),
                           new XElement(METL + "privacy", "public"),
                           new XElement(METL + "target", target),
                           new XElement(METL + "slide", slide))))).ToString(SaveOptions.DisableFormatting);
        }
        private string text(string author, string privacy, string content, int x, int y)
        {
            return (
               new XElement("strokeCollection",
                   new XElement("message",
                       new XElement(METL + "textbox",
                           new XElement(METL + "height", "200"),
                           new XElement(METL + "width", "300"),
                           new XElement(METL + "caret", "0"),
                           new XElement(METL + "x", x),
                           new XElement(METL + "y", y),
                           new XElement(METL + "text", content),
                           new XElement(METL + "tag", "{"+string.Format("author:{0},privacy:{1},id:{2}", author,privacy,DateTime.Now)+"}"),
                           new XElement(METL + "style", "Normal"),
                           new XElement(METL + "family", "Helvetica"),
                           new XElement(METL + "weight", "Bold"),
                           new XElement(METL + "size", "20"),
                           new XElement(METL + "color", "Blue"),
                           new XElement(METL + "decoration", "None"),
                           new XElement(METL + "author", author),
                           new XElement(METL + "privacy", privacy),
                           new XElement(METL + "target", target),
                           new XElement(METL + "slide", slide))))).ToString(SaveOptions.DisableFormatting);
        }
        private Color randomColor()
        {
            var buff = new byte[3];
            RANDOM.NextBytes(buff);
            return new Color
            {
                A = 255,
                R = buff[0],
                G = buff[1],
                B = buff[2]
            };
        }
        public string ink(string author, int start, int yOffset, string privacy)
        {
            return ink(author, start, randomColor(), yOffset, privacy);
        }
        private string ink(string author, int start, Color color, int yOffset, string privacy)
        {
            var pressure = 80;
            var length = 300;
            return stroke(author, color, 3.0, Enumerable.Range(start, length).Aggregate("",
                (acc,i)=>acc+string.Format("{0} {1} {2} ", i, yOffset+Math.Round(i*RANDOM.NextDouble(),2),pressure)).Trim(), privacy);
        }
        private string stroke(string author, Color color, double thickness, string points, string privacy)
        {//PRIVACY IS NOT CONTROLLED BY THIS STROKE.  IT IS IGNORED.
            return (
                new XElement("strokeCollection",
                    new XElement("message",
                        new XElement(METL+"ink",
                            new XElement(METL+"checksum", points.Split(' ').Aggregate(0.0, (acc,item)=>acc+Double.Parse(item))),
                            new XElement(METL+"points", points),
                            new XElement(METL+"color", MeTLStanzas.Ink.colorToString(color)),
                            new XElement(METL+"thickness", thickness),
                            new XElement(METL+"highlight", "False"),
                            new XElement(METL+"author", author),
                            new XElement(METL+"privacy", privacy),
                            new XElement(METL+"target", target),
                            new XElement(METL+"slide", slide))))).ToString(SaveOptions.DisableFormatting);
        }
    }
}
