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
        public void LocateAndLogin()
        {
            int userSuffix = 22;
            foreach(var obj in windows)
            {
                var window = (AutomationElement)obj;
                var name = string.Format("dhag{0}",userSuffix++);
                new Login(window).username(name).password("mon4sh2008").submit();
                new ApplicationPopup(windows[0]).AllConversations().enter("automation test");
                PeriodicallyInjectContent(windows[0], 150, name);
            }
        }
        Timer timer;
        [TestMethod]
        public void PeriodicallyInjectContent(AutomationElement window, int interval, string name)
        {
            var presentationSpace = new UserCanvasStack(window, "presentationSpace");
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
            var presentationSpace = new UserCanvasStack(windows[0], "presentationSpace");
            presentationSpace.Privacy = "public";
            presentationSpace.Ink = ink("dhag22", 30,0);
            presentationSpace.Privacy = "private";
            presentationSpace.Ink = ink("dhag22", 40,50);
        }
        [TestMethod]
        public void InjectText()
        {
            var presentationSpace = new UserCanvasStack(windows[0], "presentationSpace");
            presentationSpace.Privacy = "public";
            presentationSpace.Text = text("dhag22", "public", "Please wait...", 80,RANDOM.Next(5000));
            presentationSpace.Privacy = "private";
            presentationSpace.Text = text("dhag22", "private", "METL IS GENERATING PRIVATE CONTENT", 150,RANDOM.Next(300));
        }
        [TestMethod]
        public void InjectImages()
        {
            var presentationSpace = new UserCanvasStack(windows[0], "presentationSpace");
            presentationSpace.Privacy = "public";
            presentationSpace.Images = image("dhag22", @"C:\sandRibbon\SandRibbon\Automation\Resources\pictureOne.png", 80,RANDOM.Next(200));
            presentationSpace.Privacy = "private";
            presentationSpace.Images = image("dhag22", @"C:\sandRibbon\SandRibbon\Automation\Resources\pictureTwo.jpg", 150,RANDOM.Next(300));
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
