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
        public void UpdateAllStrokes(Action<Stroke> updateChild)
        {
            foreach (var stroke in strokeFilter.Strokes)
                updateChild(stroke);
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
        public double moveDeltaX;
        public double moveDeltaY;

        private double ReturnPositiveValue(double x)
        {
            return Math.Abs(x);
        }

        private bool PossiblyExtendTheNegativeBoundsOfTheCanvasForMoveDelta(double elementLeft, double elementTop)
        {
            Console.WriteLine("ContentBuffer::PossiblyExtendTheNegativeBoundsOfTheCanvasForMoveDelta::Called");
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

        private bool checkIfLogicalBoundsUpdates(double x, double y)
        {
            if ((x - logicalX) < -1 || (y - logicalY) < -1) return true;
            return false;
        }
        private Point generateLogicalBounds(double elementLeft, double elementTop)
        {
            var point = new Point(logicalX, logicalY);
            if (elementLeft < logicalX)
                point.X = elementLeft;
            if (elementTop < logicalY)
                point.Y = elementTop;
            return point;
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

        public Stroke adjustStroke(Stroke stroke, Func<Stroke,Stroke> adjustment)
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
            if (checkIfLogicalBoundsUpdates(localX, localY))
            {
                var newBounds = generateLogicalBounds(localX, localY);
                logicalX = newBounds.X;
                logicalY = newBounds.Y;
                translateX = ReturnPositiveValue(ReturnPositiveValue(logicalX) - ReturnPositiveValue(oldCanvasOffsetX));
                translateY = ReturnPositiveValue(ReturnPositiveValue(logicalY) - ReturnPositiveValue(oldCanvasOffsetY));

                transformMatrix = new System.Windows.Media.Matrix();
                transformMatrix.Translate(translateX, translateY);

                foreach (var tStroke in strokeFilter.Strokes)
                    if (stroke.tag().id != tStroke.tag().id)
                        tStroke.Transform(transformMatrix, false);

                foreach (var tImage in imageFilter.Images)
                {
                    InkCanvas.SetLeft(tImage, (InkCanvas.GetLeft(tImage) + translateX));
                    InkCanvas.SetTop(tImage, (InkCanvas.GetTop(tImage) + translateY));                    
                }

                foreach (var tText in textFilter.TextBoxes)
                {
                    InkCanvas.SetLeft(tText, (InkCanvas.GetLeft(tText) + translateX));
                    InkCanvas.SetTop(tText, (InkCanvas.GetTop(tText) + translateY));
                }                
            }
            return doAdjustStroke(stroke, adjustment);            
        }

        public void adjustStrokesForMoveDelta(List<String> strokeIdentities)
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
                    foreach (var tImage in imageFilter.Images)
                    {
                        InkCanvas.SetLeft(tImage, (InkCanvas.GetLeft(tImage) + translateX));
                        InkCanvas.SetTop(tImage, (InkCanvas.GetTop(tImage) + translateY));
                    }
                    foreach (var tText in textFilter.TextBoxes)
                    {
                        InkCanvas.SetLeft(tText, (InkCanvas.GetLeft(tText) + translateX));
                        InkCanvas.SetTop(tText, (InkCanvas.GetTop(tText) + translateY));
                    }
                }
                //doAdjustStroke(stroke, adjustment);
            }
        }

        public void adjustStrokeForMoveDelta(String strokeIdentity, Func<Stroke, Stroke> adjustment)
        {
            var strokes = strokeFilter.Strokes.Where(s => s.tag().id == strokeIdentity);
            if (strokes.Count() > 0)
            {
                var stroke = strokes.First();
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
                    foreach (var tImage in imageFilter.Images)
                    {
                        InkCanvas.SetLeft(tImage, (InkCanvas.GetLeft(tImage) + translateX));
                        InkCanvas.SetTop(tImage, (InkCanvas.GetTop(tImage) + translateY));
                    }
                    foreach (var tText in textFilter.TextBoxes)
                    {
                        InkCanvas.SetLeft(tText, (InkCanvas.GetLeft(tText) + translateX));
                        InkCanvas.SetTop(tText, (InkCanvas.GetTop(tText) + translateY));
                    }
                }
                doAdjustStroke(stroke, adjustment);
            }
        }
        private Stroke doAdjustStroke(Stroke stroke, Func<Stroke, Stroke> adjustment)
        {
            return adjustment(stroke);
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

        public void adjustImage(Image image, Func<Image, Image> adjustment)
        {
            var oldCanvasOffsetX = logicalX;
            var oldCanvasOffsetY = logicalY;
            double translateX = 0.0;
            double translateY = 0.0;
            var localX = InkCanvas.GetLeft(image);
            var localY = InkCanvas.GetTop(image);           
            if (checkIfLogicalBoundsUpdates(localX, localY))
            {
                var newBounds = generateLogicalBounds(localX, localY);
                logicalX = newBounds.X;
                logicalY = newBounds.Y;
                translateX = ReturnPositiveValue(ReturnPositiveValue(logicalX) - ReturnPositiveValue(oldCanvasOffsetX));
                translateY = ReturnPositiveValue(ReturnPositiveValue(logicalY) - ReturnPositiveValue(oldCanvasOffsetY));

                foreach (var tImage in imageFilter.Images)
                {
                    if(image.tag().id != (tImage as Image).tag().id)
                    {
                        InkCanvas.SetLeft(tImage,(InkCanvas.GetLeft(tImage) + translateX));
                        InkCanvas.SetTop(tImage, (InkCanvas.GetTop(tImage) + translateY));
                    }
                }

                var transformMatrix = new System.Windows.Media.Matrix();
                transformMatrix.Translate(translateX, translateY);
                foreach (var tStroke in strokeFilter.Strokes)
                {
                    var myRect = tStroke.GetBounds();
                    tStroke.Transform(transformMatrix, false);                    
                }

                foreach (var tText in textFilter.TextBoxes)
                {
                    InkCanvas.SetLeft(tText, (InkCanvas.GetLeft(tText) + translateX));
                    InkCanvas.SetTop(tText, (InkCanvas.GetTop(tText) + translateY));
                }
            }
            doAdjustImage(image, adjustment);
        }

        public void adjustImageForMoveDelta(String imageIdentity, Func<Image, Image> adjustment)
        {
            var images = imageFilter.Images.Where(i => (i as Image).tag().id == imageIdentity);
            if (images.Count() > 0)
            {
                var image = images.First();
                if (PossiblyExtendTheNegativeBoundsOfTheCanvasForMoveDelta(InkCanvas.GetLeft(image), InkCanvas.GetTop(image)))
                {
                    var translateX = ReturnPositiveValue(moveDeltaX);
                    var translateY = ReturnPositiveValue(moveDeltaY);
                    foreach (var tImage in imageFilter.Images)
                    {
                        InkCanvas.SetLeft(tImage, (InkCanvas.GetLeft(tImage) + translateX));
                        InkCanvas.SetTop(tImage, (InkCanvas.GetTop(tImage) + translateY));
                    }
                    foreach (var tStroke in strokeFilter.Strokes)
                    {
                        var transformMatrix = new System.Windows.Media.Matrix();
                        transformMatrix.Translate(translateX, translateY);
                        tStroke.Transform(transformMatrix, false);
                    }
                    foreach (var tText in textFilter.TextBoxes)
                    {
                        InkCanvas.SetLeft(tText, (InkCanvas.GetLeft(tText) + translateX));
                        InkCanvas.SetTop(tText, (InkCanvas.GetTop(tText) + translateY));
                    }
                }
                doAdjustImage(image as Image, adjustment);
            }
        }

        public void adjustImagesForMoveDelta(List<String> imageIdentities)
        {
            var images = imageFilter.Images.Where(i => imageIdentities.Contains((i as Image).tag().id));
            foreach(var image in images)            
            {
                if (PossiblyExtendTheNegativeBoundsOfTheCanvasForMoveDelta(InkCanvas.GetLeft(image), InkCanvas.GetTop(image)))
                {
                    var translateX = ReturnPositiveValue(moveDeltaX);
                    var translateY = ReturnPositiveValue(moveDeltaY);
                    foreach (var tImage in imageFilter.Images)
                    {
                        InkCanvas.SetLeft(tImage, (InkCanvas.GetLeft(tImage) + translateX));
                        InkCanvas.SetTop(tImage, (InkCanvas.GetTop(tImage) + translateY));
                    }
                    foreach (var tStroke in strokeFilter.Strokes)
                    {
                        var transformMatrix = new System.Windows.Media.Matrix();
                        transformMatrix.Translate(translateX, translateY);
                        tStroke.Transform(transformMatrix, false);
                    }
                    foreach (var tText in textFilter.TextBoxes)
                    {                     
                        InkCanvas.SetLeft(tText, (InkCanvas.GetLeft(tText) + translateX));
                        InkCanvas.SetTop(tText, (InkCanvas.GetTop(tText) + translateY));                     
                    }
                }
                //doAdjustImage(image as Image, adjustment);
            }
        }

        private void doAdjustImage(Image image, Func<Image, Image> adjustment)
        {
            image = adjustment(image);
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

        public void adjustText(TextBox box, Func<TextBox, TextBox> adjustment)
        {
            var oldCanvasOffsetX = logicalX;
            var oldCanvasOffsetY = logicalY;
            double translateX = 0.0;
            double translateY = 0.0;
            var localX = InkCanvas.GetLeft(box);
            var localY = InkCanvas.GetTop(box);
            if (checkIfLogicalBoundsUpdates(localX, localY))
            {
                var newBounds = generateLogicalBounds(localX, localY);
                logicalX = newBounds.X;
                logicalY = newBounds.Y;
                translateX = ReturnPositiveValue(ReturnPositiveValue(logicalX) - ReturnPositiveValue(oldCanvasOffsetX));
                translateY = ReturnPositiveValue(ReturnPositiveValue(logicalY) - ReturnPositiveValue(oldCanvasOffsetY));

                foreach (var tText in textFilter.TextBoxes)
                {
                    if (box.tag().id != (tText as TextBox).tag().id)
                    {
                        InkCanvas.SetLeft(tText, (InkCanvas.GetLeft(tText) + translateX));
                        InkCanvas.SetTop(tText, (InkCanvas.GetTop(tText) + translateY));
                    }
                }

                var transformMatrix = new System.Windows.Media.Matrix();
                transformMatrix.Translate(translateX, translateY);
                foreach (var tStroke in strokeFilter.Strokes)
                {
                    var myRect = tStroke.GetBounds();
                    tStroke.Transform(transformMatrix, false);
                }

                foreach (var tImage in imageFilter.Images)
                {
                    InkCanvas.SetLeft(tImage, (InkCanvas.GetLeft(tImage) + translateX));
                    InkCanvas.SetTop(tImage, (InkCanvas.GetTop(tImage) + translateY));
                }
            }
            doAdjustText(box as TextBox, adjustment);
        }

        public void adjustTextForMoveDelta(String textIdentity, Func<TextBox, TextBox> adjustment)
        {
            var boxes = textFilter.TextBoxes.Where(t => (t as TextBox).tag().id == textIdentity);
            if (boxes.Count() > 0)
            {
                var box = boxes.First();
                if (PossiblyExtendTheNegativeBoundsOfTheCanvasForMoveDelta(InkCanvas.GetLeft(box), InkCanvas.GetTop(box)))
                {
                    var translateX = ReturnPositiveValue(moveDeltaX);
                    var translateY = ReturnPositiveValue(moveDeltaY);
                    foreach (var tText in textFilter.TextBoxes)
                    {
                        InkCanvas.SetLeft(tText, (InkCanvas.GetLeft(tText) + translateX));
                        InkCanvas.SetTop(tText, (InkCanvas.GetTop(tText) + translateY));
                    }
                    foreach (var tImage in imageFilter.Images)
                    {
                        InkCanvas.SetLeft(tImage, (InkCanvas.GetLeft(tImage) + translateX));
                        InkCanvas.SetTop(tImage, (InkCanvas.GetTop(tImage) + translateY));
                    }
                    foreach (var tStroke in strokeFilter.Strokes)
                    {
                        var transformMatrix = new System.Windows.Media.Matrix();
                        transformMatrix.Translate(translateX, translateY);
                        tStroke.Transform(transformMatrix, false);
                    }
                }
                doAdjustText(box as TextBox, adjustment);
            }
        }

        public void adjustTextsForMoveDelta(List<String> textIdentities)
        {
            var boxes = textFilter.TextBoxes.Where(t => textIdentities.Contains((t as TextBox).tag().id));            
            foreach (var box in boxes)
            {
                if (PossiblyExtendTheNegativeBoundsOfTheCanvasForMoveDelta(InkCanvas.GetLeft(box), InkCanvas.GetTop(box)))
                {
                    var translateX = ReturnPositiveValue(moveDeltaX);
                    var translateY = ReturnPositiveValue(moveDeltaY);
                    foreach (var tText in textFilter.TextBoxes)
                    {
                        InkCanvas.SetLeft(tText, (InkCanvas.GetLeft(tText) + translateX));
                        InkCanvas.SetTop(tText, (InkCanvas.GetTop(tText) + translateY));
                    }
                    foreach (var tImage in imageFilter.Images)
                    {
                        InkCanvas.SetLeft(tImage, (InkCanvas.GetLeft(tImage) + translateX));
                        InkCanvas.SetTop(tImage, (InkCanvas.GetTop(tImage) + translateY));
                    }
                    foreach (var tStroke in strokeFilter.Strokes)
                    {
                        var transformMatrix = new System.Windows.Media.Matrix();
                        transformMatrix.Translate(translateX, translateY);
                        tStroke.Transform(transformMatrix, false);
                    }                    
                }
                //doAdjustImage(image as Image, adjustment);
            }
        }

        private void doAdjustText(TextBox box, Func<TextBox, TextBox> adjustment)
        {
            box = adjustment(box);
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
