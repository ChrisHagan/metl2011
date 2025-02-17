﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Automation;
using UITestFramework;
using Microsoft.Test.Input;
using System;
using System.Threading;

namespace Functional
{
    [TestClass]
    public class TextModeTests
    {
        private UITestHelper metlWindow;
        private HomeTabScreen homeTab;
        private CollapsedCanvasStack canvas;
        
        [TestInitialize]
        public void Setup()
        {
            metlWindow = MeTL.GetMainWindow();

            homeTab = new HomeTabScreen(metlWindow.AutomationElement).OpenTab();
            canvas = new CollapsedCanvasStack(metlWindow.AutomationElement);
        }

        [TestMethod]
        public void InsertTextboxAtRandomPoint()
        {
            homeTab.ActivateTextMode().TextInsertMode();

            var numTextboxes = canvas.ChildTextboxes.Count;

            Mouse.MoveTo(canvas.RandomPointWithinMargin(-40, -40));
            Mouse.Click(MouseButton.Left);

            Thread.Sleep(100);

            var textboxes = canvas.ChildTextboxes;
            if (textboxes.Count > 0)
            {
                textboxes[0].Value("Lorem ipsum");
            }

            Thread.Sleep(100);

            Assert.AreEqual(numTextboxes + 1, textboxes.Count);
        }

        [TestMethod]
        public void DeleteAllTextboxesOnCurrentPage()
        {
            homeTab.ActivateTextMode().TextSelectMode();

            var textboxes = canvas.ChildTextboxes;
            foreach (AutomationElement textbox in textboxes)
            {
                SelectTextboxWithClick(textbox);
                Thread.Sleep(500);
                //Keyboard.Press(Key.Delete);
                DeleteTextbox();
                Thread.Sleep(500);
            }
            
            Thread.Sleep(500);
            Assert.AreEqual(0, canvas.ChildTextboxes.Count);
        }

        private void SelectTextboxWithClick(AutomationElement textbox)
        {
            var bounding = textbox.Current.BoundingRectangle;
            var centreTextbox = new System.Drawing.Point((int)(bounding.X + bounding.Width / 2), (int)(bounding.Y + bounding.Height / 2));

            Mouse.MoveTo(centreTextbox);
            Mouse.Click(MouseButton.Left);
        }

        private void SelectTextboxWithMouse(AutomationElement textbox)
        {
            var bounding = textbox.Current.BoundingRectangle;

            bounding.Inflate(20, 20);

            // move around the bounding box in a clockwise direction
            Mouse.MoveTo(bounding.TopLeft.ToDrawingPoint());

            Mouse.DragTo(MouseButton.Left, bounding.TopRight.ToDrawingPoint());
            Mouse.DragTo(MouseButton.Left, bounding.BottomRight.ToDrawingPoint());
            Mouse.DragTo(MouseButton.Left, bounding.BottomLeft.ToDrawingPoint());
            Mouse.DragTo(MouseButton.Left, bounding.TopLeft.ToDrawingPoint());
            Mouse.DragTo(MouseButton.Left, bounding.TopRight.ToDrawingPoint());

            Mouse.Up(MouseButton.Left);
        }

        private void DeleteTextbox()
        {
            var deleteButton = new UITestHelper(metlWindow);
            deleteButton.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, "deleteButton"));

            var success = deleteButton.WaitForControlExist();
            Assert.IsTrue(success, ErrorMessages.WAIT_FOR_CONTROL_FAILED);

            deleteButton.AutomationElement.Invoke(); 
        }
    }
}
