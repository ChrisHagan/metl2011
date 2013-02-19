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
    public class SlideToThumbConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[1] != DependencyProperty.UnsetValue)
            {
                var source = (Image)values[0];
                var id = (int)values[1];
                var itemContainer = (FrameworkElement)values[2];
                if (itemContainer.IsVisible)
                {
                    ThumbnailProvider.thumbnail(source, id);
                }
            }
            return ThumbnailProvider.emptyImage;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public partial class SlideDisplay : UserControl, ISlideDisplay
    {
        private int myMaxSlideIndex;
        public static readonly DependencyProperty TeachersCurrentSlideIndexProperty =
            DependencyProperty.Register("TeachersCurrentSlideIndex", typeof(int), typeof(SlideDisplay), new PropertyMetadata(default(int), new PropertyChangedCallback(OnTeachersCurrentSlideIndexChanged)));

        public int TeachersCurrentSlideIndex
        {
            get { return (int) GetValue(TeachersCurrentSlideIndexProperty); }
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
            DependencyProperty.Register("IsNavigationLocked", typeof (bool), typeof (SlideDisplay), new PropertyMetadata(default(bool)));

        public bool IsNavigationLocked
        {
            get { return (bool) GetValue(IsNavigationLockedProperty); }
            set { SetValue(IsNavigationLockedProperty, value); }
        }
        public int currentSlideId = -1;
        public ObservableCollection<Slide> thumbnailList { get; set; } 
       // public static Dictionary<int, PreParser> parsers = new Dictionary<int, PreParser>();
       // public static Dictionary<int, PreParser> privateParsers = new Dictionary<int, PreParser>();
        public static SlideIndexConverter SlideIndex;
        public static SlideToThumbConverter SlideToThumb;
        private bool moveTo;
        public SlideDisplay()
        {
            thumbnailList = new ObservableCollection<Slide>();
            thumbnailList.CollectionChanged += OnThumbnailCollectionChanged;
            SlideIndex = new SlideIndexConverter(thumbnailList);
            SlideToThumb = new SlideToThumbConverter();
            myMaxSlideIndex = -1;
            TeachersCurrentSlideIndex = -1;
            IsNavigationLocked = calculateNavigationLocked();
            InitializeComponent();
            DataContext = this;
            slides.PreviewKeyDown += new KeyEventHandler(KeyPressed);
            Commands.SyncedMoveRequested.RegisterCommandToDispatcher(new DelegateCommand<int>(MoveToTeacher));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>((slideIndex) => MoveTo(slideIndex, false), slideInConversation));
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
            Dispatcher.adopt(delegate
            {
                Display(Globals.conversationDetails);
            });
            var paste = new CompositeCommand();
            paste.RegisterCommand(new DelegateCommand<object>(HandlePaste));
            slides.InputBindings.Add(new KeyBinding(paste, Key.V, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(paste, Key.V, ModifierKeys.Control));
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
            if((e.Key == Key.PageUp || e.Key == Key.Up) && Commands.MoveToPrevious.CanExecute(null))
            {
                Commands.MoveToPrevious.Execute(null);
                e.Handled = true;
            }
            if ((e.Key == Key.PageDown || e.Key == Key.Down)&& Commands.MoveToNext.CanExecute(null))
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
                catch(Exception)
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
            MeTLLib.ClientFactory.Connection().AppendSlideAfter(Globals.slide, Globals.conversationDetails.Jid);
            moveTo = true;
        }
        private bool isSlideInSlideDisplay(int slide)
        {
            return thumbnailList.Any(t => t.id == slide);
        }
        private int indexOf(int slide)
        {
            return thumbnailList.Select(s => s.id).ToList().IndexOf(slide);
        }

        private void MoveTo(int slide, bool forceRefresh)
        {
            Dispatcher.adopt((Action) delegate
            {
                //if (isSlideInSlideDisplay(slide))
                //{
                    myMaxSlideIndex = calculateMaxIndex(myMaxSlideIndex, indexOf(slide));
                    currentSlideId = slide;
                    refreshSelectedIndex(slide,forceRefresh);
                    /*
                    var currentSlide = (Slide)slides.SelectedItem;
                    if (currentSlide == null || forceRefresh || currentSlide.id != slide)
                    {
                        slides.SelectedIndex = indexOf(slide);
                        slides.ScrollIntoView(slides.SelectedItem);
                    }
                     */
                //}
            });
            //Commands.RequerySuggested(Commands.MoveToNext);
            //Commands.RequerySuggested(Commands.MoveToPrevious);
        }

        private int calculateMaxIndex(int myIndex, int index)
        {
            return myIndex > index ? myIndex : index;
        }

        public static readonly DependencyProperty LastSlideIndexProperty = DependencyProperty.Register( "LastSlideIndex", typeof(int), typeof(SlideDisplay)); 
        public int LastSlideIndex
        {
            get { return (int)GetValue(LastSlideIndexProperty); }
            set { SetValue(LastSlideIndexProperty, value); }
        }

        private const int defaultFirstSlideIndex = 0;
        public static readonly DependencyProperty FirstSlideIndexProperty = DependencyProperty.Register( "FirstSlideIndex", typeof(int), typeof(SlideDisplay), new PropertyMetadata(defaultFirstSlideIndex)); 
        public int FirstSlideIndex
        {
            get { return (int)GetValue(FirstSlideIndexProperty); }
            set { SetValue(FirstSlideIndexProperty, value); }
        }

        private void MoveToTeacher(int where)
        {
            if (Globals.isAuthor) return;
            TeachersCurrentSlideIndex = calculateTeacherSlideIndex(myMaxSlideIndex, where.ToString());
            Commands.RequerySuggested(Commands.MoveToNext);
            Commands.RequerySuggested(Commands.MoveToPrevious);
            if (!Globals.synched) return;
            var slide = Globals.slide;
            if (where == slide) return; // don't move if we're already on the slide requested
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
           //     thumbnailList.First(s => s.id == slide.id).refreshIndex();
                thumbnailList.Add(slide);
            }
            refreshSelectedIndex(-1,false);    
            //thumbnailList = thumbnailList.OrderBy(s => s.index) as ObservableCollection<Slide>;
        }
        private Timer refreshSelectedIndexTimer;
        private object timerLockObject = new Object();
        public void refreshSelectedIndex(int proposedSlideId, bool forceRefresh){
            lock (timerLockObject) {
              if (refreshSelectedIndexTimer != null)
              {
                  refreshSelectedIndexTimer.Change(Timeout.Infinite, Timeout.Infinite);
                  refreshSelectedIndexTimer.Dispose();
                  refreshSelectedIndexTimer = null;
              }
              if (refreshSelectedIndexTimer == null)
              {
                  refreshSelectedIndexTimer = new System.Threading.Timer(s =>
                  {
                      Dispatcher.adopt((Action)delegate
                      {
                          try
                          {

                              var currentSlide = 0;
                              if (proposedSlideId != -1 && proposedSlideId > 0)
                                  currentSlide = proposedSlideId;
                              else currentSlide = Globals.location.currentSlide;
                              var currentIndex = indexOf(currentSlide);
                              bool moveRequired = false;
                              if (moveTo)
                              {
                                  currentIndex++;
                                  moveTo = false;
                                  moveRequired = true;
                              }
                              if (isSlideInSlideDisplay(currentSlide))
                              {
                                  if (forceRefresh || slides.SelectedItem == null || slides.SelectedIndex == -1 || (slides.SelectedItem != null && ((Slide)slides.SelectedItem).id != Globals.location.currentSlide) || moveRequired)
                                  {
                                      slides.SelectedIndex = currentIndex;
                                      if (slides.SelectedIndex == -1)
                                          slides.SelectedIndex = 0;
                                      slides.ScrollIntoView(slides.SelectedItem);
                                  }
                                Commands.RequerySuggested(Commands.MoveToNext);
                                Commands.RequerySuggested(Commands.MoveToPrevious);
                              }
                              else
                              {
                                  Console.WriteLine("Slide isn't in display");
                            //      refreshSelectedIndexTimer.Change(50, Timeout.Infinite);
                              }
                          }
                          catch (Exception)
                          {
                              refreshSelectedIndexTimer.Change(50, Timeout.Infinite);
                          }
                      });
                  }, null, Timeout.Infinite, Timeout.Infinite);
              }
              refreshSelectedIndexTimer.Change(50, Timeout.Infinite);
           }
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
            if (string.IsNullOrEmpty(details.Jid) || !(Globals.credentials.authorizedGroups.Select(s => s.groupKey.ToLower()).Contains(details.Subject.ToLower())))
            {
                thumbnailList.Clear();
                return;
            }
            Commands.RequestTeacherStatus.Execute(new TeacherStatus{Conversation = Globals.conversationDetails.Jid, Slide="0", Teacher = Globals.conversationDetails.Author});
            IsNavigationLocked = calculateNavigationLocked();
            Commands.RequerySuggested(Commands.MoveToNext);
            Commands.RequerySuggested(Commands.MoveToPrevious);
            Slide firstSlide = null;
            if (thumbnailList.Count == 0)
            {
                foreach (var slide in details.Slides.OrderBy(s => s.index).Where(slide => slide.type == Slide.TYPE.SLIDE))
                {
                    if (firstSlide == null)
                        firstSlide = slide;
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
            var currentSlideIndex = indexOf(currentSlideId);
        
            if (firstSlide != null)
                refreshSelectedIndex(firstSlide.id, false);
            else
                refreshSelectedIndex(-1, false);
        }
        private bool isWithinTeachersRange(Slide possibleSlide)
        {
            return (!IsNavigationLocked || ( TeachersCurrentSlideIndex == -1 || possibleSlide.index <= TeachersCurrentSlideIndex));
        }
        private void slides_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var addedItems = e.AddedItems;
            if (addedItems.Count > 0)
            {
                var removedItems = e.RemovedItems;
                var selected = (Slide)addedItems[0];
                if (selected.id != currentSlideId){
                    if (isWithinTeachersRange(selected)){
                        currentSlideId = selected.id;
                        foreach (var slide in removedItems) ((Slide)slide).refresh();
                        AutomationSlideChanged(this, slides.SelectedIndex, indexOf(currentSlideId));

                        Commands.MoveTo.ExecuteAsync(currentSlideId);
                        SendSyncMove(currentSlideId);
                        refreshSelectedIndex(currentSlideId, false);
                        //slides.ScrollIntoView(selected);
                    } else if (sender is ListBox) {
                        if (removedItems.Count > 0){
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
        public SlideDisplayAutomationPeer(SlideDisplay control) : base(control)
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