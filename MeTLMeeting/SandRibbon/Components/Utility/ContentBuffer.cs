using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows;
using SandRibbon.Providers;
using MeTLLib.DataTypes;
using System.Diagnostics;

namespace SandRibbon.Components.Utility
{
    public class ContentBuffer
    {
        private StrokeFilter strokeFilter;
        private ImageFilter imageFilter;
        private TextFilter textFilter;

        // used to create a snapshot for undo/redo
        private ImageFilter imageDeltaCollection;
        private StrokeFilter strokeDeltaFilter;

        public ContentBuffer()
        {
            strokeFilter = new StrokeFilter();
            imageFilter = new ImageFilter();
            textFilter = new TextFilter();

            strokeDeltaFilter = new StrokeFilter();
            imageDeltaCollection = new ImageFilter();
        }

        public StrokeCollection FilteredStrokes(ContentVisibilityEnum contentVisibility)
        {
            return strokeFilter.FilterContent(strokeFilter.Strokes, contentVisibility); 
        }

        public IEnumerable<UIElement> FilteredTextBoxes(ContentVisibilityEnum contentVisibility)
        {
            return textFilter.FilteredContent(contentVisibility);
        }

        public IEnumerable<UIElement> FilteredImages(ContentVisibilityEnum contentVisibility)
        {
            return imageFilter.FilteredContent(contentVisibility);
        }

        public void UpdateChild(UIElement childToFind, Action<UIElement> updateChild)
        {
            if (childToFind is TextBox)
                textFilter.UpdateChild(childToFind, updateChild);
            if (childToFind is Image) 
                imageFilter.UpdateChild(childToFind, updateChild);
        }

        public void UpdateAllTextBoxes(Action<TextBox> updateChild)
        {
            textFilter.UpdateChildren(updateChild);
        }

        public void UpdateAllImages(Action<Image> updateChild)
        {
            imageFilter.UpdateChildren(updateChild);
        }

        public void Clear()
        {
            strokeFilter.Clear();
            imageFilter.Clear();
            textFilter.Clear();

            imageDeltaCollection.Clear();
            strokeDeltaFilter.Clear();
        }

        public void ClearStrokes(Action modifyVisibleContainer)
        {
            strokeFilter.Clear();
            modifyVisibleContainer();
        }

        public void ClearDeltaStrokes(Action modifyUndoContainer)
        {
            strokeDeltaFilter.Clear();
            modifyUndoContainer();
        }

        public void ClearDeltaImages(Action modifyUndoContainer)
        {
            imageDeltaCollection.Clear();
            modifyUndoContainer();
        }        

        public void AddStrokes(StrokeCollection strokes, Action<StrokeCollection> modifyVisibleContainer)
        {
            strokeFilter.Add(strokes, modifyVisibleContainer);
        }

        public void AddStroke(Stroke stroke, Action<Stroke> modifyVisibleContainer)
        {
            strokeFilter.Add(stroke, modifyVisibleContainer);
        }

        public void RemoveStroke(Stroke stroke, Action<Stroke> modifyVisibleContainer)
        {
            strokeFilter.Remove(stroke, modifyVisibleContainer);
        }

        public void RemoveStrokes(StrokeCollection strokes, Action<StrokeCollection> modifyVisibleContainer)
        {
            strokeFilter.Remove(strokes, modifyVisibleContainer);
        }

        public void AddDeltaStrokes(StrokeCollection strokes, Action<StrokeCollection> modifyUndoContainer)
        {
            strokeDeltaFilter.Add(strokes, modifyUndoContainer); 
        }

        public void AddDeltaImages(List<UIElement> images, Action<List<UIElement>> modifyUndoContainer)
        {
            imageDeltaCollection.Add(images, modifyUndoContainer);
        }

        public void ClearElements(Action modifyVisibleContainer)
        {
            imageFilter.Clear();
            textFilter.Clear();
            modifyVisibleContainer();
        }

        public void AddImage(UIElement element, Action<UIElement> modifyVisibleContainer)
        {
            Debug.Assert((element as Image) != null);
            imageFilter.Add(element, modifyVisibleContainer);
        }

        public void RemoveImage(UIElement element, Action<UIElement> modifyVisibleContainer)
        {
            Debug.Assert((element as Image) != null);
            imageFilter.Remove(element, modifyVisibleContainer);
        }

        public void AddTextBox(UIElement element, Action<UIElement> modifyVisibleContainer)
        {
            Debug.Assert((element as TextBox) != null);
            textFilter.Push(element, modifyVisibleContainer);
        }

        public void RemoveTextBox(UIElement element, Action<UIElement> modifyVisibleContainer)
        {
            Debug.Assert((element as TextBox) != null);
            textFilter.Remove(element, modifyVisibleContainer);
        }
    }
}
