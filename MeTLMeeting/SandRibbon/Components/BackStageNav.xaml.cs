using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace SandRibbon.Components
{
    public partial class BackStageNav : UserControl
    {
        public BackStageNav()
        {
            InitializeComponent();
            Commands.ShowConversationSearchBox.RegisterCommand(new DelegateCommand<object>(showConversationSearchBox));
        }

        private void showConversationSearchBox(object mode)
        {
            openCorrectTab((string)mode);
        }

        private void openMyConversations()
        {
            mine.IsChecked = true;
        }
        private void openAllConversations()
        {
            all.IsChecked = true;
        }
        private void openFindConversations()
        {
            Dispatcher.adoptAsync(() =>
            find.IsChecked = true);
        }
        private void openCorrectTab(string mode) {
            if ("MyConversations" == mode)
                openMyConversations();
            else
                openFindConversations();
        }
        public string currentMode { 
            get{
                return new[]{help,mine,all, find}.Aggregate(all, (acc, item) =>
                                                                     {
                                                                         if (true == item.IsChecked)
                                                                             return item;
                                                                         return acc;
                                                                     }).Name;
            }
        }
        private void mode_Checked(object sender, RoutedEventArgs e)
        {
            var mode = ((FrameworkElement)sender).Name;
            Commands.BackstageModeChanged.ExecuteAsync(mode);
        }
    }
}
