using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SandRibbon.Providers;
using MessageBox = System.Windows.MessageBox;
using ListBox = System.Windows.Controls.ListBox;
//using System.Windows.Forms;
using SandRibbon.Utils;
using MeTLLib.DataTypes;
using System.IO;
using Microsoft.Win32;
using SandRibbon.Components.Utility;

namespace SandRibbon.Components
{
    public partial class ConversationConfigurationDialog : Window
    {
        private static readonly string DEFAULT_POWERPOINT_PREFIX = "Presentation";
        public static IEnumerable<ConversationDetails> extantConversations = new List<ConversationDetails>();
        private ConversationDetails details;
        private ConversationConfigurationMode dialogMode;
        public enum ConversationConfigurationMode { CREATE, EDIT, IMPORT, DELETE }
        private PowerpointImportType importType;
        private string importFile;
        private string conversationJid;

        public NetworkController networkController { get; protected set; }
        public static RoutedCommand CompleteConversationDialog = new RoutedCommand();

        public ConversationConfigurationDialog(NetworkController _controller, ConversationConfigurationMode mode, string activeConversation)
            : this(_controller, mode)
        {
            conversationJid = activeConversation;
        }
        public ConversationConfigurationDialog(NetworkController _controller, ConversationConfigurationMode mode)
        {
            networkController = _controller;
            InitializeComponent();
            this.dialogMode = mode;
            this.CommandBindings.Add(new CommandBinding(CompleteConversationDialog, Create, CanCompleteDialog));
        }
        private void PopulateFields()
        {
            if (details == null) return;
            if (String.IsNullOrEmpty(conversationNameTextBox.Text))
                conversationNameTextBox.Text = details.Title;
            else
                doUpdateConversationTitle();
        }
        private void UpdateDialogBoxAppearance()
        {
            var suggestedName = ConversationDetails.DefaultName(networkController.credentials.name);
            switch (dialogMode)
            {
                case ConversationConfigurationMode.CREATE:
                    createGroup.Visibility = Visibility.Visible;
                    importGroup.Visibility = Visibility.Collapsed;
                    CommitButton.Content = "Create";
                    if (details == null)
                        details = new ConversationDetails
                        (suggestedName,"",networkController.credentials.name,new List<Slide>(),Permissions.LECTURE_PERMISSIONS,"Unrestricted",SandRibbonObjects.DateTimeFactory.Now(),SandRibbonObjects.DateTimeFactory.Now());
                    break;
                case ConversationConfigurationMode.EDIT:
                    createGroup.Visibility = Visibility.Collapsed;
                    importGroup.Visibility = Visibility.Collapsed;
                    CommitButton.Content = "Update";
                    details = App.controller.client.DetailsOf(conversationJid);
                    PopulateFields();
                    if (details == null)
                    {
                        MeTLMessage.Warning("No valid conversation currently selected.  Please ensure you are in a conversation you own when editing a conversation.");
                        this.Close();
                    }
                    break;
                case ConversationConfigurationMode.IMPORT:
                    createGroup.Visibility = Visibility.Visible;
                    importGroup.Visibility = Visibility.Visible;
                    LowQualityPowerpointListBoxItem.IsChecked = true;
                    CommitButton.Content = "Create";
                    details = new ConversationDetails
                    (suggestedName, "", networkController.credentials.name, new List<Slide>(), Permissions.LECTURE_PERMISSIONS, "Unrestricted", SandRibbonObjects.DateTimeFactory.Now(), SandRibbonObjects.DateTimeFactory.Now());
                    break;
            }
        }
        private void BrowseFiles(object sender, RoutedEventArgs e)
        {
            doBrowseFiles();
        }
        private void doBrowseFiles()
        {
            string initialDirectory = "\\";
            if (Commands.RegisterPowerpointSourceDirectoryPreference.IsInitialised)
            {
                initialDirectory = (string)Commands.RegisterPowerpointSourceDirectoryPreference.LastValue();
            }
            else
            {
                //These variables may or may not be available in any given OS
                foreach (var path in new[] { Environment.SpecialFolder.MyDocuments, Environment.SpecialFolder.DesktopDirectory, Environment.SpecialFolder.MyComputer })
                try
                {
                    initialDirectory = Environment.GetFolderPath(path);
                    break;
                }
                catch (Exception) {}
            }
            var fileBrowser = new OpenFileDialog
            {
                InitialDirectory = initialDirectory,
                Filter = "PowerPoint files (*.ppt, *.pptx)|*.ppt; *.pptx|All files (*.*)|*.*",
                FilterIndex = 0,
                RestoreDirectory = true,
                Multiselect = false
            };
            var fileDialogResult = fileBrowser.ShowDialog(this.Owner);
            if (!String.IsNullOrEmpty(fileBrowser.FileName))
            {
                importFile = fileBrowser.FileName;
                importFileTextBox.Text = importFile;
                var file = new FileInfo(fileBrowser.FileName);
                if (Globals.rememberMe)
                {
                    Commands.RegisterPowerpointSourceDirectoryPreference.Execute(System.IO.Path.GetDirectoryName(fileBrowser.FileName));
                    WorkspaceStateProvider.SaveCurrentSettings();
                }
            }
            if (!fileDialogResult.HasValue) 
                importFile = null;
        }
        private void UpdateImportFile(object sender, TextChangedEventArgs e)
        {
        }
        private string generatePresentationTitle(string currentTitle, string file)
        {
            string importFileName = System.IO.Path.GetFileNameWithoutExtension(file);
            
            if(!System.Text.RegularExpressions.Regex.IsMatch(importFile, string.Format("/^{0}\\d+/", DEFAULT_POWERPOINT_PREFIX)))
                return string.Format("({0}) {1}", importFileName, currentTitle);
            return currentTitle;
        }
        private void UpdateConversationTitle(object sender, TextChangedEventArgs e)
        {
            doUpdateConversationTitle();
        }
        private void doUpdateConversationTitle(){
            if (details != null)
                details.Title = conversationNameTextBox.Text;
        }
        private bool checkConversation(ConversationDetails proposedDetails)
        {
            if (proposedDetails == null) { return false; }
            if (extantConversations == null) { return false; }
            proposedDetails.Title = proposedDetails.Title.Trim();
            var thisIsAValidTitle = !String.IsNullOrEmpty(proposedDetails.Title.Trim());
            var thisTitleIsNotTaken = dialogMode == ConversationConfigurationMode.EDIT ? true :
                (extantConversations.Where(c => c.Title.ToLower().Equals(proposedDetails.Title.ToLower())).Count() == 0);
            var IAmTheAuthor = (details.Author == networkController.credentials.name);
            string ErrorText = "";
            if (!thisIsAValidTitle) { ErrorText += "Invalid conversation title.  "; }
            if (!thisTitleIsNotTaken) { ErrorText += "Conversation title already used.  "; }
            if (!IAmTheAuthor) { ErrorText += "You do not have permission to edit this conversation.  "; }
            errorText.Content = ErrorText;
            return thisIsAValidTitle && thisTitleIsNotTaken && IAmTheAuthor;
        }
        private void selectChoice(object sender, RoutedEventArgs e)
        {
            switch (((FrameworkElement)sender).Tag.ToString())
            {
                case "whiteboard":
                    dialogMode = ConversationConfigurationMode.CREATE;
                    UpdateDialogBoxAppearance();
                    break;
                case "editable":
                    dialogMode = ConversationConfigurationMode.IMPORT;
                    UpdateDialogBoxAppearance();
                    importType = PowerpointImportType.Shapes;
                    break;
                case "highquality":
                    dialogMode = ConversationConfigurationMode.IMPORT;
                    importType = PowerpointImportType.HighDefImage;
                    UpdateDialogBoxAppearance();
                    break;
                case "lowquality":
                    dialogMode = ConversationConfigurationMode.IMPORT;
                    importType = PowerpointImportType.Image;
                    UpdateDialogBoxAppearance();
                    break;
            }
        }
        private PowerpointSpec handleConversationDialogueCompletion()
        {
            switch (dialogMode)
            {
                case ConversationConfigurationMode.IMPORT:
                    if ((!String.IsNullOrEmpty(importFile)) && importFile.Contains(".ppt"))
                    {
                        try
                        {
                            return new PowerpointSpec
                            {
                                File = importFile,
                                Details = details,
                                Type = importType,
                                Magnification = importType == PowerpointImportType.HighDefImage ? 3 : 1 // Globals.UserOptions.powerpointImportScale == 2 ? 2 : 1 
                            };
                        }
                        catch (Exception)
                        {
                            MeTLMessage.Error("Sorry, MeTL encountered a problem while trying to import your PowerPoint.  If the conversation was created, please check whether it has imported correctly.");
                            throw;
                        }
                        finally
                        {
                            Commands.PowerpointFinished.ExecuteAsync(null);
                        }
                    }
                    else
                    {
                        MeTLMessage.Warning("Sorry. I do not know what to do with that file format");
                    }
                    break;
                case ConversationConfigurationMode.CREATE:
                    Commands.CreateConversation.ExecuteAsync(details);
                    Commands.PowerpointFinished.ExecuteAsync(null);
                    break;
                case ConversationConfigurationMode.EDIT:
                    App.controller.client.UpdateConversationDetails(details);
                    Commands.PowerpointFinished.ExecuteAsync(null);
                    break;
                case ConversationConfigurationMode.DELETE:
                    App.controller.client.UpdateConversationDetails(details);
                    Commands.PowerpointFinished.ExecuteAsync(null);
                    break;
            }
            Commands.PowerpointFinished.ExecuteAsync(null);
            return null;
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
            Commands.PowerpointFinished.ExecuteAsync(null);
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
            CommitButton.Command = CompleteConversationDialog;
            InputBindings.Add(new KeyBinding(CompleteConversationDialog, System.Windows.Input.Key.Enter,ModifierKeys.None));
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
                    case ConversationConfigurationMode.DELETE:
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
            Commands.PowerpointFinished.ExecuteAsync(null);
        }
        private void selectAll(object sender, RoutedEventArgs e)
        {
            conversationNameTextBox.SelectAll();
        }
        public void ChooseFileForImport()
        {
            if (dialogMode == ConversationConfigurationMode.IMPORT)
            {
                conversationNameTextBox.Text = ConversationDetails.DefaultName(networkController.credentials.name);
                doBrowseFiles();
            }
        }
        internal PowerpointSpec Import(PowerpointImportType _importType)
        {
            if (importFile == null) return null;
             dialogMode = ConversationConfigurationMode.IMPORT;
             importType = _importType;
             var suggestedName = generatePresentationTitle(ConversationDetails.DefaultName(networkController.credentials.name), importFile );
             details = new ConversationDetails
                    (suggestedName, "", networkController.credentials.name, new List<Slide>(), Permissions.LECTURE_PERMISSIONS, "Unrestricted", SandRibbonObjects.DateTimeFactory.Now(), SandRibbonObjects.DateTimeFactory.Now());
            if (checkConversation(details))
                return handleConversationDialogueCompletion();
            return null;
        }
    }
}