using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Interfaces;
using SandRibbon.Components.Utility;
using SandRibbon.Providers;
using MeTLLib.DataTypes;
using System.Collections.Generic;
using MeTLLib.Providers.Connection;
using System.Windows.Data;
using SandRibbon.Utils;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Automation;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media;
using MeTLLib.Utilities;
using System.ComponentModel;
using System.Windows.Threading;
using SandRibbon.Pages.Collaboration;
using SandRibbon.Pages;

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
    public partial class SlideDisplay : UserControl, ISlideDisplay
    {
        private DispatcherTimer refresher;
        private int myMaxSlideIndex;
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
        public ObservableCollection<Slide> thumbnailList { get; set; }
        public static SlideIndexConverter SlideIndex;
        public SlideAwarePage rootPage { get; protected set; }
        protected TimeSpan shortRefresh = new TimeSpan(0, 0, 1);
        protected TimeSpan longRefresh = new TimeSpan(0, 0, 10);
        public SlideDisplay()
        {
            InitializeComponent();

            var moveToTeacherCommand = new DelegateCommand<int>(MoveToTeacher);
            var forcePageRefreshCommand = new DelegateCommand<int>((slideIndex) => QueryReachable(slideIndex, true), slideInConversation);
            var updateConversationDetailsCommand = new DelegateCommand<ConversationDetails>(Display);
            var addSlideCommand = new DelegateCommand<object>(addSlide, canAddSlide);
            var moveToNextCommand = new DelegateCommand<object>(selectNext, isNext);
            var moveToPreviousCommand = new DelegateCommand<object>(selectPrevious, isPrevious);
            var receiveTeacherStatusCommand = new DelegateCommand<TeacherStatus>(receivedStatus);
            var editConversationCommand = new DelegateCommand<object>(EditConversation);
            var updateNewSlideOrderCommand = new DelegateCommand<int>(reorderSlides);
            refresher = new DispatcherTimer();
            refresher.Interval = shortRefresh;
            refresher.Tick += new EventHandler(refresherTick);
            Loaded += (s, e) =>
            {
                if (rootPage == null)
                {
                    rootPage = DataContext as SlideAwarePage;
                }
                slides.ItemsSource = new ObservableCollection<Slide>(rootPage.ConversationDetails.Slides);
                /*Observe this ordering or you'll fall into an infinite loop*/
                slides.SelectedItem = rootPage.Slide;
                slides.SelectionChanged += slides_SelectionChanged;

                SlideIndex = new SlideIndexConverter();
                myMaxSlideIndex = -1;
                TeachersCurrentSlideIndex = -1;
                IsNavigationLocked = calculateNavigationLocked();                
                slides.PreviewKeyDown += new KeyEventHandler(KeyPressed);

                Commands.SyncedMoveRequested.RegisterCommandToDispatcher(moveToTeacherCommand);
                Commands.ForcePageRefresh.RegisterCommand(forcePageRefreshCommand);
                Commands.UpdateConversationDetails.RegisterCommandToDispatcher(updateConversationDetailsCommand);
                Commands.AddSlide.RegisterCommand(addSlideCommand);
                Commands.MoveToNext.RegisterCommand(moveToNextCommand);
                Commands.MoveToPrevious.RegisterCommand(moveToPreviousCommand);
                Commands.ReceiveTeacherStatus.RegisterCommandToDispatcher(receiveTeacherStatusCommand);
                Commands.EditConversation.RegisterCommandToDispatcher(editConversationCommand);
                Commands.UpdateNewSlideOrder.RegisterCommandToDispatcher(updateNewSlideOrderCommand);
                refresherTick(this, new EventArgs());
            };
            Unloaded += (s, e) =>
            {
                refresher.Stop();
                Commands.SyncedMoveRequested.UnregisterCommand(moveToTeacherCommand);
                Commands.ForcePageRefresh.UnregisterCommand(forcePageRefreshCommand);
                Commands.UpdateConversationDetails.UnregisterCommand(updateConversationDetailsCommand);
                Commands.AddSlide.UnregisterCommand(addSlideCommand);
                Commands.MoveToNext.UnregisterCommand(moveToNextCommand);
                Commands.MoveToPrevious.UnregisterCommand(moveToPreviousCommand);
                Commands.ReceiveTeacherStatus.UnregisterCommand(receiveTeacherStatusCommand);
                Commands.EditConversation.UnregisterCommand(editConversationCommand);
                Commands.UpdateNewSlideOrder.UnregisterCommand(updateNewSlideOrderCommand);
            };
        }

        void refresherTick(object _sender, EventArgs _e)
        {
            var shouldSpeedUp = false;
            try
            {
                var view = UIHelper.FindVisualChild<ScrollViewer>(slides);
                var generator = slides.ItemContainerGenerator;
                var context = rootPage.ConversationDetails.Slides.OrderBy(s => s.index).ToList();
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
            Globals.UpdatePresenceListing(new MeTLPresence
            {
                Joining = true,
                Who = status.Teacher,
                Where = status.Conversation
            });
            if (status.Conversation == rootPage.ConversationDetails.Jid && status.Teacher == rootPage.ConversationDetails.Author)
            {
                TeachersCurrentSlideIndex = calculateTeacherSlideIndex(myMaxSlideIndex, status.Slide);
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
            var d = rootPage.ConversationDetails;
            return !d.isAuthor(rootPage.NetworkController.credentials.name) &&
                   d.Permissions.NavigationLocked &&
                   Globals.AuthorOnline(d.Author) &&
                   Globals.AuthorInRoom(d.Author, d.Jid);
        }        
        private bool canAddSlide(object _slide)
        {
            if (rootPage.ConversationDetails.ValueEquals(ConversationDetails.Empty)) return false;
            if (String.IsNullOrEmpty(rootPage.NetworkController.credentials.name)) return false;
            return (rootPage.ConversationDetails.Permissions.studentCanPublish || rootPage.ConversationDetails.Author == rootPage.NetworkController.credentials.name);
        }
        private void addSlide(object _slide)
        {
            rootPage.NetworkController.client.AppendSlideAfter(rootPage.Slide.id, rootPage.ConversationDetails.Jid);
        }
        private bool isSlideInSlideDisplay(int slide)
        {
            return thumbnailList.Any(t => t.id == slide);
        }
        private int indexOf(int slide)
        {
            return thumbnailList.Select(s => s.id).ToList().IndexOf(slide);
        }

        private void QueryReachable(int slide, bool _forceRefresh)
        {
            myMaxSlideIndex = calculateMaxIndex(myMaxSlideIndex, indexOf(slide));
            checkMovementLimits();
        }

        private int calculateMaxIndex(int myIndex, int index)
        {
            return myIndex > index ? myIndex : index;
        }        
        private const int defaultFirstSlideIndex = 0;
        public static readonly DependencyProperty FirstSlideIndexProperty = DependencyProperty.Register("FirstSlideIndex", typeof(int), typeof(SlideDisplay), new PropertyMetadata(defaultFirstSlideIndex));
        public int FirstSlideIndex
        {
            get { return (int)GetValue(FirstSlideIndexProperty); }
            set { SetValue(FirstSlideIndexProperty, value); }
        }

        private void MoveToTeacher(int where)
        {
            if (rootPage.ConversationDetails.isAuthor(rootPage.NetworkController.credentials.name)) return;
            if (!rootPage.UserConversationState.Synched) return;
            if (where == rootPage.Slide.id) return; // don't move if we're already on the slide requested
            TeachersCurrentSlideIndex = calculateTeacherSlideIndex(myMaxSlideIndex, where.ToString());
            checkMovementLimits();
            var action = (Action)(() => Dispatcher.adoptAsync(() =>
                                                                  {
                                                                      var index = rootPage.ConversationDetails.Slides.First(s => s.id == where).index;
                                                                      slides.SelectedIndex = index;
                                                                  }));
            GlobalTimers.SetSyncTimer(action);
        }
        private bool slideInConversation(int slide)
        {
            return rootPage.ConversationDetails.Slides.Select(t => t.id).Contains(slide);
        }
        private bool isPrevious(object _object)
        {
            if (rootPage.ConversationDetails != null && rootPage.ConversationDetails != ConversationDetails.Empty && rootPage.Slide != null && rootPage.Slide != Slide.Empty)
            {
                var isAtStart = rootPage.ConversationDetails.Slides.Select(s => s.index).Min() == rootPage.Slide.index;
                var slideLockNav = rootPage.ConversationDetails.isAuthor(rootPage.NetworkController.credentials.name) || ((rootPage.Slide.index < Math.Max(myMaxSlideIndex, TeachersCurrentSlideIndex)) || !IsNavigationLocked);
                var canNav = !isAtStart && slideLockNav;
                return canNav;
            }
            else return false;
        }
        private void selectPrevious(object _object)
        {
            if(isPrevious(_object))slides.SelectedIndex -= 1;            
        }
        private bool isNext(object _object)
        {
            if (rootPage.ConversationDetails != null && rootPage.ConversationDetails != ConversationDetails.Empty && rootPage.Slide != null && rootPage.Slide != Slide.Empty)
            {
                var isAtEnd = rootPage.ConversationDetails.Slides.Select(s => s.index).Max() == rootPage.Slide.index;
                var slideLockNav = rootPage.ConversationDetails.isAuthor(rootPage.NetworkController.credentials.name) || ((rootPage.Slide.index < Math.Max(myMaxSlideIndex, TeachersCurrentSlideIndex)) || !IsNavigationLocked);
                var canNav = !isAtEnd && slideLockNav;
                return canNav;
            }
            else return false;            
        }        
        private void selectNext(object _object)
        {
            if(isNext(_object)) slides.SelectedIndex += 1;            
        }
        private void reorderSlides(int conversationJid)
        {
            if (rootPage.ConversationDetails.Jid != conversationJid.ToString()) return;
            IsNavigationLocked = calculateNavigationLocked();
            var details = rootPage.ConversationDetails;
            thumbnailList.Clear();
            foreach (var slide in details.Slides.OrderBy(s => s.index).Where(slide => slide.type == Slide.TYPE.SLIDE))
            {
                thumbnailList.Add(slide);
            }
            checkMovementLimits();
        }
        public void checkMovementLimits()
        {
            Commands.RequerySuggested(Commands.MoveToNext);
            Commands.RequerySuggested(Commands.MoveToPrevious);
        }
        public void EditConversation(object _obj)
        {
            var editConversation = new EditConversation(rootPage.ConversationDetails, rootPage.NetworkController);
            editConversation.Owner = Window.GetWindow(this);
            editConversation.ShowDialog();
        }
        public void Display(ConversationDetails details)
        {//We only display the details of our current conversation (or the one we're entering)
            if (details.IsEmpty)
                return;
            if (string.IsNullOrEmpty(details.Jid) || !details.UserHasPermission(rootPage.NetworkController.credentials))
            {
                thumbnailList.Clear();
                return;
            }
            Commands.RequestTeacherStatus.Execute(new TeacherStatus { Conversation = rootPage.ConversationDetails.Jid, Slide = "0", Teacher = rootPage.ConversationDetails.Author });
            IsNavigationLocked = calculateNavigationLocked();
            checkMovementLimits();
            if (thumbnailList.Count == 0)
            {
                var joined = false;
                foreach (var slide in details.Slides.OrderBy(s => s.index).Where(slide => slide.type == Slide.TYPE.SLIDE))
                {
                    if (!joined)
                    {
                        slides.SelectedItem = slide;
                        joined = true;
                    }
                    thumbnailList.Add(slide);
                }
            }
            else if (thumbnailList.Count < details.Slides.Count)
            {
                var newSlides = details.Slides.Where(s => !thumbnailList.Contains(s)).ToList();
                foreach (var newSlide in newSlides)
                    thumbnailList.Insert(newSlide.index, newSlide);
            }
            foreach (var slide in thumbnailList)
            {
                foreach (var relatedSlide in details.Slides.Where(s => s.id == slide.id))
                {
                    if (slide.index != relatedSlide.index)
                    {
                        slide.index = relatedSlide.index;
                        slide.refreshIndex();
                    }
                }
            }
        }
        private bool isWithinTeachersRange(Slide possibleSlide)
        {
            return (!IsNavigationLocked || (TeachersCurrentSlideIndex == -1 || possibleSlide.index <= TeachersCurrentSlideIndex));
        }
        private void slides_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var addedItems = e.AddedItems;
            if (addedItems.Count > 0)
            {
                var removedItems = e.RemovedItems;
                var selected = (Slide)addedItems[0];
                if (isWithinTeachersRange(selected))
                {                                        
                    Commands.SendSyncMove.ExecuteAsync(selected.id);                    
                    rootPage.NavigationService.Navigate(new RibbonCollaborationPage(rootPage.UserGlobalState, rootPage.UserServerState, rootPage.UserConversationState, rootPage.ConversationState, new UserSlideState(), rootPage.NetworkController, rootPage.ConversationDetails, (Slide)e.AddedItems[0]));
                }
                else if (sender is ListBox)
                {
                    if (removedItems.Count > 0)
                    {
                        ((ListBox)sender).SelectedItem = removedItems[0];
                    }
                }
            }
        }        
    }
}