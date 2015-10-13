using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SandRibbon.Automation
{
    public partial class MultipleClientLauncher : Window
    {
        private static Random random = new Random();
        public static RoutedCommand LaunchMultipleClientAutomation = new RoutedCommand();
        public static RoutedCommand AllMoveTo = new RoutedCommand();
        public MultipleClientLauncher()
        {
            InitializeComponent();
            userCount.Height = 20;
        }
        List<MainWindow> windows;
        private void launchClients()
        {
            var desiredClients = Int32.Parse(userCount.Text);
            int cellLimit = (int)Math.Sqrt(desiredClients);
            int width = (int)System.Windows.SystemParameters.PrimaryScreenWidth / cellLimit;
            int height = (int)System.Windows.SystemParameters.PrimaryScreenHeight / cellLimit;
            double maxWidth = System.Windows.SystemParameters.PrimaryScreenWidth - width;
            int x = 0;
            int y = 0;
            windows = new List<MainWindow>();
            for (int i = 0; i < desiredClients; i++)
                windows.Add(new MainWindow());
            foreach (var window in windows)
            {
                window.Height = height;
                window.Width = width;
                window.Left = x;
                window.Top = y;
                window.Show();
                x += width;
                if (x > maxWidth)
                {
                    x = 0;
                    y += height;
                }
            }
            windows.First().WindowState = WindowState.Maximized; 
        }
        private void LaunchMultipleClients(object sender, ExecutedRoutedEventArgs e)
        {
            launchClients();
        }
        private void CanILaunch(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                Int32.Parse(userCount.Text);
                e.CanExecute = true;
            }
            catch(Exception)
            {
                e.CanExecute = false;
            }
        }
        public static List<int> AvailableSlides = Enumerable.Range(1, 5).ToList();
        private void MoveMultipleClients(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (var child in windows)
            {
                Commands.JoinConversation.ExecuteAsync("Sample");
                Commands.MoveToCollaborationPage.ExecuteAsync(e.Parameter);
            }
        }
        private void CanIMove(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = windows != null && windows.Count() > 0;
        }
    }
}
