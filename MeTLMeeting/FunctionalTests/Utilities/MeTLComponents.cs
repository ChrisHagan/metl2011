using System;
using System.Linq;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Controls.Primitives;
using Keys = System.Windows.Forms.SendKeys;
using System.Windows.Automation;
using Button=System.Windows.Forms.Button;
using System.Threading;
using System.Windows;
using System.Diagnostics;
using System.IO;
using UITestFramework;
using SandRibbon.Components;
using Microsoft.Test.Input;
using FunctionalTests.Utilities;
using FunctionalTests;
using System.ComponentModel;

namespace Functional
{
    public abstract class IScreenObject : INotifyPropertyChanged
    {
        private UITestHelper parent;
        public UITestHelper Parent 
        {
            get
            {
                return parent;
            }
            set
            {
                if (parent != value)
                {
                    parent = value;
                    NotifyPropertyChanged("Parent");
                }
            }
        }

        #region INotifyPropertyChanged members
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }

    public class MeTL
    {
        private static Process metlProcess;
        private static readonly string workingDirectory;

        static MeTL()
        {
            var baseDirectory = "MeTLMeeting";
            var currentDirectory = Directory.GetCurrentDirectory();
            workingDirectory = currentDirectory.Remove(currentDirectory.IndexOf(baseDirectory) + baseDirectory.Length) + @"\SandRibbon\bin\Debug";
        }

        public static UITestHelper GetEnabledMainWindow()
        {
            var metlWindow = GetMainWindow();

            var success = metlWindow.WaitForControlEnabled();
            Assert.IsTrue(success, ErrorMessages.EXPECTED_MAIN_WINDOW);

            return metlWindow;
        }

        public static UITestHelper GetMainWindow()
        {
            var metlWindow = new UITestHelper();
            metlWindow.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_MAIN_WINDOW));

            metlWindow.Find();

