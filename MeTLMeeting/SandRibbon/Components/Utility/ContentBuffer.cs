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
using SandRibbon.Pages;
using SandRibbon.Pages.Collaboration.Models;

namespace SandRibbon.Components.Utility
{
    public class ContentBuffer
    {
        public DataContextRoot rootPage { get; protected set; }
        private StrokeFilter strokeFilter;
        private ImageFilter imageFilter;
        private TextFilter textFilter;

        public StrokeFilter strokes
        {
            get
            {
                return strokeFilter;
            }
        }
        public ImageFilter images
        {
            get
            {
                return imageFilter;
            }
        }
        public TextFilter texts
        {
            get
            {
                return textFilter;
            }
        }

        // used to create a snapshot for undo/redo
        private ImageFilter imageDeltaCollection;
        private StrokeFilter strokeDeltaFilter;
        
        public ContentBuffer(DataContextRoot rootPage)
        {
            this.rootPage = rootPage;
            strokeFilter = new StrokeFilter(rootPage);
            imageFilter = new ImageFilter(rootPage);
            textFilter = new TextFilter(rootPage);

            strokeDeltaFilter = new StrokeFilter(rootPage);
            imageDeltaCollection = new ImageFilter(rootPage);
        }

        public List<PrivateAwareStroke> FilteredStrokes(List<ContentVisibilityDefinition> contentVisibility)
        {
            return strokeFilter.FilterContent(strokeFilter.Strokes, contentVisibility);
        }

        public IEnumerable<UIElement> FilteredTextBoxes(List<ContentVisibilityDefinition> contentVisibility)
        {
            return textFilter.FilteredContent(contentVisibility);
        }

        public IEnumerable<UIElement> FilteredImages(List<ContentVisibilityDefinition> contentVisibility)
        {
            return imageFilter.FilteredContent(contentVisibility);
        }
        public void UpdateChild(UIElement childToFind, Action<UIElement> updateChild)
        {
            if (childToFind is MeTLTextBox)
            {
                adjustText((MeTLTextBox)childToFind, i => i);
                textFilter.UpdateChild(childToFind, updateChild);
            }
            if (childToFind is MeTLImage)
            {
                adjustImage((MeTLImage)childToFind, i => i);
                imageFilter.UpdateChild(childToFind, updateChild);
            }
        }
        public void UpdateAllStrokes(Action<PrivateAwareStroke> updateChild)
        {
            foreach (var stroke in strokeFilter.Strokes)
                if (stroke is PrivateAwareStroke)
                {
                    adjustStroke(stroke, s => s);
                    updateChild(stroke as PrivateAwareStroke);
                }
        }
        public void UpdateAllTextBoxes(Action<MeTLTextBox> updateChild)
        {
            textFilter.TextBoxes.ForEach(t => adjustText((MeTLTextBox)t, tb => tb));
            textFilter.UpdateChildren(updateChild);
        }
        public void UpdateAllImages(Action<MeTLImage> updateChild)
        {
            imageFilter.Images.ForEach(i => adjustImage((MeTLImage)i, im => im));
            imageFilter.UpdateChildren(updateChild);
        }

        public void AdjustContent()
        {
            textFilter.TextBoxes.ForEach(t => adjustText((MeTLTextBox)t, tb => tb));
            strokeFilter.Strokes.ForEach(s => adjustStroke(s, st => st));
            imageFilter.Images.ForEach(i => adjustImage((MeTLImage)i, im => im));            
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
            return x < 0 || y < 0;
        }
        private Point generateMoveDeltaBounds(double elementLeft, double elementTop)
        {
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
            return x < 0 || y < 0;
        }
        private Point generateLogicalBounds(double elementLeft, double elementTop)
        {
            var point = new Point(logicalX, logicalY);
            if (elementLeft < 0)
                point.X += elementLeft;
            if (elementTop < 0)
                point.Y += elementTop;
            return point;
        }
        public void AddStrokes(List<PrivateAwareStroke> strokes, Action<List<PrivateAwareStroke>> modifyVisibleContainer)
        {
            strokes.ForEach(s => adjustStroke(s, st => st));
            strokeFilter.Add(strokes, modifyVisibleContainer);
        }

