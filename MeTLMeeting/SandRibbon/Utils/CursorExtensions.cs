using System;
using System.Windows.Input;
using System.Windows.Ink;
using MeTLLib.Utilities;

namespace SandRibbon.Utils
{
    class CursorExtensions
    {
        private static DrawingAttributes previousPen = null;
        private static Cursor previousCursor;
        private static Object lockObject = new Object();
        public static Cursor generateCursor(DrawingAttributes pen) 
        {
            using (DdMonitor.Lock(lockObject))
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
