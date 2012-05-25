using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows;
using SandRibbon.Providers;
using MeTLLib.DataTypes;
using System.Diagnostics;

namespace SandRibbon.Components.Utility
{
    public class ContentBuffer
    {
        private List<UIElement> uiCollection;
        private StrokeFilter strokeFilter;

        // used to create a snapshot for undo/redo
        private List<UIElement> imageDeltaCollection;
        private List<UIElement> textDeltaCollection;
        private List<StrokeChecksum> strokeChecksumCollection;
        private StrokeFilter strokeDeltaFilter;

        public ContentBuffer()
        {
            uiCollection = new List<UIElement>();
            strokeFilter = new StrokeFilter();

            imageDeltaCollection = new List<UIElement>();
            textDeltaCollection = new List<UIElement>();
            strokeDeltaFilter = new StrokeFilter();
            strokeChecksumCollection = new List<StrokeChecksum>();
        }

        #region Collection helpers

        private void ClearElements()
        {
            uiCollection.Clear();
        }

        private void ClearDeltaImages()
        {
            imageDeltaCollection.Clear(); 
        }

        private void ClearDeltaText()
        {
            textDeltaCollection.Clear();
        }

        private void ClearStrokeChecksums()
        {
            strokeChecksumCollection.Clear();
        }

        private void AddStrokeChecksum(StrokeChecksum checksum)
        {
            if (strokeChecksumCollection.Contains(checksum))
                return;

            strokeChecksumCollection.Add(checksum);
        }
        
        private void RemoveStrokeChecksum(StrokeChecksum checksum)
        {
            try
            {
                strokeChecksumCollection.Remove(checksum);
            }
            catch (ArgumentException) { }
        }

        private bool CollectionContainsElement(UIElement element)
        {
            if (element is Image)
            {
                var imageTag = (element as Image).tag().id;
                return uiCollection.OfType<Image>().Where(img => img.tag().id == imageTag).Count() > 0;
            }
            else if (element is TextBox)
            {
                var textTag = (element as TextBox).tag().id;
                return uiCollection.OfType<TextBox>().Where(txt => txt.tag().id == textTag).Count() > 0;
            }
            else
            {
                Debug.Fail("Unexpected type in the collection");
                return uiCollection.Contains(element);
            }
        }

        private void AddElement(UIElement element)
        {
            if (CollectionContainsElement(element))
                return;
            uiCollection.Add(element);
        }

        private void AddDeltaImage(UIElement element)
        {
            if (CollectionContainsElement(element))
                return;
            imageDeltaCollection.Add(element);
        }

        private void AddDeltaText(UIElement element)
        {
            if (CollectionContainsElement(element))
                return;
            textDeltaCollection.Add(element);
        }

        private void AddElements(UIElementCollection elements)
        {
            foreach (UIElement element in elements)
            {
                uiCollection.Add(element);
            }
        }

        private void AddDeltaImage(List<UIElement> elements)
        {
            foreach (UIElement element in elements)
            {
                imageDeltaCollection.Add(element);
            }
        }

        private void AddDeltaText(List<UIElement> elements)
        {
            foreach (UIElement element in elements)
            {
                textDeltaCollection.Add(element);
            }
        }

        private void RemoveElement(UIElement element)
        {
            try
            {
                // find by tag().id then remove the element found
                var found = uiCollection.Find(elem =>
                {
                    if (elem is Image && element is Image)
                    {
                        return IdFromElementTag(elem) == IdFromElementTag(element); 
                    }
                    if (elem is TextBox && element is TextBox)
                    {
                        return IdFromElementTag(elem) == IdFromElementTag(element);
                    }
                    return false;
                });

                uiCollection.Remove(found);
            }
            catch (ArgumentException) { }
        }

        private void RemoveDeltaImage(UIElement element)
        {
            try
            {
                imageDeltaCollection.Remove(element);
            }
            catch (ArgumentException) { }
        }

