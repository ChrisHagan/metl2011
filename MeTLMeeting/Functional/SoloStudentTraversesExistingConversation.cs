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
using MeTLLib.DataTypes;
using System.Text;
using System.Windows;
using Keys = System.Windows.Forms.SendKeys;
using Microsoft.Test;
using Mouse = Microsoft.Test.Mouse;
using System.Windows.Input;

namespace Functional
{
    [TestClass]
    public class SoloStudentTraversesExistingConversation
    {
        public SoloStudentTraversesExistingConversation()
        {
        }
        public TestContext testContextInstance;
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
        public static AutomationElementCollection windows;
        [TestInitialize]
        public void AttachToProcess()
        {
            if (windows == null)
                windows = MeTL.GetAllMainWindows(); 
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
            //TeacherMoveForward();
            EditConversation();
            Thread.Sleep(2000);
            for (var i = 0; i < 3; i++)
            {
                InjectContent();
                //TeacherMoveForward();
                TeacherAdd();
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
        public void TeacherLoginAndViewSubmissions()
        {
            var window = windows[0];
            LocateAndLogin();
            Thread.Sleep(3000);
            var search = new ConversationSearcher(window);
            Thread.Sleep(1000);
            search.searchField("SuperTest");
            Thread.Sleep(1000);
            search.Search();

            var results = window.Descendant("SearchResults");
            var buttons = results.Descendants(typeof(Button));
            buttons[1].Invoke();

            Thread.Sleep(3000);
            TeacherViewSubmissions();
            Thread.Sleep(3000);
            TeacherImportSubmission();
        }
        [TestMethod]
        public void TeacherLoginAndImportPowerpoint()
        {
            LocateAndLogin();
            Thread.Sleep(8000);
            ImportPowerpoint();
        }
        [TestMethod]
        public void TeacherConversationCreation()
        {
            LocateAndLogin();
            Thread.Sleep(5000);
            CreateConversation();
        }
        [TestMethod]
        public void TeacherCreateAndAddContent()
        {
            LocateAndLogin();
            Thread.Sleep(5000);
            CreateConversation();
            Thread.Sleep(1000);
            // ready to create. add something, then change colour and add something else
            var canvas = windows[0].Descendant(typeof(UserCanvasStack));
            DrawSpirographWaveOnCanvas(canvas);
        }
            
        private void DrawSpirographWaveOnCanvas(AutomationElement canvasElement)
        {
            var bounds = canvasElement.Current.BoundingRectangle;

            var centerX = (int)(bounds.X + bounds.Width /2);
            int centerY = (int)(bounds.Y + bounds.Height / 2);

            var points = GetPointsForSpirograph(centerX, centerY, 1.02, 5, 2, 0, 300);

            Mouse.MoveTo(points.First());
            Mouse.Down(MouseButton.Left);

            AnimateMouseThroughPoints(points);

            Mouse.Up(MouseButton.Left);
        }

        private IEnumerable<System.Drawing.Point> GetPointsForSpirograph(int centerX, int centerY, double littleR, double bigR, double a, int tStart, int tEnd)
        {
            // Equations from http://www.mathematische-basteleien.de/spirographs.htm
            for (double t = tStart; t < tEnd; t += 0.1)
            {
                var rDifference = bigR - littleR;
                var rRatio = littleR / bigR;
                var x = (rDifference * Math.Cos(rRatio * t) + a * Math.Cos((1 - rRatio) * t)) * 25;
                var y = (rDifference * Math.Sin(rRatio * t) - a * Math.Sin((1 - rRatio) * t)) * 25;

                yield return new System.Drawing.Point(centerX + (int)x, centerY + (int)y);
            }
        }

        private void AnimateMouseThroughPoints(IEnumerable<System.Drawing.Point> points)
        {
            var window = windows[0];
            var listItems = window.Descendants(typeof(ListBoxItem));
            var list = new List<SelectionItemPattern>();

            foreach (AutomationElement item in listItems)
            {
                if (item.Current.Name.Contains("PenColors"))
                {
                    var selection = item.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
                    list.Add(selection);
                }
            }
            var count = 0;
            var color = 0; 
            foreach (var point in points)
            {
                if (count % 10 == 0)
                {
                    Mouse.Up(MouseButton.Left);
                    Mouse.Down(MouseButton.Left);
                    list[color].Select();
                    if (++color >= (list.Count - 1))
                        color = 0;
                }
                Mouse.MoveTo(point);
                Thread.Sleep(5);
                count++;
        
            }
        }

        [TestMethod]
        public void openQuizToAnswer()
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
        public void answerAQuiz()
        {
            new QuizAnswer(windows[0]).answer();
        }
        [TestMethod]
        public void OpenQuiz()
        {
            windows[0].SetFocus();
            new Quiz(windows[0]).openTab().open();
        }
        [TestMethod]
        public void CreateConversationAndQuiz()
        {
            var window = windows[0];
            LocateAndLogin();
            window.pause(5000);
            CreateConversation();
            window.pause(1000);
            QuizCreationAndAnswering();
        }
        [TestMethod]
        public void CreateQuiz()
        {
            new QuizCreate(windows[0]).question("What's the colour of the sky?").options().create();
        }

        [TestMethod]
        public void StudentSubmitScreenshot()
        {
            windows[1].SetFocus();
            new Submission(windows[1]).openTab().submit();
        }
        [TestMethod]
        public void TeacherViewSubmissions()
        {
            windows[0].SetFocus();
            new Submission(windows[0]).openTab().view();
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
            //TeacherMoveForward();
            for (var i = 0; i < 5; i++)
            {
                TeacherAdd();
                InjectContent();
                //TeacherMoveForward();
            }
            TeacherMoveBack();
            TeacherMoveBack();
        }
        [TestMethod]
        public void StudentJoin()
        {
            //var window = windows[1]; 
            SearchForConversation("AutomatedConversation");
        }
        [TestMethod]
        public void LoginAndChangePagesViaShortcuts()
        {
            SearchForConversation("AutomatedPageChange");

            var window = windows[0];
            var results = window.Descendant("SearchResults");
            var buttons = results.Descendants(typeof(Button));
            buttons[1].Invoke();

            for (int i = 0; i < 3; i++)
            {
                TeacherMoveForwardViaShortcut();
            }

            for (int i = 0; i < 3; i++)
            {
                TeacherMoveBackViaShortcut();
            }
        }
        [TestMethod]
        public void LocateAndLogin()
        {
            string[,] credentials = { { "joshuaj", "TqTAg3t7KbfP" }, { "jpjor1", "h3lp1nh4nd" } };
            var count = 0;

            foreach (AutomationElement window in windows)
            {
                var name = credentials[count,0]; 
                var password = credentials[count,1]; 
                count++;
                new Login(window).username(name).password(password);
                new Login(window).submit();
            }
        }
        private void SearchForConversation(string searchString)
        {
            if (String.IsNullOrEmpty(searchString))
                searchString = "jpjor1";

            var window = windows[0];
            LocateAndLogin();
            window.pause(5000);
            SearchConversation();
            window.pause(1000);
            var search = new ConversationSearcher(window);
            search.searchField(searchString).Search();
            window.pause(300);
            search.GetResults();
        }
        [TestMethod]
        public void SearchConversationAndEdit()
        {
            SearchForConversation("");
        }
        [TestMethod]
        public void SearchForConversationAndRename()
        {
            SearchForConversation("");
            var window = windows[0];
            var rename = window.Descendant("renameConversation");
            rename.Invoke();

            var title = window.Descendant("renameTitle");
            title.Value(title.Value() + " Edited");

            var save = window.Descendant("saveEdit");
            save.Invoke();
        }
        [TestMethod]
        public void SearchForConversationAndChangePrivacy()
        {
            SearchForConversation("");

            var window = windows[0];
            var sharing = window.Descendant("shareConversation");
            sharing.Invoke();

            var groups = window.Descendant("groupsList");
            groups.SelectListItem("jpjor1");

            var save = window.Descendant("saveEdit");
            save.Invoke();
        }
        [TestMethod]
        public void SearchForConversationAndDelete()
        {
            SearchForConversation("");

            var window = windows[0];
            var delete = window.Descendant("deleteConversation");
            delete.Invoke();

            window.pause(200);

            // select yes on confirmation dialog box
            var buttons = window.Descendants(typeof(Button));
            foreach (AutomationElement button in buttons)
            {
                if (button.Current.Name.Equals("Yes"))
                {
                    button.Invoke();
                    break;
                }
            }
        }
        [TestMethod]
        public void CreateConversationAndAddPage()
        {
            var window = windows[0];
            LocateAndLogin();
            window.pause(5000);
            CreateConversation();
            window.pause(1000);
            TeacherAdd();

            var canvasStack = window.Descendant(typeof(UserCanvasStack));
            var canvasSize = ((Rect)canvasStack.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty)).Size;
            var expectSize = new Size(759, 569); // Hardcoded values for the expected size of canvas 
            Assert.AreEqual(expectSize, canvasSize);
        }
        [TestMethod]
        public void LoginAndSaveCredentials()
        {
            foreach (AutomationElement window in windows)
            {
                var name = "jpjor1";
                var password = "h3lp1nh4nd";
                var login = new Login(window).username(name).password(password);
                
                window.pause(250);
                login.remember().submit();
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
        public void TeacherMoveForwardViaShortcut()
        {
            var window = windows[0];
            window.pause(500);
            Keys.SendWait("{PGDN}"); 
        }
        [TestMethod]
        public void TeacherMoveBack()
        {
            var window = windows[0];
            window.pause(500);
            new SlideNavigation(window).Back();
        }
        [TestMethod]
        public void TeacherMoveBackViaShortcut()
        {
            var window = windows[0];
            window.pause(500);
            Keys.SendWait("{PGUP}");
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
            new ApplicationPopup(windows[0]).CreateConversation();
        }
        public void SearchConversation()
        {
            new ApplicationPopup(windows[0]).SearchConversation();
        }
        [TestMethod]
        public void ImportPowerpoint()
        {
            new ApplicationPopup(windows[0]).ImportPowerpoint(@"C:\Users\monash\Desktop\scala.ppt");
        }
        [TestMethod]
        public void EditConversation()
        {
            var window = windows[0];
            SearchConversation();
            window.pause(1000);
            var search = new ConversationSearcher(window);
            search.searchField("SuperTest").Search();
            window.pause(300);
            search.GetResults();

            var rename = window.Descendant("renameConversation");
            rename.Invoke();

            var title = window.Descendant("renameTitle");
            title.Value(title.Value() + " Edited");

            var save = window.Descendant("saveEdit");
            save.Invoke();

            window.pause(500);

            var current = window.Descendant("current");
            current.Invoke();
        }
        [TestMethod]
        public void JoinConversation()
        {
            foreach (AutomationElement window in windows)
            {
                var search = new ConversationSearcher(window);
                Thread.Sleep(1000);
                search.searchField("SuperTest");
            }
            foreach (AutomationElement window in windows)
            {
                var search = new ConversationSearcher(window);
                Thread.Sleep(1000);
                search.Search();

                var results = window.Descendant("SearchResults");
                var buttons = results.Descendants(typeof(Button));
                buttons[1].Invoke();
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
        public void InjectStrokesSelectingColours()
        {
            var presentationSpace = new UserCanvasStack(windows[0], "canvas");

            var window = windows[0];
            var listItems = window.Descendants(typeof(ListBoxItem));

            foreach (AutomationElement item in listItems)
            {
                if (item.Current.Name.Contains("PenColors"))
                {
                    var selection = item.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
                    selection.Select();
                    // TODO: change x and y offsets to random
                    presentationSpace.Ink = ink("jpjor1", 30, 0, "public");
                    window.pause(500);
                }
            }
        }
        [TestMethod]
        public void InjectStrokes()
        {
            var presentationSpace = new UserCanvasStack(windows[0], "canvas");
            presentationSpace.Ink = ink("jpjor1", 30,0, "public");
        }
        [TestMethod]
        public void InjectStudentStrokes()
        {
            var presentationSpace = new UserCanvasStack(windows[1], "canvas");
            presentationSpace.Ink = ink("jpjor1", 100,100, "public");
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
        public void ChangeTab()
        {
            
        }
        [TestMethod]
        public void InjectImages()
        {
            var presentationSpace = new UserCanvasStack(windows[0], "canvas");
            presentationSpace.Images = @"C:\specialMeTL\robot.jpg"; 
            //presentationSpace.Images = image("dhag22", @"http://i144.photobucket.com/albums/r181/jssst21/mortalwombatbannercopy.jpg", 80, RANDOM.Next(200));
        }
        public string image(string author, string url, int x, int y)
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
        public string text(string author, string privacy, string content, int x, int y)
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
        public Color randomColor()
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
        public string ink(string author, int start, Color color, int yOffset, string privacy)
        {
            var pressure = 80;
            var length = 300;
            return stroke(author, color, 3.0, Enumerable.Range(start, length).Aggregate("",
                (acc,i)=>acc+string.Format("{0} {1} {2} ", i, yOffset+Math.Round(i*RANDOM.NextDouble(),2),pressure)).Trim(), privacy);
        }
        public string stroke(string author, Color color, double thickness, string points, string privacy)
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
