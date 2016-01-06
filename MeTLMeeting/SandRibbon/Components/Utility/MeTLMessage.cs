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
            var dialogOwner = owner;
            var result = MessageBoxResult.None;

            if (dialogOwner != null)
            {
                if (dialogOwner.Dispatcher != null)
                {
                    dialogOwner.Dispatcher.adoptAsync(() =>
                    {
                        result = MessageBox.Show(dialogOwner, message, "MeTL", button, image);
                    });
                } else
                {
                    Application.Current.Dispatcher.adoptAsync(() =>
                    {
                        result = MessageBox.Show(dialogOwner, message, "MeTL", button, image);
                    });

                }
            }
            else
            {
                // calling from non-ui thread
                if (Application.Current != null && Application.Current.Dispatcher != null)
                    Application.Current.Dispatcher.adoptAsync(() =>
                    {
                        dialogOwner = GetMainWindow();
                        result = MessageBox.Show(dialogOwner, message, "MeTL", button, image);
                    });
            }

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
