using Microsoft.VisualStudio.TestTools.UnitTesting;
using UITestFramework;
using System.Windows.Automation;
using System.Windows;
using SandRibbon.Components;
using FunctionalTests.DSL;
using FunctionalTests;
using System;

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
        public void ShowPageButtonExistsAndCanBeInvoked()
        {
            ScreenActionBuilder.Create().WithWindow(metlWindow.AutomationElement)
                .Ensure<HomeTabScreen>((home) =>
                {
                    if (!home.IsActive)
                        home.OpenTab();

                    home.ShowPage();
                    return true;
                });
        }

        [TestMethod]
        public void ShowAllPageButtonExistsAndCanBeInvoked()
        {
            ScreenActionBuilder.Create().WithWindow(metlWindow.AutomationElement)
                .Ensure<HomeTabScreen>((home) =>
                {
                    if (!home.IsActive)
                        home.OpenTab();

                    home.ShowAllPage();
                    return true;
                });
        }

        [TestMethod]
        public void ExtendPageButtonExistsAndCanBeInvoked()
        {
            ScreenActionBuilder.Create().WithWindow(metlWindow.AutomationElement)
                .Ensure<HomeTabScreen>((home) =>
                {
                    if (!home.IsActive)
                        home.OpenTab();

                    home.ExtendPage();
                    return true;
                });
        }

        [TestMethod]
        public void ExtendCanvasViaButton()
        {
            ScreenActionBuilder.Create().WithWindow(metlWindow.AutomationElement)
                .Ensure<HomeTabScreen>(home =>
                {
                    if (!home.IsActive) home.OpenTab();
                    home.ActivateTextMode().TextInsertMode().ExtendPage().ShowPage();

                    UITestHelper.Wait(TimeSpan.FromSeconds(1));

                    var scrollBarAdorner = metlWindow.AutomationElement.Descendant("adornerScroll");
                    var vScroll = scrollBarAdorner.Descendant("VScroll");
                    var hScroll = scrollBarAdorner.Descendant("HScroll");

                    var vPattern = vScroll.GetCurrentPattern(RangeValuePattern.Pattern) as RangeValuePattern;
                    vPattern.Current.IsReadOnly.ShouldBeFalse();

                    var hPattern = hScroll.GetCurrentPattern(RangeValuePattern.Pattern) as RangeValuePattern;
                    hPattern.Current.IsReadOnly.ShouldBeFalse();

                    return true; 
                });
        }

        [TestMethod]
        public void ExtendCanvasViaContent()
        {
            ScreenActionBuilder.Create().WithWindow(metlWindow.AutomationElement)
                .Ensure<HomeTabScreen>(home =>
                {
                    if (!home.IsActive) home.OpenTab();
                    home.ActivateTextMode().TextInsertMode();
                    return true;
                })
                .With<CollapsedCanvasStack>(canvas =>
                {
                    var slideNav = new SlideNavigation(metlWindow.AutomationElement);

                    var randomPage = slideNav.ChangeToRandomPage();
                    canvas.Refresh();
                    var originalSize = canvas.BoundingRectangle;

                    var inserted = canvas.InsertTextbox(new System.Drawing.Point((int)(originalSize.Width * 0.95), (int)(originalSize.Height / 2)), "Extend canvas size by content --------->");

                    canvas.Refresh();
                    Assert.IsFalse(originalSize.Equals(canvas.BoundingRectangle));

                    var currentPage = slideNav.CurrentPage;

                    randomPage = slideNav.ChangeToRandomPage(currentPage);
                    UITestHelper.Wait(TimeSpan.FromSeconds(3));
                    slideNav.WaitForPageChange(randomPage);
                    slideNav.ChangeToPage(currentPage);
                    UITestHelper.Wait(TimeSpan.FromSeconds(3));
                    slideNav.WaitForPageChange(currentPage);

                    new HomeTabScreen(metlWindow.AutomationElement).ActivateTextMode().TextSelectMode();
                    canvas.Refresh();
                    foreach (AutomationElement textbox in canvas.ChildTextboxes)
                    {
                        canvas.SelectTextboxWithClick(textbox);
                        UITestHelper.Wait(TimeSpan.FromMilliseconds(500));
                        canvas.DeleteSelectedContent();
                        UITestHelper.Wait(TimeSpan.FromMilliseconds(500));
                    }
                });
        }

        [TestMethod]
        public void InsertBoldText()
        {
            ScreenActionBuilder.Create().WithWindow(metlWindow.AutomationElement)
                .Ensure<HomeTabScreen>((home) =>
                {
                    if (!home.IsActive) home.OpenTab();
                    home.ActivateTextMode().TextInsertMode();
                    return true;
                })
                .With<CollapsedCanvasStack>((canvas) => 
                {
                    var insertedText = canvas.InsertTextbox(canvas.RandomPointWithinMargin(-40, -40), "Bolded text");

                    var home = new HomeTabScreen(metlWindow.AutomationElement).ActivateTextMode();
                    canvas.SelectTextboxWithClick(insertedText);
                    home.ToggleBoldText();
                    var slideNav = new SlideNavigation(metlWindow.AutomationElement).Add().Back();

                    home.IsBoldChecked.ShouldBeTrue();
                });
        }
    }
}
