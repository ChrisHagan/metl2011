using Microsoft.VisualStudio.TestTools.UnitTesting;
using UITestFramework;
using System.Windows.Automation;
using System.Windows;
using SandRibbon.Components;

namespace Functional
{
    [TestClass]
    public class EditConversationTests
    {
        private UITestHelper metlWindow;
        private HomeTabScreen homeTab;
        private CollapsedCanvasStack canvas;

        [TestInitialize]
        public void Setup()
        {
            metlWindow = MeTL.GetMainWindow();
            canvas = new CollapsedCanvasStack(metlWindow.AutomationElement);
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
            results.OverrideTimeout = 5000;

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
            results.OverrideTimeout = 5000;

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
            results.OverrideTimeout = 5000;

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
            var slideDisplay = new UITestHelper(metlWindow, metlWindow.AutomationElement.Descendant(typeof(SlideDisplay)));
            var rangeValue = slideDisplay.AutomationElement.GetCurrentPattern(RangeValuePattern.Pattern) as RangeValuePattern;
            var currentCount = rangeValue.Current.Maximum;

            var navi = new SlideNavigation(metlWindow.AutomationElement);

            navi.Add();

            slideDisplay.WaitForControlCondition((uiControl) =>
            {
                var range = uiControl.GetCurrentPattern(RangeValuePattern.Pattern) as RangeValuePattern;
                return range.Current.Maximum == currentCount;
            });
    
            Assert.AreEqual(currentCount + 1, rangeValue.Current.Maximum);
        }

        [TestMethod]
        public void ExtendCanvasViaButton()
        {
            var originalSize = canvas.BoundingRectangle;

            new HomeTabScreen(metlWindow.AutomationElement).OpenTab().ExtendPage();

            var waitCanvas = new UITestHelper(metlWindow);
            waitCanvas.SearchProperties.Add(new PropertyExpression(AutomationElement.ClassNameProperty, typeof(SandRibbon.Components.CollapsedCanvasStack).Name));

            var success = waitCanvas.WaitForControlCondition((uiControl) => { return (Rect)uiControl.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty) == originalSize; });
            
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void ExtendCanvasViaContent()
        {

        }
    }
}
