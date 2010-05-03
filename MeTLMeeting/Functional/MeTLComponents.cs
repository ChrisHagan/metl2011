using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Controls.Primitives;
using Keys = System.Windows.Forms.SendKeys;
using System.Windows.Automation;

namespace Functional
{
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
    public class ApplicationPopup
    {
        private AutomationElement _parent;
        public ApplicationPopup(AutomationElement parent)
        {
            _parent = parent;
        }
        private void open()
        {
            var buttons = _parent.Children(typeof(Button));
            var appButton = buttons[3];
            var rect = appButton.Current.BoundingRectangle;
            Assert.AreEqual(46,rect.Width);
            Assert.AreEqual(46,rect.Height);
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
    }
    public class ConversationPicker
    {
        private AutomationElementCollection _conversations;
        public ConversationPicker(AutomationElement parent)
        {
            var children = parent.Descendants().Aggregate("",(acc,item)=>acc+" "+item.Current.ClassName+":"+item.Current.AutomationId);
            _conversations = parent.Descendant("conversations").Descendants(typeof(Button));
        }
        public void enter(string title)
        {
            foreach (var conversation in _conversations)
            {
                var conversationTitle = ((AutomationElement)conversation).GetCurrentPropertyValue(AutomationElement.NameProperty);
                if (title.Equals(conversationTitle))
                {
                    ((AutomationElement)conversation).Invoke();
                    return;
                }
            }
            Assert.Fail(string.Format("Attempted to enter a nonexistent conversation called {0}", title)); 
        }
    }
    public class UserCanvasStack
    {
        private AutomationElement _stack;
        private AutomationElement _handwriting;
        private AutomationElement _text;
        private AutomationElement _images;
        private AutomationElement _privacyTools;
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
        public string Privacy
        {
            get
            {
                return _privacyTools.Value();
            }
            set
            {
                _privacyTools.Value(value);
            }
        }
        public UserCanvasStack(AutomationElement parent, string target)
        {
            _privacyTools = parent.Descendant("privacyTools");
            _stack = parent.Descendant(target);
            _handwriting = _stack.Descendant("handwriting");
            _text = _stack.Descendant("text");
            _images = _stack.Descendant("images");
        }
    }
    public class SyncButton
    {
        private AutomationElement _button;
        public SyncButton(AutomationElement parent)
        {
            _button = parent.Descendant("syncButton");
        }
        public void Toggle()
        {
            _button.Invoke();
        }
    }
    public class AddSlideButton
    {
        private AutomationElement _button;
        public AddSlideButton(AutomationElement parent)
        {
            _button = parent.Descendant("addSlideButton");
        }
        public void Add()
        {
            _button.Invoke();
        }
    }
    public class Login
    {
        private AutomationElement _login;
        private AutomationElement _username;
        private AutomationElement _password;
        private AutomationElement _submit;
        public Login(AutomationElement parent)
        {
            _login = parent.Descendant("login");
            _username = _login.Descendant("username");
            _password = _login.Descendant("password");
            _submit = _login.Descendant(typeof(Button));
        }
        private void moveWindowToTestingDatabase()
        {
            _login.Value(Constants.TEST_DB);
            Assert.AreEqual(_login.Value(), Constants.TEST_DB, "MeTL has not been pointed at the testing DB.  Testing cannot continue.");
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
            Keys.SendWait(value);
            return this;
        }
        public Login submit()
        {
            _submit.Invoke();
            return this;
        }
    }
}
