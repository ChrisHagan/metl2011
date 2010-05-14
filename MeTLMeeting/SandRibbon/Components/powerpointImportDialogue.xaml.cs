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
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Utils.Connection;
using SandRibbon.Providers.Structure;
using SandRibbonObjects;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;
using ListBox = System.Windows.Controls.ListBox;
using System.Windows.Forms;

namespace SandRibbon.Components
{
    public partial class powerpointImportDialogue : Window
    {
        public static IEnumerable<ConversationDetails> extantConversations = new List<ConversationDetails>();
        public SandRibbon.Utils.Connection.JabberWire.Credentials credentials;
        public ObservableCollection<string> authorizedGroups = new ObservableCollection<string>();
        public ConversationDetails details;
        public string me;

        private ConversationConfigurationMode DialogMode;
        private enum ConversationConfigurationMode { Create, Edit, Import, Delete }

        //PowerpointImportOptions
        public int magnification = 4;
        public string importType;
        public string importFile;
        public string action;
        public bool isComplete;

        public static RoutedCommand CompleteConversationDialog = new RoutedCommand();

        public powerpointImportDialogue(string mode)
        {
            InitializeComponent();
            switch (mode)
            {
                case "import":
                    DialogMode = ConversationConfigurationMode.Import;
                    break;
                case "create":
                    DialogMode = ConversationConfigurationMode.Create;
                    break;
                case "edit":
                    DialogMode = ConversationConfigurationMode.Edit;
                    break;
                case "delete":
                    DialogMode = ConversationConfigurationMode.Delete;
                    break;
            }
            extantConversations = ConversationDetailsProviderFactory.Provider.ListConversations();
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            this.CommandBindings.Add(new CommandBinding(CompleteConversationDialog, Create, CanCompleteDialog));
            //CompleteConversationDialog. RegisterCommand(new DelegateCommand<object>(Create, CanCompleteDialog));
        }
        private void PopulateFields()
        {
            conversationNameTextBox.Text = details.Title;
            conversationTagTextBox.Text = details.Tag;
            if (conversationSubjectListBox.Items.Count > 0)
                conversationSubjectListBox.SelectedItem = conversationSubjectListBox.Items[conversationSubjectListBox.Items.IndexOf(details.Subject.ToString())];
            if (conversationStyleListBox.Items.Count > 0)
            {
                switch ((Permissions.InferredTypeOf(details.Permissions)).Label)
                {
                    case "lecture":
                        conversationStyleListBox.SelectedItem = conversationStyleListBox.Items[0];
                        break;
                    case "tutorial":
                        conversationStyleListBox.SelectedItem = conversationStyleListBox.Items[1];
                        break;
                    case "meeting":
                        conversationStyleListBox.SelectedItem = conversationStyleListBox.Items[2];
                        break;
                    default:
                        conversationStyleListBox.SelectedItem = conversationStyleListBox.Items[0];
                        break;
                }
            }
        }
        private void UpdateDialogBoxAppearance()
        {
            switch (DialogMode)
            {
                case ConversationConfigurationMode.Create:
                    createGroup.Visibility = Visibility.Visible;
                    importGroup.Visibility = Visibility.Collapsed;
                    CommitButton.Content = "Create Conversation";
                    Height = 400;
                    Width = 800;
                    action = "create";
                    if (details == null)
                        details = new ConversationDetails { Author = me, Created = DateTime.Now, Subject = "Unrestricted", Title = "Please enter title here", Permissions = Permissions.LECTURE_PERMISSIONS };
                    break;
                case ConversationConfigurationMode.Edit:
                    createGroup.Visibility = Visibility.Collapsed;
                    importGroup.Visibility = Visibility.Collapsed;
                    CommitButton.Content = "Update Conversation";
                    action = "edit";
                    Height = 400;
                    Width = 380;
                    if (details == null)
                    {
                        MessageBox.Show("No valid conversation currently selected.  Please ensure you are in a conversation you own when editing a conversation.");
                        this.Close();
                    }
                    break;
                case ConversationConfigurationMode.Import:
                    createGroup.Visibility = Visibility.Visible;
                    importGroup.Visibility = Visibility.Visible;
                    CommitButton.Content = "Import powerpoint into Conversation";
                    importSelector.SelectedItem = importSelector.Items[0];
                    action = "import";
                    Height = 400;
                    Width = 800;
                    if (details == null)
                        details = new ConversationDetails { Author = me, Created = DateTime.Now, Subject = "Unrestricted", Title = "Please enter title here", Permissions = Permissions.LECTURE_PERMISSIONS };
                    break;
                case ConversationConfigurationMode.Delete:
                    createGroup.Visibility = Visibility.Collapsed;
                    importGroup.Visibility = Visibility.Collapsed;
                    CommitButton.Content = "Delete Conversation";
                    action = "delete";
                    Height = 400;
                    Width = 380;
                    if (details == null)
                    {
                        MessageBox.Show("No valid conversation currently selected.  Please ensure you are in a conversation you own when deleting a conversation.");
                        this.Close();
                    }
                    break;
            }
        }
        private void BrowseFiles(object sender, RoutedEventArgs e)
        {
            var fileBrowser = new OpenFileDialog
            {
                InitialDirectory = "c:\\",
                Filter = "PowerPoint files (*.ppt, *.pptx)|*.ppt; *.pptx|All files (*.*)|*.*",
                FilterIndex = 0,
                RestoreDirectory = true,
                Multiselect = false
            };
            var fileDialogResult = fileBrowser.ShowDialog();
            if (!String.IsNullOrEmpty(fileBrowser.FileName))
            {
                importFile = fileBrowser.FileName;
                importFileTextBox.Text = importFile;
            }
        }
        private void UpdateConversationDetails(ConversationDetails details)
        {
            extantConversations = null;
            extantConversations = ConversationDetailsProviderFactory.Provider.ListConversations();
        }
        private void UpdateImportFile(object sender, TextChangedEventArgs e)
        {
            importFile = ((TextBox)sender).Text;
        }
        private void UpdateConversationTitle(object sender, TextChangedEventArgs e)
        {
            if (details != null)
                details.Title = ((TextBox)sender).Text;
        }
        private void UpdateConversationTag(object sender, TextChangedEventArgs e)
        {
            if (details != null)
                details.Tag = ((TextBox)sender).Text;
        }
        private bool checkConversation(ConversationDetails proposedDetails)
        {
            if (proposedDetails == null) return false;
            if (extantConversations == null) return false;
            proposedDetails.Title = proposedDetails.Title.Trim();
            var currentDetails = details;
            var thisIsAValidTitle = !String.IsNullOrEmpty(proposedDetails.Title.Trim());
            var thisTitleIsNotTaken = (extantConversations.Where(c => c.Title.ToLower().Equals(proposedDetails.Title.ToLower())).Count() == 0);
            var IAmTheAuthor = (details.Author == me);
            return thisIsAValidTitle && thisTitleIsNotTaken && IAmTheAuthor;
        }


