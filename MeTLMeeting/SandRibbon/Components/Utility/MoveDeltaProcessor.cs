namespace SandRibbon.Components.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Ink;
    using System.Windows.Media;
    using MeTLLib.DataTypes;
    using System.Collections.ObjectModel;
    using Pages;

    public abstract class MoveDeltaProcessor
    {
        public InkCanvas Canvas { get; private set; }

        public string Target { get; private set; }
        public ConversationState conversationState { get; set; }

        public ContentBuffer contentBuffer { get; private set; }
        public string me { get; protected set; }

        protected MoveDeltaProcessor(InkCanvas canvas, string target, ContentBuffer contentBuffer, ConversationState _details, string _me)
        {
            this.Canvas = canvas;
            this.Target = target;            
            this.me = _me;
            conversationState = _details;
            this.contentBuffer = contentBuffer;
        }

        protected abstract void AddStroke(PrivateAwareStroke stroke);

        protected abstract void RemoveStroke(PrivateAwareStroke stroke);
        protected abstract void RemoveImage(MeTLImage image);
        protected abstract void RemoveText(MeTLTextBox textbox);

        protected abstract void ChangeImagePrivacy(MeTLImage image, Privacy newPrivacy);
        protected abstract void ChangeTextPrivacy(MeTLTextBox textbox, Privacy newPrivacy);

        private List<String> sentDeltas = new List<String>();

        public void rememberSentMoveDelta(TargettedMoveDelta moveDelta)
        {
            sentDeltas.Add(moveDelta.identity);
        }
        public void clearRememberedSentMoveDeltas()
        {
            sentDeltas.Clear();
        }
        public void ReceiveMoveDelta(TargettedMoveDelta moveDelta, string recipient, bool processHistory)
        {
            if (!processHistory && sentDeltas.Contains(moveDelta.identity))
            {
                //sentDeltas.Remove(moveDelta.identity);
                return;
            }

            if (moveDelta.HasSameTarget(Target))
            {
                if (moveDelta.isDeleted)
                {
                    ContentDelete(moveDelta);
                    return;
                }

                if (moveDelta.newPrivacy != Privacy.NotSet)
                {
                    ContentPrivacyChange(moveDelta);
                    return;
                }

                ContentTranslateAndScale(moveDelta);
            }
        }

        private PrivateAwareStroke AdjustVisual(PrivateAwareStroke stroke, double xTranslate, double yTranslate, double xScale, double yScale)
        {
            var newStroke = stroke.Clone(); 
            if (xTranslate == 0.0 && yTranslate == 0.0 && xScale == 1.0 && yScale == 1.0)
                return newStroke;

            //Get bounds of the stroke, translate it to 0, scale it, add the move delta translate to the bounds and tranlate them back
            Rect myRect = new Rect();
            myRect = newStroke.GetBounds();

            var transformMatrix = new Matrix();
            if (xScale != 1.0 || yScale != 1.0)
            {   
                transformMatrix.Translate(-myRect.X, -myRect.Y);

                newStroke.Transform(transformMatrix, false);

                transformMatrix = new Matrix();
                transformMatrix.Scale(xScale,yScale);
                
                if(double.IsNaN(xTranslate))
                {
                    xTranslate = 0;
                }
                if (double.IsNaN(yTranslate))
                {
                    yTranslate = 0;
                }
                xTranslate = xTranslate + myRect.X;
                yTranslate = yTranslate + myRect.Y;
            }
            
            transformMatrix.Translate(xTranslate, yTranslate);
            newStroke.Transform(transformMatrix, false);
            return newStroke;
        }

        protected void ContentTranslateAndScale(TargettedMoveDelta moveDelta)
        {
            var xTrans = moveDelta.xTranslate;
            var yTrans = moveDelta.yTranslate;
            var xScale = moveDelta.xScale;
            var yScale = moveDelta.yScale;

            var totalBounds = Double.IsNaN(moveDelta.yOrigin) || Double.IsNaN(moveDelta.xOrigin) ? contentBuffer.getBoundsOfMoveDelta(moveDelta) : new Rect(moveDelta.xOrigin, moveDelta.yOrigin, 0.0, 0.0);
            var tbX = totalBounds.Left - contentBuffer.logicalX;
            var tbY = totalBounds.Top - contentBuffer.logicalY;
            //var totalBounds = contentBuffer.getBoundsOfMoveDelta(moveDelta);

            foreach (var inkId in moveDelta.inkIds)
            {
                contentBuffer.adjustStrokeForMoveDelta(inkId, (s) =>
                {
                    if (dirtiesThis(moveDelta,s)){

                        var sBounds = s.GetBounds();
                        var internalX = sBounds.Left - tbX;
                        var internalY = sBounds.Top - tbY;
                        var offsetX = -(internalX - (internalX * xScale));
                        var offsetY = -(internalY - (internalY * yScale));

                        var adjustedStroke = AdjustVisual(s, xTrans + offsetX, yTrans + offsetY, xScale, yScale);
                        RemoveStroke(s);
                        AddStroke(adjustedStroke);

                        return adjustedStroke;
                    }
                    return s;
                });
            }

            foreach (var textId in moveDelta.textIds)
            {
                contentBuffer.adjustTextForMoveDelta(textId, (t) =>
                {
                    if (dirtiesThis(moveDelta, t))
                    {
                        TranslateAndScale(t, xTrans, yTrans, xScale, yScale, totalBounds.Left,totalBounds.Top);//tbX,tbY);
                    }
                    return t;
                });                
            }

            foreach (var imageId in moveDelta.imageIds)
            {
                contentBuffer.adjustImageForMoveDelta(imageId, (i) =>
                {
                    if (dirtiesThis(moveDelta, i))
                    {
                        TranslateAndScale(i, xTrans, yTrans, xScale, yScale, totalBounds.Left,totalBounds.Top);//tbX,tbY);
                    }
                    return i;
                });
            }
        }

        private void TranslateAndScale(FrameworkElement element, double xTrans, double yTrans, double xScale, double yScale, double tbX, double tbY)
        {
            var myLeft = 0.0;
            if (element is MeTLTextBox){
                myLeft = InkCanvas.GetLeft(element) + (element as MeTLTextBox).offsetX;
            } else if (element is MeTLImage){
                myLeft = InkCanvas.GetLeft(element) + (element as MeTLImage).offsetX;
            }
            var myTop = 0.0;
            if (element is MeTLTextBox){
                myTop = InkCanvas.GetTop(element) + (element as MeTLTextBox).offsetY;
            } else if (element is MeTLImage){
                myTop = InkCanvas.GetTop(element) + (element as MeTLImage).offsetY;
            }
            myLeft -= tbX;
            myTop -= tbY;

            var left = InkCanvas.GetLeft(element) + xTrans;
            var top = InkCanvas.GetTop(element) + yTrans;

            var leftCorrection = -(myLeft - (myLeft * xScale));
            var topCorrection = -(myTop - (myTop * yScale));

            InkCanvas.SetLeft(element, left + leftCorrection);
            InkCanvas.SetTop(element, top + topCorrection);

            /* 
            // buggy

            // special case for images
            if (element is Image && !element.IsLoaded)
            {
                // don't like this at all... but how else are we going to scale an image that's still being retrieved over the network but we have the move delta now?
                ScaleImageAfterLoad(element, xScale, yScale);
                return;
            }
            */

            CorrectWidthAndHeight(element);

            element.Width *= xScale;
            element.Height *= yScale;
        }

        private void ScaleImageAfterLoad(FrameworkElement image, double xScale, double yScale)
        {
            RoutedEventHandler scaler = null; 
            scaler = (sender, args) =>
            {
                image.Loaded -= scaler;
                CorrectWidthAndHeight(image);

                image.Width *= xScale;
                image.Height *= yScale;
            };

            image.Loaded += scaler;
        }

        private void CorrectWidthAndHeight(FrameworkElement element)
        {
            if (double.IsNaN(element.Width) || double.IsNaN(element.Height))
            {
                // if we're trying to change the element's width and height before a measure pass the actual* aren't going to help
                if (element.IsMeasureValid)
                {
                    element.Height = element.ActualHeight;
                    element.Width = element.ActualWidth;
                }
            }
        }

        private bool dirtiesThis(TargettedMoveDelta moveDelta, Stroke elem)
        {
            return moveDelta.inkIds.Any(i => elem.tag().id == i && elem.tag().privacy == moveDelta.privacy && elem.tag().timestamp < moveDelta.timestamp);
        }

        private bool dirtiesThis(TargettedMoveDelta moveDelta, MeTLImage elem)
        {
            return moveDelta.imageIds.Any(i => elem.tag().id == i && elem.tag().privacy == moveDelta.privacy && elem.tag().timestamp < moveDelta.timestamp);
        }

        private bool dirtiesThis(TargettedMoveDelta moveDelta, MeTLTextBox elem)
        {
            return moveDelta.textIds.Any(i => elem.tag().id == i && elem.tag().privacy == moveDelta.privacy && elem.tag().timestamp < moveDelta.timestamp);
        }

        protected void ContentDelete(TargettedMoveDelta moveDelta)
        {
            var deadStrokes = new List<PrivateAwareStroke>();
            var deadTextboxes = new List<MeTLTextBox>();
            var deadImages = new List<MeTLImage>();

            foreach (var inkId in moveDelta.inkIds)
            {
                foreach (var stroke in Canvas.Strokes.Where((s) => s is PrivateAwareStroke && s.tag().id == inkId.Identity).Select(s => s as PrivateAwareStroke))                
                {
                    if(dirtiesThis(moveDelta,stroke))
                    {
                        deadStrokes.Add(stroke);        
                    }                    
                }
            }

            foreach (var textId in moveDelta.textIds)
            {
                foreach (var textBox in Canvas.TextChildren().Where((t) => t.tag().id == textId.Identity))
                {
                    if(dirtiesThis(moveDelta,textBox))
                    {
                        deadTextboxes.Add(textBox);
                    }
                }
            }

            foreach (var imageId in moveDelta.imageIds)
            {
                foreach (var image in Canvas.ImageChildren().Where((i) => i.tag().id == imageId.Identity))
                {
                    if(dirtiesThis(moveDelta,image))
                    {
                        deadImages.Add(image);
                    }

                }
            }

            // improve the contentbuffer to remove items either:
            // - by using list.RemoveAll(predicate)
            // - iterating backwards and removing the item at the index
            // - find all the items to be removed then list.Except(listDeletions) on the list
            foreach (var text in deadTextboxes)
            {
                RemoveText(text);
            }

            foreach (var image in deadImages)
            {
                RemoveImage(image);
            }

            foreach (var stroke in deadStrokes)
            {
                RemoveStroke(stroke);
            }
        }

        protected void ContentPrivacyChange(TargettedMoveDelta moveDelta)
        {
            var privacyStrokes = new List<PrivateAwareStroke>();
            var privacyTextboxes = new List<MeTLTextBox>();
            var privacyImages = new List<MeTLImage>();

            foreach (var inkId in moveDelta.inkIds)
            {
                foreach (var stroke in Canvas.Strokes.Where((s) => s is PrivateAwareStroke && s.tag().id == inkId.Identity).Select(s => s as PrivateAwareStroke))
                {
                    if (dirtiesThis(moveDelta, stroke))
                    {
                        privacyStrokes.Add(stroke);
                    }
                }
            }

            foreach (var textId in moveDelta.textIds)
            {
                foreach (var textBox in Canvas.TextChildren().Where((t) => t.tag().id == textId.Identity))
                {
                    if (dirtiesThis(moveDelta, textBox))
                    {
                        privacyTextboxes.Add(textBox);
                    }
                }
            }

            foreach (var imageId in moveDelta.imageIds)
            {
                foreach (var image in Canvas.ImageChildren().Where((i) => i.tag().id == imageId.Identity))
                {
                    if (dirtiesThis(moveDelta, image))
                    {
                        privacyImages.Add(image);
                    }
                }
            }

            foreach (var stroke in privacyStrokes)
            {
                var oldTag = stroke.tag();
//                RemoveStroke(stroke);
                stroke.tag(new StrokeTag(oldTag, moveDelta.privacy));
//                AddStroke(stroke);
            }

            foreach (var image in privacyImages)
            {
                var oldTag = image.tag();

                image.tag(new ImageTag(oldTag, moveDelta.privacy));
                ChangeImagePrivacy(image, moveDelta.privacy);
            }

            foreach (var text in privacyTextboxes)
            {
                var oldTag = text.tag();

                text.tag(new TextTag(oldTag, moveDelta.privacy));
                ChangeTextPrivacy(text, moveDelta.privacy);
            }
        }
    }
}
