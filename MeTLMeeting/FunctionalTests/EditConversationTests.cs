using Microsoft.VisualStudio.TestTools.UnitTesting;
using UITestFramework;
using System.Windows.Automation;
using System.Windows;

namespace Functional
{
    [TestClass]
    public class EditConversationTests
    {
        private UITestHelper metlWindow;
        
        [TestInitialize]
        public void Setup()
        {
            metlWindow = MeTL.GetMainWindow();
        }


        [TestMethod]
        public void ChangePrivacyToOnlyOwner()
        {
            var results = new UITestHelper(metlWindow);
            results.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_CONVERSATION_SEARCH_RESULTS));

            results.WaitForControlCondition((uiControl) => { return Rect.Empty.Equals(uiControl.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty)); });

            var editConversation = new ConversationEditScreen(results.AutomationElement);

            editConversation.ChangeGroup(TestConstants.OWNER_USERNAME).Save().ReturnToCurrent();
        }

        [TestMethod]
        public void PrivacyIsSetToOwner()
        {
            var results = new UITestHelper(metlWindow);
            results.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_CONVERSATION_SEARCH_RESULTS));

            results.WaitForControlCondition((uiControl) => { return Rect.Empty.Equals(uiControl.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty)); });

            var editConversation = new ConversationEditScreen(results.AutomationElement);

            bool selected = false;
                
            editConversation.IsGroupSelected(TestConstants.OWNER_USERNAME, out selected).Save().ReturnToCurrent();
            Assert.IsTrue(selected, ErrorMessages.EXPECTED_SET_PRIVACY);
        }

        [TestMethod]
        public void PrivacyIsSetToUnrestricted()
        {
            var results = new UITestHelper(metlWindow);
            results.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_CONVERSATION_SEARCH_RESULTS));

            results.WaitForControlCondition((uiControl) => { return Rect.Empty.Equals(uiControl.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty)); });

            var editConversation = new ConversationEditScreen(results.AutomationElement);

            bool selected = false;
                
            editConversation.IsGroupSelected(TestConstants.AUTH_GROUP_UNRESTRICTED, out selected).Save().ReturnToCurrent();
            Assert.IsTrue(selected, ErrorMessages.EXPECTED_SET_PRIVACY);
        }

        [TestMethod]
        public void ChangePrivacyToUnrestricted()
        {
            var results = new UITestHelper(metlWindow);
            results.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_CONVERSATION_SEARCH_RESULTS));

            results.WaitForControlCondition((uiControl) => { return Rect.Empty.Equals(uiControl.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty)); });

            var editConversation = new ConversationEditScreen(results.AutomationElement);

            editConversation.ChangeGroup(TestConstants.AUTH_GROUP_UNRESTRICTED).Save().ReturnToCurrent();
        }

        [TestMethod]
        public void DeleteCurrentConversation()
        {
            var results = new UITestHelper(metlWindow);
            results.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_CONVERSATION_SEARCH_RESULTS));

            results.WaitForControlCondition((uiControl) => { return Rect.Empty.Equals(uiControl.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty)); });
            
            var editConversation = new ConversationEditScreen(results.AutomationElement);

            editConversation.Delete();
        }

        [TestMethod]
        public void AddPageToConversation()
        {
            new SlideNavigation(metlWindow.AutomationElement).Add();
            // TODO: Check that there are now two pages in the conversation
        }
    }
}
