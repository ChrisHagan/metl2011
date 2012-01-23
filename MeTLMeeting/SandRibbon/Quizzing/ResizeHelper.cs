using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using SandRibbon.Providers;

namespace SandRibbon.Quizzing
{
    public class ResizeHelper
    {
        public static Rect ScaleMajorAxisToCanvasSize(FrameworkElement element)
        {
            // use the same scaling factor to maintain aspect ratio, using the dominant side
            var scalingFactor = Math.Max((element.ActualHeight + (Globals.QuizMargin * 2)) / (double)Globals.DefaultCanvasSize.Height, 
                (element.ActualWidth + (Globals.QuizMargin * 2)) / (double)Globals.DefaultCanvasSize.Width);
            var scaledHeight = element.ActualHeight / scalingFactor;
            var scaledWidth = element.ActualWidth / scalingFactor;

            return new Rect(0, 0, scaledWidth, scaledHeight);
        }
    }
}
