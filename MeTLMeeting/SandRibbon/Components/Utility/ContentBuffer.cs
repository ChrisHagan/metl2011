using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows;
using System.Windows.Media;
using MeTLLib.Providers.Connection;
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
        private bool checkIfMoveDeltaBoundsUpdates(double x, double y)
        {
            if (x < 0 || y < 0) return true;
            return false;
        }
        private Point generateMoveDeltaBounds(double elementLeft, double elementTop)
        {
            Console.WriteLine("ContentBuffer::generateMoveDeltaBounds::Called");
            var point = new Point(moveDeltaX, moveDeltaY);
            if (elementLeft < 0.0)
                point.X = elementLeft;
            if (elementTop < 0.0)
                point.Y = elementTop;

            return point;
        }
        private void updateMoveDeltaBounds(double x, double y)
        {
            var point = generateMoveDeltaBounds(x, y);
            moveDeltaX = point.X;
            logicalX = logicalX + moveDeltaX;
            moveDeltaY = point.Y;
            logicalY = moveDeltaY + logicalY;
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
        private void updateCanvasPositioning(IEnumerable<Stroke> strokes, IEnumerable<UIElement> textboxes, IEnumerable<UIElement> images, double translateX, double translateY)
        {
            Console.WriteLine("updating positioning");
            Matrix transformMatrix;
            transformMatrix = new System.Windows.Media.Matrix();
            transformMatrix.Translate(translateX, translateY);
            foreach (var tStroke in strokes)
                    tStroke.Transform(transformMatrix, false);
            foreach (var tImage in images)
            {
                InkCanvas.SetLeft(tImage, (InkCanvas.GetLeft(tImage) + translateX));
                InkCanvas.SetTop(tImage, (InkCanvas.GetTop(tImage) + translateY));
            }
            foreach (var tText in textboxes)
            {
                InkCanvas.SetLeft(tText, (InkCanvas.GetLeft(tText) + translateX));
                InkCanvas.SetTop(tText, (InkCanvas.GetTop(tText) + translateY));
            }
        }
        public Stroke adjustStroke(Stroke stroke, Func<Stroke,Stroke> adjustment)
        {
            //var stroke = incomingStroke.Clone();
            var oldCanvasOffsetX = logicalX;
            var oldCanvasOffsetY = logicalY;
            double translateX = 0.0;
            double translateY = 0.0;
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

                updateCanvasPositioning(strokeFilter.Strokes.Where(s => s.tag().id != stroke.tag().id),
                    textFilter.TextBoxes,
                    imageFilter.Images
                    , translateX, 
                    translateY);
            }
            return doAdjustStroke(stroke, adjustment);            
        }
        public void adjustStrokesForMoveDelta(List<String> strokeIdentities)
        {

            var strokes = strokeFilter.Strokes.Where(s => strokeIdentities.Contains(s.tag().id));
            foreach(var stroke in strokes)
            {
                if (checkIfMoveDeltaBoundsUpdates(stroke.GetBounds().X, stroke.GetBounds().Y))
                {
                    updateMoveDeltaBounds(stroke.GetBounds().X, stroke.GetBounds().Y);
                    updateCanvasPositioning( strokeFilter.Strokes, textFilter.TextBoxes, imageFilter.Images, Math.Abs(moveDeltaX), Math.Abs(moveDeltaY));
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
                if (checkIfMoveDeltaBoundsUpdates(stroke.GetBounds().X, stroke.GetBounds().Y))
                {
                    updateMoveDeltaBounds(stroke.GetBounds().X, stroke.GetBounds().Y);
                    updateCanvasPositioning( strokeFilter.Strokes, textFilter.TextBoxes, imageFilter.Images, Math.Abs(moveDeltaX), Math.Abs(moveDeltaY));
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
        public Image adjustImage(Image image, Func<Image, Image> adjustment)
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

                updateCanvasPositioning( strokeFilter.Strokes, textFilter.TextBoxes, 
                    imageFilter.Images.Where(i => ((Image)(i)).tag().id != image.tag().id).Select(i => (UIElement)i), translateX, translateY );
           }
            return doAdjustImage(image, adjustment);
        }

        public void adjustImageForMoveDelta(String imageIdentity, Func<Image, Image> adjustment)
        {
            var images = imageFilter.Images.Where(i => (i as Image).tag().id == imageIdentity);
            if (images.Count() > 0)
            {
                var image = images.First();
                if (checkIfMoveDeltaBoundsUpdates(InkCanvas.GetLeft(image), InkCanvas.GetTop(image)))
                {
                    updateMoveDeltaBounds(InkCanvas.GetLeft(image), InkCanvas.GetTop(image)) ;
                    updateCanvasPositioning(strokeFilter.Strokes, textFilter.TextBoxes, imageFilter.Images,
                                            Math.Abs(moveDeltaX), Math.Abs(moveDeltaY));
                }
                doAdjustImage(image as Image, adjustment);
            }
        }
        public void adjustImagesForMoveDelta(List<String> imageIdentities)
        {
            var images = imageFilter.Images.Where(i => imageIdentities.Contains((i as Image).tag().id));
            foreach(var image in images)            
            {
                if (checkIfMoveDeltaBoundsUpdates(InkCanvas.GetLeft(image), InkCanvas.GetTop(image)))
                {
                    updateMoveDeltaBounds(InkCanvas.GetLeft(image), InkCanvas.GetTop(image));
                    updateCanvasPositioning( strokeFilter.Strokes, textFilter.TextBoxes, imageFilter.Images, Math.Abs(moveDeltaX), Math.Abs(moveDeltaY));
                }
                //doAdjustImage(image as Image, adjustment);
            }
        }
        private Image doAdjustImage(Image image, Func<Image, Image> adjustment)
        {
            return adjustment(image);
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

                updateCanvasPositioning(strokeFilter.Strokes, textFilter.TextBoxes.Where(t => ((TextBox)t).tag().id != box.tag().id), imageFilter.Images,
                                        translateX, translateY);
            }
            doAdjustText(box as TextBox, adjustment);
        }

        public void adjustTextForMoveDelta(String textIdentity, Func<TextBox, TextBox> adjustment)
        {
            var boxes = textFilter.TextBoxes.Where(t => (t as TextBox).tag().id == textIdentity);
            if (boxes.Count() > 0)
            {
                var box = boxes.First();
                if (checkIfMoveDeltaBoundsUpdates(InkCanvas.GetLeft(box), InkCanvas.GetTop(box)))
                {
                    updateMoveDeltaBounds(InkCanvas.GetLeft(box), InkCanvas.GetTop(box));
                    updateCanvasPositioning( strokeFilter.Strokes, textFilter.TextBoxes, imageFilter.Images, Math.Abs(moveDeltaX), Math.Abs(moveDeltaY));
                }
                doAdjustText(box as TextBox, adjustment);
            }
        }

        public void adjustTextsForMoveDelta(List<String> textIdentities)
        {
            var boxes = textFilter.TextBoxes.Where(t => textIdentities.Contains((t as TextBox).tag().id));            
            foreach (var box in boxes)
            {
                if (checkIfMoveDeltaBoundsUpdates(InkCanvas.GetLeft(box), InkCanvas.GetTop(box)))
                {
                    updateMoveDeltaBounds(InkCanvas.GetLeft(box), InkCanvas.GetTop(box));
                    updateCanvasPositioning( strokeFilter.Strokes, textFilter.TextBoxes, imageFilter.Images, Math.Abs(moveDeltaX), Math.Abs(moveDeltaY));
                }
                //doAdjustImage(image as Image, adjustment);
            }
        }

        private void doAdjustText(TextBox box, Func<TextBox, TextBox> adjustment)
        {
            adjustment(box);
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
