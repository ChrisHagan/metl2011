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
using SandRibbon.Providers;
using SandRibbon.Providers.Structure;
using SandRibbonObjects;
using MeTLLib.DataTypes;

namespace SandRibbon.Components
{
    public partial class PrivacyToggleButton : UserControl
    {
        public PrivacyToggleButton(PrivacyToggleButtonInfo mode, Rect bounds)
        {
            InitializeComponent();
            System.Windows.Controls.Canvas.SetLeft(privacyButtons, bounds.Right);
            System.Windows.Controls.Canvas.SetTop(privacyButtons, bounds.Top);
            privacyButtons.Height = bounds.Height;
            if (mode.showDelete)
                deleteButton.Visibility = Visibility.Visible;
            else
                deleteButton.Visibility = Visibility.Collapsed;
            if(!Globals.conversationDetails.Permissions.studentCanPublish && !Globals.isAuthor)
            {
                showButton.Visibility = Visibility.Collapsed;
                hideButton.Visibility = Visibility.Collapsed;
            }
            else if (mode.privacyChoice == "show")
            {
                showButton.Visibility = Visibility.Visible;
                hideButton.Visibility = Visibility.Collapsed;
            
            }
            else if(mode.privacyChoice == "hide")
            {
                showButton.Visibility = Visibility.Collapsed;
                hideButton.Visibility = Visibility.Visible;
            }
            else
            {
                showButton.Visibility = Visibility.Visible;
                hideButton.Visibility = Visibility.Visible;
            }
            // Commented out for 173 staging
            /*
            if (Globals.conversationDetails.Author == Globals.me)
                banhammerButton.Visibility = Visibility.Visible;
            else
                banhammerButton.Visibility = Visibility.Collapsed;
             */
        }
        private void showContent(object sender, RoutedEventArgs e)
        {
            Commands.SetPrivacyOfItems.ExecuteAsync("public");
        }
        private void hideContent(object sender, RoutedEventArgs e)
        {
            Commands.SetPrivacyOfItems.ExecuteAsync("private");
        }
        private void deleteContent(object sender, RoutedEventArgs e)
        {
            Commands.DeleteSelectedItems.ExecuteAsync(null);
        }
        private void banhammerContent(object sender, RoutedEventArgs e)
        {
            Commands.BanhammerSelectedItems.Execute(null);
        }
        public class PrivacyToggleButtonInfo
        {
            public string privacyChoice;
            public Rect ElementBounds;
            public bool showDelete;
            public PrivacyToggleButtonInfo(string privacy, Boolean delete, Rect bounds)
            {
                privacyChoice = privacy;
                showDelete = delete;
                ElementBounds = bounds;
            }
        }
        private void bubbleContent(object sender, RoutedEventArgs e)
        {
            var slide = Globals.slide;
            var currentDetails = Globals.conversationDetails;
            string target = null;
            var selection = new List<SelectedIdentity>();
            foreach(var registeredCommand in Commands.DoWithCurrentSelection.RegisteredCommands)
                registeredCommand.Execute((Action<SelectedIdentity>)(id=>{
                    target = id.target;
                    selection.Add(id);
                }));
            if (selection.Count() > 0)
            {
                var details = MeTLLib.ClientFactory.Connection().AppendSlideAfter(Globals.slide, currentDetails.Jid, Slide.TYPE.THOUGHT);
                var newSlide = details.Slides.Select(s => s.id).Max();
                Commands.SendNewBubble.ExecuteAsync(new TargettedBubbleContext
                (slide,Globals.me,target,"public",selection,newSlide));
            }
        }

    }
}
