using System;
using System.Collections.Generic;
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

namespace SandRibbon.Utils
{
    class CursorExtensions
    {
        public static Cursor generateCursor(DrawingAttributes pen)
        {
            Cursor cursor = null;
            App.Current.Dispatcher.adopt(() =>
            {
                var colour = new SolidColorBrush(pen.Color);
                var poly = new System.Windows.Shapes.Ellipse
                {
                    Height = pen.Height,
                    Width = pen.Width,
                    Fill = colour,
                    Stroke = colour,
                    StrokeThickness = 2,
                    StrokeLineJoin = PenLineJoin.Round,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center

                };
                cursor = CursorHelper.CreateCursor(poly, (int)(pen.Width / 2), (int)(pen.Height / 2));
            });
            return cursor;
        }
        public static Cursor generateCursor(FrameworkElement fe, Point hotspot)
        {
            Cursor cursor = null;
            App.Current.Dispatcher.adopt(() =>
                {
                    cursor = CursorHelper.CreateCursor(fe, (int)hotspot.X, (int)hotspot.Y);
                });
            return cursor;
        }
    }
}
