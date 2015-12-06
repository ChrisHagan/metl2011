using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Utility;
using SandRibbon.Providers;
using MeTLLib.DataTypes;
using System.Windows.Data;
using MeTLLib.Utilities;
using System.Windows.Threading;
using SandRibbon.Pages.Collaboration.Models;

namespace SandRibbon.Components
{
    public class SlideIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var slide = value as Slide;
            return slide.index + 1;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    public partial class SlideDisplay : UserControl
    {
        private DispatcherTimer refresher;
        public static readonly DependencyProperty TeachersCurrentSlideIndexProperty =
            DependencyProperty.Register("TeachersCurrentSlideIndex", typeof(int), typeof(SlideDisplay), new PropertyMetadata(default(int), new PropertyChangedCallback(OnTeachersCurrentSlideIndexChanged)));

        public int TeachersCurrentSlideIndex
        {
            get { return (int)GetValue(TeachersCurrentSlideIndexProperty); }
            set { SetValue(TeachersCurrentSlideIndexProperty, value); }
        }        
        private static void OnTeachersCurrentSlideIndexChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var slideDisplay = (SlideDisplay)obj;
            var oldValue = (int)args.OldValue;
            var newValue = (int)args.NewValue;
            var e = new RoutedPropertyChangedEventArgs<int>(oldValue, newValue, ValueChangedEvent);
            slideDisplay.OnValueChanged(e);
        }
        protected virtual void OnValueChanged(RoutedPropertyChangedEventArgs<int> args)
        {
            RaiseEvent(args);
        }
        #region Events
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            "ValueChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<int>), typeof(SlideDisplay));

        public event RoutedPropertyChangedEventHandler<int> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        #endregion

        public static readonly DependencyProperty IsNavigationLockedProperty =
            DependencyProperty.Register("IsNavigationLocked", typeof(bool), typeof(SlideDisplay), new PropertyMetadata(default(bool)));

