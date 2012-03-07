using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows;
using SandRibbon.Providers;
using MeTLLib.DataTypes;

namespace SandRibbon.Components.Utility
{
    public class ContentBuffer
    {
        private UIElementCollection uiCollection;
        private StrokeCollection strokeCollection;

        public ContentBuffer(UIElement visualParent, FrameworkElement logicalParent)
        {
            uiCollection = new UIElementCollection(visualParent, logicalParent);
            strokeCollection = new StrokeCollection();
        }

        private void ClearStrokes()
        {
            strokeCollection.Clear();
        }

        private void AddStrokes(StrokeCollection strokes)
        {
            strokeCollection.Add(strokes);
        }

        private void AddStrokes(Stroke stroke)
        {
            strokeCollection.Add(stroke);
        }

        private void RemoveStrokes(StrokeCollection strokes)
        {
            strokeCollection.Remove(strokes);
        }
        
        private void RemoveStrokes(Stroke stroke)
        {
            strokeCollection.Remove(stroke);
        }

        ContentVisibilityEnum CurrentContentVisibility
        {
            get
            {
                #if TOGGLE_CONTENT
                var currentContent = Commands.SetContentVisibility.IsInitialised ? (ContentVisibilityEnum)Commands.SetContentVisibility.LastValue() : ContentVisibilityEnum.AllVisible;
                #else
                var currentContent = ContentVisibilityEnum.AllVisible;
                #endif
                return currentContent;
            }
        }

        public StrokeCollection Strokes
        {
            get
            {
                return strokeCollection;
            }
        }

        public StrokeCollection FilteredStrokes(ContentVisibilityEnum contentVisibility)
        {
            return FilterStrokes(Strokes, contentVisibility);
        }

        public void ClearStrokes(Action modifyVisibleContainer)
        {
            ClearStrokes();
            modifyVisibleContainer();
        }

        public void AddStrokes(StrokeCollection strokes, Action<StrokeCollection> modifyVisibleContainer)
        {
            AddStrokes(strokes);
            modifyVisibleContainer(FilterStrokes(strokes, CurrentContentVisibility));
        }

        public void AddStrokes(Stroke stroke, Action<StrokeCollection> modifyVisibleContainer)
        {
            var strokes = new StrokeCollection();
            strokes.Add(stroke);

            AddStrokes(stroke);
            modifyVisibleContainer(FilterStrokes(strokes, CurrentContentVisibility));
        }

        public void RemoveStrokes(StrokeCollection strokes, Action<StrokeCollection> modifyVisibleContainer)
        {
            RemoveStrokes(strokes);
            modifyVisibleContainer(FilterStrokes(strokes, CurrentContentVisibility));
        }

        private StrokeCollection FilterStrokes(StrokeCollection strokes, ContentVisibilityEnum contentVisibility)
        {
            var owner = OwnerVisibility(contentVisibility);
            var theirs = IsVisibilityFlagSet(contentVisibility, ContentVisibilityEnum.TheirsVisible);
            // for each of the children of the canvas
            List<Func<string, string, bool>> comparer = new List<Func<string,string,bool>>();

            if (owner)
                comparer.Add((str1, str2) => str1 == str2);

            if (theirs)
                comparer.Add((str1, str2) => str1 != str2);

            return new StrokeCollection(strokes.Where(s => comparer.Any((comp) => comp(s.tag().author, Globals.me))));
            /*foreach (var remove in Work.Children.ToList().Where(c =>
                {
                    if (comparer.Any((comp) => (c is Image && comp(((Image)c).tag().author, Globals.me))) ||
                        comparer.Any((comp) => (c is MeTLTextBox && comp(((MeTLTextBox)c).tag().author, Globals.me))))
                        return true;

                    return false;
                }))
            {
                Work.Children.Remove(remove);
            }*/
        }

        private bool IsVisibilityFlagSet(ContentVisibilityEnum contentVisible, ContentVisibilityEnum flag)
        {
            return (contentVisible & flag) != ContentVisibilityEnum.NoneVisible;
        }

        private bool OwnerVisibility(ContentVisibilityEnum contentVisibility)
        {
            return IsVisibilityFlagSet(contentVisibility, ContentVisibilityEnum.OwnerVisible) && Globals.isAuthor || IsVisibilityFlagSet(contentVisibility, ContentVisibilityEnum.MineVisible);
        }
    }

}