        public void AddStroke(PrivateAwareStroke stroke, Action<PrivateAwareStroke> modifyVisibleContainer)
        {
            adjustStroke(stroke, s => s);
            strokeFilter.Add(stroke, modifyVisibleContainer);
        }

        public void RemoveStroke(PrivateAwareStroke stroke, Action<PrivateAwareStroke> modifyVisibleContainer)
        {
            strokeFilter.Remove(stroke, modifyVisibleContainer);
        }

        public void RemoveStrokes(List<PrivateAwareStroke> strokes, Action<List<PrivateAwareStroke>> modifyVisibleContainer)
        {
            strokeFilter.Remove(strokes, modifyVisibleContainer);
        }
        private void updateCanvasPositioning(IEnumerable<PrivateAwareStroke> strokes, IEnumerable<UIElement> textboxes, IEnumerable<UIElement> images, double translateX, double translateY)
        {
            foreach (var tStroke in strokes)
                reassociateStrokeToCanvas(tStroke);
            foreach (var tImage in images)
                reassociateImageToCanvas((MeTLImage)tImage);
            foreach (var tText in textboxes)
                reassociateTextboxToCanvas((MeTLTextBox)tText);
        }
        public PrivateAwareStroke adjustStroke(PrivateAwareStroke stroke, Func<PrivateAwareStroke, PrivateAwareStroke> adjustment)
        {
            reassociateStrokeToCanvas(stroke);
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
                reassociateStrokeToCanvas(stroke);
            }
            return doAdjustStroke(stroke, adjustment);
        }
        public void adjustStrokeForMoveDelta(String strokeIdentity, Func<PrivateAwareStroke, PrivateAwareStroke> adjustment)
        {
            var strokes = strokeFilter.Strokes.Where(s => s is PrivateAwareStroke && s.tag().id == strokeIdentity).Select(s => s as PrivateAwareStroke);
            if (strokes.Count() > 0 && strokes.First() != null)
            {
                adjustStroke(strokes.First(), adjustment);
            }
            /*
            var strokes = strokeFilter.Strokes.Where(s => s is PrivateAwareStroke && s.tag().id == strokeIdentity).Select(s => s as PrivateAwareStroke);
            if (strokes.Count() > 0)
            {
                var stroke = strokes.First();
                if (checkIfMoveDeltaBoundsUpdates(stroke.GetBounds().X, stroke.GetBounds().Y))
                {
                    updateMoveDeltaBounds(stroke.GetBounds().X, stroke.GetBounds().Y);
                    updateCanvasPositioning( strokeFilter.Strokes.Where(s => s is PrivateAwareStroke).Select(s => s as PrivateAwareStroke), textFilter.TextBoxes, imageFilter.Images, Math.Abs(moveDeltaX), Math.Abs(moveDeltaY));
                }
                doAdjustStroke(stroke, adjustment);
            }
             */
        }
        private PrivateAwareStroke doAdjustStroke(PrivateAwareStroke stroke, Func<PrivateAwareStroke, PrivateAwareStroke> adjustment)
        {
            return adjustment(stroke);
        }

        public void AddDeltaStrokes(List<PrivateAwareStroke> strokes, Action<List<PrivateAwareStroke>> modifyUndoContainer)
        {
            strokeDeltaFilter.Add(strokes, modifyUndoContainer);
        }
        public Rect getBoundsOfMoveDelta(TargettedMoveDelta moveDelta)
        {
            var relevantImages = imageFilter.Images.Where(i => i is MeTLImage && moveDelta.imageIds.Select(id => id.Identity).Contains(((MeTLImage)i).tag().id)).Select(i => i as MeTLImage).ToList();
            var relevantTexts = textFilter.TextBoxes.Where(t => t is MeTLTextBox && moveDelta.textIds.Select(id => id.Identity).Contains(((MeTLTextBox)t).tag().id)).Select(t => t as MeTLTextBox).ToList();
            var relevantStrokes = strokeFilter.Strokes.Where(s => moveDelta.inkIds.Select(id => id.Identity).Contains((s).tag().id)).ToList();
            return getLogicalBoundsOfContent(relevantImages, relevantTexts, relevantStrokes);
        }

