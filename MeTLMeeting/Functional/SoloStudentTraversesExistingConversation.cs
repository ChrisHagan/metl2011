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
            LocateAndLoginTeacher();
            LocateAndLoginStudent();
            JoinConversationTeacher();
            JoinConversationStudent();
            StudentSync();
            InjectContent();
            TeacherAdd();
            InjectContent();
            TeacherMoveForward();
            EditConversation();
            for (var i = 0; i < 5; i++)
            {
                InjectContent();
                TeacherMoveForward();
            }
            TeacherMoveBack();
            TeacherMoveBack();


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
        public void LocateAndLoginTeacher()
        {
            int userSuffix = 22;
            var window = (AutomationElement) windows[0];
            var name = string.Format("dhag{0}",userSuffix++);
            new Login(window).username(name).password("mon4sh2008");
            new Login((AutomationElement)window).submit();
        }
        [TestMethod]
        public void LocateAndLoginStudent()
        {
            if (windows.Count >= 2)
            {
                int userSuffix = 23;
                var window = (AutomationElement) windows[1];
                var name = string.Format("dhag{0}",userSuffix++);
                new Login(window).username(name).password("mon4sh2008");
                new Login((AutomationElement)window).submit();   
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
        public void JoinConversationTeacher()
        {
            var window = windows[0];
            var search = new ConversationSearcher(window);
            search.searchField("AutomatedConversation").Search();
        }
        [TestMethod]
        public void JoinConversationStudent()
        {
            var window = windows[1];
            var search = new ConversationSearcher(window);
            search.searchField("AutomatedConversation").Search();
        }
        Timer timer;
        [TestMethod]
        public void PeriodicallyInjectContent(AutomationElement window, int interval, string name)
        {
            var presentationSpace = new UserCanvasStack(window, "canvas");
            Random random = new Random();
            var strokes = File.ReadAllLines(@"availableStrokes.txt");
            var x = 20;
            var y = 0;
            var color = randomColor();
            var xLimit = random.Next(8000, 20000);
            timer = new Timer((_state) =>
            {
                x += 60;
                if (x > xLimit)
                {
                    x = 0;
                    y += 100;
                }
                if (random.Next(6) == 1)
                    return;//Space
                var sourcePoints = strokes[random.Next(strokes.Count())].Split(' ');
                var relocatedStroke = new StringBuilder();
                for (int i = 0; i < sourcePoints.Count(); )
                    relocatedStroke.AppendFormat(" {0} {1} {2}", 
                        Double.Parse(sourcePoints[i++])+x, 
                        Double.Parse(sourcePoints[i++])+y, 
                        (int)(255*Double.Parse(sourcePoints[i++])));
                presentationSpace.Ink = stroke(name, randomColor(), 3, relocatedStroke.ToString().Trim());
            }, null, 0, interval);
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
            presentationSpace.Ink = ink("dhag22", 30,0);
        }
        [TestMethod]
        public void InjectText()
        {
            var presentationSpace = new UserCanvasStack(windows[0], "canvas");
            presentationSpace.Text = "Some TEXT";
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
        public string ink(string author, int start, int yOffset)
        {
            return ink(author, start, randomColor(), yOffset);
        }
        private string ink(string author, int start, Color color, int yOffset)
        {
            var pressure = 80;
            var length = 300;
            return stroke(author, color, 3.0, Enumerable.Range(start, length).Aggregate("",
                (acc,i)=>acc+string.Format("{0} {1} {2} ", i, yOffset+Math.Round(i*RANDOM.NextDouble(),2),pressure)).Trim());
        }
        private string stroke(string author, Color color, double thickness, string points)
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
                            new XElement(METL+"privacy", "public"),
                            new XElement(METL+"target", target),
                            new XElement(METL+"slide", slide))))).ToString(SaveOptions.DisableFormatting);
        }
    }
}
