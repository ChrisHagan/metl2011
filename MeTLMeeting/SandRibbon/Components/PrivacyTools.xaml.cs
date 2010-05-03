using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbonObjects;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace SandRibbon.Components
{
    public partial class PrivacyTools : UserControl
    {
        public static readonly DependencyProperty PrivateProperty =
            DependencyProperty.Register("Private", typeof(string), typeof(PrivacyTools), new UIPropertyMetadata("public"));
        public static PrivacyEnablementChecker PrivacySetterIsEnabled = new PrivacyEnablementChecker();
        private ConversationDetails currentDetails;

        public PrivacyTools()
        {
            InitializeComponent();
            Commands.SetPrivacy.RegisterCommand(new DelegateCommand<string>( SetPrivacy,canSetPrivacy));
            DataContext = this;
        }
        private bool canSetPrivacy(string privacy)
        {
            return privacy != (string)GetValue(PrivateProperty);
        }
        private void SetPrivacy(string p)
        {
            var doPrivacy = (Action) delegate
                                         {
                                             SetValue(PrivateProperty, p);
                                             Commands.RequerySuggested(Commands.SetPrivacy);
                                         };
            if (Thread.CurrentThread != Dispatcher.Thread)
                Dispatcher.BeginInvoke(doPrivacy);
            else
                doPrivacy();
        }
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
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