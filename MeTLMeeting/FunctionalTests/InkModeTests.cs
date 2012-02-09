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

        [TestMethod]
        public void InsertInkStrokesAtRandomPoint()
        {
            homeTab.ActivatePenMode();

            DrawSpirographWaveOnCanvas(canvas);

            Thread.Sleep(200);
        }

        [TestMethod]
        public void DeleteAllInkStrokesOnCurrentPage()
        {
            homeTab.ActivatePenMode().PenSelectMode();

            var inkStrokes = canvas.FindInkStrokes();
            foreach (AutomationElement ink in inkStrokes)
            {
                SelectInkStroke(ink);

                Keyboard.Press(Key.Delete);
            }

            canvas = new CollapsedCanvasStack(metlWindow.AutomationElement);
            Assert.AreEqual(0, canvas.FindInkStrokes().Count);
        }

        private void SelectInkStroke(AutomationElement element)
        {
            var bounding = element.Current.BoundingRectangle;

            bounding.Inflate(-20, -20);

            // move around the bounding box in a clockwise direction
            Mouse.MoveTo(bounding.TopLeft.ToDrawingPoint());
            Mouse.Down(MouseButton.Left);

            Mouse.MoveTo(bounding.TopRight.ToDrawingPoint());
            Thread.Sleep(10);
            Mouse.MoveTo(bounding.BottomRight.ToDrawingPoint());
            Thread.Sleep(10);
            Mouse.MoveTo(bounding.BottomLeft.ToDrawingPoint());
            Thread.Sleep(10);
            Mouse.MoveTo(bounding.TopLeft.ToDrawingPoint());
            Thread.Sleep(10);
            Mouse.MoveTo(bounding.TopRight.ToDrawingPoint());
            Thread.Sleep(10);

            Mouse.Up(MouseButton.Left);
        }

        private System.Drawing.Point SelectCentrePoint(CollapsedCanvasStack canvas)
        {
            var bounds = canvas.BoundingRectangle;

            var centreX = bounds.X + (bounds.Width / 2);
            var centreY = bounds.Y + (bounds.Height / 2);

            return new System.Drawing.Point((int)centreX, (int)centreY);
        }

        private void DrawSpirographWaveOnCanvas(CollapsedCanvasStack canvas)
        {
            var centre = SelectCentrePoint(canvas);

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
