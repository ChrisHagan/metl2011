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
        public static Cursor generateCursor(DrawingAttributes pen) {
            var color = System.Drawing.Color.FromArgb(pen.Color.A, pen.Color.R, pen.Color.G, pen.Color.B);
            return CursorHelper.CreateCursor((int)pen.Width,(int)pen.Height,color,(int)(pen.Width / 2), (int)(pen.Height / 2));
        }
    }
}
