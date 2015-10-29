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

namespace SandRibbon.Components
{
    public class SlideIndexConverter : IValueConverter
    {
        private ObservableCollection<Slide> collection;
        public SlideIndexConverter(ObservableCollection<Slide> collection)
        {
            this.collection = collection;
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int)
                return ((int)value) + 1;
            else return "?";
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

        private static void AutomationSlideChanged(SlideDisplay slideDisplay, int oldValue, int newValue)
        {
            #region Automation events
            if (AutomationPeer.ListenerExists(AutomationEvents.PropertyChanged))
            {
                var peer = UIElementAutomationPeer.FromElement(slideDisplay) as SlideDisplayAutomationPeer;

                if (peer != null)
                {
                    peer.RaisePropertyChangedEvent(
                        RangeValuePatternIdentifiers.ValueProperty,
                        (double)oldValue,
                        (double)newValue);
                }
            }
            #endregion
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
        public int currentSlideId = -1;
        public ObservableCollection<Slide> thumbnailList { get; set; }
        public static SlideIndexConverter SlideIndex;
        public SlideDisplay()
        {
            refresher = new DispatcherTimer();
            refresher.Interval = new TimeSpan(0, 0, 5);
            refresher.Tick += new EventHandler(refresherTick);
            refresher.Start();
            thumbnailList = new ObservableCollection<Slide>();
            thumbnailList.CollectionChanged += OnThumbnailCollectionChanged;
            SlideIndex = new SlideIndexConverter(thumbnailList);
            myMaxSlideIndex = -1;
            TeachersCurrentSlideIndex = -1;
            IsNavigationLocked = calculateNavigationLocked();
            InitializeComponent();
            DataContext = this;
            slides.PreviewKeyDown += new KeyEventHandler(KeyPressed);
            Commands.SyncedMoveRequested.RegisterCommandToDispatcher(new DelegateCommand<int>(MoveToTeacher));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>((slideIndex) => MoveTo(slideIndex, true), slideInConversation));
            Commands.ForcePageRefresh.RegisterCommand(new DelegateCommand<int>((slideIndex) => MoveTo(slideIndex, true), slideInConversation));
            Commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<ConversationDetails>(Display));
            Commands.AddSlide.RegisterCommand(new DelegateCommand<object>(addSlide, canAddSlide));
            Commands.MoveToNext.RegisterCommand(new DelegateCommand<object>(moveToNext, isNext));
            Commands.MoveToPrevious.RegisterCommand(new DelegateCommand<object>(moveToPrevious, isPrevious));
            Commands.JoinConversation.RegisterCommandToDispatcher(new DelegateCommand<object>(JoinConversation));
            Commands.ReceiveTeacherStatus.RegisterCommandToDispatcher(new DelegateCommand<TeacherStatus>(receivedStatus, (_unused) => { return StateHelper.mustBeInConversation(); }));
            Commands.EditConversation.RegisterCommandToDispatcher(new DelegateCommand<object>(EditConversation));
            Commands.UpdateNewSlideOrder.RegisterCommandToDispatcher(new DelegateCommand<int>(reorderSlides));
            Commands.LeaveLocation.RegisterCommand(new DelegateCommand<object>(resetLocationLocals));
            var paste = new CompositeCommand();
            paste.RegisterCommand(new DelegateCommand<object>(HandlePaste));
            slides.InputBindings.Add(new KeyBinding(paste, Key.V, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(paste, Key.V, ModifierKeys.Control));
        }

        void refresherTick(object sender, EventArgs e)
        {
            var view = UIHelper.FindVisualChild<ScrollViewer>(slides);
            var generator = slides.ItemContainerGenerator;
            var context = Globals.conversationDetails.Slides.OrderBy(s => s.index).ToList();
            var top = view.VerticalOffset;
            var bottom = Math.Min(context.Count - 1, Math.Ceiling(top + view.ViewportHeight));
            for (var i = (int) Math.Floor(top); i <= bottom; i++)
            {
                var id = context[i].id;
                var container = generator.ContainerFromIndex(i);
                try {
                    ThumbnailProvider.thumbnail(UIHelper.FindVisualChild<Image>(container), id);
                } catch { }
            }
        }

        private void OnThumbnailCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var thumbnails = sender as ObservableCollection<Slide>;
            if (e.NewItems != null && e.NewItems.Count != 0)
            {
                LastSlideIndex = thumbnails.Count;
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
            if (status.Conversation == Globals.location.activeConversation && status.Teacher == Globals.conversationDetails.Author)
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
            return !Globals.isAuthor &&
                   Globals.conversationDetails.Permissions.NavigationLocked &&
                   Globals.AuthorOnline(Globals.conversationDetails.Author) &&
                   Globals.AuthorInRoom(Globals.conversationDetails.Author, Globals.conversationDetails.Jid);
        }
        private void resetLocationLocals(object _unused)
        {
            currentSlideId = -1;
        }
        private void JoinConversation(object obj)
        {
            myMaxSlideIndex = -1;
            TeachersCurrentSlideIndex = -1;
            thumbnailList.Clear();
        }

