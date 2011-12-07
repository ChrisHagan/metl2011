using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SandRibbon.Components.Utility
{
    public class MeTLMessage
    {
        private static Window GetMainWindow()
        {
            return Application.Current.MainWindow;
        }
        
        private static MessageBoxResult DisplayMessage(string message, MessageBoxImage image)
        {
            return DisplayMessage(message, image, MessageBoxButton.OK); 
        }

        private static MessageBoxResult DisplayMessage(string message, MessageBoxImage image, MessageBoxButton button)
        {
            return MessageBox.Show(GetMainWindow(), message, "MeTL", button, image);
        }

        public static MessageBoxResult Error(string message)
        {
            return DisplayMessage(message, MessageBoxImage.Error);
        }

        public static MessageBoxResult Warning(string message)
        {
            return DisplayMessage(message, MessageBoxImage.Warning);
        }

        public static MessageBoxResult Information(string message)
        {
            return DisplayMessage(message, MessageBoxImage.Information);
        }

        public static MessageBoxResult Question(string message)
        {
            return DisplayMessage(message, MessageBoxImage.Question, MessageBoxButton.YesNo);
        }
    }
}