        private void RemoveDeltaText(UIElement element)
        {
            try
            {
                textDeltaCollection.Remove(element);
            }
            catch (ArgumentException) { }
        }
        #endregion

        private ContentVisibilityEnum CurrentContentVisibility
        {
            get
            {
                var currentContent = Commands.SetContentVisibility.IsInitialised ? (ContentVisibilityEnum)Commands.SetContentVisibility.LastValue() : ContentVisibilityEnum.AllVisible;
                return currentContent;
            }
        }

        public StrokeCollection FilteredStrokes(ContentVisibilityEnum contentVisibility)
        {
            return strokeFilter.FilterContent(strokeFilter.Strokes, contentVisibility); 
        }

        public IEnumerable<UIElement> FilteredElements(ContentVisibilityEnum contentVisibility)
        {
            return FilterElements(uiCollection, contentVisibility);
        }

        public void UpdateChild<TypeOfChild>(TypeOfChild childToFind, Action<TypeOfChild> updateChild) where TypeOfChild : UIElement
        {
            var child = uiCollection.Find((elem) => elem == childToFind);
            if (child != null)
            {
                updateChild(child as TypeOfChild);
            }
        }
        public void UpdateChildren<TypeOfChildren>(Action<TypeOfChildren> updateChild) 
        {
            foreach (var uiElement in uiCollection.OfType<TypeOfChildren>())
            {
                updateChild(uiElement);
            }
        }

        public void Clear()
        {
            strokeFilter.Clear();
            ClearElements();
        }

        public void ClearStrokes(Action modifyVisibleContainer)
        {
            strokeFilter.Clear();
            modifyVisibleContainer();
        }

        public void ClearDeltaStrokes(Action modifyUndoContainer)
        {
            strokeDeltaFilter.Clear();
            modifyUndoContainer();
        }

        public void ClearDeltaImages(Action modifyUndoContainer)
        {
            ClearDeltaImages();
            modifyUndoContainer();
        }

        public void ClearDeltaText(Action modifyUndoContainer)
        {
            ClearDeltaText();
            modifyUndoContainer();
        }

        public void AddStrokes(StrokeCollection strokes, Action<StrokeCollection> modifyVisibleContainer)
        {
            strokeFilter.Add(strokes, modifyVisibleContainer);
        }

        public void AddStroke(Stroke stroke, Action<Stroke> modifyVisibleContainer)
        {
            strokeFilter.Add(stroke, modifyVisibleContainer);
        }

        public void RemoveStroke(Stroke stroke, Action<Stroke> modifyVisibleContainer)
        {
            strokeFilter.Remove(stroke, modifyVisibleContainer);
        }

        public void RemoveStrokes(StrokeCollection strokes, Action<StrokeCollection> modifyVisibleContainer)
        {
            strokeFilter.Remove(strokes, modifyVisibleContainer);
        }

        public void AddDeltaStrokes(StrokeCollection strokes, Action<StrokeCollection> modifyUndoContainer)
        {
            strokeDeltaFilter.Add(strokes, modifyUndoContainer); 
        }

        public void AddDeltaImages(List<UIElement> images, Action<IEnumerable<UIElement>> modifyUndoContainer)
        {
            AddDeltaImage(images);
#if TOGGLE_CONTENT
            modifyUndoContainer(FilterElements(images, CurrentContentVisibility));
#else
            modifyUndoContainer(images);
#endif
        }


        private Stroke FilterStroke(Stroke stroke, ContentVisibilityEnum contentVisibility)
        {
            var comparer = BuildComparer(contentVisibility);
            return comparer.Any((comp) => comp(stroke.tag().author)) ? stroke : null;
        }

        #region Handle images and text

        public void ClearElements(Action modifyVisibleContainer)
        {
            ClearElements();
            modifyVisibleContainer();
        }

        public void AddElement(UIElement element, Action<UIElement> modifyVisibleContainer)
        {
            AddElement(element);
#if TOGGLE_CONTENT
            var filteredElement = FilterElement(element, CurrentContentVisibility);
            if (filteredElement != null)
            { 
                modifyVisibleContainer(filteredElement);
            }
#else
            modifyVisibleContainer(element);
#endif
        }