        public static Rect getBoundsOfContent(IEnumerable<UIElement> images, IEnumerable<UIElement> texts, IEnumerable<PrivateAwareStroke> strokes)
        {
            return getBoundsOfContent(images.OfType<MeTLImage>(), texts.OfType<MeTLTextBox>(), strokes);
        }
        public static Rect getBoundsOfContent(IEnumerable<MeTLImage> images, IEnumerable<MeTLTextBox> texts, IEnumerable<PrivateAwareStroke> strokes)
        {
            var top = 0.0;
            var bottom = 0.0;
            var left = 0.0;
            var right = 0.0;
            bool firstItem = true;
            Func<double, double, double, double, bool> updateRect = (t, l, h, w) =>
            {
                var b = t + h;
                var r = l + w;
                bool changed = false;
                if (firstItem)
                {
                    top = t;
                    bottom = b;
                    left = l;
                    right = r;
                    firstItem = false;
                    changed = true;
                }
                else
                {
                    if (t < top)
                    {
                        top = t;
                        changed = true;
                    }
                    if (b > bottom)
                    {
                        bottom = b;
                        changed = true;
                    }
                    if (l < left)
                    {
                        left = l;
                        changed = true;
                    }
                    if (r > right)
                    {
                        right = r;
                        changed = true;
                    }
                }
                return changed;
            };
            foreach (var i in images)
                updateRect(InkCanvas.GetTop(i), InkCanvas.GetLeft(i), i.Height, i.Width);
            foreach (var t in texts)
                updateRect(InkCanvas.GetTop(t), InkCanvas.GetLeft(t), t.Height, t.Width);
            foreach (var s in strokes)
            {
                var sBounds = s.GetBounds();
                updateRect(sBounds.Top, sBounds.Left, sBounds.Height, sBounds.Width);
            }
            return new Rect(new Point(left, top), new Size(right - left, bottom - top));
        }
        public static Rect getLogicalBoundsOfContent(IEnumerable<UIElement> images, IEnumerable<UIElement> texts, IEnumerable<PrivateAwareStroke> strokes)
        {
            var coImages = images.OfType<MeTLImage>();
            var coTexts = texts.OfType<MeTLTextBox>();
            return getLogicalBoundsOfContent(coImages, coTexts, strokes);
        }
        public static Rect getLogicalBoundsOfContent(IEnumerable<MeTLImage> images, IEnumerable<MeTLTextBox> texts, IEnumerable<PrivateAwareStroke> strokes)
        {
            var top = 0.0;
            var bottom = 0.0;
            var left = 0.0;
            var right = 0.0;
            bool firstItem = true;
            Func<double, double, double, double, bool> updateRect = (t, l, h, w) =>
            {
                var b = t + h;
                var r = l + w;
                bool changed = false;
                if (firstItem)
                {
                    top = t;
                    bottom = b;
                    left = l;
                    right = r;
                    firstItem = false;
                    changed = true;
                }
                else
                {
                    if (t < top)
                    {
                        top = t;
                        changed = true;
                    }
                    if (b > bottom)
                    {
                        bottom = b;
                        changed = true;
                    }
                    if (l < left)
                    {
                        left = l;
                        changed = true;
                    }
                    if (r > right)
                    {
                        right = r;
                        changed = true;
                    }
                }
                return changed;
            };
            foreach (var i in images)
                updateRect(InkCanvas.GetTop(i) + i.offsetY, InkCanvas.GetLeft(i) + i.offsetX, i.Height, i.Width);
            foreach (var t in texts)
                updateRect(InkCanvas.GetTop(t) + t.offsetY, InkCanvas.GetLeft(t) + t.offsetX, t.Height, t.Width);
            foreach (var s in strokes)
            {
                var sBounds = s.GetBounds();
                updateRect(sBounds.Top + s.offsetY, sBounds.Left + s.offsetX, sBounds.Height, sBounds.Width);
            }
            return new Rect(new Point(left, top), new Size(right - left, bottom - top));
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
        public MeTLImage adjustImage(MeTLImage image, Func<MeTLImage, MeTLImage> adjustment)
        {
            reassociateImageToCanvas(image);
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

                updateCanvasPositioning(strokeFilter.Strokes, textFilter.TextBoxes,
                    imageFilter.Images.Where(i => ((MeTLImage)(i)).tag().id != image.tag().id).Select(i => (UIElement)i), translateX, translateY);
                reassociateImageToCanvas(image);
            }
            return doAdjustImage(image, adjustment);
        }

