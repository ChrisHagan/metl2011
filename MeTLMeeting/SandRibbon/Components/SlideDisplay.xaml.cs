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

namespace SandRibbon.Components
{
    public partial class SlideDisplay : UserControl, ISlideDisplay
    {
        public int currentSlideIndex = -1;
        public int currentSlideId = -1;
        public ObservableCollection<ThumbnailInformation> thumbnailList = new ObservableCollection<ThumbnailInformation>();
        public bool synced = false;
        public bool isAuthor = false;
        private bool moveTo;
        private int realLocation;
        public SlideDisplay()
        {
            InitializeComponent();
            Commands.SyncedMoveRequested.RegisterCommand(new DelegateCommand<int>(moveToTeacher, _i=> synced));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>(MoveTo, slideInConversation));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(jid =>
            {
                currentSlideIndex = 0;
                slides.SelectedIndex = 0;
                slides.ScrollIntoView(slides.SelectedIndex);
            }));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<SandRibbonObjects.ConversationDetails>(Display));
            Commands.SetSync.RegisterCommand(new DelegateCommand<object>(syncChanded));
            Commands.ThumbnailAvailable.RegisterCommand(new DelegateCommand<int>(loadThumbnail));
            Commands.AddSlide.RegisterCommand(new DelegateCommand<object>(addSlide, canAddSlide));
            Commands.MoveToNext.RegisterCommand(new DelegateCommand<object>(moveToNext, isNext));
            Commands.MoveToPrevious.RegisterCommand(new DelegateCommand<object>(moveToPrevious, isPrevious));
            try
            {
                Display(Globals.conversationDetails);
            }
            catch (NotSetException)
            {
                    //YAAAAAY
            }
        }
        private void syncChanded(object obj)
        {
            synced = !synced;
            Commands.RequerySuggested(Commands.SyncedMoveRequested);
        }
        private bool canAddSlide(object _slide)
        {
            try
            {
                var details = Globals.conversationDetails;
                if (String.IsNullOrEmpty(Globals.me) || details == null) return false;
                return (details.Permissions.studentCanPublish || details.Author == Globals.me);
            }
            catch(NotSetException e)
            {
                return false;
            }
        }
        private void addSlide(object _slide)
        {
            Commands.CreateThumbnail.Execute(Globals.slide);
            ConversationDetailsProviderFactory.Provider.AppendSlideAfter(Globals.slide, Globals.conversationDetails.Jid);
            moveTo = true;
        }
        private void MoveTo(int slide)
        {
            var doMove = (Action) delegate
                                      {
                                          //The real location may not be a displayable thumb
                                          realLocation = slide;
                                          var typeOfDestination =
                                              Globals.conversationDetails.Slides.Where(s => s.id == slide).Select(s => s.type).
                                                  FirstOrDefault();
                                          var currentSlide = (ThumbnailInformation) slides.SelectedItem;
                                          if (currentSlide == null || currentSlide.slideId != slide)
                                          {
                                              slides.SelectedIndex =
                                                  thumbnailList.Select(s => s.slideId).ToList().IndexOf(slide);
                                              if (slides.SelectedIndex != -1)
                                                  currentSlideIndex = slides.SelectedIndex;
                                          }
                                          slides.ScrollIntoView(slides.SelectedItem);
                                      };
            if (Thread.CurrentThread != Dispatcher.Thread)
                Dispatcher.BeginInvoke(doMove);
            else
                doMove();
            Commands.RequerySuggested(Commands.MoveToNext);
            Commands.RequerySuggested(Commands.MoveToPrevious);

        }
        private void moveToTeacher(int where)
        {
            if(isAuthor) return;
            var action = (Action) (() => Dispatcher.BeginInvoke((Action) delegate
                                         {
                                             if (thumbnailList.Where( t => t.slideId == where).Count()==1&&synced)
                                                 Commands.MoveTo.Execute(where);
                                         }));
            GlobalTimers.SetSyncTimer(action);
             
        }
        private bool slideInConversation(int slide)
        {
            var result = Globals.conversationDetails.Slides.Select(t => t.id).Contains(slide);
            return result;
        }
        private bool isPrevious(object _object)
        {
            return slides != null && slides.SelectedIndex > 0;
        }
        private void moveToPrevious(object _object)
        {
            var previousIndex = slides.SelectedIndex - 1;
            if(previousIndex < 0) return;
            slides.SelectedIndex = previousIndex;
        }
        private bool isNext(object _object)
        {
            return (slides != null && slides.SelectedIndex < thumbnailList.Count() - 1); 
        }
        private void moveToNext(object _object)
        {
            var nextIndex = slides.SelectedIndex + 1;
            slides.SelectedIndex = nextIndex;
        }
        public void Display(ConversationDetails details)
        {//We only display the details of our current conversation (or the one we're entering)
            var doDisplay = (Action)delegate
            {
                if (Globals.me == details.Author)
                    isAuthor = true;
                else
                    isAuthor = false;
                var thumbs = new ObservableCollection<ThumbnailInformation>();
                foreach (var slide in details.Slides)
                {
                    if (slide.type == Slide.TYPE.SLIDE)
                    {
                        thumbs.Add(
                            new ThumbnailInformation
                                {
                                    slideId = slide.id,
                                    slideNumber = details.Slides.Where(s => s.type == Slide.TYPE.SLIDE).ToList().IndexOf(slide) + 1,
                                });
                    }
                }
                slides.ItemsSource = thumbs;
                if(moveTo)
                {
                    currentSlideIndex++;
                    moveTo = false;
                }
                slides.SelectedIndex = currentSlideIndex;
                if (slides.SelectedIndex == -1)
                    slides.SelectedIndex = 0;
                thumbnailList = thumbs;
                foreach (var thumb in thumbnailList)
                    loadThumbnail(thumb.slideId);
            };
            if (Thread.CurrentThread != Dispatcher.Thread)
                Dispatcher.BeginInvoke(doDisplay);
            else
                doDisplay();
            Commands.RequerySuggested(Commands.MoveTo);
        }
        private void slides_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)sender).SelectedItem != null)
            {
                if(e.RemovedItems.Count > 0){//If we're moving FROM somewhere
                    var oldThumb = e.RemovedItems[0];
                    Commands.CreateThumbnail.Execute(((ThumbnailInformation)oldThumb).slideId);
                }
                var newThumb = (ThumbnailInformation)(((ListBox)sender).SelectedItem);
                var proposedIndex =((ListBox) sender).SelectedIndex;
                var proposedId =newThumb.slideId;
                if (proposedId == currentSlideId) return;
                Logger.Log("At {0} moving to {1}", currentSlideId, newThumb.slideId);
                currentSlideIndex = proposedIndex;
                currentSlideId = proposedId;
                Commands.MoveTo.Execute( newThumb.slideId);
            }
        }
        private void showHistoryProgress(object sender, ExecutedRoutedEventArgs e)
        {
            var progress = (int[]) e.Parameter;
            loadProgress.Show(progress[0], progress[1]); 
        }
        private void toggleSync(object sender, RoutedEventArgs e)
        {
            Commands.SetSync.Execute(null);
            BitmapImage source;
            var synced = new Uri(Directory.GetCurrentDirectory() + "\\Resources\\SyncRed.png");
            var deSynced = new Uri(Directory.GetCurrentDirectory() + "\\Resources\\SyncGreen.png");
            if(syncButton.Icon.ToString().Contains("SyncGreen"))
                source = new BitmapImage(synced);
            else
            {
                source = new BitmapImage(deSynced);
            }
            syncButton.Icon = source;
        }
        private void loadThumbnail(int slideId)
        {
            var doLoad = (Action)delegate
            {
                var directory = Directory.GetCurrentDirectory();
                var unknownSlidePath = directory + "\\Resources\\slide_Not_Loaded.png";
                var path = string.Format(@"{0}\thumbs\{1}\{2}.png", directory, Globals.me, slideId);
                ImageSource thumbnailSource;
                if (File.Exists(path))
                    thumbnailSource = loadedCachedImage(path);
                else
                    thumbnailSource = loadedCachedImage(unknownSlidePath);
                var brush = new ImageBrush(thumbnailSource);
                var thumb = thumbnailList.Where(t => t.slideId == slideId).FirstOrDefault();
                if (thumb != null) thumb.thumbnail = brush;
            };
            if (Dispatcher.Thread != Thread.CurrentThread)
                Dispatcher.BeginInvoke(doLoad);
            else
                doLoad();
        }
        private static BitmapImage loadedCachedImage(string uri)
        {
            BitmapImage bi = new BitmapImage();
            try
            {
                bi.BeginInit();
                bi.UriSource = new Uri(uri);
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bi.EndInit();
                bi.Freeze();
            }
            catch (Exception e)
            {
                Logger.Log("Loaded cached image failed");
            }
            return bi;
        }
    }
}