        private void selectChoice(object sender, RoutedEventArgs e)
        {
            var choice = ((FrameworkElement)sender).Tag;
            importType = choice.ToString();
 }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (String.IsNullOrEmpty(importType) || (!(checkConversation(details))))
                importType = "cancel";
        }
        private void Create(object sender, ExecutedRoutedEventArgs e)
        {
            if ((!String.IsNullOrEmpty(importType)) && checkConversation(details))
            {
                this.isComplete = true;
                this.Close();
            }
        }
        private void Close(object sender, RoutedEventArgs e)
        {
            importType = "cancel";
            this.Close();
        }
        private void startingContentListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (((ListBoxItem)(((ListBox)sender).SelectedItem)).Tag.ToString())
            {
                case "whiteboard":
                    DialogMode = ConversationConfigurationMode.Create;
                    UpdateDialogBoxAppearance();
                    break;
                case "powerpoint":
                    DialogMode = ConversationConfigurationMode.Import;
                    UpdateDialogBoxAppearance();
                    break;
            }
 }

        private void conversationStyleListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Permissions newPermissions;
            if (String.IsNullOrEmpty(((ListBoxItem)((ListBox)sender).SelectedItem).Tag.ToString()))
                return;
            switch (((ListBoxItem)(((ListBox)sender).SelectedItem)).Tag.ToString())
            {
                case "LECTURE_PERMISSIONS":
                    newPermissions = Permissions.LECTURE_PERMISSIONS;
                    break;
                case "TUTORIAL_PERMISSIONS":
                    newPermissions = Permissions.TUTORIAL_PERMISSIONS;
                    break;
                case "MEETING_PERMISSIONS":
                    newPermissions = Permissions.MEETING_PERMISSIONS;
                    break;
                case "LABORATORY_PERMISSIONS":
                    newPermissions = Permissions.LABORATORY_PERMISSIONS;
                    break;
                default:
                    newPermissions = Permissions.TUTORIAL_PERMISSIONS;
                    break;
            }
            if (details != null)
                details.Permissions = newPermissions;
 
        }

        private void conversationSubjectListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (String.IsNullOrEmpty((((ListBox)sender).SelectedItem).ToString()))
                return;
            if (details != null)
                details.Subject = (((ListBox)sender).SelectedItem).ToString();
        }
        private void AttachHandlers()
        {
            conversationSubjectListBox.SelectionChanged += conversationSubjectListBox_SelectionChanged;
            conversationStyleListBox.SelectionChanged += conversationStyleListBox_SelectionChanged;
            startingContentSelector.SelectionChanged += startingContentListBox_SelectionChanged;
            CommitButton.Command = CompleteConversationDialog;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            conversationSubjectListBox.ItemsSource = authorizedGroups;
            conversationSubjectListBox.SelectedItem = conversationSubjectListBox.Items[0];
            UpdateDialogBoxAppearance();
            PopulateFields();
            AttachHandlers();
        }

        private void CanCompleteDialog(object sender, CanExecuteRoutedEventArgs e)
        {
            bool canExecute = false;
            if (details != null)
            {
                switch (DialogMode)
                {
                    case ConversationConfigurationMode.Import:
                        if (checkConversation(details) && (!String.IsNullOrEmpty(importFile)) && (!String.IsNullOrEmpty(importFile)))
                            canExecute = true;
                        break;
                    case ConversationConfigurationMode.Edit:
                        if (checkConversation(details))
                            canExecute = true; 
                            break;
                    case ConversationConfigurationMode.Create:
                        if (checkConversation(details))
                            canExecute = true; 
                            break;
                    default:
                            break;
                }
            }
            e.CanExecute = canExecute;
        }
    }
}
