namespace SandRibbon.Components.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using MeTLLib.DataTypes;
    using SandRibbon.Providers;
    using System.Windows.Media;
    using System.Windows.Media.Effects;
    using Pages;

    public static class FrameworkElementExtensions
    {
        private const double PUBLIC_OPACITY = 1.0;
        private const double PRIVATE_OPACITY = 0.7;

        public static void ApplyPrivacyStyling(this FrameworkElement element, string target, Privacy newPrivacy, ConversationState state, string me)
        {
            element.ApplyPrivacyStyling(null, target, newPrivacy, state, me);
        }

        public static void ApplyPrivacyStyling(this FrameworkElement element, ContentBuffer contentBuffer, string target, Privacy newPrivacy, ConversationState details, string me)
        {
            if ((!details.StudentsCanPublish && !details.IsAuthor || (target == "notepad")))
            {
                element.RemovePrivacyStyling(contentBuffer); 
                return;
            }
            if (newPrivacy != Privacy.Private)
            {
                element.RemovePrivacyStyling(contentBuffer); 
                return;
            }

            ApplyShadowEffect(element, contentBuffer, Colors.Black);
        }

        public static void RemovePrivacyStyling(this FrameworkElement element)
        {
            element.Effect = null;
            element.Opacity = PUBLIC_OPACITY;
        }

        public static void RemovePrivacyStyling(this FrameworkElement element, ContentBuffer contentBuffer)
        {
            RemovePrivacyStyling(element);
            
            if (contentBuffer != null)
            {
                contentBuffer.UpdateChild(element, (elem) =>
                {
                    elem.Effect = null;
                    elem.Opacity = PUBLIC_OPACITY;
                });
            }
        }

        private static Effect CreateShadowEffect(Color color)
        {
            return new DropShadowEffect { BlurRadius = 50, Color = color, ShadowDepth = 0, Opacity = 1 };
        }

        private static void ApplyShadowEffect(FrameworkElement element, Color color)
        {
            element.Effect = CreateShadowEffect(color);
            element.Opacity = PRIVATE_OPACITY;
        }

        private static void ApplyShadowEffect(FrameworkElement element, ContentBuffer contentBuffer, Color color)
        {
            ApplyShadowEffect(element, color);

            if (contentBuffer != null)
            {
                contentBuffer.UpdateChild(element, (elem) =>
                {
                    elem.Effect = CreateShadowEffect(color); 
                    elem.Opacity = PRIVATE_OPACITY;
                });
            }
        }
    }
}
