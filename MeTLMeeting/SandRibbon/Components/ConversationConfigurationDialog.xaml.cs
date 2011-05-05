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
using MeTLLib.DataTypes;
using System.IO;

namespace SandRibbon.Components
{
    public partial class ConversationConfigurationDialog : Window
    {
        private static readonly string DEFAULT_POWERPOINT_PREFIX = "Presentation";
        public static IEnumerable<ConversationDetails> extantConversations = new List<ConversationDetails>();
        private ConversationDetails details;
        private ConversationConfigurationMode dialogMode;
        public enum ConversationConfigurationMode { CREATE, EDIT, IMPORT, DELETE }
        private int magnification = 2;
        private PowerPointLoader.PowerpointImportType importType;
        private string importFile;
        private string conversationJid;

        public static RoutedCommand CompleteConversationDialog = new RoutedCommand();

        public ConversationConfigurationDialog(ConversationConfigurationMode mode, string activeConversation)
            : this(mode)
        {
            conversationJid = activeConversation;
        }
        public ConversationConfigurationDialog(ConversationConfigurationMode mode)
        {
            InitializeComponent();
            this.dialogMode = mode;
            extantConversations = MeTLLib.ClientFactory.Connection().AvailableConversations; 
            Commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            this.CommandBindings.Add(new CommandBinding(CompleteConversationDialog, Create, CanCompleteDialog));
            if (mode == ConversationConfigurationMode.IMPORT)
            {
                conversationNameTextBox.Text = ConversationDetails.DefaultName(Globals.me);
                doBrowseFiles();
            }
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
            var suggestedName = ConversationDetails.DefaultName(Globals.me);
            switch (dialogMode)
            {
                case ConversationConfigurationMode.CREATE:
                    createGroup.Visibility = Visibility.Visible;
                    importGroup.Visibility = Visibility.Collapsed;
                    CommitButton.Content = "Create";
                    if (details == null)
                        details = new ConversationDetails
                        (suggestedName,"",Globals.me,new List<Slide>(),Permissions.LECTURE_PERMISSIONS,"Unrestricted",SandRibbonObjects.DateTimeFactory.Now(),SandRibbonObjects.DateTimeFactory.Now());
                    break;
                case ConversationConfigurationMode.EDIT:
                    createGroup.Visibility = Visibility.Collapsed;
                    importGroup.Visibility = Visibility.Collapsed;
                    CommitButton.Content = "Update";
                    details = MeTLLib.ClientFactory.Connection().DetailsOf(conversationJid);
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
                    LowQualityPowerpointListBoxItem.IsChecked = true;
                    CommitButton.Content = "Create";
                    details = new ConversationDetails
                    (suggestedName, "", Globals.me, new List<Slide>(), Permissions.LECTURE_PERMISSIONS, "Unrestricted", SandRibbonObjects.DateTimeFactory.Now(), SandRibbonObjects.DateTimeFactory.Now());
                    break;
            }
        }
        private void BrowseFiles(object sender, RoutedEventArgs e)
        {
            doBrowseFiles();
        }
        private void doBrowseFiles(){
            string initialDirectory = "\\";
            try
            {
                initialDirectory = (string)Commands.RegisterPowerpointSourceDirectoryPreference.lastValue();
            }
            catch (NotSetException) {//These variables may or may not be available in any given OS
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
            var fileDialogResult = fileBrowser.ShowDialog();
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
            if (fileDialogResult == System.Windows.Forms.DialogResult.Cancel)
                importFile = null;
        }
        private static void UpdateConversationDetails(ConversationDetails details)
        {
            extantConversations = null;
            extantConversations = MeTLLib.ClientFactory.Connection().AvailableConversations;
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
            switch (((FrameworkElement)sender).Tag.ToString())
            {
                case "whiteboard":
                    dialogMode = ConversationConfigurationMode.CREATE;
                    UpdateDialogBoxAppearance();
                    break;
                case "editable":
                    dialogMode = ConversationConfigurationMode.IMPORT;
                    UpdateDialogBoxAppearance();
                    importType = PowerPointLoader.PowerpointImportType.Shapes;
                    break;
                case "highquality":
                    dialogMode = ConversationConfigurationMode.IMPORT;
                    importType = PowerPointLoader.PowerpointImportType.HighDefImage;
                    UpdateDialogBoxAppearance();
                    break;
                case "lowquality":
                    dialogMode = ConversationConfigurationMode.IMPORT;
                    importType = PowerPointLoader.PowerpointImportType.Image;
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
                                Magnification = Globals.UserOptions.powerpointImportScale == 2 ? 2 : 1 
                            };
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Sorry, MeTL encountered a problem while trying to import your PowerPoint.  If the conversation was created, please check whether it has imported correctly.");
                            throw;
                        }
                        finally
                        {
                            Commands.PowerpointFinished.ExecuteAsync(null);
                        }
                        return null;
                    }
                    else
                    {
                        MessageBox.Show("Sorry. I do not know what to do with that file format");
                    }
                    break;
                case ConversationConfigurationMode.CREATE:
                    Commands.CreateConversation.ExecuteAsync(details);
                    Commands.PowerpointFinished.ExecuteAsync(null);
                    break;
                case ConversationConfigurationMode.EDIT:
                    MeTLLib.ClientFactory.Connection().UpdateConversationDetails(details);
                    Commands.PowerpointFinished.ExecuteAsync(null);
                    break;
                case ConversationConfigurationMode.DELETE:
                    MeTLLib.ClientFactory.Connection().UpdateConversationDetails(details);
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
        internal PowerpointSpec Import()
        {
            var myImportType = Globals.UserOptions.powerpointImportScale == 3 ? PowerPointLoader.PowerpointImportType.Shapes : PowerPointLoader.PowerpointImportType.Image;
            if (importFile == null) return null;
             dialogMode = ConversationConfigurationMode.IMPORT;
             importType = myImportType;
             var suggestedName = generatePresentationTitle(ConversationDetails.DefaultName(Globals.me), importFile );
             details = new ConversationDetails
                    (suggestedName, "", Globals.me, new List<Slide>(), Permissions.LECTURE_PERMISSIONS, "Unrestricted", SandRibbonObjects.DateTimeFactory.Now(), SandRibbonObjects.DateTimeFactory.Now());
            if (checkConversation(details))
                return handleConversationDialogueCompletion();
            return null;
        }
    }
}