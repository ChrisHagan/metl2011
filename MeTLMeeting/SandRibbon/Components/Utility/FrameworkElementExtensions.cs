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

    public static class FrameworkElementExtensions
    {
        private const double PUBLIC_OPACITY = 1.0;
        private const double PRIVATE_OPACITY = 0.7;

        public static void ApplyPrivacyStyling(this FrameworkElement element, ContentBuffer contentBuffer, string target, Privacy newPrivacy)
        {
            if ((!Globals.conversationDetails.Permissions.studentCanPublish && !Globals.isAuthor) || (target == "notepad"))
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

        public static void RemovePrivacyStyling(this FrameworkElement element, ContentBuffer contentBuffer)
        {
            element.Effect = null;
            element.Opacity = PUBLIC_OPACITY;

            contentBuffer.UpdateChild(element, (elem) =>
            {
                elem.Effect = null;
                elem.Opacity = PUBLIC_OPACITY;
            });
        }

        private static Effect CreateShadowEffect(Color color)
        {
            return new DropShadowEffect { BlurRadius = 50, Color = color, ShadowDepth = 0, Opacity = 1 };
        }

        private static void ApplyShadowEffect(FrameworkElement element, ContentBuffer contentBuffer, Color color)
        {
            element.Effect = CreateShadowEffect(color);
            element.Opacity = PRIVATE_OPACITY;

            contentBuffer.UpdateChild(element, (elem) =>
            {
                elem.Effect = CreateShadowEffect(color); 
                elem.Opacity = PRIVATE_OPACITY;
            });
        }
    }
}
