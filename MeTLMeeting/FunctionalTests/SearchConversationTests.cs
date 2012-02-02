using Microsoft.VisualStudio.TestTools.UnitTesting;
using UITestFramework;
using System.Windows.Automation;
using System.Windows;

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
            var search = new ConversationSearcher(metlWindow.AutomationElement);

            search.searchField("CITestSearchTestsOwner");
            search.Search();

            var results = new UITestHelper(metlWindow);
            results.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, "SearchResults"));

            results.WaitForControlCondition((uiControl) => { return Rect.Empty.Equals(uiControl.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty)); });

            search.JoinQueried("CITestSearchTestsOwner");
        }

        [TestMethod]
        public void SearchForDeletedConversation()
        {
            var search = new ConversationSearcher(metlWindow.AutomationElement);

            search.searchField("CITestSearchTestsDeleted");
            search.Search();

            var results = new UITestHelper(metlWindow);
            results.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, "SearchResults"));

            results.WaitForControlCondition((uiControl) => { return Rect.Empty.Equals(uiControl.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty)); });

            var success = search.IsEmptyResult();
            Assert.IsTrue(success, ErrorMessages.EXPECTED_NO_RESULTS);
        }

        [TestMethod]
        public void SearchForHavePermissionAndJoin()
        {
            var search = new ConversationSearcher(metlWindow.AutomationElement);

            search.searchField("CITestSearchTestsNonOwner");
            search.Search();

            var results = new UITestHelper(metlWindow);
            results.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, "SearchResults"));

            results.WaitForControlCondition((uiControl) => { return Rect.Empty.Equals(uiControl.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty)); });

            search.JoinQueried("CITestSearchTestsNonOwner");
        }

        [TestMethod]
        public void SearchForConversation()
        {
            var search = new ConversationSearcher(metlWindow.AutomationElement);

            search.searchField("TEST_FindAndJoinConversationOwned");
            search.Search();

            var results = new UITestHelper(metlWindow);
            results.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, "SearchResults"));

            results.WaitForControlCondition((uiControl) => { return Rect.Empty.Equals(uiControl.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty)); });
        }

        [TestMethod]
        public void SwitchToSearchMyConversations()
        {
            new ApplicationPopup(metlWindow.AutomationElement).SearchMyConversation();

            var filter = new UITestHelper(metlWindow);
            filter.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, "searchConversations"));

            var success = filter.WaitForControlVisible();
            Assert.IsTrue(success, ErrorMessages.WAIT_FOR_CONTROL_FAILED);

            var filterButton = filter.AutomationElement;
            Assert.AreEqual("Filter my Conversations", filterButton.Current.Name, ErrorMessages.EXPECTED_CONTENT);
        }
    }
}
