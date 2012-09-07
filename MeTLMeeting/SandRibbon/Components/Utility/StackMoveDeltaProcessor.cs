namespace SandRibbon.Components.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using MeTLLib.DataTypes;
    using SandRibbon.Providers;
    using System.Windows.Media;
    using System.Windows.Controls;
    using System.Windows.Ink;

    public class StackMoveDeltaProcessor : MoveDeltaProcessor
    {
        private ContentBuffer ContentBuffer { get; set; }

        public StackMoveDeltaProcessor(InkCanvas canvas, ContentBuffer contentBuffer, string target) : base(canvas, target)
        {
            ContentBuffer = contentBuffer;
        }

        public override void ReceiveMoveDelta(TargettedMoveDelta moveDelta, string recipient, bool processHistory)
        {
            if (!processHistory && moveDelta.HasSameAuthor(recipient)) 
                return;

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

                // define work to be done based on fields
                var xTrans = moveDelta.xTranslate;
                var yTrans = moveDelta.yTranslate;
                var xScale = moveDelta.xScale;
                var yScale = moveDelta.yScale;

                var transformMatrix = new Matrix();
                transformMatrix.Scale(xScale, yScale);
                transformMatrix.Translate(xTrans, yTrans);

                foreach (var inkId in moveDelta.inkIds)
                {
                    var deadStrokes = new List<Stroke>();
                    foreach (var stroke in Canvas.Strokes.Where((s) => s.tag().id == inkId.Identity))
                    {
                        stroke.Transform(transformMatrix, false);
                    }
                }

                foreach (var textId in moveDelta.textIds)
                {
                    foreach (var textBox in Canvas.TextChildren().Where((t) => t.tag().id == textId.Identity))
                    {
                        // translate
                        var left = InkCanvas.GetLeft(textBox) + xTrans;
                        var top = InkCanvas.GetTop(textBox) + yTrans;

                        Console.WriteLine(String.Format("me[{3}] Moving text {0} to x[{1}] y[{2}]", textBox.Text, left, top, Globals.me));

                        InkCanvas.SetLeft(textBox, left);
                        InkCanvas.SetTop(textBox, top);

                        // scale
                        /*if (double.IsNaN(textBox.Width) || double.IsNaN(textBox.Height))
                        {
                            textBox.Width = textBox.ActualWidth;
                            textBox.Height = textBox.ActualHeight;
                        }*/
                        textBox.Width *= xScale;
                        textBox.Height *= yScale;
                    }
                }

                foreach (var imageId in moveDelta.imageIds)
                {
                    foreach (var image in Canvas.ImageChildren().Where((i) => i.tag().id == imageId.Identity))
                    {
                        var left = InkCanvas.GetLeft(image) + xTrans;
                        var top = InkCanvas.GetTop(image) + yTrans;

                        InkCanvas.SetLeft(image, left);
                        InkCanvas.SetTop(image, top);

                        image.Width *= xScale;
                        image.Height *= yScale;
                    }
                }
            }
        }

        protected override void ContentDelete(TargettedMoveDelta moveDelta) 
        {
            var deadStrokes = new List<Stroke>();
            var deadTextboxes = new List<TextBox>();
            var deadImages = new List<Image>();

            foreach (var inkId in moveDelta.inkIds)
            {
                foreach (var stroke in Canvas.Strokes.Where((s) => s.tag().id == inkId.Identity))
                {
                    deadStrokes.Add(stroke);
                }
            }

            foreach (var textId in moveDelta.textIds)
            {
                foreach (var textBox in Canvas.TextChildren().Where((t) => t.tag().id == textId.Identity))
                {
                    deadTextboxes.Add(textBox);
                }
            }

            foreach (var imageId in moveDelta.imageIds)
            {
                foreach (var image in Canvas.ImageChildren().Where((i) => i.tag().id == imageId.Identity))
                {
                    deadImages.Add(image);
                }
            }

            // improve the contentbuffer to remove items either:
            // - by using list.RemoveAll(predicate)
            // - iterating backwards and removing the item at the index
            // - find all the items to be removed then list.Except(listDeletions) on the list
            foreach (var text in deadTextboxes)
            {
                ContentBuffer.RemoveTextBox(text, (tb) => Canvas.Children.Remove(tb));
            }

            foreach (var image in deadImages)
            {
                ContentBuffer.RemoveImage(image, (img) => Canvas.Children.Remove(img));
            }

            foreach (var stroke in deadStrokes)
            {
                ContentBuffer.RemoveStroke(stroke, (col) => Canvas.Strokes.Remove(col));
                // hopefully don't need to keep track of checksums anymore and can just use the stroke's identity
                //contentBuffer.RemoveStrokeChecksum(stroke, (cs) => strokeChecksums.Remove(cs));
            }
        }

        protected override void ContentPrivacyChange(TargettedMoveDelta moveDelta)
        {
            var privacyStrokes = new List<Stroke>();
            var privacyTextboxes = new List<TextBox>();
            var privacyImages = new List<Image>();

            Func<Stroke, bool> wherePredicate = (s) => { /* compare tag identity and check if privacy differs*/ return true; };

            foreach (var inkId in moveDelta.inkIds)
            {
                foreach (var stroke in Canvas.Strokes.Where((s) => 
                    { 
                        var strokeTag = s.tag();
                        return strokeTag.id == inkId.Identity && strokeTag.privacy == moveDelta.newPrivacy; 
                    }))
                {
                    privacyStrokes.Add(stroke);
                }
            }

            foreach (var textId in moveDelta.textIds)
            {
                foreach (var textBox in Canvas.TextChildren().Where((t) => 
                    { 
                        var textTag = t.tag(); 
                        return textTag.id == textId.Identity && textTag.privacy == moveDelta.newPrivacy; 
                    }))
                {
                    privacyTextboxes.Add(textBox);
                }
            }

            foreach (var imageId in moveDelta.imageIds)
            {
                foreach (var image in Canvas.ImageChildren().Where((i) => 
                    {
                        var imageTag = i.tag();
                        return i.tag().id == imageId.Identity && imageTag.privacy == moveDelta.newPrivacy;
                    }))
                {
                    privacyImages.Add(image);
                }
            }

            foreach (var stroke in privacyStrokes)
            {
                var oldTag = stroke.tag();
                ContentBuffer.RemoveStroke(stroke, (col) => Canvas.Strokes.Remove(col));

                stroke.tag(new StrokeTag(oldTag.author, moveDelta.newPrivacy, oldTag.id, oldTag.startingSum, oldTag.isHighlighter)); 
                ContentBuffer.AddStroke(stroke, (col) => Canvas.Strokes.Add(col));
            }

            foreach (var image in privacyImages)
            {
                var oldTag = image.tag();
                oldTag.privacy = moveDelta.newPrivacy;

                image.tag(oldTag);
                //ApplyPrivacyStylingToElement(image, moveDelta.newPrivacy);
            }

            foreach (var text in privacyTextboxes)
            {
                var oldTag = text.tag();
                oldTag.privacy = moveDelta.newPrivacy;

                text.tag(oldTag);
                //ApplyPrivacyStylingToElement(text, moveDelta.newPrivacy);
            }
        }
    }
}
