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
using SandRibbon.Providers;
using SandRibbon.Utils.Connection;
using SandRibbon.Providers.Structure;
using SandRibbonObjects;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;
using ListBox = System.Windows.Controls.ListBox;
using System.Windows.Forms;
using SandRibbon.Utils;
using SandRibbon.Components.Pedagogicometry;

namespace SandRibbon.Components
{
    public partial class ConversationConfigurationDialog : Window
    {
        public static IEnumerable<ConversationDetails> extantConversations = new List<ConversationDetails>();
        private ConversationDetails details;
        private ConversationConfigurationMode dialogMode;
        public enum ConversationConfigurationMode { CREATE, EDIT, IMPORT, DELETE }
        private int magnification = 2;
        private PowerPointLoader.PowerpointImportType importType;
        private string importFile;

        public static RoutedCommand CompleteConversationDialog = new RoutedCommand();

        public ConversationConfigurationDialog(ConversationConfigurationMode mode)
        {
            InitializeComponent();
            this.dialogMode = mode;
            conversationSubjectListBox.ItemsSource = Globals.authorizedGroups.Select(ag => ag.groupKey);
            extantConversations = ConversationDetailsProviderFactory.Provider.ListConversations();
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            this.CommandBindings.Add(new CommandBinding(CompleteConversationDialog, Create, CanCompleteDialog));
        }
        private void PopulateFields()
        {
            if (details == null) return;
            conversationNameTextBox.Text = details.Title;
            if (conversationSubjectListBox.Items.Count > 0)
                conversationSubjectListBox.SelectedItem = conversationSubjectListBox.Items[conversationSubjectListBox.Items.IndexOf(details.Subject.ToString())];
        }
        private void UpdateDialogBoxAppearance()
        {
            switch (dialogMode)
            {
                case ConversationConfigurationMode.CREATE:
                    createGroup.Visibility = Visibility.Visible;
                    importGroup.Visibility = Visibility.Collapsed;
                    CommitButton.Content = "Create Conversation";
                    Height = 400;
                    Width = 800;
                    if (startingContentSelector != null && startingContentSelector.Items.Count > 0)
                        startingContentSelector.SelectedIndex = 0;
                    if (details == null)
                        details = new ConversationDetails { Author = Globals.me, Created = DateTime.Now, Subject = "Unrestricted", Title = "Please enter title here", Permissions = Permissions.LECTURE_PERMISSIONS };
                    break;
                case ConversationConfigurationMode.EDIT:
                    createGroup.Visibility = Visibility.Collapsed;
                    importGroup.Visibility = Visibility.Collapsed;
                    CommitButton.Content = "Update Conversation";
                    Height = 400;
                    Width = 380;
                    details = ConversationDetailsProviderFactory.Provider.DetailsOf(Globals.location.activeConversation);
                    PopulateFields();
                    if (details == null)
                    {
                        MessageBox.Show("No valid conversation currently selected.  Please ensure you are in a conversation you own when editing a conversation.");
                        this.Close();
                    }
                    break;
                case ConversationConfigurationMode.IMPORT:
                    createGroup.Visibility = Visibility.Visible;
                    importGroup.Visibility = Visibility.Visible;
                    CommitButton.Content = "Import powerpoint into Conversation";
                    importSelector.SelectedItem = importSelector.Items[0];
                    Height = 400;
                    Width = 800;
                    if (startingContentSelector != null && startingContentSelector.Items.Count > 1)
                        startingContentSelector.SelectedIndex = 1;
                    if (details == null)
                        details = new ConversationDetails { Author = Globals.me, Created = DateTime.Now, Subject = "Unrestricted", Title = "Please enter title here", Permissions = Permissions.LECTURE_PERMISSIONS };
                    break;
                case ConversationConfigurationMode.DELETE:
                    createGroup.Visibility = Visibility.Collapsed;
                    importGroup.Visibility = Visibility.Collapsed;
                    CommitButton.Content = "Delete Conversation";
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

            string initialDirectory = "c:\\";
            foreach (var path in new[] { Environment.SpecialFolder.MyDocuments, Environment.SpecialFolder.DesktopDirectory, Environment.SpecialFolder.MyComputer })
                try
                {
                    initialDirectory = Environment.GetFolderPath(path);
                    break;
                }
                catch (Exception)
                {
                }
            var fileBrowser = new OpenFileDialog
            {
                InitialDirectory = initialDirectory,
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
            string importFileName = importFile.Split(new char[] { '\\' }).LastOrDefault();
            if (conversationNameTextBox != null && conversationNameTextBox.Text == "Please enter title here")
            {
                if (importFileName.EndsWith(".ppt"))
                    importFileName = importFileName.Substring(0, importFileName.Length - 4);
                else if (importFileName.EndsWith(".pptx"))
                    importFileName = importFileName.Substring(0, importFileName.Length - 5);
                conversationNameTextBox.Text = importFileName;
            }
        }
        private void UpdateConversationTitle(object sender, TextChangedEventArgs e)
        {
            if (details != null)
                details.Title = ((TextBox)sender).Text;
        }
        private bool checkConversation(ConversationDetails proposedDetails)
        {
            if (proposedDetails == null) { return false; }
            if (extantConversations == null) { return false; }
            proposedDetails.Title = proposedDetails.Title.Trim();
            var currentDetails = details;
            var thisIsAValidTitle = !String.IsNullOrEmpty(proposedDetails.Title.Trim());
            var thisTitleIsNotTaken = dialogMode == ConversationConfigurationMode.EDIT ? true :
                (extantConversations.Where(c => c.Title.ToLower().Equals(proposedDetails.Title.ToLower())).Count() == 0);
            var IAmTheAuthor = (details.Author == Globals.me);
            string ErrorText = "";
            if (!thisIsAValidTitle) { ErrorText += "Invalid conversation title.  "; }
            if (!thisTitleIsNotTaken) { ErrorText += "Conversation title already used.  "; }
            if (!IAmTheAuthor) { ErrorText += "You do not have permission to edit this conversation.  "; }
            errorText.Content = ErrorText;
            return thisIsAValidTitle && thisTitleIsNotTaken && IAmTheAuthor;
        }
        private void selectChoice(object sender, RoutedEventArgs e)
        {
            var choice = ((FrameworkElement)sender).Tag;
            importType = (PowerPointLoader.PowerpointImportType)Enum.Parse(typeof(PowerPointLoader.PowerpointImportType), choice.ToString());
        }
        private void handleConversationDialogueCompletion()
        {
            switch (dialogMode)
            {
                case ConversationConfigurationMode.IMPORT:
                    if ((!String.IsNullOrEmpty(importFile)) && importFile.Contains(".ppt"))
                    {
                        try
                        {
                            Commands.UploadPowerpoint.Execute(new PowerpointSpec
                            {
                                File = importFile,
                                Details = details,
                                Type = importType,
                                Magnification = importType == (PowerPointLoader.PowerpointImportType)Enum.Parse(typeof(PowerPointLoader.PowerpointImportType), "HighDefImage") ? magnification : 1
                            });
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Sorry, MeTL encountered a problem while trying to import your powerpoint.  If the conversation was created, please check whether it has imported correctly.");
                            throw e;
                        }
                        finally
                        {
                            Commands.PowerPointLoadFinished.Execute(null);
                        }
                        return;
                    }
                    else
                    {
                        MessageBox.Show("Sorry. I do not know what to do with that file format");
                    }
                    break;
                case ConversationConfigurationMode.CREATE:
                    Commands.CreateConversation.Execute(details);
                    Commands.PowerPointLoadFinished.Execute(null);
                    break;
                case ConversationConfigurationMode.EDIT:
                    ConversationDetailsProviderFactory.Provider.Update(details);
                    Commands.PowerPointLoadFinished.Execute(null);
                    break;
            }
            Commands.PowerPointLoadFinished.Execute(null);
        }
        private void Create(object sender, ExecutedRoutedEventArgs e)
        {
            if (checkConversation(details))
            {
                handleConversationDialogueCompletion();
                this.Close();
            }
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            Commands.PowerPointLoadFinished.Execute(null);
            this.Close();
        }
        private void startingContentListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (((ListBoxItem)(((ListBox)sender).SelectedItem)).Tag.ToString())
            {
                case "whiteboard":
                    dialogMode = ConversationConfigurationMode.CREATE;
                    UpdateDialogBoxAppearance();
                    break;
                case "powerpoint":
                    dialogMode = ConversationConfigurationMode.IMPORT;
                    UpdateDialogBoxAppearance();
                    break;
            }
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
            startingContentSelector.SelectionChanged += startingContentListBox_SelectionChanged;
            CommitButton.Command = CompleteConversationDialog;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateDialogBoxAppearance();
            PopulateFields();
            AttachHandlers();
        }
        private void CanCompleteDialog(object sender, CanExecuteRoutedEventArgs e)
        {
            bool canExecute = false;
            if (details != null)
            {
                switch (dialogMode)
                {
                    case ConversationConfigurationMode.IMPORT:
                        if (checkConversation(details) && (!String.IsNullOrEmpty(importFile)) && (!String.IsNullOrEmpty(importFile)))
                            canExecute = true;
                        break;
                    case ConversationConfigurationMode.EDIT:
                        if (checkConversation(details))
                            canExecute = true;
                        break;
                    case ConversationConfigurationMode.CREATE:
                        if (checkConversation(details))
                            canExecute = true;
                        break;
                    default:
                        break;
                }
            }
            e.CanExecute = canExecute;
        }

        private void createConversation_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Commands.PowerPointLoadFinished.Execute(null);
        }

        private void selectAll(object sender, RoutedEventArgs e)
        {
            conversationNameTextBox.SelectAll();
        }
    }
}