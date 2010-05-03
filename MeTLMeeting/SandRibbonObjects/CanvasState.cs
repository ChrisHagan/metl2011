using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;

namespace SandRibbonObjects
{
    public static class UIElementCollectionExtensions
    {
        public static List<UIElement> ToList(this UIElementCollection elements)
        {
            var result = new UIElement[elements.Count];
            elements.CopyTo(result, 0);
            return result.ToList();
        }
    }
    public class CanvasState
    {
        public StrokeCollection strokes;
        public List<UIElement> children;
    }
}
