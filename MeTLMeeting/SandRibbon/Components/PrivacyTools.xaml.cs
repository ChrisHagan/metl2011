using System;
using System.Windows;
using System.Windows.Data;
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using SandRibbon.Components.Pedagogicometry;
using SandRibbon.Providers;
using System.Windows.Controls.Ribbon;
using SandRibbon.Pages.Collaboration;
using SandRibbon.Pages;
using System.ComponentModel;
using SandRibbon.Pages.Collaboration.Models;

namespace SandRibbon.Components
{
    public partial class PrivacyTools : RibbonGroup
    {
        public static readonly DependencyProperty PrivateProperty =
            DependencyProperty.Register("Private", typeof(string), typeof(PrivacyTools), new UIPropertyMetadata("public"));
        public static PrivacyEnablementChecker PrivacySetterIsEnabled = new PrivacyEnablementChecker();
        
        public PrivacyTools()
        {
            InitializeComponent();
            var setPrivacyCommand = new DelegateCommand<string>(SetPrivacy, canSetPrivacy);
            var updateConversationDetailsCommand = new DelegateCommand<ConversationDetails>(updateConversationDetails);
            var textboxFocusedCommand = new DelegateCommand<TextInformation>(UpdatePrivacyFromSelectedTextBox);
            var rootPage = DataContext as DataContextRoot;
            var privacyChangedEventHandler = new EventHandler((evs, eve) =>
            {
                var newPrivacy = rootPage.UserConversationState.Privacy;
                publicMode.IsEnabled = rootPage.ConversationState.AnyoneCanPublish;
                updateVisual(newPrivacy);
            });
            var privacyProperty = DependencyPropertyDescriptor.FromProperty(UserConversationState.PrivacyProperty, typeof(UserConversationState));
            Loaded += (s, e) =>
            {
                Commands.SetPrivacy.RegisterCommand(setPrivacyCommand);
                Commands.UpdateConversationDetails.RegisterCommand(updateConversationDetailsCommand);
                Commands.TextboxFocused.RegisterCommandToDispatcher(textboxFocusedCommand);
                privacyProperty.AddValueChanged(this, privacyChangedEventHandler);

                
                var userConv = rootPage.UserConversationState;
                if (userConv.Privacy == Privacy.NotSet || userConv.Privacy == Privacy.Public) {
                    if (rootPage.ConversationState.AnyoneCanPublish)
                        userConv.Privacy = Privacy.Public;
                    else
                        rootPage.UserConversationState.Privacy = Privacy.Private;
                }
                updateVisual(userConv.Privacy);

            };
            Unloaded += (s, e) =>
            {
                privacyProperty.RemoveValueChanged(this, privacyChangedEventHandler);
                Commands.SetPrivacy.UnregisterCommand(setPrivacyCommand);
                Commands.UpdateConversationDetails.UnregisterCommand(updateConversationDetailsCommand);
                Commands.TextboxFocused.UnregisterCommand(textboxFocusedCommand);
            };
        }
        protected void updateVisual(Privacy newPrivacy)
        {
            Dispatcher.adopt(delegate
            {
                if (newPrivacy == Privacy.Public)
                {
                    publicMode.IsChecked = true;
                }
                else if (newPrivacy == Privacy.Private)
                {
                    privateMode.IsChecked = true;
                }
            });
        }
        private void UpdatePrivacyFromSelectedTextBox(TextInformation info)
        {
            var rootPage = DataContext as DataContextRoot;
            if (info.Target == GlobalConstants.PRESENTATIONSPACE)
                rootPage.UserConversationState.Privacy = info.IsPrivate ? Privacy.Private : Privacy.Public;
        }

        private void updateConversationDetails(ConversationDetails details)
        {
            var rootPage = DataContext as DataContextRoot;
            var userConv = rootPage.UserConversationState;
            if (userConv.Privacy != Privacy.Private &&  (!details.isAuthor(rootPage.NetworkController.credentials.name) && !details.Permissions.studentCanPublish))
                userConv.Privacy = Privacy.Private;
        }
        private bool canSetPrivacy(string privacy)
        {
            var rootPage = DataContext as DataContextRoot;
            if (String.IsNullOrEmpty(privacy)) return false;
            var newPrivacy = (Privacy)Enum.Parse(typeof(Privacy), privacy, true);
            if (newPrivacy == Privacy.Private)
            {
                return true;
            } else if (newPrivacy == Privacy.Public){
                var userConv = rootPage.UserConversationState;
                return rootPage.ConversationState.AnyoneCanPublish;
            }
            else return true;
        }
        private void SetPrivacy(string privacy)
        {
            var rootPage = DataContext as DataContextRoot;
            if (String.IsNullOrEmpty(privacy)) return;
            rootPage.UserConversationState.Privacy = (Privacy)Enum.Parse(typeof(Privacy), privacy, true);
        }
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new PrivacyToolsAutomationPeer(this);
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