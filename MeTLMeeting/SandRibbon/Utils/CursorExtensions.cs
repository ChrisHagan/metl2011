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
        public static Cursor ConvertToCursor(FrameworkElement fe, Point hotSpot)
        {
            try
            {
                var resultStream = new MemoryStream();
                App.Current.Dispatcher.adopt(() =>
                {
                    int width = (int)fe.Width;
                    int height = (int)fe.Height;
                    fe.Measure(new Size(fe.Width, fe.Height));
                    fe.Arrange(new Rect(0, 0, fe.Width, fe.Height));
                    fe.UpdateLayout();
                    if (width < 1) width = 1;
                    if (height < 1) height = 1;

                    var bitmapSource = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Default);
                    bitmapSource.Render(fe);
                    
                    var pixels = new UInt32[width * height];
                    bitmapSource.Freeze();
                    bitmapSource.CopyPixels(pixels, width * 4, 0);
                    var bitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb); 
                    var converter = new System.Drawing.ColorConverter();
                    for (int y = 0; y < height; y++)
                        for (int x = 0; x < width; x++)
                        {
                            //Tell me please that there's a faster way of doing this!
                            var Color = System.Drawing.Color.FromArgb((int)pixels[(y * width) + x]);
                            if (Color.A == 0 || Color.A == 255)
                            {
                                bitmap.SetPixel(x, y, Color);
                            }
                            else
                            {
                                var Alpha = Color.A;
                                double PreMult = Color.A / 255;
                                var Red = ((byte)(Color.R * PreMult));
                                var Green = ((byte)(Color.G * PreMult));
                                var Blue = ((byte)(Color.B * PreMult));
                                var colorString = new Color { A = (byte)Alpha, R = (byte)Red, G = (byte)Green, B = (byte)Blue }.ToString();
                                var newColor = (System.Drawing.Color)converter.ConvertFromString(colorString);
                                bitmap.SetPixel(x, y, newColor);
                            }
                        }       
                    var handle = bitmap.GetHicon();
                    var icon = System.Drawing.Icon.FromHandle(handle);
                    System.Drawing.Icon.FromHandle(handle).Save(resultStream);
                    var hsY = (byte)(int)(hotSpot.Y * height);
                    var hsX = (byte)(int)(hotSpot.X * width);
                    resultStream.Seek(2, SeekOrigin.Begin);
                    resultStream.Write(resultStream.ToArray(), 2, 1);
                    resultStream.Seek(8, SeekOrigin.Begin);
                    resultStream.WriteByte(0);
                    resultStream.Seek(10, SeekOrigin.Begin);
                    resultStream.Seek(10, System.IO.SeekOrigin.Begin);
                    resultStream.WriteByte(hsX);
                    resultStream.Seek(12, System.IO.SeekOrigin.Begin);
                    resultStream.WriteByte(hsY);
                    resultStream.Seek(0, SeekOrigin.Begin);
                });
                var cursor = new System.Windows.Input.Cursor(resultStream);
                return cursor;
            }
            catch (System.Runtime.InteropServices.ExternalException)
            {
                App.Now("cursor generation exception");
                return Cursors.Cross;
            }
        }
        private static Cursor internalGenerateCursor(FrameworkElement fe, Point hotspot)
        {
            Cursor cursor = null;
            App.Current.Dispatcher.adopt(() =>
            {
                if (fe.Height < 1) fe.Height = 1;
                if (fe.Width < 1) fe.Width = 1;
                var grid = new System.Windows.Controls.Grid
                {
                    Height = 128,
                    Width = 128,
                    Background = Brushes.Transparent
                };
                var innerGrid = new System.Windows.Controls.Grid
                {
                    Height = fe.Height,
                    Width = fe.Width,
                    Background = Brushes.Transparent,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                innerGrid.Children.Add(fe);
                grid.Children.Add(innerGrid);
                double UpdatedY = (((grid.Height - innerGrid.Height) / 2) + (hotspot.X * innerGrid.Width)) / grid.Width;
                double UpdatedX = (((grid.Width - innerGrid.Width) / 2) + (hotspot.Y * innerGrid.Height))/ grid.Height;
                cursor = CursorExtensions.ConvertToCursor(grid, new Point(UpdatedX,UpdatedY));
            });
            return cursor;
        }
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
                cursor = internalGenerateCursor(poly, new Point(0.5, 0.5));
            });
            return cursor;
        }
        public static Cursor generateCursor(FrameworkElement fe, Point hotspot)
        {
            Cursor cursor = null;
            App.Current.Dispatcher.adopt(() =>
                {
                    cursor = internalGenerateCursor(fe, hotspot);
                });
            return cursor;
        }
        private static Cursor _selectCursor;
        private static Cursor selectCursor
        {
            get
            {
                if (_selectCursor == null)
                {
                    var surround = new Border { Background = Brushes.Orange, Height = 8, Width = 8, CornerRadius = new CornerRadius(4), BorderBrush = Brushes.Red, BorderThickness = new Thickness(1) };
                    _selectCursor = generateCursor(surround, new Point(0.5, 0.5));
                }
                return _selectCursor;
            }
        }
        private static Cursor _eraseCursor;
        private static Cursor eraseCursor
        {
            get
            {
                if (_eraseCursor == null)
                {
                    var surround = new Border { Background = Brushes.White, Height = 10, Width = 10, CornerRadius = new CornerRadius(1), BorderBrush = Brushes.DarkGray, BorderThickness = new Thickness(1) };
                    _eraseCursor = generateCursor(surround, new Point(0.5, 0.5));
                }
                return _eraseCursor;
            }
        }
        public static Cursor generateCursor(InkCanvasEditingMode mode)
        {
            FrameworkElement poly;
            Point hotspot;
            switch (mode.ToString())
            {
                case "Select":
                    return selectCursor;
                    break;
                case "EraseByStroke":
                    return eraseCursor;
                    break;
                case "None":
                    poly = new Grid { Height = 10, Width = 10 };
                    hotspot = new Point(0, 0);
                    break;
                default:
                    poly = new Grid { Height = 32, Width = 32 };
                    ((Grid)poly).Children.Add(new System.Windows.Shapes.Rectangle { Fill = Brushes.Black, Height = Double.NaN, Width = 2, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Stretch });
                    ((Grid)poly).Children.Add(new System.Windows.Shapes.Rectangle { Fill = Brushes.Black, Height = 2, Width = Double.NaN, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center });
                    hotspot = new System.Windows.Point(0.5, 0.5);
                    break;
            }
            return generateCursor(poly, hotspot);
        }
    }
}
