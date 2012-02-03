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
            var totalWidth = element.ActualWidth;
            var totalHeight = element.ActualHeight;
            var aspectRatio = totalWidth / totalHeight;

            double scaledWidth = 0;
            double scaledHeight = 0;
            if (totalWidth > totalHeight)
            {
                scaledWidth = ((double)Globals.DefaultCanvasSize.Width - (Globals.QuizMargin * 2));
                scaledHeight = scaledWidth / aspectRatio;
            }
            else
            {
                scaledHeight = ((double)Globals.DefaultCanvasSize.Height - (Globals.QuizMargin * 2));
                scaledWidth = scaledHeight * aspectRatio;
            }
            
            return new Rect(0, 0, scaledWidth, scaledHeight);
        }
    }
}
