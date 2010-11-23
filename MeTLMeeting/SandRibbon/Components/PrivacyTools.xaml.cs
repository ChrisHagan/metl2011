using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using SandRibbon.Components.Pedagogicometry;
using SandRibbon.Providers;

namespace SandRibbon.Components
{
    public partial class PrivacyTools
    {
        public static readonly DependencyProperty PrivateProperty =
            DependencyProperty.Register("Private", typeof(string), typeof(PrivacyTools), new UIPropertyMetadata("public"));
        public static PrivacyEnablementChecker PrivacySetterIsEnabled = new PrivacyEnablementChecker();

        public PrivacyTools()
        {
            InitializeComponent();
            Commands.SetPrivacy.RegisterCommand(new DelegateCommand<string>(SetPrivacy, canSetPrivacy));
            try
            {
                if (Globals.isAuthor)
                    Commands.SetPrivacy.ExecuteAsync("public");
                else
                    Commands.SetPrivacy.ExecuteAsync("private");
                settingEnabledModes(Globals.conversationDetails);
                settingSelectedMode(Globals.privacy);
            }
            catch (NotSetException)
            {
                Commands.SetPrivacy.ExecuteAsync("Private");
            }
            Commands.SetPedagogyLevel.RegisterCommand(new DelegateCommand<PedagogyLevel>(setPedagogy));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(updateConversationDetails));
            DataContext = this;
           
        }

        private void updateConversationDetails(ConversationDetails details)
        {
            settingEnabledModes(details);
        }

        private void settingEnabledModes(ConversationDetails details)
        {
            if (details.Permissions.studentCanPublish || Globals.isAuthor)
                publicMode.IsEnabled = true;
            else
                publicMode.IsEnabled = false;
        }

        private void setPedagogy(PedagogyLevel obj)
        {
            //if (!canBecomePublic())
              //  WorkPubliclyButton.IsChecked = (Globals.privacy == "private") ? false : true;
        }
        private bool canBecomePublic()
        {
            try
            {
                return (Globals.conversationDetails.Permissions.studentCanPublish || Globals.conversationDetails.Author == Globals.me);
            }
            catch (Exception)
            {
                return false;
            }
        }
        private bool canSetPrivacy(string privacy)
        {
            try
            {
                return privacy != (string)GetValue(PrivateProperty)
                && (Globals.conversationDetails.Permissions.studentCanPublish || Globals.conversationDetails.Author == Globals.me);
            }
            catch (Exception)
            {
                return false;
            }
        }
        private void SetPrivacy(string p)
        {
            Dispatcher.adoptAsync(delegate
                                      {
                                          settingSelectedMode(p);
                                          //WorkPubliclyButton.IsChecked = p == "public";
                                          SetValue(PrivateProperty, p);
                                          Commands.RequerySuggested(Commands.SetPrivacy);
                                      });
        }

        private void settingSelectedMode(string p)
        {
            if (p == "public")
                publicMode.IsChecked = true;
            else
                privateMode.IsChecked = true;
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new PrivacyToolsAutomationPeer(this);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var thisButton = ((CheckBox)sender);
            if (!canBecomePublic())
            {
                thisButton.IsChecked = (Globals.privacy == "private") ? false : true;
                return;
            }
            Commands.SetPrivacy.ExecuteAsync(thisButton.IsChecked == true ? "public" : "private");
        }

        private void privacyChange(object sender, RoutedEventArgs e)
        {
        
            Commands.SetPrivacy.Execute(((FrameworkElement)sender).Tag.ToString());
        }
    }
    class PrivacyToolsAutomationPeer : FrameworkElementAutomationPeer, IValueProvider
    {
        public PrivacyToolsAutomationPeer(FrameworkElement parent)
            : base(parent)
        {
        }
        public PrivacyTools PrivacyTools
        {
            get { return (PrivacyTools)Owner; }
        }
        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Value)
                return this;
            return base.GetPattern(patternInterface);
        }
        public bool IsReadOnly
        {
            get { return false; }
        }
        public void SetValue(string value)
        {
            Commands.SetPrivacy.ExecuteAsync(value);
        }
        public string Value
        {
            get { return (string)PrivacyTools.GetValue(PrivacyTools.PrivateProperty); }
        }
    }
    public class PrivacyEnablementChecker : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value != parameter;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}