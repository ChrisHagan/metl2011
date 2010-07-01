using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Automation;
using System.Windows;
using System.Windows.Automation.Peers;
using Functional;

namespace HeadfulClassRoom
{
    /*PLEASE NOTE THIS NEEDS TO BE RUN IN DEBUG MODE FOR IT TO WORK*/
    class Program
    {
        public static int population = 2;
        public static Dictionary<AutomationElement, string> usernames = new Dictionary<AutomationElement, string>();
        private static Random RANDOM = new Random();
        static void Main(string[] args)
        {
            try
            {
                try
                {
                    File.Delete(Directory.GetCurrentDirectory() + "\\Workspace\\state.xml");
                }
                catch(Exception e)
                {
                    
                }
                foreach (var i in Enumerable.Range(0, population))
                {
                    Process.Start(@"MeTL.exe");
                }
                AutomationElementCollection windows;
                while (true)
                {
                    windows = AutomationElement
                        .RootElement
                        .FindAll(TreeScope.Children, 
                                    new PropertyCondition(AutomationElement.AutomationIdProperty, 
                                    "ribbonWindow"));
                    if (windows.Count >= population)
                        break;
                    Thread.Sleep(250);
                }
                var bounds = System.Windows.Forms.Screen.AllScreens.First().Bounds;
                var screenWidth = bounds.Width;
                var screenHeight = bounds.Height;
                var cells = Convert.ToInt32(Math.Sqrt(population));
                var width = Convert.ToInt32(screenWidth / cells);
                var height = Convert.ToInt32(screenHeight / cells);
                var x = 0;
                var y = 0;
                for (int i = 0; i < windows.Count; i++)
                {
                    var window = windows[i];
                    x += width;
                    if (x > screenWidth - width)
                    {
                        x = 0;
                        y += height;
                    }
                }/*
                var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                int user = 22;
                foreach (AutomationElement window in windows)
                {
                    var name = string.Format("dhag{0}", user);
                    user++;
                    new Functional.Login(window).username(name).password("mon4sh2008");
                    window.SetPosition(width, height, x, y);
                }
                foreach (var window in windows)
                    new Functional.Login((AutomationElement)window).submit();
                foreach (var window in windows)
                {
                    Thread.Sleep(1000);
                    joinConversation(window);
                }
                Thread.Sleep(3000);
                openAQuiz((AutomationElement)windows[0]);
                Thread.Sleep(1000);
                createAQuiz();

                openQuizToAnswer(windows[1]);
                Thread.Sleep(1000);
                answerQuiz(windows[1]);
 */
                 Console.ReadLine();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private static void answerQuiz(AutomationElement element)
        {
            new QuizAnswer().answer();
        }

        private static void openQuizToAnswer(AutomationElement element)
        {
            var elements = AutomationElement
                        .RootElement
                        .FindAll(TreeScope.Children, 
                                    new PropertyCondition(AutomationElement.AutomationIdProperty, 
                                    "ribbonWindow"));
            Thread.Sleep(1000);
            foreach(AutomationElement window in elements)
                new Quiz(window).openQuiz();
        }

        private static void openAQuiz(AutomationElement element)
        {
            new Quiz(element).open();
            Thread.Sleep(1000);
        }
        private static void createAQuiz()
        {
            new QuizCreate().options().create();
        }
        private static void submitAScreenShot(object obj)
        {
            var window = (AutomationElement) obj;
            new Submission(window).submit(); 
        }

        private static void ImportScreenshot(object obj)
        {
            new SubmissionViewer((AutomationElement)obj).import();
        }

        private static void moveForward(object obj)
        {
            var window = (AutomationElement) obj;
            window.pause(500);
            new SlideNavigation(window).Forward();
        }

        private static void joinConversation(object windowObject)
        {
            var window = (AutomationElement) windowObject;
            var search = new ConversationSearcher(window);
            search.searchField("AutomatedConversation").Search();

        }

        private static void createConversation(object windowObject)
        {
            var window = (AutomationElement) windowObject;
            new ApplicationPopup(window).CreateConversation()
                .title(string.Format("Automated{0}", DateTime.Now)).createType(1)
                .powerpointType(2).file(@"C:\Users\monash\Desktop\beards.ppt").create();
                //create(string.Format("Automated{0}", DateTime.Now), @"C:\\Users\\monash\\Desktop\\beards.ppt");
        }

        private static void enterConversation(object windowObject)
        {
            var window = (AutomationElement)windowObject;
            var pos = window.Current.BoundingRectangle;
            window.SetPosition(800, 600, 50, 50);
            var title = "Quick!  In here!";
            new ApplicationPopup(window).AllConversations().enter(title);
            window.SetPosition(pos.Width, pos.Height, pos.X, pos.Y);
        }
    }
}