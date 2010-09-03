using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
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
                    Commands.SetPrivacy.Execute("public");
                else
                    Commands.SetPrivacy.Execute("private");
            }
            catch (NotSetException)
            {
                Commands.SetPrivacy.Execute("Private");
            }
            DataContext = this;
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
                                          WorkPubliclyButton.IsChecked = p == "public";
                                          SetValue(PrivateProperty, p);
                                          Commands.RequerySuggested(Commands.SetPrivacy);
                                      });
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
            if (thisButton.IsChecked == true)
            {
                Commands.SetPrivacy.Execute("public");
            }
            else
                Commands.SetPrivacy.Execute("private");
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
            Commands.SetPrivacy.Execute(value);
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