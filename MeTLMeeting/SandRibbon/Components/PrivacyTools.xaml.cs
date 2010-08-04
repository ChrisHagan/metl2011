using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbonObjects;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using SandRibbon.Providers;

namespace SandRibbon.Components
{
    public partial class PrivacyTools : UserControl
    {
        public static readonly DependencyProperty PrivateProperty =
            DependencyProperty.Register("Private", typeof(string), typeof(PrivacyTools), new UIPropertyMetadata("public"));
        public static PrivacyEnablementChecker PrivacySetterIsEnabled = new PrivacyEnablementChecker();
        
        public PrivacyTools()
        {
            InitializeComponent();
            Commands.SetPrivacy.RegisterCommand(new DelegateCommand<string>(SetPrivacy,canSetPrivacy));
            try
            {
                if (Globals.isAuthor)
                    Commands.SetPrivacy.Execute("public");
                else
                    Commands.SetPrivacy.Execute("private");
            }
            catch (NotSetException) { 
                Commands.SetPrivacy.Execute("Private");
            }
            DataContext = this;
        }
        private bool canSetPrivacy(string privacy)
        {
            try
            {
                return privacy != (string)GetValue(PrivateProperty)
                && (Globals.conversationDetails.Permissions.studentCanPublish || Globals.conversationDetails.Author == Globals.me);
            }
            catch (Exception e)
            {
                return false;
            }
        }
        private void SetPrivacy(string p)
        {
            Dispatcher.adoptAsync((Action) delegate
                                         {
                                             if (p == "public")
                                                 WorkPubliclyButton.IsChecked = true;
                                             else
                                                 WorkPubliclyButton.IsChecked = false;
                                             SetValue(PrivateProperty, p);
                                             Commands.RequerySuggested(Commands.SetPrivacy);
                                         });
        }
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new PrivacyToolsAutomationPeer(this);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
           // if(!canSetPrivacy("private")) return;
            var thisButton = ((CheckBox)sender);
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
            get { return (PrivacyTools)base.Owner; }
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
            get { return (string)this.PrivacyTools.GetValue(PrivacyTools.PrivateProperty); }
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