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

    public abstract class MoveDeltaProcessor
    {
        public InkCanvas Canvas { get; private set; }

        public string Target { get; private set; }

        public ContentBuffer contentBuffer { get; private set; }

        protected MoveDeltaProcessor(InkCanvas canvas, string target, ContentBuffer contentBuffer)
        {
            this.Canvas = canvas;
            this.Target = target;
            this.contentBuffer = contentBuffer;
        }

        protected abstract void AddStroke(PrivateAwareStroke stroke);

        protected abstract void RemoveStroke(PrivateAwareStroke stroke);
        protected abstract void RemoveImage(MeTLImage image);
        protected abstract void RemoveText(TextBox textbox);

        protected abstract void ChangeImagePrivacy(MeTLImage image, Privacy newPrivacy);
        protected abstract void ChangeTextPrivacy(TextBox textbox, Privacy newPrivacy);

        private List<String> sentDeltas = new List<String>();

        public void rememberSentMoveDelta(TargettedMoveDelta moveDelta)
        {
            sentDeltas.Add(moveDelta.identity);
        }

        public void ReceiveMoveDelta(TargettedMoveDelta moveDelta, string recipient, bool processHistory)
        {
            if (!processHistory && sentDeltas.Contains(moveDelta.identity)){
            //if (!processHistory && moveDelta.HasSameAuthor(recipient))
            //{
                sentDeltas.Remove(moveDelta.identity);
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

        protected void ContentTranslateAndScale(TargettedMoveDelta moveDelta)
        {
            var xTrans = moveDelta.xTranslate;
            var yTrans = moveDelta.yTranslate;
            var xScale = moveDelta.xScale;
            var yScale = moveDelta.yScale;

            var totalBounds = contentBuffer.getBoundsOfMoveDelta(moveDelta);

            foreach (var inkId in moveDelta.inkIds)
            {
                contentBuffer.adjustStrokeForMoveDelta(inkId, (s) =>
                {
                    if (dirtiesThis(moveDelta,s)){
                        var sBounds = s.GetBounds();
                        var myLeft = sBounds.Left + s.offsetX;
                        var myTop = sBounds.Top + s.offsetY;
                        myLeft -= totalBounds.Left;
                        myTop -= totalBounds.Top;
                        var leftCorrection = -(myLeft - (myLeft * xScale));
                        var topCorrection = -(myTop - (myTop * yScale));

                      var scaleMatrix = new Matrix();
                      var translateMatrix = new Matrix();
                      scaleMatrix.Scale(xScale, yScale);
                      translateMatrix.Translate(xTrans + leftCorrection, yTrans + topCorrection);
                      s.Transform(scaleMatrix,false);
                      s.Transform(translateMatrix,false);
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
                        TranslateAndScale(t, xTrans, yTrans, xScale, yScale, totalBounds);
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
                        TranslateAndScale(i, xTrans, yTrans, xScale, yScale, totalBounds);
                    }
                    return i;
                });
            }
        }

        private void TranslateAndScale(FrameworkElement element, double xTrans, double yTrans, double xScale, double yScale, Rect totalBounds)
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
            myTop -= totalBounds.Top;
            myLeft -= totalBounds.Left;

            var left = InkCanvas.GetLeft(element) + xTrans;
            var top = InkCanvas.GetTop(element) + yTrans;

            var topCorrection = -(myTop - (myTop * yScale));
            var leftCorrection = -(myLeft - (myLeft * xScale));

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

        private bool dirtiesThis(TargettedMoveDelta moveDelta, TextBox elem)
        {
            return moveDelta.textIds.Any(i => elem.tag().id == i && elem.tag().privacy == moveDelta.privacy && elem.tag().timestamp < moveDelta.timestamp);
        }

        protected void ContentDelete(TargettedMoveDelta moveDelta)
        {
            var deadStrokes = new List<PrivateAwareStroke>();
            var deadTextboxes = new List<TextBox>();
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
            var privacyTextboxes = new List<TextBox>();
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
