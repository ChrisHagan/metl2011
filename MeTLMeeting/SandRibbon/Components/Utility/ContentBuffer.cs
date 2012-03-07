using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows;

namespace SandRibbon.Components.Utility
{
    public class ContentBuffer
    {
        private UIElementCollection uiCollection;
        private StrokeCollection strokeCollection;

        public ContentBuffer(UIElement visualParent, FrameworkElement logicalParent)
        {
            uiCollection = new UIElementCollection(visualParent, logicalParent);
            strokeCollection = new StrokeCollection();
        }

        public void ClearStrokes()
        {
            strokeCollection.Clear();
        }

        public void AddStrokes(StrokeCollection strokes)
        {
            strokeCollection.Add(strokes);
        }

        public void AddStrokes(Stroke stroke)
        {
            strokeCollection.Add(stroke);
        }

        public void RemoveStrokes(StrokeCollection strokes)
        {
            strokeCollection.Remove(strokes);
        }
        
        public void RemoveStrokes(Stroke stroke)
        {
            strokeCollection.Remove(stroke);
        }

        public StrokeCollection Strokes
        {
            get
            {
                return strokeCollection;
            }
        }
    }

}
