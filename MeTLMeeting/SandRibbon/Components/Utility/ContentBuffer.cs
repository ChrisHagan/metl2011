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

        public double logicalX;
        public double logicalY;
        private double moveDeltaX;
        private double moveDeltaY;

        private double ReturnPositiveValue(double x)
        {
            if (x < 0.0)
            {
                return (-1 * x);
            }
            return x;
        }

        private bool PossiblyExtendTheNegativeBoundsOfTheCanvasForMoveDelta(double elementLeft, double elementTop)
        {
            var isExtending = false;
            if (elementLeft < 0.0)
            {
                logicalX = elementLeft + logicalX;
                moveDeltaX = elementLeft;
                isExtending = true;
            }
            if (elementTop < 0.0)
            {
                logicalY = elementTop + logicalY;
                moveDeltaY = elementTop;
                isExtending = true;
            }
            return isExtending;
        }

        private bool PossiblyExtendTheNegativeBoundsOfTheCanvas(double elementLeft, double elementTop)
        {
            var isExtending = false;
            if (elementLeft < logicalX)
            {
                logicalX = elementLeft;
                isExtending = true;
            }
            if (elementTop < logicalY)
            {
                logicalY = elementTop;
                isExtending = true;
            }
            return isExtending;
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

        public void adjustStroke(Stroke stroke, Func<Stroke,Stroke> adjustment)
        {
            //var stroke = incomingStroke.Clone();
            var oldCanvasOffsetX = logicalX;
            var oldCanvasOffsetY = logicalY;
            double translateX = 0.0;
            double translateY = 0.0;
            var transformMatrix = new System.Windows.Media.Matrix();
            var myIncomingRect = stroke.GetBounds();
            var localX = myIncomingRect.X;
            var localY = myIncomingRect.Y;
            if (PossiblyExtendTheNegativeBoundsOfTheCanvas(stroke.GetBounds().X, stroke.GetBounds().Y))
            {
                translateX = ReturnPositiveValue(ReturnPositiveValue(logicalX) - ReturnPositiveValue(oldCanvasOffsetX));
                translateY = ReturnPositiveValue(ReturnPositiveValue(logicalY) - ReturnPositiveValue(oldCanvasOffsetY));

                transformMatrix = new System.Windows.Media.Matrix();
                transformMatrix.Translate(translateX, translateY);

                foreach (var tStroke in strokeFilter.Strokes)
                {
                    if (stroke.tag().id != tStroke.tag().id)
                    {
                        var myRect = tStroke.GetBounds();
                        tStroke.Transform(transformMatrix, false);
                    }
                }
            }
            doAdjustStroke(stroke, adjustment);
        }

        public void adjustStrokesForMoveDelta(List<String> strokeIdentities, Func<Stroke, Stroke> adjustment)
        {
            var strokes = strokeFilter.Strokes.Where(s => strokeIdentities.Contains(s.tag().id));
            foreach(var stroke in strokes)
            {
                if (PossiblyExtendTheNegativeBoundsOfTheCanvasForMoveDelta(stroke.GetBounds().X, stroke.GetBounds().Y))
                {
                    var translateX = ReturnPositiveValue(moveDeltaX);
                    var translateY = ReturnPositiveValue(moveDeltaY);
                    foreach (var tStroke in strokeFilter.Strokes)
                    {
                        var transformMatrix = new System.Windows.Media.Matrix();
                        transformMatrix.Translate(translateX, translateY);
                        tStroke.Transform(transformMatrix, false);
                    }
                }
                doAdjustStroke(stroke, adjustment);
            }
        }

        public void adjustStrokeForMoveDelta(String strokeIdentity, Func<Stroke, Stroke> adjustment)
        {
            var stroke = strokeFilter.Strokes.Where(s => s.tag().id == strokeIdentity).First();
            if (PossiblyExtendTheNegativeBoundsOfTheCanvasForMoveDelta(stroke.GetBounds().X, stroke.GetBounds().Y))
            {
                var translateX = ReturnPositiveValue(moveDeltaX);
                var translateY = ReturnPositiveValue(moveDeltaY);
                foreach (var tStroke in strokeFilter.Strokes)
                {
                    var transformMatrix = new System.Windows.Media.Matrix();
                    transformMatrix.Translate(translateX, translateY);
                    tStroke.Transform(transformMatrix, false);
                }
            }
            doAdjustStroke(stroke,adjustment);
        }
        private void doAdjustStroke(Stroke stroke, Func<Stroke, Stroke> adjustment)
        {
            stroke = adjustment(stroke);
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