        public void adjustImageForMoveDelta(String imageIdentity, Func<MeTLImage, MeTLImage> adjustment)
        {
            var images = imageFilter.Images.Where(i => (i as MeTLImage).tag().id == imageIdentity).Select(i => i as MeTLImage);
            if (images.Count() > 0 && images.First() != null)
            {
                adjustImage(images.First(), adjustment);
            }
            /*
            if (images.Count() > 0)
            {
                var image = images.First();
                if (checkIfMoveDeltaBoundsUpdates(InkCanvas.GetLeft(image), InkCanvas.GetTop(image)))
                {
                    updateMoveDeltaBounds(InkCanvas.GetLeft(image), InkCanvas.GetTop(image)) ;
                    updateCanvasPositioning(strokeFilter.Strokes, textFilter.TextBoxes, imageFilter.Images,
                                            Math.Abs(moveDeltaX), Math.Abs(moveDeltaY));
                }
                doAdjustImage(image as MeTLImage, adjustment);
            }
             */
        }
        public void adjustImagesForMoveDelta(List<String> imageIdentities)
        {
            var images = imageFilter.Images.Where(i => imageIdentities.Contains((i as MeTLImage).tag().id));
            foreach (var image in images)
            {
                if (checkIfMoveDeltaBoundsUpdates(InkCanvas.GetLeft(image), InkCanvas.GetTop(image)))
                {
                    updateMoveDeltaBounds(InkCanvas.GetLeft(image), InkCanvas.GetTop(image));
                    updateCanvasPositioning(strokeFilter.Strokes, textFilter.TextBoxes, imageFilter.Images, Math.Abs(moveDeltaX), Math.Abs(moveDeltaY));
                }
                //doAdjustImage(image as Image, adjustment);
            }
        }
        private MeTLImage doAdjustImage(MeTLImage image, Func<MeTLImage, MeTLImage> adjustment)
        {
            return adjustment(image);
        }
        public void AddImage(UIElement element, Action<UIElement> modifyVisibleContainer)
        {
            if (element is MeTLImage)
            {
                var elem = element as MeTLImage;
                //Debug.Assert((element as MeTLImage) != null);
                adjustImage(elem, i => i);
                imageFilter.Add(elem, modifyVisibleContainer);
            }
        }

        public void RemoveImage(UIElement element, Action<UIElement> modifyVisibleContainer)
        {
            if (element is MeTLImage)
            {
                // Debug.Assert((element as MeTLImage) != null);
                imageFilter.Remove(element as MeTLImage, modifyVisibleContainer);
            }
        }

