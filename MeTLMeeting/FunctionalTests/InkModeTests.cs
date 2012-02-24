using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UITestFramework;
using Functional;
using Microsoft.Test.Input;
using System.Windows.Automation;
using System.Threading;
using System.Windows.Controls;

namespace FunctionalTests
{
    [TestClass]
    public class InkModeTests
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

        private void CollectRandomPoints()
        {
            
        }

        [TestMethod]
        public void DrawRandomInkStrokes()
        {
            CollectRandomPoints();
        }

        [TestMethod]
        public void InsertInkStrokesAtRandomPoint()
        {
            homeTab.ActivatePenMode();

            DrawSpirographWaveOnCanvas(canvas);
        }

        [TestMethod]
        public void SelectAllInkOnCurrentPage()
        {
            homeTab.ActivatePenMode().PenSelectMode();

            SelectInkStroke(canvas.BoundingRectangle);

            UITestHelper.Wait(TimeSpan.FromMilliseconds(500)); // give some time for the canvas to select all the ink strokes
        }

        [TestMethod]
        public void MoveSelectedInkToRandomCorner()
        {
            Mouse.MoveTo(canvas.CentrePoint());
            Mouse.Down(MouseButton.Left);

            // drag to a random corner
            var bounding = canvas.BoundingRectangle;
            
            bounding.Inflate(-5, -5);
            var coords = new List<System.Drawing.Point>();
            var randCorner = new Random();

            coords.Add(bounding.TopLeft.ToDrawingPoint());
            coords.Add(bounding.TopRight.ToDrawingPoint());
            coords.Add(bounding.BottomRight.ToDrawingPoint());
            coords.Add(bounding.BottomLeft.ToDrawingPoint());

            Mouse.MoveTo(coords[randCorner.Next(3)]);
            Mouse.Up(MouseButton.Left);

            var adorner = canvas.Parent.AutomationElement.Descendant(typeof(System.Windows.Documents.AdornerLayer));
        }

        [TestMethod]
        public void DeleteAllInkStrokesOnCurrentPage()
        {
            homeTab.ActivatePenMode().PenSelectMode();

            SelectInkStroke(canvas.BoundingRectangle);

            UITestHelper.Wait(TimeSpan.FromMilliseconds(500)); // give some time for the canvas to select all the ink strokes
            DeleteInkStroke();

            UITestHelper.Wait(TimeSpan.FromSeconds(10)); // wait a bit before checking the count, and let the server get the updated information first

            Assert.AreEqual(0, canvas.NumberOfInkStrokes());
        }

        private void DeleteInkStroke()
        {
            var deleteButton = new UITestHelper(metlWindow);
            deleteButton.SearchProperties.Add(new PropertyExpression(AutomationElement.AutomationIdProperty, "deleteButton"));

            var success = deleteButton.WaitForControlExist();
            Assert.IsTrue(success, ErrorMessages.WAIT_FOR_CONTROL_FAILED);

            deleteButton.AutomationElement.Invoke(); 
        }

        private List<System.Drawing.Point> PopulateCoords(System.Windows.Rect bounding)
        {
            var coords = new List<System.Drawing.Point>();

            coords.Add(bounding.TopLeft.ToDrawingPoint());
            coords.Add(bounding.TopRight.ToDrawingPoint());
            coords.Add(bounding.BottomRight.ToDrawingPoint());
            coords.Add(bounding.BottomLeft.ToDrawingPoint());
            coords.Add(bounding.TopLeft.ToDrawingPoint());
            coords.Add(bounding.TopRight.ToDrawingPoint());

            return coords;
        }

        private void SelectInkStroke(System.Windows.Rect boundingRectangle)
        {
            var bounding = boundingRectangle;

            bounding.Inflate(-5, -5);

            var coords = PopulateCoords(bounding);

            // move around the bounding box in a clockwise direction
            Mouse.MoveTo(coords.First());
            Mouse.Down(MouseButton.Left);

            foreach (var coord in coords.Skip(1))
            {
                Mouse.MoveTo(coord);
                Thread.Sleep(100);
            }

            Mouse.Up(MouseButton.Left);
        }

        private void DrawSpirographWaveOnCanvas(CollapsedCanvasStack canvas)
        {
            var centre = canvas.CentrePoint();

            var rand = new Random();
            var points = GetPointsForSpirograph(centre.X, centre.Y, rand.NextDouble() * 1.25 + 0.9, rand.NextDouble() * 5 + 3, rand.NextDouble() * 2, 0, 300);

            Mouse.MoveTo(points.First());
            Mouse.Down(MouseButton.Left);

            AnimateMouseThroughPoints(points);

            Mouse.Up(MouseButton.Left);
        }

        private IEnumerable<System.Drawing.Point> GetPointsForSpirograph(int centerX, int centerY, double littleR, double bigR, double a, int tStart, int tEnd)
        {
            // Equations from http://www.mathematische-basteleien.de/spirographs.htm
            for (double t = tStart; t < tEnd; t += 0.1)
            {
                var rDifference = bigR - littleR;
                var rRatio = littleR / bigR;
                var x = (rDifference * Math.Cos(rRatio * t) + a * Math.Cos((1 - rRatio) * t)) * 25;
                var y = (rDifference * Math.Sin(rRatio * t) - a * Math.Sin((1 - rRatio) * t)) * 25;

                yield return new System.Drawing.Point(centerX + (int)x, centerY + (int)y);
            }
        }

        private void AnimateMouseThroughPoints(IEnumerable<System.Drawing.Point> points)
        {
            var window = metlWindow.AutomationElement;
            var listItems = window.Descendants(typeof(ListBoxItem));
            var list = new List<SelectionItemPattern>();

            foreach (AutomationElement item in listItems)
            {
                if (item.Current.Name.Contains("PenColors"))
                {
                    var selection = item.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
                    list.Add(selection);
                }
            }

            var count = 0;
            list[new Random().Next(list.Count - 1)].Select();

            foreach (var point in points)
            {
                if (count % 10 == 0)
                {
                    Mouse.Up(MouseButton.Left);
                    Mouse.Down(MouseButton.Left);
                }
                Mouse.MoveTo(point);
                Thread.Sleep(5);
                count++;
            }
        }
    }
}
