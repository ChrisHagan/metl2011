using System;
using System.Windows.Controls;
using System.Windows.Data;
using SandRibbonObjects;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Windows.Input;
using System.Windows;
using SandRibbon.Utils.Connection;

namespace SandRibbon.Components.SimpleImpl
{
    public class TitleHydrator : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //var permissions = values[2] == DependencyProperty.UnsetValue ? Permissions.LECTURE_PERMISSIONS : (Permissions)values[2];
            string sSubject = values[2] == DependencyProperty.UnsetValue ? String.Empty : (string)values[2]; // ST***
            var permissions = values[3] == DependencyProperty.UnsetValue ? Permissions.LECTURE_PERMISSIONS : (Permissions)values[3];//ST***
            return new ConversationDetails
            {
                Title = (string)values[0],
                Tag = (string)values[1],
                Subject = sSubject,  //ST***
                Permissions = permissions
            };
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class LabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.Format("{0} {1}", value, parameter);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public partial class SimpleConversationCreator : UserControl
    {
        public DelegateCommand<ConversationDetails> create;
        public string me { get; set; }
        public JabberWire.AuthorizedGroups oGroups { get; set; }
        private CompositeCommand createActionProperty;
        public CompositeCommand CreateAction
        {
            get { return createActionProperty; }
            set
            {
                if (CreateAction != null)
                    CreateAction.UnregisterCommand(create);
                createActionProperty = value;
                CreateAction.RegisterCommand(create);
                // ST: This isn't working (null for groups) because the user has not yet logged in.
                //SandRibbon.Utils.GroupsManagement.fillComboWithEligibleGroups(subjectList, oGroups);
            }
        }
        public string ActionDescriptor { get; set; }
        public static Permissions LECTURE_PERMISSIONS = new Permissions
        {
            studentCanPublish = false,
            studentCanOpenFriends = false,
            usersAreCompulsorilySynced = true
        };
        public static Permissions LABORATORY_PERMISSIONS = new Permissions
        {
            studentCanPublish = false,
            studentCanOpenFriends = true,
            usersAreCompulsorilySynced = false
        };
        public static Permissions TUTORIAL_PERMISSIONS = new Permissions
        {
            studentCanPublish = true,
            studentCanOpenFriends = true,
            usersAreCompulsorilySynced = false
        };
        public static Permissions MEETING_PERMISSIONS = new Permissions
        {
            studentCanPublish = true,
            studentCanOpenFriends = true,
            usersAreCompulsorilySynced = false
        };
        public SimpleConversationCreator()
        {
            InitializeComponent();
            create = new DelegateCommand<ConversationDetails>((_details) => { }, canCreate);
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<SandRibbon.Utils.Connection.JabberWire.Credentials>(
                author => me = author.name));
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<SandRibbon.Utils.Connection.JabberWire.Credentials>(
                author => oGroups = author.authorizedGroups)); DataContext = this;
        }
        private bool canCreate(ConversationDetails details)
        {
            return details.Title.Length > 0;
        }
        private void checkCanSubmit(object sender, TextChangedEventArgs e)
        {
            if (create != null)
                create.RaiseCanExecuteChanged();
        }
        public void FocusCreate()
        {
            SandRibbon.Utils.GroupsManagement.fillComboWithEligibleGroups(subjectList, oGroups);
            conversationName.Focus();
        }
    }
}