        public void adjustText(MeTLTextBox box, Func<MeTLTextBox, MeTLTextBox> adjustment)
        {
            reassociateTextboxToCanvas(box);
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

                updateCanvasPositioning(strokeFilter.Strokes, textFilter.TextBoxes.Where(t => ((MeTLTextBox)t).tag().id != box.tag().id), imageFilter.Images,
                                        translateX, translateY);
                reassociateTextboxToCanvas(box);
            }
            doAdjustText(box, adjustment);
        }



        public void adjustTextForMoveDelta(String textIdentity, Func<MeTLTextBox, MeTLTextBox> adjustment)
        {
            var boxes = textFilter.TextBoxes.Where(t => (t as MeTLTextBox).tag().id == textIdentity).Select(t => t as MeTLTextBox);
            if (boxes.Count() > 0 && boxes.First() != null)
            {
                adjustText(boxes.First(), adjustment);
            }
            /*
            if (boxes.Count() > 0)
            {
                var box = boxes.First() as MeTLTextBox;
                if (checkIfMoveDeltaBoundsUpdates(InkCanvas.GetLeft(box), InkCanvas.GetTop(box)))
                {
                    updateMoveDeltaBounds(InkCanvas.GetLeft(box), InkCanvas.GetTop(box));
                    updateCanvasPositioning( strokeFilter.Strokes, textFilter.TextBoxes, imageFilter.Images, Math.Abs(moveDeltaX), Math.Abs(moveDeltaY));
                }
                doAdjustText(box, adjustment);
            }
             */
        }

        public void adjustTextsForMoveDelta(List<String> textIdentities)
        {
            var boxes = textFilter.TextBoxes.Where(t => textIdentities.Contains((t as MeTLTextBox).tag().id));
            foreach (var box in boxes)
            {
                if (checkIfMoveDeltaBoundsUpdates(InkCanvas.GetLeft(box), InkCanvas.GetTop(box)))
                {
                    updateMoveDeltaBounds(InkCanvas.GetLeft(box), InkCanvas.GetTop(box));
                    updateCanvasPositioning(strokeFilter.Strokes, textFilter.TextBoxes, imageFilter.Images, Math.Abs(moveDeltaX), Math.Abs(moveDeltaY));
                }
                //doAdjustImage(image as Image, adjustment);
            }
        }

        private void doAdjustText(MeTLTextBox box, Func<MeTLTextBox, MeTLTextBox> adjustment)
        {
            adjustment(box);
        }

        public void AddTextBox(UIElement element, Action<UIElement> modifyVisibleContainer)
        {
            if (element is MeTLTextBox)
            {
                //Debug.Assert((element as MeTLTextBox) != null);
                var elem = element as MeTLTextBox;
                adjustText(elem, t => t);
                textFilter.Push(elem, modifyVisibleContainer);
            }
        }

        public void RemoveTextBox(UIElement element, Action<UIElement> modifyVisibleContainer)
        {
            if (element is MeTLTextBox)
            {
                //   Debug.Assert((element as MeTLTextBox) != null);
                textFilter.Remove(element as MeTLTextBox, modifyVisibleContainer);
            }
        }
        private PrivateAwareStroke reassociateStrokeToCanvas(PrivateAwareStroke stroke)
        {
            var diffX = logicalX - stroke.offsetX;
            var diffY = logicalY - stroke.offsetY;
            if (diffX != 0 || diffY != 0)
            {
                var m = new Matrix();
                m.Translate(-diffX, -diffY);
                stroke.Transform(m, false);
                stroke.offsetX = logicalX;
                stroke.offsetY = logicalY;
            }
            return stroke;
        }
        private MeTLTextBox reassociateTextboxToCanvas(MeTLTextBox text)
        {
            var diffX = logicalX - text.offsetX;
            var diffY = logicalY - text.offsetY;
            if (diffX != 0 || diffY != 0)
            {
                InkCanvas.SetLeft(text, InkCanvas.GetLeft(text) - diffX);
                InkCanvas.SetTop(text, InkCanvas.GetTop(text) - diffY);
                text.offsetX = logicalX;
                text.offsetY = logicalY;
            }
            return text;
        }
        private MeTLImage reassociateImageToCanvas(MeTLImage image)
        {
            var diffX = logicalX - image.offsetX;
            var diffY = logicalY - image.offsetY;
            if (diffX != 0 || diffY != 0)
            {
                InkCanvas.SetLeft(image, InkCanvas.GetLeft(image) - diffX);
                InkCanvas.SetTop(image, InkCanvas.GetTop(image) - diffY);
                image.offsetX = logicalX;
                image.offsetY = logicalY;
            }
            return image;
        }
    }
}
