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

        public StackMoveDeltaProcessor(InkCanvas canvas, ContentBuffer contentBuffer, string target) : base(canvas, target, contentBuffer)
        {
            ContentBuffer = contentBuffer;
        }

        protected override void AddStroke(PrivateAwareStroke stroke)
        {
            ContentBuffer.AddStroke(stroke, (col) => Canvas.Strokes.Add(col));
        }

        protected override void RemoveStroke(PrivateAwareStroke stroke)
        {
            ContentBuffer.RemoveStroke(stroke, (col) => Canvas.Strokes.Remove(col));
            // hopefully don't need to keep track of checksums anymore and can just use the stroke's identity
            //contentBuffer.RemoveStrokeChecksum(stroke, (cs) => strokeChecksums.Remove(cs));
        }

        protected override void RemoveImage(MeTLImage image) 
        { 
            ContentBuffer.RemoveImage(image, (img) => Canvas.Children.Remove(img));
        }

        protected override void RemoveText(TextBox text)
        {
            ContentBuffer.RemoveTextBox(text, (tb) => Canvas.Children.Remove(tb));
        }

        protected override void ChangeImagePrivacy(MeTLImage image, Privacy newPrivacy)
        {
            image.ApplyPrivacyStyling(ContentBuffer, Target, newPrivacy);
        }

        protected override void ChangeTextPrivacy(TextBox text, Privacy newPrivacy)
        {
            text.ApplyPrivacyStyling(ContentBuffer, Target, newPrivacy);
        }
    }
}