        private bool canAddSlide(object _slide)
        {
            var details = Globals.conversationDetails;
            if (details.ValueEquals(ConversationDetails.Empty)) return false;
            if (String.IsNullOrEmpty(Globals.me)) return false;
            return (details.Permissions.studentCanPublish || details.Author == Globals.me);
        }
        private void addSlide(object _slide)
        {
            App.controller.client.AppendSlideAfter(Globals.slide, Globals.conversationDetails.Jid);
        }
        private bool isSlideInSlideDisplay(int slide)
        {
            return thumbnailList.Any(t => t.id == slide);
        }
        private int indexOf(int slide)
        {
            return thumbnailList.Select(s => s.id).ToList().IndexOf(slide);
        }

        private void MoveTo(int slide, bool _forceRefresh)
        {
            myMaxSlideIndex = calculateMaxIndex(myMaxSlideIndex, indexOf(slide));
            currentSlideId = slide;
            checkMovementLimits();
        }

        private int calculateMaxIndex(int myIndex, int index)
        {
            return myIndex > index ? myIndex : index;
        }

        public static readonly DependencyProperty LastSlideIndexProperty = DependencyProperty.Register("LastSlideIndex", typeof(int), typeof(SlideDisplay));
        public int LastSlideIndex
        {
            get { return (int)GetValue(LastSlideIndexProperty); }
            set { SetValue(LastSlideIndexProperty, value); }
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
            if (Globals.isAuthor) return;
            if (!Globals.synched) return;
            if (where == Globals.slide) return; // don't move if we're already on the slide requested
            TeachersCurrentSlideIndex = calculateTeacherSlideIndex(myMaxSlideIndex, where.ToString());
            checkMovementLimits();
            var action = (Action)(() => Dispatcher.adoptAsync(() =>
                                                                  {
                                                                      try
                                                                      {
                                                                          var index = Globals.conversationDetails.Slides.First(s => s.id == where).index;
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
            var normalNav = slides != null && slides.SelectedIndex < thumbnailList.Count() - 1;
            var slideLockNav = Globals.isAuthor || ((IsNavigationLocked && slides.SelectedIndex < Math.Max(myMaxSlideIndex, TeachersCurrentSlideIndex) || !IsNavigationLocked));
            var canNav = normalNav && slideLockNav;
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
            if (Globals.conversationDetails.Jid != conversationJid.ToString()) return;
            IsNavigationLocked = calculateNavigationLocked();
            var details = Globals.conversationDetails;
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
            var editConversation = new EditConversation();
            editConversation.Owner = Window.GetWindow(this);
            editConversation.ShowDialog();
        }
        public void Display(ConversationDetails details)
        {//We only display the details of our current conversation (or the one we're entering)
            if (details.IsEmpty)
                return;
            if (string.IsNullOrEmpty(details.Jid) || !details.UserHasPermission(Globals.credentials))
            {
                thumbnailList.Clear();
                return;
            }
            Commands.RequestTeacherStatus.Execute(new TeacherStatus { Conversation = Globals.conversationDetails.Jid, Slide = "0", Teacher = Globals.conversationDetails.Author });
            IsNavigationLocked = calculateNavigationLocked();
            checkMovementLimits();
            if (thumbnailList.Count == 0)
            {
                var joined = false;
                foreach (var slide in details.Slides.OrderBy(s => s.index).Where(slide => slide.type == Slide.TYPE.SLIDE))
                {
                    if (!joined) {
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
                if (selected.id != currentSlideId)
                {
                    if (isWithinTeachersRange(selected))
                    {
                        currentSlideId = selected.id;
                        foreach (var slide in removedItems) ((Slide)slide).refresh();
                        AutomationSlideChanged(this, slides.SelectedIndex, indexOf(currentSlideId));

                        Commands.MoveTo.ExecuteAsync(currentSlideId);
                        SendSyncMove(currentSlideId);
                        checkMovementLimits();
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

        public static void SendSyncMove(int currentSlideId)
        {
            if (Globals.isAuthor && Globals.synched)
            {
                Commands.SendSyncMove.ExecuteAsync(currentSlideId);
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new SlideDisplayAutomationPeer(this);
        }
    }

    public class SlideDisplayAutomationPeer : FrameworkElementAutomationPeer, IRangeValueProvider
    {
        public SlideDisplayAutomationPeer(SlideDisplay control)
            : base(control)
        {
        }

        protected override string GetClassNameCore()
        {
            return "SlideDisplay";
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Slider;
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.RangeValue)
            {
                return this;
            }
            return base.GetPattern(patternInterface);
        }

        #region IRangeValueProvider members

        bool IRangeValueProvider.IsReadOnly
        {
            get { return !IsEnabled(); }
        }

        double IRangeValueProvider.LargeChange
        {
            get { return 1; }
        }

        double IRangeValueProvider.Maximum
        {
            get { return (double)Control.LastSlideIndex; }
        }

        double IRangeValueProvider.Minimum
        {
            get { return (double)Control.FirstSlideIndex; }
        }

        void IRangeValueProvider.SetValue(double value)
        {
            if (!IsEnabled())
            {
                throw new ElementNotEnabledException();
            }

            var slideIndex = (int)value;
            if (slideIndex < Control.FirstSlideIndex || slideIndex > Control.LastSlideIndex)
            {
                throw new ArgumentOutOfRangeException("value");
            }

            Control.MoveToSlide(slideIndex);
        }

        double IRangeValueProvider.SmallChange
        {
            get { return 1; }
        }

        double IRangeValueProvider.Value
        {
            get { return (double)Control.TeachersCurrentSlideIndex; }
        }

        #endregion

        private SlideDisplay Control
        {
            get
            {
                return (SlideDisplay)base.Owner;
            }
        }
    }
}