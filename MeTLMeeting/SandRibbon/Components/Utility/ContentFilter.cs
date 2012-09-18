using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows;
using SandRibbon.Providers;
using MeTLLib.DataTypes;
using System.Diagnostics;
using SandRibbon.Utils;

namespace SandRibbon.Components.Utility
{
    public abstract class ContentFilter<C, T> where C : class, ICollection<T>, new() 
    {
        protected C contentCollection;

        public ContentFilter()
        {
            contentCollection = new C();
        }

        protected abstract bool Equals(T item1, T item2);
        protected abstract bool CollectionContains(T item);
        protected virtual string AuthorFromTag(T element)
        {
            return string.Empty;
        }
        protected virtual Privacy PrivacyFromTag(T element)
        {
            return Privacy.NotSet;
        }

        public void Add(T element)
        {
            if (CollectionContains(element))
                return;

            contentCollection.Add(element);
        }

        private void Add(C elements)
        {
            foreach (T element in elements)
            {
                Add(element);
            }
        }

        private T Find(T element)
        {
            foreach (T elem in contentCollection)
            {
                if (Equals(elem, element))
                {
                    return elem;
                }
            }

            return default(T);
        }

        public void Remove(T element)
        {
            try
            {
                contentCollection.Remove(Find(element));
            }
            catch (ArgumentException) { }
        }

        private void Remove(C elements)
        {
            try
            {
                foreach (var elem in elements)
                {
                    var foundElem = Find(elem);
                    if (foundElem != null)
                    {
                        contentCollection.Remove(foundElem);
                    }
                }
            }
            catch (ArgumentException) { }
        }

        protected ContentVisibilityEnum CurrentContentVisibility
        {
            get
            {
                return Commands.SetContentVisibility.IsInitialised ? (ContentVisibilityEnum)Commands.SetContentVisibility.LastValue() : ContentVisibilityEnum.AllVisible;
            }
        }

        public void Clear()
        {
            contentCollection.Clear();
        }
        
        public C FilteredContent(ContentVisibilityEnum contentVisibility)
        {
            return FilterContent(contentCollection, contentVisibility);
        }

        public void UpdateChild(T childToFind, Action<T> updateChild) 
        {
            var child = Find(childToFind); 
            if (child != null)
            {
                updateChild(child);
            }
        }

        public void UpdateChildren<V>(Action<V> updateChild) where V : UIElement
        {
            foreach (var uiElement in contentCollection.OfType<V>())
            {
                updateChild(uiElement);
            }
        }

        public void Clear(Action modifyVisibleContainer)
        {
            Clear();
            modifyVisibleContainer();
        }
        public void Add(T element, Action<T> modifyVisibleContainer)
        {
            Add(element);
            var filteredElement = FilterContent(element, CurrentContentVisibility);
            if (filteredElement != null)
            { 
                modifyVisibleContainer(filteredElement);
            }
        }

        public void Add(C elements, Action<C> modifyVisibleContainer)
        {
            Add(elements);
            var filteredElements = FilterContent(elements, CurrentContentVisibility);
            if (filteredElements != null)
            {
                modifyVisibleContainer(filteredElements);
            }
        }

        public void Remove(T element, Action<T> modifyVisibleContainer)
        {
            Remove(element);
            var filteredElement = FilterContent(element, CurrentContentVisibility);
            if (filteredElement != null)
            { 
                modifyVisibleContainer(filteredElement);
            }
        }

        public void Remove(C elements, Action<C> modifyVisibleContainer)
        {
            Remove(elements);
            modifyVisibleContainer(FilterContent(elements, CurrentContentVisibility));
        }

        public T FilterContent(T element, ContentVisibilityEnum contentVisibility)
        {
            var comparer = BuildComparer(contentVisibility);
            return comparer.Any((comp) => comp(AuthorFromTag(element), PrivacyFromTag(element))) ? element : default(T);
        }

        public C FilterContent(C elements, ContentVisibilityEnum contentVisibility)
        {
            var comparer = BuildComparer(contentVisibility);
            var tempList = new C();
            var matchedElements = elements.Where(elem => comparer.Any((comp) => comp(AuthorFromTag(elem), PrivacyFromTag(elem))));

            foreach (var elem in matchedElements)
            {
                tempList.Add(elem);
            }
            
            return tempList;
        }

        #region Helpers

        private List<Func<string, Privacy, bool>> BuildComparer(ContentVisibilityEnum contentVisibility)
        {
            var comparer = new List<Func<string, Privacy, bool>>();
            var conversationAuthor = Globals.conversationDetails.Author;

            if (contentVisibility.HasFlag(ContentVisibilityEnum.OwnerVisible))
                comparer.Add((elementAuthor, _unused) => elementAuthor == conversationAuthor);

            if (contentVisibility.HasFlag(ContentVisibilityEnum.TheirsVisible))
                comparer.Add((elementAuthor, _unused) => (elementAuthor != Globals.me && elementAuthor != conversationAuthor));

            if (contentVisibility.HasFlag(ContentVisibilityEnum.MyPrivateVisible))
                comparer.Add((elementAuthor, elementPrivacy) => elementAuthor == Globals.me && elementPrivacy == Privacy.Private);

            if (contentVisibility.HasFlag(ContentVisibilityEnum.MyPublicVisible))
                comparer.Add((elementAuthor, elementPrivacy) => elementAuthor == Globals.me && elementPrivacy == Privacy.Public);

            return comparer;
        }

        #endregion
    }
}
