using Microsoft.VisualStudio.TestTools.UnitTesting;
using UITestFramework;
using System.Windows.Automation;
using System.Windows;
using System.Threading;
using System;
using Microsoft.Test.Input;

namespace Functional
{
    [TestClass]
    public class SearchConversationTests
    {
        private UITestHelper metlWindow;
        
        [TestInitialize]
        public void Setup()
        {
            metlWindow = MeTL.GetMainWindow();
        }
        
        [TestMethod]
        public void SearchForOwnedAndJoin()
        {
            var conversation = new FunctionalTests.Actions.SearchConversation();
            conversation.SearchForConversationAndJoin(metlWindow, TestConstants.OWNER_CONVERSATION_TITLE);
        }

        [TestMethod]
        public void SearchForOwnedAndRename()
        {
            var search = new ConversationSearcher(metlWindow.AutomationElement);

            search.searchField(TestConstants.OWNER_CONVERSATION_TITLE);
            search.Search();

            var results = new UITestHelper(metlWindow);
            results.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_CONVERSATION_SEARCH_RESULTS));

            results.WaitForControlCondition((uiControl) => { return Rect.Empty.Equals(uiControl.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty)); });

            if (!search.ResultsContainQueried(TestConstants.OWNER_CONVERSATION_TITLE))
            {
                CreateAndRenameConversation();
            }

            search.SelectConversation(TestConstants.OWNER_CONVERSATION_TITLE);
            var guid = Guid.NewGuid();

            var edit = new ConversationEditScreen(metlWindow.AutomationElement);
            edit.Rename(TestConstants.OWNER_CONVERSATION_TITLE + "renamed" + guid).Save();

            search.SelectConversation(TestConstants.OWNER_CONVERSATION_TITLE + "renamed" + guid);
        }

        [TestMethod]
        public void CreateAndRenameConversation()
        {
            var conversation = new FunctionalTests.Actions.SearchConversation();
            conversation.CreateAndRenameConversation(metlWindow, TestConstants.OWNER_CONVERSATION_TITLE);
        }

        [TestMethod]
        public void SearchForDeletedConversation()
        {
            var search = new ConversationSearcher(metlWindow.AutomationElement);

            search.searchField(TestConstants.DELETED_CONVERSATION_TITLE);
            search.Search();

            var results = new UITestHelper(metlWindow);
            results.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_CONVERSATION_SEARCH_RESULTS));

            results.WaitForControlCondition((uiControl) => { return Rect.Empty.Equals(uiControl.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty)); });

            var success = search.IsEmptyResult();
            Assert.IsTrue(success, ErrorMessages.EXPECTED_NO_RESULTS);
        }

        [TestMethod]
        public void SearchForHavePermissionAndJoin()
        {
            var search = new ConversationSearcher(metlWindow.AutomationElement);

            search.searchField(TestConstants.NONOWNER_CONVERSATION_TITLE);
            search.Search();

            var results = new UITestHelper(metlWindow);
            results.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_CONVERSATION_SEARCH_RESULTS));

            results.WaitForControlCondition((uiControl) => { return Rect.Empty.Equals(uiControl.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty)); });

            search.JoinQueried(TestConstants.NONOWNER_CONVERSATION_TITLE);
        }

        [TestMethod]
        public void HighlightConversationCurrentlyJoined()
        {
            var search = new ConversationSearcher(metlWindow.AutomationElement);
            search.SelectConversation(TestConstants.OWNER_CONVERSATION_TITLE);
        }

        [TestMethod]
        public void SearchForConversation()
        {
            var search = new ConversationSearcher(metlWindow.AutomationElement);

            search.searchField(TestConstants.OWNER_CONVERSATION_TITLE);
            search.Search();

            var results = new UITestHelper(metlWindow);
            results.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_CONVERSATION_SEARCH_RESULTS));

            results.WaitForControlCondition((uiControl) => { return Rect.Empty.Equals(uiControl.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty)); });
        }

        [TestMethod]
        public void SwitchToSearchMyConversations()
        {
            new ApplicationPopup(metlWindow.AutomationElement).SearchMyConversation();

            var filter = new UITestHelper(metlWindow);
            filter.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_SEARCH_ALL_CONVERSATIONS_BUTTON));

            var success = filter.WaitForControlVisible();
            Assert.IsTrue(success, ErrorMessages.WAIT_FOR_CONTROL_FAILED);

            var filterButton = filter.AutomationElement;
            Assert.AreEqual("Filter my Conversations", filterButton.Current.Name, ErrorMessages.EXPECTED_CONTENT);

            var results = new UITestHelper(metlWindow);
            results.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, Constants.ID_METL_CONVERSATION_SEARCH_RESULTS));

            results.WaitForControlCondition((uiControl) => { return Rect.Empty.Equals(uiControl.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty)); });
        }

        [TestMethod]
        public void SwitchToSearchCurrentConversation()
        {
            var conversation = new FunctionalTests.Actions.SearchConversation();
            conversation.SwitchToSearchCurrentConversation(metlWindow);
        }
        
    }
}
