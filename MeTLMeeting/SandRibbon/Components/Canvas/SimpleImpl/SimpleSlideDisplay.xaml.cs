using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SandRibbon.Components.Interfaces;
using SandRibbon.Utils.Connection;
using SandRibbonInterop;
using SandRibbonObjects;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Utils;
using System.ComponentModel;

namespace SandRibbon.Components
{
    public partial class SimpleSlideDisplay : UserControl, ISlideDisplay
    {
        public string me;
        public int currentSlideIndex = -1;
        public int currentSlideId = -1;
        public ObservableCollection<ThumbnailInformation> thumbnailList = new ObservableCollection<ThumbnailInformation>();
        private ConversationDetails conversationDetails;
        public DelegateCommand<SandRibbon.Utils.Connection.JabberWire.Credentials> setAuthor;
        public bool synced = false;
        public bool isAuthor = false;
        private int ThumbnailDetail = 265;
        private int realLocation;
        public SimpleSlideDisplay()
        {
            InitializeComponent();
            setAuthor = new DelegateCommand<SandRibbon.Utils.Connection.JabberWire.Credentials>(
                author => me = author.name);
            Commands.SetThumbnailDetail.Execute(ThumbnailDetail);
            Commands.SetIdentity.RegisterCommand(setAuthor);
            Commands.SyncedMoveRequested.RegisterCommand(new DelegateCommand<int>(moveToTeacher));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>((slide)=>{
                                          Dispatcher.Invoke((Action) delegate
                                             {//The real location may not be a displayable thumb
                                                 realLocation = slide;
                                                 var visibility = Visibility.Collapsed;
                                                 var typeOfDestination = conversationDetails.Slides.Where(s => s.id == slide).Select(s => s.type).FirstOrDefault();
                                                 if (typeOfDestination != null && typeOfDestination == Slide.TYPE.POLL)
                                                     visibility = Visibility.Visible;
                                                 var currentSlide = (ThumbnailInformation)slides.SelectedItem;
                                                 if ( currentSlide == null || currentSlide.slideId !=slide)
                                                 {
                                                     slides.SelectedIndex =thumbnailList.Select(s=>s.slideId).ToList().IndexOf(slide);
                                                     if(slides.SelectedIndex != -1)
                                                        currentSlideIndex = slides.SelectedIndex;
                                                 }
                                                 slides.ScrollIntoView(slides.SelectedItem);
                                              });
            }, slideInConversation));
            Commands.MoveToQuiz.RegisterCommand(new DelegateCommand<QuizDetails>(d=>realLocation = d.targetSlide));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(_arg =>
            {
                currentSlideIndex = 0;
                slides.SelectedIndex = 0;
            }));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<SandRibbonObjects.ConversationDetails>(Display));
            Commands.SetSync.RegisterCommand(new DelegateCommand<object>(_obj => synced = !synced));
            Commands.ThumbnailAvailable.RegisterCommand(new DelegateCommand<int>(id=>loadThumbnail(id)));
        }
        private void moveToTeacher(int where)
        {
            Dispatcher.Invoke((Action)delegate
            {
                if (!isAuthor && thumbnailList.Where(t => t.slideId == where).Count() == 1 && synced)
                    Commands.MoveTo.Execute(where);
            });
        }
        private bool slideInConversation(int slide)
        {
            var result = conversationDetails.Slides.Select(t => t.id).Contains(slide);
            return result;
        }
        private void isPrevious(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = slides != null && slides.SelectedIndex > 0;
        }
        private void moveToPrevious(object sender, ExecutedRoutedEventArgs e)
        {
            var previousIndex = slides.SelectedIndex - 1;
            slides.SelectedIndex = previousIndex;
        }
        private void isNext(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = slides != null && slides.SelectedIndex < thumbnailList.Count() - 1; 
        }
        private void moveToNext(object sender, ExecutedRoutedEventArgs e)
        {
            var nextIndex = slides.SelectedIndex + 1;
            slides.SelectedIndex = nextIndex;
        }
        public void Display(ConversationDetails details)
        {
            Dispatcher.Invoke((Action) delegate
            {
                conversationDetails = details;
                if (me == details.Author)
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
                                    slideNumber = details.Slides.Where(s=>s.type==Slide.TYPE.SLIDE).ToList().IndexOf(slide) + 1,
                                });
                    }
                }
                slides.ItemsSource = thumbs;
                slides.SelectedIndex = currentSlideIndex;
                if (slides.SelectedIndex == -1)
                    slides.SelectedIndex = 0;
                thumbnailList = thumbs;
                foreach(var thumb in thumbnailList)
                    loadThumbnail(thumb.slideId);
            });
            Commands.RequerySuggested(Commands.MoveTo);
        }
        private void slides_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)sender).SelectedItem != null)
            {
                if(e.RemovedItems.Count > 0){
                    var oldThumb = (ThumbnailInformation)e.RemovedItems[0];
                    Commands.PrinterThumbnail.Execute(oldThumb);
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
            Dispatcher.Invoke((Action)delegate
            {
                var directory = Directory.GetCurrentDirectory();
                var unknownSlidePath = directory + "\\Resources\\slide_Not_Loaded.png";
                var path = directory + "\\thumbs\\"+slideId+".png";
                ImageSource thumbnailSource;
                if (File.Exists(path))
                    thumbnailSource = loadedCachedImage(path);
                else
                    thumbnailSource = loadedCachedImage(unknownSlidePath);
                var brush = new ImageBrush(thumbnailSource);
                var thumb = thumbnailList.Where(t => t.slideId == slideId).FirstOrDefault();
                if (thumb != null) thumb.thumbnail = brush;
            });
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