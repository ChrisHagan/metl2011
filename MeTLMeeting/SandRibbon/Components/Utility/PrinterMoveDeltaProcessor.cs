namespace SandRibbon.Components.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using MeTLLib.DataTypes;
    using SandRibbon.Providers;
    using System.Windows.Media;
    using System.Windows.Ink;
    using System.Windows.Controls;
    using Pages;

    public class PrinterMoveDeltaProcessor : MoveDeltaProcessor
    {
        public PrinterMoveDeltaProcessor(InkCanvas canvas, string target, ContentBuffer contentBuffer, ConversationState details, string me) : base(canvas, target, contentBuffer,details,me)
        {
        }

        protected override void AddStroke(PrivateAwareStroke stroke)
        {
            Canvas.Strokes.Add(stroke);
        }

        protected override void RemoveStroke(PrivateAwareStroke stroke)
        {
            Canvas.Strokes.Remove(stroke);
        }

        protected override void RemoveImage(MeTLImage image)
        {
            Canvas.Children.Remove(image);
        }

        protected override void RemoveText(MeTLTextBox text)
        {
            Canvas.Children.Remove(text);
        }

        protected override void ChangeImagePrivacy(MeTLImage image, Privacy newPrivacy)
        {
            image.ApplyPrivacyStyling(Target, newPrivacy,conversationState,me);
        }

        protected override void ChangeTextPrivacy(MeTLTextBox text, Privacy newPrivacy)
        {
            text.ApplyPrivacyStyling(Target, newPrivacy,conversationState,me);
        }
    }
}