            return metlWindow;
        }

        private static AutomationElementCollection FindAllWindows()
        {
            return AutomationElement.RootElement.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.AutomationIdProperty, Constants.ID_METL_MAIN_WINDOW));
        }

        public static AutomationElementCollection GetAllMainWindows()
        {
            return GetAllMainWindows(0, false);
        }
        public static AutomationElementCollection GetAllMainWindows(int expectedCount, bool wait = false)
        {
            var mainWindows = FindAllWindows();

            if (mainWindows.Count < expectedCount && wait)
            {
                int totalTime = 0;
                int sleepIncrement = 100;
                int maxTime = 5000;

                while (mainWindows.Count < expectedCount && totalTime < maxTime)
                {
                    mainWindows = FindAllWindows();
                    Thread.Sleep(sleepIncrement);

                    totalTime += sleepIncrement;
                }
            }

            return mainWindows;
        }

        public static AutomationElement StartProcess()
        {
            const int MAX_WAIT_TIME = 30000;
            const int WAIT_INCREMENT = 100;
            AutomationElement metlWindow = null;

            metlProcess = new Process();
            metlProcess.StartInfo.UseShellExecute = false;
            metlProcess.StartInfo.LoadUserProfile = true;
            metlProcess.StartInfo.WorkingDirectory = workingDirectory;
            metlProcess.StartInfo.FileName = workingDirectory + @"\MeTL Staging.exe";
            metlProcess.Start();

            int waitTime = 0;
            while (metlProcess.MainWindowHandle.Equals(IntPtr.Zero))
            {
                if (waitTime > MAX_WAIT_TIME)
                    Assert.Fail(ErrorMessages.UNABLE_TO_FIND_EXECUTABLE);

                Thread.Sleep(WAIT_INCREMENT);
                waitTime += WAIT_INCREMENT;

                metlProcess.Refresh();
            }

            try
            {
                while (metlWindow == null)
                {
                    metlWindow = AutomationElement.RootElement.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.AutomationIdProperty, Constants.ID_METL_MAIN_WINDOW));

                    if (waitTime > MAX_WAIT_TIME)
                        Assert.Fail(ErrorMessages.UNABLE_TO_FIND_EXECUTABLE);
    
                    Thread.Sleep(WAIT_INCREMENT);
                    waitTime += WAIT_INCREMENT;
                }
            }
            catch (ElementNotAvailableException)
            {
                Assert.Fail(ErrorMessages.EXPECTED_MAIN_WINDOW);
            }

            return metlWindow;
        }
    }

    public class WindowScreen : IWindowScreenObject
    {
        private UITestHelper windowScreen;

        public IWindowScreenObject WithWindow(AutomationElement window)
        {
            windowScreen = new UITestHelper(UITestHelper.RootElement, window);
            return this;
        }

        public UITestHelper ParentWindow
        {
            get { return windowScreen; }
        }

        public IScreenAction Ensure<T>(Func<T, bool> func) where T : IScreenObject, new()
        {
            var element = new T();
            element.Parent = ParentWindow;

            if (func(element))
                return new ScreenAction(ParentWindow);

            return new NullScreenAction();
        }
    }

    public class PresentationSpace
    {
        private AutomationElement _parent;
        private AutomationElement _canvas;
        public PresentationSpace(AutomationElement parent)
        {
            _parent = parent;
            _canvas = _parent.Descendant("canvas"); 
        }
        public string Style()
        {
            return _canvas.Value();
        }
    }
    public class QuickLaunch
    {
        private AutomationElement _parent;
        private AutomationElement _tute;
        private AutomationElement _lecture;
        public QuickLaunch(AutomationElement parent)
        {
            _parent = parent;
            _tute = _parent.Descendant("quickLaunchTutorial");
            _lecture = _parent.Descendant("quickLaunchLecture");
        }
        public void BecomeTutorial()
        {
            _tute.Invoke();
        }
        public void BecomeLecture()
        {
            _lecture.Invoke();
        }
    }
    public class ApplicationPopup : ConversationAction
    {
        private AutomationElement _parent;
        public ApplicationPopup(AutomationElement parent)
        {
            _parent = parent;
        }
        public void open()
        {
            var appButton = _parent.Descendant("PART_ApplicationButton");
            
            Assert.IsNotNull(appButton, "MeTL main menu 'ApplicationButton' button was not found.");
            appButton.Invoke();
        }
        public ConversationPicker RecommendedConversations()
        {
            open();
            var popup = _parent.Descendant(typeof(Popup));
            var menuItems = popup.Descendants(typeof(Divelements.SandRibbon.MenuItem));
            var prev = popup.Descendants();
            menuItems[0].Invoke();
            var post = popup.Descendants().Except(prev);
            var children = post.Aggregate("",(acc,item)=>acc+" "+item.Current.ClassName);
            return new ConversationPicker(post.Where(e => e.Current.ClassName == "SimpleConversationSelector").First());
        }
        public ConversationPicker AllConversations()
        {    
            open();
            var popup = _parent.Descendant(typeof(Popup));
            var menuItems = popup.Descendants(typeof(Divelements.SandRibbon.MenuItem));
            var prev = popup.Descendants();
            menuItems[1].Invoke();
            var post = popup.Descendants().Except(prev);
            var children = post.Aggregate("",(acc,item)=>acc+" "+item.Current.ClassName+":"+item.Current.AutomationId);
            return new ConversationPicker(post.Where(e => e.Current.ClassName == "SimpleConversationFilterer").First());
        }

        public void ImportPowerpoint(string filename)
        {
            open();
            var popup = _parent.Descendant(typeof(Popup));
            var menuItems = popup.Descendants(typeof(Divelements.SandRibbon.MenuItem));
            try
            {
                // Invoke the import powerpoint menuitem
                var menu = menuItems[1];
                menu.Invoke();
            }
            catch (Exception)
            {
            }
        }
        public void CreateConversation()
        {
            open();
            var popup = _parent.Descendant(typeof(Popup));
            var menuItems = popup.Descendants(typeof(Divelements.SandRibbon.MenuItem));
            try
            {
                // invoke create new conversation
                var menu = menuItems[0];
                menu.Invoke();

                WaitUntilConversationJoined(_parent);
            }
            catch (Exception) 
            {
            }
        }
        public void SearchConversation()
        {
            open();
            var popup = _parent.Descendant(typeof(Popup));
            var menuItems = popup.Descendants(typeof(Divelements.SandRibbon.MenuItem));
            try
            {
                // invoke search conversation
                var menu = menuItems[2];
                menu.Invoke();
            }
            catch (Exception) 
            {
            }
        }
        public void SearchMyConversation()
        {
            open();
            var popup = _parent.Descendant(typeof(Popup));
            var menuItems = popup.Descendants(typeof(Divelements.SandRibbon.MenuItem));
            try
            {
                // invoke search conversation
                var menu = menuItems[3];
                menu.Invoke();
            }
            catch (Exception) 
            {
            }
        }
        public AutomationElementCollection RecentConversations()
        {
            open();
            var popup = _parent.Descendant(typeof(Popup));
            return popup.Descendants(typeof(ListBoxItem));
        }
        public void Quit()
        {
            open();
            var quitButton = _parent.Descendant("PART_ExitButton");

            Assert.IsNotNull(quitButton, "MeTL main menu 'Quit' button was not found.");
            quitButton.Invoke();
        }
        public void LogoutAndQuit()
        {
            open();
            var popup = _parent.Descendant(typeof(Popup));
            var menuItems = popup.Descendants(typeof(Divelements.SandRibbon.MenuItem));

            AutomationElement logoutAndQuit = null;
            
            if (menuItems.Count >= 7)
                logoutAndQuit = menuItems[6];

            Assert.IsNotNull(logoutAndQuit, "MeTL main menu 'Logout And Quit' button was not found.");
            logoutAndQuit.Invoke();
        }
    }
    public class ConversationPicker
    {
        private AutomationElement _title;
        private AutomationElement _restriction;
        private AutomationElement _creationType;
        private AutomationElement _file;
        private AutomationElement _importType;
        private AutomationElement _create;

        public ConversationPicker(AutomationElement parent)
        {
            _title = parent.Descendant("conversationNameTextBox");
            _restriction = parent.Descendant("conversationSubjectListBox");
            _creationType = parent.Descendant("startingContentSelector");
            _file = parent.Descendant("importFileTextBox");
            _importType = parent.Descendant("importSelector");
            _create = parent.Descendant("CommitButton");
        }
        public ConversationPicker title (string value)
        {
            _title.Value("");
            _title.SetFocus();
            _title.Value(value);
            return this;
        }
        public ConversationPicker file(string value)
        {
            _file.Value("");
            _file.SetFocus();
            _file.Value(value);
            return this;
        }
        public ConversationPicker createType(int position)
        {
            ((SelectionItemPattern)_creationType.Children(typeof(ListBoxItem))[position]
                .GetCurrentPattern(SelectionItemPattern.Pattern)).Select();
            return this;
        }
        public ConversationPicker powerpointType(int position)
        {
            ((SelectionItemPattern)_importType.Children(typeof(ListBoxItem))[position]
                .GetCurrentPattern(SelectionItemPattern.Pattern)).Select();
            return this;
        }
        public void enter(string title)
        {
        }
        public void update()
        {
            _title.SetFocus();
            ((SelectionItemPattern)_restriction.Children(typeof(ListBoxItem))[0]
                .GetCurrentPattern(SelectionItemPattern.Pattern)).Select();
            _title.SetFocus();
            _create.Invoke();
        }
        public void create()
        {
            _title.SetFocus();
            _file.SetFocus();
            _title.SetFocus();
            _create.Invoke();
        }
    }
    public class UserCanvasStack
    {
        private AutomationElement _stack;
        private AutomationElement _handwriting;
        private AutomationElement _text;
        private AutomationElement _images;
        public string Ink
        {
            get 
            {
                return _handwriting.Value();
            }
            set
            {
                _handwriting.Value(value);
            }
        }
        public string Text
        {
            get
            {
                return _text.Value();
            }
            set
            {
                _text.Value(value);
            }
        }
        public string Images
        {
            get
            {
                return _images.Value();
            }
            set
            {
                _images.Value(value);
            }
        }
      
        public UserCanvasStack(AutomationElement parent, string target)
        {
            _stack = parent.Descendant(target);
            _handwriting = _stack.Descendant("handwriting");
            _text = _stack.Descendant("text");
            _images = _stack.Descendant("images");
        }
    }

    public class CollapsedCanvasStack : IScreenObject
    {
        private AutomationElement canvas;

        public CollapsedCanvasStack(AutomationElement parent)
        {
            PropertyChanged += (sender, args) => { Populate(); };
            Parent = new UITestHelper(UITestHelper.RootElement, parent);
        }

        public CollapsedCanvasStack()
        {
            PropertyChanged += (sender, args) => { Populate(); };
        }

        private void Populate()
        {
            canvas = Parent.AutomationElement.Descendant(typeof(SandRibbon.Components.CollapsedCanvasStack));
        }

        public Rect BoundingRectangle
        {
            get 
            {
                Rect boundingRect = Rect.Empty;
                try
                {
                    boundingRect = canvas.Current.BoundingRectangle;
                }
                catch (ElementNotAvailableException)
                {
                }

                return boundingRect;
            }
        }

        public AutomationElementCollection ChildTextboxes
        {
            get
            {
                return canvas.Descendants(typeof(SandRibbon.Components.Utility.MeTLTextBox));
            }
        }

        public int NumberOfInkStrokes()
        {
            var valuePattern = canvas.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;

            return Convert.ToInt32(valuePattern.Current.Value);
        }

        public System.Drawing.Point RandomPointWithinMargin(int marginWidth, int marginHeight)
        {
            var bounds = BoundingRectangle;
            bounds.Inflate(-40, -40);

            var random = new Random();

            var randX = (int)(bounds.X + random.NextDouble() * bounds.Width);
            var randY = (int)(bounds.Y + random.NextDouble() * bounds.Height);

            return new System.Drawing.Point(randX, randY);
        }

        public void InsertTextbox(System.Drawing.Point location, string text)
        {
            Mouse.MoveTo(location);
            Mouse.Click(MouseButton.Left);

            UITestHelper.Wait(TimeSpan.FromMilliseconds(100));

            var textbox = AutomationElement.FocusedElement;
            textbox.Current.ClassName.ShouldEqual("MeTLTextBox");

            textbox.Value(text);

            UITestHelper.Wait(TimeSpan.FromMilliseconds(100));
        }
    }

    public class Quiz
    {
        private AutomationElement _parent;
        public Quiz(AutomationElement parent)
        {
            _parent = parent;
        }
        public Quiz openTab()
        {
            Keys.SendWait("%");
            Thread.Sleep(100);
            Keys.SendWait("Q");
            return this;
        }
        public void open()
        {
            var buttons = _parent.Descendants(typeof(Button));
            foreach (AutomationElement button in buttons)
            {
                if (button.Current.AutomationId.ToLower().Contains("createquiz"))
                {
                    button.Invoke();
                    return;
                }
            }
        }

        public void openQuiz()
        {
            var allButtons = _parent.Descendants(typeof(Button));
            foreach(AutomationElement button in allButtons)
            {
                // this will only find the first quiz
                if (button.Current.AutomationId.ToLower().Equals("quiz"))
                {
                    button.Invoke();
                    return;
                }
            }
        }
    }
    public class QuizAnswer
    {
        private AutomationElement _buttons;
        public QuizAnswer(AutomationElement parent)
        {
             var _dialog = AutomationElement.RootElement
                                        .FindFirst(TreeScope.Children, 
                                                    new PropertyCondition(AutomationElement.AutomationIdProperty, 
                                                    "answerAQuiz"));
            _buttons = _dialog.Descendant("quizOptions"); 
        }
        public void answer()
        {
            ((SelectionItemPattern)_buttons.Children(typeof(ListBoxItem))[0]
                .GetCurrentPattern(SelectionItemPattern.Pattern)).Select();
        }
    }
    public class QuizCreate
    {
        private AutomationElement _create;
        private AutomationElement _title;
        private AutomationElement _question;
        private AutomationElement _parent;
        private AutomationElement _dialog;
        private AutomationElementCollection _options;
        public QuizCreate(AutomationElement parent)
        {
            _parent = parent;
            _dialog = _parent.Descendant("createAQuiz");
            _create = _dialog.Descendant("quizCommitButton");
            _question = _dialog.Descendant("question");
            _title = _dialog.Descendant("quizTitle");
            _options = _dialog.Descendants(typeof(TextBox));
        }
        public QuizCreate question(string value)
        {
            _question.Value(value);
            return this; 
        }
        public QuizCreate options()
        {
            var count = 0;
            foreach (AutomationElement element in _options)
            {
                if (element.Current.AutomationId.ToLower().Equals("quizanswer"))
                {
                    element.Value("Answer " + count.ToString());
                    count++;
                }
            }
            return this;
        }
        public void create()
        {
            _create.Invoke();
        }
    }
    public class SubmissionViewer
    {
        private AutomationElement _submissionList;
        private AutomationElement _import;
        public SubmissionViewer(AutomationElement parent)
        {
            var window = parent.Descendant("viewSubmissions");
            _submissionList = window.Descendant("submissions");
            _import = window.Descendant("importSelectedSubmission");
        }
        public void import()
        {
            ((SelectionItemPattern)_submissionList.Children(typeof(ListBoxItem))[0]
                .GetCurrentPattern(SelectionItemPattern.Pattern)).Select();
            _import.Invoke();
        }
    }

    public class Submission
    {
        private AutomationElement _parent;

        public Submission(AutomationElement parent)
        {
            _parent = parent;
        }
        public Submission openTab()
        {
            Keys.SendWait("%");
            Thread.Sleep(100);
            Keys.SendWait("S");
            return this;
        }
        public void open()
        {
        }
        public void view()
        {
            var buttons = _parent.Descendants(typeof(Button));
            foreach (AutomationElement button in buttons)
            {
                if (button.Current.AutomationId.Contains("viewSubmission"))
                {
                    button.Invoke();
                    return;
                }
            }
        }
        public void submit()
        {
            var buttons = _parent.Descendants(typeof(Button));
            foreach (AutomationElement button in buttons)
            {
                if (button.Current.AutomationId.Contains("submitSubmission"))
                {
                    button.Invoke();
                    break;
                }
            }

            Thread.Sleep(200);

            buttons = _parent.Descendants(typeof(Button));
            foreach (AutomationElement button in buttons)
            {
                if (button.Current.Name.Equals("OK"))
                {
                    button.Invoke();
                    break;
                }
            }
        }
    }
    
    public class ConversationSearcher : ConversationAction
    {
        private AutomationElement _searchField;
        private AutomationElement _searchButton;
        private AutomationElement _searchResults;
        private AutomationElement _parent;

        public ConversationSearcher(AutomationElement parent)
        {
            _parent = parent;
            _searchField = parent.Descendant("SearchInput");
            _searchButton = parent.Descendant("searchConversations");
        }
        public ConversationSearcher searchField(string value)
        {
            var searchField = new UITestHelper(_searchField);
            searchField.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, "SearchInput"));
            searchField.WaitForControlEnabled();

            _searchField.Value("");
            _searchField.SetFocus();
            _searchField.Value(value);

            return this;
        }
        public ConversationSearcher Search()
        {
            _searchButton.Invoke();
            return this;
        }
        public ConversationSearcher JoinFirstFound()
        {
            _searchResults = _parent.Descendant("SearchResults");
            var buttons = _searchResults.Descendants(typeof(Button));

            if (buttons.Count <= 1)
                Assert.Fail(ErrorMessages.UNABLE_TO_FIND_CONVERSATION);

            buttons[1].Invoke();

            WaitUntilConversationJoined(_parent);
            
            return this;
        }

        public ConversationSearcher SelectConversation(string query)
        {
            _searchResults = _parent.Descendant("SearchResults");

            var searchResults = new UITestHelper(_parent, _searchResults);
            searchResults.SearchProperties.Add(new PropertyExpression(AutomationElement.NameProperty, "SearchResults"));
            var resultsReturned = searchResults.WaitForControlCondition((uiControl) => { return Rect.Empty.Equals(uiControl.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty)); });

            if (resultsReturned == false)
                Assert.Fail(ErrorMessages.UNABLE_TO_FIND_CONVERSATION);

            var success = false;
            var buttons = _searchResults.Descendants(typeof(Button));
            foreach (AutomationElement button in buttons)
            {
                if (button.WalkAllElements(query) != null && (bool)button.GetCurrentPropertyValue(AutomationElement.IsKeyboardFocusableProperty) == true)
                {
                    success = true;                    
                    button.SetFocus();
                    break;
                }
            }

            Assert.IsTrue(success, ErrorMessages.UNABLE_TO_FIND_CONVERSATION);

            return this;
        }

        /*public ConversationSearcher SelectConversation(string query)
        {
            _searchResults = _parent.Descendant("SearchResults");

            var currentConversation = new UITestHelper(_searchResults);
            currentConversation.SearchProperties.Add(new PropertyExpression(AutomationElement.IsOffscreenProperty, false));
            currentConversation.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, "currentConversationButton"));

            if (currentConversation.WaitForControlEnabled() == false)
                Assert.Fail(ErrorMessages.UNABLE_TO_FIND_CONVERSATION);

            var success = false;
            currentConversation.WaitForControlCondition((uiControl) => { return TreeWalker.RawViewWalker.GetNextSibling(uiControl) == null; });
            var conversationParent = TreeWalker.RawViewWalker.GetNextSibling(currentConversation.AutomationElement);
            // make sure we've got the right one
            if (conversationParent != null && conversationParent.WalkAllElements(query) != null)
            {
                success = true;
                conversationParent.SetFocus();
            }

            Assert.IsTrue(success, ErrorMessages.UNABLE_TO_FIND_CONVERSATION);

            return this;
        }*/

        public bool ResultsContainQueried(string query)
        {
            _searchResults = _parent.Descendant("SearchResults");
            var buttons = _searchResults.Descendants(typeof(Button));

            if (buttons.Count <= 1)
                Assert.Fail(ErrorMessages.UNABLE_TO_FIND_CONVERSATION);

            var success = false;
            foreach (AutomationElement button in buttons)
            {
                var conversation = button.WalkAllElements(query);
                if (conversation != null)
                {
                    success = true;
                    break;
                }
            }

            return success;
        }

        public ConversationSearcher JoinQueried(string query)
        {
            _searchResults = _parent.Descendant("SearchResults");
            var buttons = _searchResults.Descendants(typeof(Button));

            if (buttons.Count <= 1)
                Assert.Fail(ErrorMessages.UNABLE_TO_FIND_CONVERSATION);

            var success = false;
            foreach (AutomationElement button in buttons)
            {
                var conversation = button.WalkAllElements(query);
                if (conversation != null)
                {
                    success = true;
                    button.Invoke();
                    break;
                }
            }

            Assert.IsTrue(success, ErrorMessages.UNABLE_TO_FIND_CONVERSATION);

            WaitUntilConversationJoined(_parent);
            
            return this;
        }

        public ConversationSearcher GetResults()
        {
            _searchResults = _parent.Descendant("SearchResults");
            Assert.AreNotEqual(Rect.Empty, _searchResults.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty));
            return this;
        }

        public bool IsEmptyResult()
        {
            _searchResults = _parent.Descendant("SearchResults");
            var buttons = _searchResults.Descendants(typeof(Button));

            return (buttons.Count <= 1);
        }
        
        public int GetResultsCount()
        {
            var resultsParent = _parent.Descendant("SearchResults");
            return resultsParent != null ? resultsParent.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "conversationButton")).Count : 0;
        }
    }
    public class SlideNavigation 
    {
        private AutomationElement _forward;
        private AutomationElement _back;
        private AutomationElement _add;
        private AutomationElement _sync;

        public SlideNavigation(AutomationElement parent)
        {
            _forward = parent.Descendant("moveToNext");
            _back = parent.Descendant("moveToPrevious");
            _add = parent.Descendant("addSlideButton");
            _sync = parent.Descendant("syncButton");
        }
        public void Add()
        {
            _add.Invoke();
        }
        public void Back()
        {
            _back.Invoke();
        }
        public void Forward()
        {
            _forward.Invoke();
        }
        public void Sync()
        {
            _sync.Invoke();
        }
    }

    public class Login : IScreenObject
    {
        private AutomationElement _login;
        private AutomationElement _username;
        private AutomationElement _password;
        private AutomationElement _submit;
        private AutomationElement _remember;

        public Login(AutomationElement parent)
        {
            PropertyChanged += (sender, args) => { Populate(); };
            Parent = new UITestHelper(UITestHelper.RootElement, parent);
        }
        
        public Login()
        {
            PropertyChanged += (sender, args) => { Populate(); };
        }

        private void Populate()
        {
            _login = Parent.Descendant("login");
            _username = _login.Descendant("username");
            _password = _login.Descendant("password");
            _submit = _login.Descendant("submit");
            _remember = _login.Descendant("rememberMe");
        }

        public Login username(string value)
        {
            _username.Value("");
            _username.SetFocus();
            _username.Value(value);
            return this;
        }
        public Login password(string value)
        {
            _password.Value("");
            _password.SetFocus();
            _password.Value(value);

            Keys.SendWait("{TAB}");
            return this;
        }
        public Login remember()
        {
            _remember.Toggle();
            return this;
        }

        public Login SubmitAndWaitForError()
        {
            var submitButton = new UITestHelper(_login);

            submitButton.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, "submit"));
            submitButton.WaitForControlEnabled();

            submitButton.AutomationElement.Invoke();

            WaitForLoginError();

            return this;
        }

        public Login submit()
        {
            var submitButton = new UITestHelper(_login);

            submitButton.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, "submit"));
            submitButton.WaitForControlEnabled();

            submitButton.AutomationElement.Invoke();

            // return when the search box is visible
            WaitForSearchScreen();

            return this;
        }
        
        public Login WaitForLoginError()
        {
            var control = new UITestHelper(Parent);
            control.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_LOGIN_ERROR_LABEL));

            var success = control.WaitForControlVisible();
            Assert.IsTrue(success, ErrorMessages.WAIT_FOR_CONTROL_FAILED);

            return this;
        }

        public Login WaitForSearchScreen()
        {
            var control = new UITestHelper(Parent);
            control.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_CONVERSATION_SEARCH_TEXTBOX));
            
            var success = control.WaitForControlEnabled();
            Assert.IsTrue(success, ErrorMessages.WAIT_FOR_CONTROL_FAILED);

            return this;
        }
    }

    public class ZoomButtons
    {
        private AutomationElement _parent;
        private AutomationElement _zoomIn;
        private AutomationElement _zoomOut;
        private AutomationElement _initiateGrab;

        public ZoomButtons(AutomationElement parent)
        {
            _parent = parent;

            _zoomIn = _parent.Descendant("ZoomIn");
            _zoomOut = _parent.Descendant("ZoomOut");
            _initiateGrab = _parent.Descendant("InitiateGrabZoom");
        }

        public ZoomButtons ZoomIn()
        {
            _zoomIn.Invoke();
            return this;
        }

        public ZoomButtons ZoomOut()
        {
            _zoomOut.Invoke();
            return this;
        }
    }

    public class ConversationEditScreen : ConversationAction
    {
        private AutomationElement _parent;
        private AutomationElement _rename;
        private AutomationElement _share;
        private AutomationElement _delete;
        private AutomationElement _save;
        private AutomationElement _groups;
        private AutomationElement _returnToCurrent;

        public ConversationEditScreen(AutomationElement parent)
        {
            _parent = parent;
            _rename = _parent.Descendant(Constants.ID_METL_CONVERSATION_RENAME_BUTTON);
            _share = _parent.Descendant(Constants.ID_METL_CONVERSATION_SHARE_BUTTON);
            _delete = _parent.Descendant(Constants.ID_METL_CONVERSATION_DELETE_BUTTON);

            _returnToCurrent = _parent.Descendant(Constants.ID_METL_CURRENT_CONVERSATION_BUTTON);
        }

        public ConversationEditScreen Rename(string conversationTitle)
        {
            _rename.Invoke();

            var title = _parent.Descendant("renameTitle");
            title.Value(conversationTitle);

            return this;
        }

        public ConversationEditScreen Delete()
        {
            _delete.Invoke();

            var yesButton = new UITestHelper(_parent);
            yesButton.SearchProperties.Add(new PropertyExpression(AutomationElement.NameProperty, "Yes"));
            var success = yesButton.WaitForControlExist();
            Assert.IsTrue(success, ErrorMessages.WAIT_FOR_CONTROL_FAILED);

            yesButton.AutomationElement.Invoke();

            return this;
        }

        public ConversationEditScreen ChangeGroup(string groupName)
        {
            var shareButton = new UITestHelper(_share);
            shareButton.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_CONVERSATION_SHARE_BUTTON));

            var success = shareButton.WaitForControlEnabled();
            Assert.IsTrue(success, ErrorMessages.WAIT_FOR_CONTROL_FAILED);
            shareButton.AutomationElement.Invoke();

            var authList = new UITestHelper(_parent);
            authList.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, "groupsList"));

            success = authList.WaitForControlVisible();
            Assert.IsTrue(success, ErrorMessages.WAIT_FOR_CONTROL_FAILED);

            _groups = authList.AutomationElement; //_parent.Descendant("groupsList");
            _groups.SelectListItem(groupName);

            return this;
        }
        public ConversationEditScreen IsGroupSelected(string groupName, out bool isSelected)
        {
            var shareButton = new UITestHelper(_share);
            shareButton.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_CONVERSATION_SHARE_BUTTON));

            var success = shareButton.WaitForControlEnabled();
            Assert.IsTrue(success, ErrorMessages.WAIT_FOR_CONTROL_FAILED);
            shareButton.AutomationElement.Invoke();

            var authList = new UITestHelper(_parent);
            authList.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, "groupsList"));

            success = authList.WaitForControlVisible();
            Assert.IsTrue(success, ErrorMessages.WAIT_FOR_CONTROL_FAILED);

            _groups = authList.AutomationElement; //_parent.Descendant("groupsList");

            _groups = _parent.Descendant("groupsList");
            isSelected = _groups.IsListItemSelected(groupName);

            return this;
        }
        public ConversationEditScreen Save()
        {
            _save = _parent.Descendant("saveEdit");
            _save.Invoke();
            return this;
        }

        public ConversationEditScreen ReturnToCurrent()
        {
            var returnButton = new UITestHelper(_parent);
            returnButton.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_CURRENT_CONVERSATION_BUTTON));

            returnButton.WaitForControlEnabled();
            returnButton.AutomationElement.Invoke();

            WaitUntilConversationJoined(MeTL.GetMainWindow().AutomationElement);

            return this;
        }
    }
    public class HomeTabScreen : IScreenObject
    {
        private AutomationElement _inkButton;
        private AutomationElement _textButton;
        private AutomationElement _imageButton;
        private AutomationElement _extendButton;
        private AutomationElement _tab;

        public HomeTabScreen()
        {
            PropertyChanged += (sender, args) => { Populate(); };
        }

        public HomeTabScreen(AutomationElement parent)
        {
            PropertyChanged += (sender, args) => { Populate(); };
            Parent = new UITestHelper(UITestHelper.RootElement, parent);
        }

        public HomeTabScreen OpenTab()
        {
            Parent.SetFocus();

            Keys.SendWait("%");
            Thread.Sleep(100);
            Keys.SendWait("H");

            Populate();

            return this;
        }

        #region Properties

        public bool IsActive
        {
            get
            {
                return _tab != null ? !(bool)_tab.GetCurrentPropertyValue(AutomationElement.IsOffscreenProperty) : false;
            }
        }

        #endregion


        public HomeTabScreen ActivatePenMode()
        {
            var inkButton = new UITestHelper(Parent, _inkButton);
            inkButton.AutomationElement.Select();

            Mouse.MoveTo(inkButton.AutomationElement.GetClickablePoint().ToDrawingPoint());
            Mouse.Click(MouseButton.Left);

            inkButton.WaitForControlCondition((uiControl) => 
            { 
                var selection = uiControl.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
                return (selection != null && selection.Current.IsSelected == false);
            });

            return this;
        }

        public HomeTabScreen ActivateTextMode()
        {
            var textButton = new UITestHelper(Parent, _textButton);
            textButton.AutomationElement.Select();

            Mouse.MoveTo(textButton.AutomationElement.GetClickablePoint().ToDrawingPoint());
            Mouse.Click(MouseButton.Left);

            textButton.WaitForControlCondition((uiControl) => 
            { 
                var selection = uiControl.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
                return (selection != null && selection.Current.IsSelected == false);
            });

            return this;
        }

        public HomeTabScreen TextInsertMode()
        {
            var textInput = new UITestHelper(Parent);
            textInput.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, "type"));
            textInput.WaitForControlVisible();

            textInput.AutomationElement.Select();

            var clickPoint = textInput.AutomationElement.GetClickablePoint();
            Mouse.MoveTo(new System.Drawing.Point((int)clickPoint.X, (int)clickPoint.Y));
            Mouse.Click(MouseButton.Left);

            textInput.WaitForControlCondition((uiControl) => 
            { 
                var selection = uiControl.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
                return (selection != null && selection.Current.IsSelected == false);
            });

            return this;    
        }

        public HomeTabScreen TextSelectMode()
        {
            var textInput = new UITestHelper(Parent);
            textInput.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, "select"));
            textInput.WaitForControlVisible();

            textInput.AutomationElement.Select();

            var clickPoint = textInput.AutomationElement.GetClickablePoint();
            Mouse.MoveTo(new System.Drawing.Point((int)clickPoint.X, (int)clickPoint.Y));
            Mouse.Click(MouseButton.Left);

            textInput.WaitForControlCondition((uiControl) => 
            { 
                var selection = uiControl.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
                return (selection != null && selection.Current.IsSelected == false);
            });

            return this;
        }

        public HomeTabScreen PenSelectMode()
        {
            var penInput = new UITestHelper(Parent);
            penInput.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, "selectRadio"));
            penInput.WaitForControlVisible();

            penInput.AutomationElement.Select();

            var clickPoint = penInput.AutomationElement.GetClickablePoint();
            Mouse.MoveTo(new System.Drawing.Point((int)clickPoint.X, (int)clickPoint.Y));
            Mouse.Click(MouseButton.Left);

            penInput.WaitForControlCondition((uiControl) => 
            { 
                var selection = uiControl.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
                return (selection != null && selection.Current.IsSelected == false);
            });

            return this;
        }
        public HomeTabScreen ActivateImageMode()
        {
            _imageButton.Select();
            return this;
        }

        public HomeTabScreen ExtendPage()
        {
            //_extendButton.Invoke();

            return this;
        }

        public void Populate()
        {
            _inkButton = Parent.Descendant("Pen");
            _textButton = Parent.Descendant("Text");
            _imageButton = Parent.Descendant("Image");
            //_extendButton = Parent.Descendant("ExtendPage");
            _tab = Parent.AutomationElement.WalkAllElements("HomeTab");
        }
    }
}
