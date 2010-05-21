using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Utils.Connection;
using SandRibbonObjects;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
            return string.Format("Begin in {0} style", parameter);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public partial class SimpleConversationCreator : UserControl
    {
        public DelegateCommand<ConversationDetails> create;
        private ObservableCollection<string> groupNames = new ObservableCollection<string>();
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
            DataContext = this;
            Commands.SetIdentity.RegisterCommand(new DelegateCommand<JabberWire.Credentials>(SetIdentity));
            subjectList.ItemsSource = groupNames;
            subjectList.SelectedIndex = 0;
        }
        private void SetIdentity(JabberWire.Credentials credentials)
        {
            groupNames.Clear();
            foreach (var groupName in credentials.authorizedGroups.Select(g => g.groupKey))
                groupNames.Add(groupName);
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
            conversationName.Focus();
        }

        public void Clear()
        {
            conversationName.Clear();
            conversationTag.Clear();
            subjectList.SelectedIndex = 0;
        }
    }
}