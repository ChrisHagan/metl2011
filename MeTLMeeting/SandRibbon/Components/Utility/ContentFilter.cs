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
using System.ComponentModel;

namespace SandRibbon.Components.Utility
{
    public class ContentVisibilityDefinition : INotifyPropertyChanged
    {
        public string Description { get; protected set; }
        public string Label { get; protected set; }
        protected bool _subscribed;
        public bool Subscribed {
            get {
                return _subscribed;
            }
            set {
                _subscribed = value;
                ContentFilterVisibility.refreshVisibilities();
            }
        }
        public string GroupId { get; protected set; }
        //= (author, privacy, conversation, slide) => false;
        public Func<string, Privacy, ConversationDetails, Slide, bool> Comparer { get; protected set; }
        public ContentVisibilityDefinition(string label, string description, string groupId, bool subscribed, Func<string, Privacy, ConversationDetails, Slide, bool> comparer)
        {
            Label = label;
            Description = description;
            _subscribed = subscribed;
            Comparer = comparer;
            GroupId = groupId;
            PropertyChanged += (s, a) => {
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RefreshSubscription()
        {
            PropertyChanged(this, new PropertyChangedEventArgs("Subscribed"));
        }
    }

    public static class ContentFilterVisibility
    {
        public static readonly ContentVisibilityDefinition myPublic = new ContentVisibilityDefinition("My public", "", "", true, (a, p, c, s) => a == Globals.me && p == Privacy.Public);
        public static readonly ContentVisibilityDefinition myPrivate = new ContentVisibilityDefinition("My private", "", "", true, (a, p, c, s) => a == Globals.me && p == Privacy.Private);
        public static readonly ContentVisibilityDefinition ownersPublic = new ContentVisibilityDefinition("Owner's", "", "", true, (a, p, c, s) => a == c.Author && p == Privacy.Public);
        public static readonly ContentVisibilityDefinition peersPublic = new ContentVisibilityDefinition("Everyone else's", "", "", true, (a, p, c, s) => a != c.Author && a != Globals.me && p == Privacy.Public);
        public static readonly List<ContentVisibilityDefinition> defaultVisibilities = new List<ContentVisibilityDefinition> { myPublic, myPrivate, ownersPublic, peersPublic };
        public static readonly List<ContentVisibilityDefinition> defaultGroupVisibilities = new List<ContentVisibilityDefinition> { myPublic, myPrivate, ownersPublic, peersPublic };
        public static readonly List<ContentVisibilityDefinition> printVisibilities = new List<ContentVisibilityDefinition> { new ContentVisibilityDefinition("everything", "", "", true, (a, p, c, s) => true) };

        public static List<ContentVisibilityDefinition> allVisible = Globals.contentVisibility.ToArray().ToList().Select(cvd => { cvd.Subscribed = true; return cvd; }).ToList();
        public static void refreshVisibilities()
        {
            Commands.SetContentVisibility.Execute(CurrentContentVisibility);
        }
        public static void reEnableMyPublic()
        {
            if (!myPublic.Subscribed)
            {
                myPublic.Subscribed = true;
                refreshVisibilities();
            }
        }
        public static void reEnableMyPrivate()
        {
            if (!myPrivate.Subscribed)
            {
                myPrivate.Subscribed = true;
                refreshVisibilities();
            }
        }
        public static void reEnableAll()
        {
            CurrentContentVisibility.ForEach(cv => cv.Subscribed = true);
            refreshVisibilities();
        }

        public static bool isGroupSlide
        {
            get
            {
                try
                {
                    return Globals.conversationDetails.Slides.Find(s => s.id == Globals.slide).type == Slide.TYPE.GROUPSLIDE;
                }
                catch
                {
                    return false;
                }
            }
        }
        public static List<ContentVisibilityDefinition> CurrentContentVisibility
        {
            get
            {
                return Commands.SetContentVisibility.IsInitialised ? (List<ContentVisibilityDefinition>)Commands.SetContentVisibility.LastValue() : defaultVisibilities;
            }
        }
    }

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
            {
                Remove(element);
            }
            
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

        private List<T> FindAll(T element)
        {
            var foundElements = new List<T>();
            foreach (T elem in contentCollection)
            {
                if (Equals(elem, element)) 
                {
                    foundElements.Add(elem);
                }
            }

            return foundElements;
        }

        public void Remove(T element)
        {
            try
            {
                RemoveAll(FindAll(element));
            }
            catch (ArgumentException) { }
        }

        private void RemoveAll(List<T> element)
        {
            element.ForEach(e => contentCollection.Remove(e));
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

        public void Clear()
        {
            contentCollection.Clear();
        }
        
        public C FilteredContent(List<ContentVisibilityDefinition> contentVisibility)
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
        public List<ContentVisibilityDefinition> CurrentContentVisibility { get { return Globals.contentVisibility; } }
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

        public T FilterContent(T element, List<ContentVisibilityDefinition> contentVisibility)
        {
            return contentVisibility.Where(cv => cv.Subscribed).Any(cv => cv.Comparer(AuthorFromTag(element), PrivacyFromTag(element),Globals.conversationDetails,Globals.slideDetails)) ? element : default(T);
        }

        public C FilterContent(C elements, List<ContentVisibilityDefinition> contentVisibility)
        {
            var tempList = new C();
            var enabledContentVisibilities = contentVisibility.Where(cv => cv.Subscribed);
            var matchedElements = elements.Where(elem => enabledContentVisibilities.Any(cv => {
                return cv.Subscribed && cv.Comparer(AuthorFromTag(elem), PrivacyFromTag(elem), Globals.conversationDetails, Globals.slideDetails);
            }));

            foreach (var elem in matchedElements)
            {
                tempList.Add(elem);
            }
            
            return tempList;
        }
    }
}
