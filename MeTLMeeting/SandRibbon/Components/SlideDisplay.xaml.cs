using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Interfaces;
using SandRibbon.Components.Utility;
using SandRibbon.Providers;
using SandRibbon.Utils;
using SandRibbonInterop;
using SandRibbonObjects;
using SandRibbon.Providers.Structure;
using Divelements.SandRibbon;
using SandRibbon.Utils.Connection;
using System.Collections.Generic;
using System.Windows.Ink;
using System.Drawing;

namespace SandRibbon.Components
{
    public partial class SlideDisplay : UserControl, ISlideDisplay
    {
        public int currentSlideIndex = -1;
        public int currentSlideId = -1;
        public ObservableCollection<ThumbnailInformation> thumbnailList = new ObservableCollection<ThumbnailInformation>();
        public static Dictionary<int, PreParser> parsers = new Dictionary<int, PreParser>();
        public static Dictionary<int, PreParser> privateParsers = new Dictionary<int, PreParser>();
        public bool isAuthor = false;
        private bool moveTo;
        private int realLocation;
        public SlideDisplay()
        {
            InitializeComponent();
            slides.ItemsSource = thumbnailList;
            Commands.SyncedMoveRequested.RegisterCommand(new DelegateCommand<int>(moveToTeacher));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>(MoveTo, slideInConversation));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(jid =>
            {
                currentSlideIndex = 0;
                slides.SelectedIndex = 0;
                slides.ScrollIntoView(slides.SelectedIndex);
                DelegateCommand<ConversationDetails> onConversationDetailsReady = null;
                onConversationDetailsReady = new DelegateCommand<ConversationDetails>(details =>
                                                  {
                                                      Commands.UpdateConversationDetails.UnregisterCommand(onConversationDetailsReady);
                                                      Commands.SneakInto.Execute(details.Jid);
                                                  });
                Commands.UpdateConversationDetails.RegisterCommand(onConversationDetailsReady);

            }));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<SandRibbonObjects.ConversationDetails>(Display));
            Commands.AddSlide.RegisterCommand(new DelegateCommand<object>(addSlide, canAddSlide));
            Commands.MoveToNext.RegisterCommand(new DelegateCommand<object>(moveToNext, isNext));
            Commands.MoveToPrevious.RegisterCommand(new DelegateCommand<object>(moveToPrevious, isPrevious));
            Commands.ThumbnailAvailable.RegisterCommand(new DelegateCommand<int>(ThumbnailAvailable));

            try
            {
                Display(Globals.conversationDetails);
            }
            catch (NotSetException)
            {
                //YAAAAAY
            }
        }
        private void ThumbnailAvailable(int slideId)
        {
            App.Now("Thumbnail available for " + slideId);
            Dispatcher.adoptAsync(()=>
            thumbnailList.Where(ti => ti.slideId == slideId).First().ThumbnailBrush = ThumbnailProvider.get(slideId));
            App.Now("Thumbnail bound for " + slideId);
        }
        private bool canAddSlide(object _slide)
        {
            try
            {
                var details = Globals.conversationDetails;
                if (String.IsNullOrEmpty(Globals.me) || details == null) return false;
                return (details.Permissions.studentCanPublish || details.Author == Globals.me);
            }
            catch (NotSetException e)
            {
                return false;
            }
        }
        private void addSlide(object _slide)
        {
            ConversationDetailsProviderFactory.Provider.AppendSlideAfter(Globals.slide, Globals.conversationDetails.Jid);
            moveTo = true;
        }
        private bool isSlideInSlideDisplay(int slide)
        {
            bool isTrue = false;
            foreach (ThumbnailInformation info in slides.Items)
            {
                if (info.slideId == slide) isTrue = true;
            }
            return isTrue;
        }
        private void MoveTo(int slide)
        {
            Dispatcher.adoptAsync(delegate
                                      {

                                          if (isSlideInSlideDisplay(slide))
                                          {
                                              var currentSlide = (ThumbnailInformation)slides.SelectedItem;
                                              if (currentSlide == null || currentSlide.slideId != slide)
                                              {
                                                  slides.SelectedIndex =
                                                      thumbnailList.Select(s => s.slideId).ToList().IndexOf(slide);
                                                  slides.ScrollIntoView(slides.SelectedItem);
                                              }
                                          }
                                      });
            Commands.RequerySuggested(Commands.MoveToNext);
            Commands.RequerySuggested(Commands.MoveToPrevious);
        }
        private void moveToTeacher(int where)
        {
            if (isAuthor) return;
            if (!Globals.synched) return;
            var action = (Action)(() => Dispatcher.adoptAsync((Action)delegate
                                         {
                                             if (thumbnailList.Where(t => t.slideId == where).Count() == 1)
                                                 Commands.MoveTo.Execute(where);
                                         }));
            GlobalTimers.SetSyncTimer(action);
        }
        private bool slideInConversation(int slide)
        {
            return Globals.conversationDetails.Slides.Select(t => t.id).Contains(slide);
        }
        private bool isPrevious(object _object)
        {
            return slides != null && slides.SelectedIndex > 0;
        }
        private void moveToPrevious(object _object)
        {
            var previousIndex = slides.SelectedIndex - 1;
            if (previousIndex < 0) return;
            slides.SelectedIndex = previousIndex;
            slides.ScrollIntoView(slides.SelectedItem);
        }
        private bool isNext(object _object)
        {
            return (slides != null && slides.SelectedIndex < thumbnailList.Count() - 1);
        }
        private void moveToNext(object _object)
        {
            var nextIndex = slides.SelectedIndex + 1;
            slides.SelectedIndex = nextIndex;
            slides.ScrollIntoView(slides.SelectedItem);
        }
        public void Display(ConversationDetails details)
        {//We only display the details of our current conversation (or the one we're entering)
            Dispatcher.adoptAsync((Action)delegate
            {
                if (Globals.me == details.Author)
                    isAuthor = true;
                else
                    isAuthor = false;
                thumbnailList.Clear();
                //Commands.SneakInto.Execute(details.Jid);
                App.Now("beginning creation of slideDisplay");
                foreach (var slide in details.Slides)
                {
                    if (slide.type == Slide.TYPE.SLIDE)
                    {
                        thumbnailList.Add(
                            new ThumbnailInformation
                                {
                                    slideId = slide.id,
                                    slideNumber = details.Slides.Where(s => s.type == Slide.TYPE.SLIDE).ToList().IndexOf(slide) + 1,
                                    Exposed = slide.exposed
                                    //ThumbnailBrush = ThumbnailProvider.get(slide.id)
                                });
                        ThumbnailAvailable(slide.id);
                        App.Now("slideDisplay item created: " + slide.id);
                    }
                }
                if (moveTo)
                {
                    currentSlideIndex++;
                    moveTo = false;
                }
                slides.SelectedIndex = currentSlideIndex;
                if (slides.SelectedIndex == -1)
                    slides.SelectedIndex = 0;
            });
        }
        private bool isSlideExposed(ThumbnailInformation slide)
        {
            var isFirst = slide.slideNumber == 0;
            var isPedagogicallyAbleToSeeSlides = Globals.pedagogy.code >= 3;
            var isExposedIfNotCurrentSlide = isAuthor || isFirst || isPedagogicallyAbleToSeeSlides;
            try
            {
                return Globals.slide == slide.slideId || isExposedIfNotCurrentSlide;
            }
            catch (NotSetException)
            {//Don't have a current slide
                return isExposedIfNotCurrentSlide;
            }
        }
        private void slides_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var source = (ListBox)sender;
            if (source.SelectedItem != null)
            {
                var proposedIndex = source.SelectedIndex;
                var proposedId = ((ThumbnailInformation)source.SelectedItem).slideId;
                if (proposedId == currentSlideId) return;
                currentSlideIndex = proposedIndex;
                currentSlideId = proposedId;
                Commands.MoveTo.Execute(currentSlideId);
                slides.ScrollIntoView(slides.SelectedItem);
            }
        }
    }
    public class ThumbListBox : ListBox
    {
        public static Dictionary<int, ListBoxItem> visibleContainers = new Dictionary<int, ListBoxItem>();
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            var slide = (ThumbnailInformation)item;
            var container = (ListBoxItem)element;
            container.Content = null;
            visibleContainers.Remove(slide.slideId);
        }
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var slide = (ThumbnailInformation)item;
            var container = (ListBoxItem)element;
            visibleContainers[slide.slideId] = container;
            if (SlideDisplay.parsers.ContainsKey(slide.slideId))
                Add(slide.slideId, SlideDisplay.parsers[slide.slideId]);
            else Add(slide.slideId, new PreParser(slide.slideId));
        }
        public static void Add(int id, PreParser parser)
        {
            if (!visibleContainers.ContainsKey(id)) return;
            visibleContainers[id].Content = parser.ToVisual();
        }
    }
}