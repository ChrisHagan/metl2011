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
using SandRibbonInterop.MeTLStanzas;
using Keys = System.Windows.Forms.SendKeys;
using System.Text;
using System.Windows;
namespace Pedal
{
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }
        private static AutomationElement metl;
        public void AttachToProcess()
        {
            if(metl == null)
                try
                {
                    metl = AutomationElement.RootElement.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.AutomationIdProperty, "ribbonWindow"))[0];
                }
                catch (Exception)
                {
                    MessageBox.Show("Could not find a process named MeTL.  Have you started an instance (it can be clickonce)");
                }
        }
        string target = "presentationSpace";
        string slide;
        string name;
        int xOffset;
        int delayInMilis;
        XNamespace METL = "monash:metl";
        Random RANDOM = new Random();
        Timer timer;
        public void PeriodicallyInjectContent()
        {
            AttachToProcess();
            Dispatcher.Invoke((Action)delegate
            {
                if (String.IsNullOrEmpty(username.Text))
                {
                    MessageBox.Show("You must specify a valid jid in username (an authcate is a good bet");
                    return;
                }
                name = username.Text;
                slide = String.IsNullOrEmpty(optionalSlide.Text) ? "1001" : optionalSlide.Text;
                delayInMilis = String.IsNullOrEmpty(optionalSpeed.Text) ? 150 : Int32.Parse(optionalSpeed.Text);
                xOffset = String.IsNullOrEmpty(optionalXOffset.Text) ? 0 : Int32.Parse(optionalXOffset.Text);
                var presentationSpace = new UserCanvasStack(metl, "presentationSpace");
                Random random = new Random();
                var strokes = File.ReadAllLines("availableStrokes.txt");
                var x = xOffset;
                var y = 0;
                var color = randomColor();
                var xLimit = xOffset+2000;
                timer = new Timer((_state) =>
                {
                    x += 60;
                    if (x > xLimit)
                    {
                        x = xOffset;
                        y += 100;
                        color = randomColor();
                    }
                    if (random.Next(8) == 1)
                        return;//Space
                    var sourcePoints = strokes[random.Next(strokes.Count())].Split(' ');
                    var relocatedStroke = new StringBuilder();
                    for (int i = 0; i < sourcePoints.Count(); )
                        relocatedStroke.AppendFormat(" {0} {1} {2}",
                            Double.Parse(sourcePoints[i++]) + x,
                            Double.Parse(sourcePoints[i++]) + y,
                            (int)(255 * Double.Parse(sourcePoints[i++])));
                    presentationSpace.Ink = stroke(name, randomColor(), 3, relocatedStroke.ToString().Trim());
                }, null, 0, delayInMilis);
            });
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
        private string ink(string author, int start, int yOffset)
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
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PeriodicallyInjectContent();
        }
        private void Help(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
@"Q: How do I stop the test?
A: Pedal can be paused by pressing the red 'X' in the top right corner of the window.  Pressing this again will resume.
Q: Why doesn't the text say anything meaningful?
A: Our research indicates that only the first three lines of any document are");
        }
    }
    class UserCanvasStack
    {
        private AutomationElement _stack;
        private AutomationElement _handwriting;
        private AutomationElement _text;
        private AutomationElement _images;
        private AutomationElement _privacyTools;
        public string Ink
        {
            get 
            {
                return _handwriting.Value();
            }
            set
            {
                _handwriting.Value(value);
            }
        }
        public string Text
        {
            get
            {
                return _text.Value();
            }
            set
            {
                _text.Value(value);
            }
        }
        public string Images
        {
            get
            {
                return _images.Value();
            }
            set
            {
                _images.Value(value);
            }
        }
        public string Privacy
        {
            get
            {
                return _privacyTools.Value();
            }
            set
            {
                _privacyTools.Value(value);
            }
        }
        public UserCanvasStack(AutomationElement parent, string target)
        {
            _privacyTools = parent.Descendant("privacyTools");
            _stack = parent.Descendant(target);
            _handwriting = _stack.Descendant("handwriting");
            _text = _stack.Descendant("text");
            _images = _stack.Descendant("images");
        }
    }
    public static class AutomationExtensions
    {
        public static AutomationElement Descendant(this AutomationElement element, string name)
        {
            var result = element.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, name));
            return result;
        }
        public static AutomationElement Descendant(this AutomationElement element, Type type)
        {
            var result = element.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ClassNameProperty, type.Name));
            return result;
        }
        public static AutomationElementCollection Descendants(this AutomationElement element, Type type)
        {
            var result = element.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.ClassNameProperty, type.Name));
            return result;
        }
        public static IEnumerable<AutomationElement> Descendants(this AutomationElement element)
        {
            var result = new AutomationElement[1024];
            element.FindAll(TreeScope.Descendants, System.Windows.Automation.Condition.TrueCondition).CopyTo(result, 0);
            return result.TakeWhile(e => e != null).ToArray();
        }
        
        public static AutomationElementCollection Children(this AutomationElement element, Type type)
        {
            var result = element.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ClassNameProperty, type.Name));
            return result;
        }
        public static AutomationElement Child(this AutomationElement element, Type type)
        {
            var result = element.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ClassNameProperty, type.Name));
            return result;
        }
        public static string Value(this AutomationElement element)
        {
            return ((ValuePattern)element.GetCurrentPattern(ValuePattern.Pattern)).Current.Value;
        }
        public static AutomationElement Value(this AutomationElement element, string value)
        {
            ((ValuePattern)element.GetCurrentPattern(ValuePattern.Pattern)).SetValue(value);
            return element;
        }
        public static AutomationElement Invoke(this AutomationElement element)
        {
            ((InvokePattern)element.GetCurrentPattern(InvokePattern.Pattern)).Invoke();
            return element;
        }
        public static string AutomationId(this AutomationElement element)
        {
            return element.GetCurrentPropertyValue(AutomationElement.AutomationIdProperty).ToString();
        }
    }
}