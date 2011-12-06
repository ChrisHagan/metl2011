using System.Windows;
using System.Windows.Media;

namespace MeTLLib.Utilities
{
    public class UIHelper
    {
        public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            // immediate children (breadth-first)
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                    return child as T;
                else
                {
                    var childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }

            return null;
        }

        public static T FindVisualParent<T>(DependencyObject obj) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(obj);
            if (parent != null && parent is T)
                return parent as T;
            else 
            {
                var parentOfParent = FindVisualParent<T>(parent);
                if (parentOfParent != null)
                    return parentOfParent;
            }

            return null;
        }
    }
}
