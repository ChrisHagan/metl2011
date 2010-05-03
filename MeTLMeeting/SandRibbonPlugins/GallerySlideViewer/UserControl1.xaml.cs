using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon;
using System.Linq;
using System.Windows.Input;
using SandRibbon.Components.Interfaces;
using SandRibbonObjects;
using SandRibbonInterop;

namespace GallerySlideViewer
{
    public partial class PluginMain : System.Windows.Controls.UserControl, ISlideDisplay
    {
        public string me;
        public int currentSlideIndex = -1;
        public int currentSlideId = -1;
        public ObservableCollection<ThumbnailInformation> thumbnailList = new ObservableCollection<ThumbnailInformation>();
        public DelegateCommand<string> setAuthor;
        public string preferredTab = "Navigate";
        public bool synced = false;
        public bool isAuthor = false;
        private int ThumbnailDetail = 240;
        private ListBox slides;
        private GridSplitter currentGridSplitter;
        private Border currentSlideViewer;
        private ColumnDefinition GridSplitterColumnDefinition;
        private ColumnDefinition SlideViewerColumnDefinition;

        public PluginMain()
        {
            InitializeComponent();
            slides = (ListBox)FindResource("slidesControl");
            setAuthor = new DelegateCommand<string>((author) => me = author);
            Commands.SetThumbnailDetail.Execute(ThumbnailDetail);
            Commands.SetIdentity.RegisterCommand(setAuthor);
            Commands.SyncedMoveRequested.RegisterCommand(new DelegateCommand<int>(moveToTeacher));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>((slide) =>
            {
                Dispatcher.Invoke((Action)delegate
                   {
                       var currentSlide = (ThumbnailInformation)slides.SelectedItem;
                       if (currentSlide == null || currentSlide.slideId != slide)
                       {
                           slides.SelectedIndex = thumbnailList.Select(s => s.slideId).ToList().IndexOf(slide);
                           if (slides.SelectedIndex != -1)
                               currentSlideIndex = slides.SelectedIndex;
                           slides.ScrollIntoView(slides.SelectedItem);
                       }
                   });
            }, slideInConversation));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(_arg =>
            {
                currentSlideIndex = 0;
                slides.SelectedIndex = 0;
            }));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<SandRibbonObjects.ConversationDetails>(Display));
            Commands.SetSync.RegisterCommand(new DelegateCommand<object>(_obj => synced = !synced));
            Commands.ThumbnailAvailable.RegisterCommand(new DelegateCommand<int>(id => loadThumbnail(id)));
        }
        private void moveToTeacher(int where)
        {
            if (thumbnailList.Where(t => t.slideId == where).Count() == 1 && synced && !isAuthor)
                Commands.MoveTo.Execute(where);
        }
        private bool slideInConversation(int slide)
        {
            return thumbnailList.Select(t => t.slideId).Contains(slide);
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
            Dispatcher.Invoke((Action)delegate
               {
                   if (me == details.Author)
                       isAuthor = true;
                   else
                       isAuthor = false;
                   thumbnailList = new ObservableCollection<ThumbnailInformation>();
                   foreach (var slide in details.Slides)
                   {
                       thumbnailList.Add(
                           new ThumbnailInformation
                               {
                                   slideId = slide.id,
                                   slideNumber = details.Slides.IndexOf(slide) + 1
                               });
                       loadThumbnail(slide.id);
                   }
                   slides.ItemsSource = thumbnailList;
                   slides.SelectedIndex = currentSlideIndex;
                   if (slides.SelectedIndex == -1)
                       slides.SelectedIndex = 0;
               });
        }
        private void slides_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)sender).SelectedItem != null)
            {
                if (e.RemovedItems.Count > 0)
                {
                    var oldThumb = (ThumbnailInformation)e.RemovedItems[0];
                    Commands.PrinterThumbnail.Execute(oldThumb);
                }
                var newThumb = (ThumbnailInformation)(((ListBox)sender).SelectedItem);
                var proposedIndex = ((ListBox)sender).SelectedIndex;
                var proposedId = newThumb.slideId;
                if (proposedId == currentSlideId) return;
                currentSlideIndex = proposedIndex;
                currentSlideId = proposedId;
                slides.ScrollIntoView(newThumb);
                Commands.MoveTo.Execute(newThumb.slideId);
            }
        }
        private void showHistoryProgress(object sender, ExecutedRoutedEventArgs e)
        {
            var progress = (int[])e.Parameter;
        }
        private void toggleSync(object sender, RoutedEventArgs e)
        {
            Commands.SetSync.Execute(null);
            BitmapImage source;
            var synced = new Uri(Directory.GetCurrentDirectory() + "\\Resources\\SyncRed.png");
            var deSynced = new Uri(Directory.GetCurrentDirectory() + "\\Resources\\SyncGreen.png");
            if (syncButton.Icon.ToString().Contains("SyncGreen"))
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
                var path = directory + "\\thumbs\\" + slideId + ".png";
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
            bi.BeginInit();
            bi.UriSource = new Uri(uri);
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bi.EndInit();
            return bi;
        }

        private void OpenPopup(object sender, RoutedEventArgs e)
        {
            if (currentGridSplitter == null
                && currentSlideViewer == null
                && SlideViewerColumnDefinition == null
                && GridSplitterColumnDefinition == null)
            {
                try
                {
                    Border SlideViewerBorder = (Border)FindResource("SlideViewer");
                    Window ParentWindow = (Window.GetWindow(this));
                    Panel layoutGrid = (Panel)ParentWindow.Content;
                    ColumnDefinition GridSplitterColumn = new ColumnDefinition();
                    ColumnDefinition SlideViewerColumn = new ColumnDefinition();
                    SlideViewerColumn.Width = new GridLength(260);
                    GridSplitter slideViewerGridSplitter = (GridSplitter)FindResource("slideViewerGridSplitter");
                    ((Grid)layoutGrid).ColumnDefinitions.Insert(((Grid)layoutGrid).ColumnDefinitions.Count(), GridSplitterColumn);
                    ((Grid)layoutGrid).ColumnDefinitions.Insert(((Grid)layoutGrid).ColumnDefinitions.Count(), SlideViewerColumn);
                    ((Grid)(SlideViewerBorder.Child)).Children.Add(slides);
                    layoutGrid.Children.Add(slideViewerGridSplitter);
                    layoutGrid.Children.Add(SlideViewerBorder);
                    currentGridSplitter = slideViewerGridSplitter;
                    currentSlideViewer = SlideViewerBorder;
                    SlideViewerColumnDefinition = SlideViewerColumn;
                    GridSplitterColumnDefinition = GridSplitterColumn;
                    slides.ScrollIntoView(slides.SelectedItem);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error on opening GallerySlideViewer: " + ex);
                }
            }
            else
            {
                ClosePopup(sender, e);
            }
        }
        private void ClosePopup(object sender, RoutedEventArgs e)
        {
            try
            {
                Window ParentWindow = (Window.GetWindow(this));
                Panel layoutGrid = (Panel)ParentWindow.Content;
                if (GridSplitterColumnDefinition != null)
                {
                    ((Grid)layoutGrid).ColumnDefinitions.Remove(GridSplitterColumnDefinition);
                    GridSplitterColumnDefinition = null;
                }
                if (SlideViewerColumnDefinition != null)
                {
                    ((Grid)layoutGrid).ColumnDefinitions.Remove(SlideViewerColumnDefinition);
                    SlideViewerColumnDefinition = null;
                }
                if (currentGridSplitter != null)
                {
                    layoutGrid.Children.Remove(currentGridSplitter);
                    currentGridSplitter = null;
                }
                if (currentSlideViewer != null)
                {
                    ((Grid)(currentSlideViewer.Child)).Children.Remove(slides);
                    layoutGrid.Children.Remove(currentSlideViewer);
                    currentSlideViewer = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error on closing GallerySlideViewer: " + ex);
            }
        }
    }
}