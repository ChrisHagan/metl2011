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

            search.searchField("TEST_FindAndJoinConversationOwned");
            search.Search();

            var results = new UITestHelper(metlWindow);
            results.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, "SearchResults"));

            results.WaitForControlCondition((uiControl) => { return Rect.Empty.Equals(uiControl.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty)); });

            search.JoinFirstFound();
        }

        [TestMethod]
        public void SearchForHavePermissionAndJoin()
        {
            var search = new ConversationSearcher(metlWindow.AutomationElement);

            search.searchField("TEST_FindAndJoinConversationNotOwned");
            search.Search();

            var results = new UITestHelper(metlWindow);
            results.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, "SearchResults"));

            results.WaitForControlCondition((uiControl) => { return Rect.Empty.Equals(uiControl.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty)); });

            search.JoinFirstFound();
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
        }
    }
}
