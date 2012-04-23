using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics;

namespace SandRibbon.Components.Utility
{
    public class MeTLMessage
    {
        private static Window GetMainWindow()
        {
            return Application.Current.MainWindow;
        }
        
        private static MessageBoxResult DisplayMessage(string message, MessageBoxImage image, Window owner)
        {
            return DisplayMessage(message, image, MessageBoxButton.OK, owner); 
        }

        private static MessageBoxResult DisplayMessage(string message, MessageBoxImage image, MessageBoxButton button, Window owner)
        {
            var dialogOwner = owner != null ? owner : GetMainWindow(); 
            var result = MessageBoxResult.None;

            Application.Current.Dispatcher.adopt( () => { result = MessageBox.Show(dialogOwner, message, "MeTL", button, image); });

            return result; 
        }

        public static MessageBoxResult Error(string message, Window owner = null)
        {
            return DisplayMessage(message, MessageBoxImage.Error, owner);
        }

        public static MessageBoxResult Warning(string message, Window owner = null)
        {
            return DisplayMessage(message, MessageBoxImage.Warning, owner);
        }

        public static MessageBoxResult Information(string message, Window owner = null)
        {
            return DisplayMessage(message, MessageBoxImage.Information, owner);
        }

        public static MessageBoxResult Question(string message, Window owner = null)
        {
            return DisplayMessage(message, MessageBoxImage.Question, MessageBoxButton.YesNo, owner);
        }
    }
}
