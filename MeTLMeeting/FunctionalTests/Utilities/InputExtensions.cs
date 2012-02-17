using Microsoft.Test.Input;
using Functional;
using System.Collections.Generic;
using System.Threading;

namespace FunctionalTests.Utilities
{
    public static class MouseExtensions
    {
        public static void ClickAt(System.Windows.Point location, MouseButton button)
        {
            Mouse.MoveTo(location.ToDrawingPoint());
            Mouse.Click(button);
        }

        public static void AnimateThroughPoints(IEnumerable<System.Drawing.Point> points)
        {
            var count = 0;
            foreach (var point in points)
            {
                // give the client a chance to send the data over the network
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
