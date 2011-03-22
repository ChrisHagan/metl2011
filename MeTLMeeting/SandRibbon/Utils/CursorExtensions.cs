using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Ink;
using System.IO;
using SandRibbon;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace SandRibbon.Utils
{
    class CursorExtensions
    {
        private static DrawingAttributes previousPen = null;
        private static Cursor previousCursor;
        private static Object lockObject = new Object();
        public static Cursor generateCursor(DrawingAttributes pen) {
            lock (lockObject)
            {
                if (pen == previousPen) return previousCursor;
                var color = System.Drawing.Color.FromArgb(pen.Color.A, pen.Color.R, pen.Color.G, pen.Color.B);
                previousPen = pen;
                previousCursor = CursorHelper.CreateCursor((int)pen.Width, (int)pen.Height, color, (int)(pen.Width / 2), (int)(pen.Height / 2));
                return previousCursor;
            }
        }
    }
}
