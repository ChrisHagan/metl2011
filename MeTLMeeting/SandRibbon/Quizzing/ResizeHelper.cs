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
            return ScaleMajorAxisToCanvasSize(element, new Size());
        }

        public static Rect ScaleMajorAxisToCanvasSize(FrameworkElement element, Size overrideSize)
        {
            var totalWidth = element.ActualWidth;
            var totalHeight = element.ActualHeight;
            var aspectRatio = totalWidth / totalHeight;

            if (overrideSize == null || (overrideSize.Width * overrideSize.Height) < 1e-8)
            {
                overrideSize.Width = ((double)Globals.DefaultCanvasSize.Width - (Globals.QuizMargin * 2));
                overrideSize.Height = ((double)Globals.DefaultCanvasSize.Height - (Globals.QuizMargin * 2)); 
            }

            double scaledWidth = 0;
            double scaledHeight = 0;
            if (totalWidth > totalHeight)
            {
                scaledWidth = overrideSize.Width; //((double)Globals.DefaultCanvasSize.Width - (Globals.QuizMargin * 2));
                scaledHeight = scaledWidth / aspectRatio;
            }
            else
            {
                scaledHeight = overrideSize.Height; // ((double)Globals.DefaultCanvasSize.Height - (Globals.QuizMargin * 2));
                scaledWidth = scaledHeight * aspectRatio;
            }
            
            return new Rect(0, 0, scaledWidth, scaledHeight);
        }
    }
}
