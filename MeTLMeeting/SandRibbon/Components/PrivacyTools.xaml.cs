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

namespace SandRibbon.Components
{
    public partial class PrivacyTools : RibbonGroup
    {
        public static readonly DependencyProperty PrivateProperty =
            DependencyProperty.Register("Private", typeof(string), typeof(PrivacyTools), new UIPropertyMetadata("public"));
        public static PrivacyEnablementChecker PrivacySetterIsEnabled = new PrivacyEnablementChecker();

        public RibbonCollaborationPage rootPage { get; protected set; }
        public PrivacyTools()
        {
            InitializeComponent();
            var setPrivacyCommand = new DelegateCommand<string>(SetPrivacy, canSetPrivacy);
            var updateConversationDetailsCommand = new DelegateCommand<ConversationDetails>(updateConversationDetails);
            var textboxFocusedCommand = new DelegateCommand<TextInformation>(UpdatePrivacyFromSelectedTextBox);
            Loaded += (s, e) => {
                if (rootPage == null)
                {
                    rootPage = DataContext as RibbonCollaborationPage;
                }
                Commands.SetPrivacy.RegisterCommand(setPrivacyCommand);
                try
                {
                    if (String.IsNullOrEmpty(Globals.privacy) || rootPage.details == null)
                    {
                        Commands.SetPrivacy.ExecuteAsync(Globals.PRIVATE);
                    }
                    else
                    {
                        if (rootPage.details.isAuthor(Globals.me))
                            Commands.SetPrivacy.ExecuteAsync(Globals.PUBLIC);
                        else
                            Commands.SetPrivacy.ExecuteAsync(Globals.PRIVATE);
                        settingEnabledModes(rootPage.details);
                        settingSelectedMode(Globals.privacy);
                    }
                }
                catch (NotSetException)
                {
                    Commands.SetPrivacy.ExecuteAsync(Globals.PRIVATE);
                }
                Commands.UpdateConversationDetails.RegisterCommand(updateConversationDetailsCommand);
                Commands.TextboxFocused.RegisterCommandToDispatcher(textboxFocusedCommand);
                DataContext = this;
            };
            Unloaded += (s, e) => {
                Commands.SetPrivacy.UnregisterCommand(setPrivacyCommand);
                Commands.UpdateConversationDetails.UnregisterCommand(updateConversationDetailsCommand);
                Commands.TextboxFocused.UnregisterCommand(textboxFocusedCommand);
            };
        }

        private void UpdatePrivacyFromSelectedTextBox(TextInformation info)
        {
            if (info.Target == "presentationSpace")
            {
                string setPrivacy = info.IsPrivate ? Globals.PRIVATE : Globals.PUBLIC;
                Commands.SetPrivacy.ExecuteAsync(setPrivacy);
            }           
        }

        private void updateConversationDetails(ConversationDetails details)
        {
            if (ConversationDetails.Empty.Equals(details)) return;
            settingEnabledModes(details);
        }

        private void settingEnabledModes(ConversationDetails details)
        {
            Dispatcher.adopt(() =>
                                  {
                                      if ((details.Permissions.studentCanPublish && !details.blacklist.Contains(Globals.me)) || rootPage.details.isAuthor(Globals.me))
                                      {
                                          publicMode.IsEnabled = true;
                                          var privacy = rootPage.details.isAuthor(Globals.me) ? Globals.PUBLIC : Globals.PRIVATE;
                                          SetPrivacy(privacy);
                                      }

                                      else
                                      {
                                          publicMode.IsEnabled = false;
                                          SetPrivacy(Globals.PRIVATE);
                                          Commands.SetPrivacy.Execute(Globals.PRIVATE);
                                      }
                                  });
        }

        private bool canSetPrivacy(string privacy)
        {
            try
            {
                var result = privacy != (string)GetValue(PrivateProperty)
                && ((rootPage.details.Permissions.studentCanPublish && !rootPage.details.blacklist.Contains(Globals.me)) || rootPage.details.Author == Globals.me);
                return result;
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
                                          SetValue(PrivateProperty, p);
                                          Commands.RequerySuggested(Commands.SetPrivacy);
                                      });
        }

        private void settingSelectedMode(string p)
        {
            if (p == "public")
            {
                publicMode.IsChecked = true;
                privateMode.IsChecked = false;                
            }
            else
            {
                publicMode.IsChecked = false;
                privateMode.IsChecked = true;                
            }
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