using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Automation;
using System.Windows;
using System.Windows.Automation.Peers;
using Functional;
using SandRibbon.Components;

namespace HeadfulClassRoom
{
    class Program
    {
        public static int population = 4;
        public static Dictionary<AutomationElement, string> usernames = new Dictionary<AutomationElement, string>();
        private static Random RANDOM = new Random();
        static void Main(string[] args)
        {
            try
            {
                foreach(var i in Enumerable.Range(0,population))
                    Process.Start(@"MeTL.exe");
                AutomationElementCollection windows;
                while (true)
                {
                    windows = AutomationElement.RootElement.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.AutomationIdProperty, "ribbonWindow"));
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
                    var window = (AutomationElement)windows[i];
                    window.SetPosition(width, height, x, y);
                    x += width;
                    if (x > screenWidth - width)
                    {
                        x = 0;
                        y += height;
                    }
                }
                var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                foreach (var window in windows)
                {
                    var name = string.Format("Admirable{0}{1}at{2}", chars[RANDOM.Next(chars.Length)],chars[RANDOM.Next(chars.Length)], DateTime.Now.Millisecond);
                    new Functional.Login((AutomationElement)window).username(name).password("noPassword");
                }
                foreach (var window in windows)
                    new Functional.Login((AutomationElement)window).submit();
                foreach (var window in windows)
                    enterConversation(window);
                foreach (var window in windows)
                    new SyncButton((AutomationElement)window).Toggle();
                Console.ReadLine();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
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