        public bool IsNavigationLocked
        {
            get { return (bool)GetValue(IsNavigationLockedProperty); }
            set { SetValue(IsNavigationLockedProperty, value); }
        }        
        protected TimeSpan shortRefresh = new TimeSpan(0, 0, 1);
        protected TimeSpan longRefresh = new TimeSpan(0, 0, 10);
        public SlideDisplay()
        {
            InitializeComponent();            
            var moveToTeacherCommand = new DelegateCommand<int>(MoveToTeacher);            
            var addSlideCommand = new DelegateCommand<object>(addSlide, canAddSlide);
            var moveToNextCommand = new DelegateCommand<object>(moveToNext, isNext);
            var moveToPreviousCommand = new DelegateCommand<object>(moveToPrevious, isPrevious);
            var receiveTeacherStatusCommand = new DelegateCommand<TeacherStatus>(receivedStatus);            
            var updateNewSlideOrderCommand = new DelegateCommand<int>(reorderSlides);
            refresher = new DispatcherTimer();
            refresher.Interval = shortRefresh;
            refresher.Tick += new EventHandler(refresherTick);
            Loaded += (s, e) =>
            {                                                                
                IsNavigationLocked = calculateNavigationLocked();                
                slides.PreviewKeyDown += new KeyEventHandler(KeyPressed);
      
                Commands.SyncedMoveRequested.RegisterCommandToDispatcher(moveToTeacherCommand);              
                Commands.MoveToNext.RegisterCommand(moveToNextCommand);
                Commands.MoveToPrevious.RegisterCommand(moveToPreviousCommand);
                Commands.ReceiveTeacherStatus.RegisterCommandToDispatcher(receiveTeacherStatusCommand);                
                Commands.UpdateNewSlideOrder.RegisterCommandToDispatcher(updateNewSlideOrderCommand);
                refresherTick(this, new EventArgs());
            };
            Unloaded += (s, e) =>
            {
                refresher.Stop();
                Commands.SyncedMoveRequested.UnregisterCommand(moveToTeacherCommand);
                Commands.AddSlide.UnregisterCommand(addSlideCommand);
                Commands.MoveToNext.UnregisterCommand(moveToNextCommand);
                Commands.MoveToPrevious.UnregisterCommand(moveToPreviousCommand);
                Commands.ReceiveTeacherStatus.UnregisterCommand(receiveTeacherStatusCommand);                
                Commands.UpdateNewSlideOrder.UnregisterCommand(updateNewSlideOrderCommand);
            };
        }        
        void refresherTick(object _sender, EventArgs _e)
        {
            var rootPage = DataContext as DataContextRoot;
            var shouldSpeedUp = false;
            try
            {
                var view = UIHelper.FindVisualChild<ScrollViewer>(slides);
                var generator = slides.ItemContainerGenerator;
                var context = rootPage.ConversationState.Slides;
                if (view != null)
                {
                    var top = view.VerticalOffset;
                    var bottom = Math.Min(context.Count - 1, Math.Ceiling(top + view.ViewportHeight));
                    for (var i = (int)Math.Floor(top); i <= bottom; i++)
                    {
                        var id = context[i].id;
                        var container = generator.ContainerFromIndex(i);
                        if (container == null)
                        {
                            shouldSpeedUp = true;
                        }
                        else
                        {
                            shouldSpeedUp = false;
                            try
                            {
                                var slideImage = UIHelper.FindVisualChild<Image>(container);
                                if (slideImage == null)
                                    shouldSpeedUp = true;
                                rootPage.UserServerState.ThumbnailProvider.thumbnail(slideImage, id);
                            }
                            catch
                            {
                                shouldSpeedUp = true;
                            }
                        }
                    }
                }
            }
            catch
            {
                shouldSpeedUp = true;
            }
            finally
            {
                refresher.Interval = shouldSpeedUp ? shortRefresh : longRefresh;
                refresher.Stop();
                refresher.Start();
            }
        }        
        private void HandlePaste(object obj)
        {
            Commands.ClipboardManager.Execute(ClipboardAction.Paste);
        }
        private static void KeyPressed(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.PageUp || e.Key == Key.Up) && Commands.MoveToPrevious.CanExecute(null))
            {
                Commands.MoveToPrevious.Execute(null);
                e.Handled = true;
            }
            if ((e.Key == Key.PageDown || e.Key == Key.Down) && Commands.MoveToNext.CanExecute(null))
            {
                Commands.MoveToNext.Execute(null);
                e.Handled = true;
            }
        }
        private void receivedStatus(TeacherStatus status)
        {
            var rootPage = DataContext as DataContextRoot;
            Globals.UpdatePresenceListing(new MeTLPresence
            {
                Joining = true,
                Who = status.Teacher,
                Where = status.Conversation
            });
            if (status.Conversation == rootPage.ConversationState.Jid && status.Teacher == rootPage.ConversationState.Author)
            {
                IsNavigationLocked = calculateNavigationLocked();
            }
        }
        private int calculateTeacherSlideIndex(int myIndex, string jid)
        {
            try
            {
                var index = indexOf(Int32.Parse(jid));
                if (myIndex > index)
                    return myIndex;
                return index;
            }
            catch (Exception)
            {
                return 0;
            }
        }
        private bool calculateNavigationLocked()
        {
            var rootPage = DataContext as DataContextRoot;
            var d = rootPage.ConversationState;
            return !d.IsAuthor &&
                   !d.StudentsCanMoveFreely &&
                   Globals.AuthorOnline(d.Author) &&
                   Globals.AuthorInRoom(d.Author, d.Jid);
        }       
        private bool canAddSlide(object _slide)
        {
            var rootPage = DataContext as DataContextRoot;
            if (String.IsNullOrEmpty(rootPage.NetworkController.credentials.name)) return false;
            return rootPage.ConversationState.ICanPublish;
        }
        private void addSlide(object _slide)
        {
            var rootPage = DataContext as DataContextRoot;
            rootPage.NetworkController.client.AppendSlideAfter(rootPage.ConversationState.Slide.id, rootPage.ConversationState.Jid);
        }
        private bool isSlideInSlideDisplay(int slide)
        {
            var root = DataContext as DataContextRoot;
            return root.ConversationState.Slides.Any(s => s.id == slide);
        }
        private int indexOf(int slide)
        {
            var root = DataContext as DataContextRoot;
            return root.ConversationState.Slides.Where(s => s.id == slide).First().index;
        }                
        private const int defaultFirstSlideIndex = 0;        
        private void MoveToTeacher(int where)
        {
            var rootPage = DataContext as DataContextRoot;
            if (rootPage.ConversationState.IsAuthor) return;
            if (!rootPage.UserConversationState.Synched) return;
            if (where == rootPage.ConversationState.Slide.id) return; // don't move if we're already on the slide requested
            checkMovementLimits();
            var action = (Action)(() => Dispatcher.adoptAsync(() =>
                                                                  {
                                                                      try
                                                                      {
                                                                          var index = rootPage.ConversationState.Slides.First(s => s.id == where).index;
                                                                          slides.SelectedIndex = index;
                                                                          slides.ScrollIntoView(slides.SelectedItem);
                                                                      }
                                                                      catch (Exception)
                                                                      {
                                                                      }
                                                                  }));
            GlobalTimers.SetSyncTimer(action);
        }
        private bool slideInConversation(int slide)
        {
            var rootPage = DataContext as DataContextRoot;
            return rootPage.ConversationState.Slides.Select(t => t.id).Contains(slide);
        }
        private bool isPrevious(object _object)
        {
            var rootPage = DataContext as DataContextRoot;
            var isAtStart = rootPage.ConversationState.Slides.Select(s => s.index).Min() == rootPage.ConversationState.Slide.index;
            var slideLockNav = rootPage.ConversationState.IsAuthor || ((rootPage.ConversationState.Slide.index < TeachersCurrentSlideIndex) || !IsNavigationLocked);
            var canNav = !isAtStart && slideLockNav;// normalNav && slideLockNav;
            return canNav;
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
            var rootPage = DataContext as DataContextRoot;
            var isAtEnd = rootPage.ConversationState.Slides.Select(s => s.index).Max() == rootPage.ConversationState.Slide.index;
            var slideLockNav = rootPage.ConversationState.IsAuthor || ((rootPage.ConversationState.Slide.index < TeachersCurrentSlideIndex) || !IsNavigationLocked);
            var canNav = !isAtEnd && slideLockNav;// normalNav && slideLockNav;
            return canNav;
        }

        public void MoveToSlide(int slideIndex)
        {
            slides.SelectedIndex = slideIndex;
            slides.ScrollIntoView(slides.SelectedItem);
        }

        private void moveToNext(object _object)
        {
            var nextIndex = slides.SelectedIndex + 1;
            slides.SelectedIndex = nextIndex;
            slides.ScrollIntoView(slides.SelectedItem);
        }
        private void reorderSlides(int conversationJid)
        {
            var rootPage = DataContext as DataContextRoot;
            if (rootPage.ConversationState.Jid != conversationJid.ToString()) return;
            IsNavigationLocked = calculateNavigationLocked();                                    
            checkMovementLimits();
        }
        public void checkMovementLimits()
        {
            Commands.RequerySuggested(Commands.MoveToNext);
            Commands.RequerySuggested(Commands.MoveToPrevious);
        }                
        private bool isWithinTeachersRange(Slide possibleSlide)
        {
            return (!IsNavigationLocked || (TeachersCurrentSlideIndex == -1 || possibleSlide.index <= TeachersCurrentSlideIndex));
        }                
        public void SendSyncMove(int currentSlideId)
        {
            var rootPage = DataContext as DataContextRoot;
            if (rootPage.ConversationState.IsAuthor && rootPage.UserConversationState.Synched)
            {
                Commands.SendSyncMove.ExecuteAsync(currentSlideId);
            }
        }        
    }    
}