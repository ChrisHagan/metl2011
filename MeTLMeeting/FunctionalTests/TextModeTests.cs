using Microsoft.VisualStudio.TestTools.UnitTesting;
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

            var numTextboxes = canvas.FindTextboxes().Count;

            Mouse.MoveTo(SelectRandomPoint(canvas));
            Mouse.Click(MouseButton.Left);

            var textboxes = canvas.FindTextboxes();
            if (textboxes.Count > 0)
            {
                textboxes[0].Value("Lorem ipsum");
            }

            Assert.AreEqual(numTextboxes + 1, textboxes.Count);
        }

        [TestMethod]
        public void DeleteAllTextboxesOnCurrentPage()
        {
            homeTab.ActivateTextMode().TextSelectMode();

            var textboxes = canvas.FindTextboxes();
            foreach (AutomationElement textbox in textboxes)
            {
                SelectTextbox(textbox);

                Keyboard.Press(Key.Delete);
            }

            Assert.AreEqual(0, canvas.FindTextboxes().Count);
        }

        private void SelectTextbox(AutomationElement textbox)
        {
            var bounding = textbox.Current.BoundingRectangle;

            bounding.Inflate(20, 20);

            // move around the bounding box in a clockwise direction
            Mouse.MoveTo(bounding.TopLeft.ToDrawingPoint());
            Mouse.Down(MouseButton.Left);

            Mouse.MoveTo(bounding.TopRight.ToDrawingPoint());
            Thread.Sleep(5);
            Mouse.MoveTo(bounding.BottomRight.ToDrawingPoint());
            Thread.Sleep(5);
            Mouse.MoveTo(bounding.BottomLeft.ToDrawingPoint());
            Thread.Sleep(5);
            Mouse.MoveTo(bounding.TopLeft.ToDrawingPoint());
            Thread.Sleep(5);

            Mouse.Up(MouseButton.Left);
        }

        private System.Drawing.Point SelectRandomPoint(CollapsedCanvasStack canvas)
        {
            var bounds = canvas.BoundingRectangle;
            var random = new Random();

            var randX = (int)(bounds.X + random.NextDouble() * bounds.Width);
            var randY = (int)(bounds.Y + random.NextDouble() * bounds.Height);

            return new System.Drawing.Point(randX, randY);
        }
    }
}