        public void RemoveElement(UIElement element, Action<UIElement> modifyVisibleContainer)
        {
            RemoveElement(element);
#if TOGGLE_CONTENT
            var filteredElement = FilterElement(element, CurrentContentVisibility);
            if (filteredElement != null)
            { 
                modifyVisibleContainer(filteredElement);
            }
#else
            modifyVisibleContainer(element);
#endif
        }

        public void RemoveStrokesAndMatchingChecksum(IEnumerable<string> checksums, Action<IEnumerable<string>> modifyVisibleContainer)
        {
            // 1. find the strokes in the contentbuffer that have matching checksums 
            // 2. remove those strokes and corresponding checksums in the content buffer
            // 3. for the strokes that also exist in the canvas, remove them and their checksums
            var dirtyStrokes = strokeFilter.StrokesWithChecksums(checksums); 
            foreach (var stroke in dirtyStrokes)
            {
                RemoveStrokeChecksum(stroke.sum());
                strokeFilter.Remove(stroke); 
            }

            modifyVisibleContainer(checksums);
        }

        public void AddStrokeChecksum(Stroke stroke, Action<StrokeChecksum> modifyVisibleContainer)
        {
            var checksum = stroke.sum();

            AddStrokeChecksum(checksum);

            modifyVisibleContainer(checksum);
        }

        public void RemoveStrokeChecksum(Stroke stroke, Action<StrokeChecksum> modifyVisibleContainer)
        {
            var checksum = stroke.sum();

            RemoveStrokeChecksum(checksum);
            modifyVisibleContainer(checksum);
        }

        private UIElement FilterElement(UIElement element, ContentVisibilityEnum contentVisibility)
        {
            var tempList = new List<UIElement>();
            tempList.Add(element);

            return FilterElements(tempList, contentVisibility).FirstOrDefault();
        }

        private string AuthorFromElementTag(UIElement element)
        {
            if (element is Image)
                return ((Image)element).tag().author;

            if (element is TextBox)
                return ((TextBox)element).tag().author;

            Debug.Fail("Element should be either Image or TextBox");
            return string.Empty;
        }

        private string IdFromElementTag(UIElement element)
        {
            if (element is Image)
                return ((Image)element).tag().id;

            if (element is TextBox)
                return ((TextBox)element).tag().id;

            Debug.Fail("Element should be either Image or TextBox");
            return string.Empty;
        }

        private IEnumerable<UIElement> FilterElements(List<UIElement> elements, ContentVisibilityEnum contentVisibility)
        {
            var comparer = BuildComparer(contentVisibility);
            return elements.Where(elem => comparer.Any((comp) => comp(AuthorFromElementTag(elem))));
        }

        private List<Func<string, bool>> BuildComparer(ContentVisibilityEnum contentVisibility)
        {
            var comparer = new List<Func<string,bool>>();
            var conversationAuthor = Globals.conversationDetails.Author;

            if (IsVisibilityFlagSet(contentVisibility, ContentVisibilityEnum.OwnerVisible))
                comparer.Add((elementAuthor) => elementAuthor == conversationAuthor);

            if (IsVisibilityFlagSet(contentVisibility, ContentVisibilityEnum.TheirsVisible))
                comparer.Add((elementAuthor) => (elementAuthor != Globals.me && elementAuthor != conversationAuthor));

            if (IsVisibilityFlagSet(contentVisibility, ContentVisibilityEnum.MineVisible))
                comparer.Add((elementAuthor) => elementAuthor == Globals.me);

            return comparer;
        }

        #endregion

        private bool IsVisibilityFlagSet(ContentVisibilityEnum contentVisible, ContentVisibilityEnum flag)
        {
            return (contentVisible & flag) != ContentVisibilityEnum.NoneVisible;
        }
    }

}
