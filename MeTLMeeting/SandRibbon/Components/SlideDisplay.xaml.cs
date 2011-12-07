using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Interfaces;
using SandRibbon.Components.Utility;
using SandRibbon.Providers;
using MeTLLib.DataTypes;
using System.Collections.Generic;
using MeTLLib.Providers.Connection;
using System.Windows.Data;
using SandRibbon.Utils;

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
            //return collection.IndexOf((Slide)value) + 1;
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
            var source = (System.Windows.Controls.Image)values[0];
            var id = (int)values[1];
            ThumbnailProvider.thumbnail(source, id);
            return null;
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
            DependencyProperty.Register("TeachersCurrentSlideIndex", typeof (int), typeof (SlideDisplay), new PropertyMetadata(default(int)));

        public int TeachersCurrentSlideIndex
        {
            get { return (int) GetValue(TeachersCurrentSlideIndexProperty); }
            set { SetValue(TeachersCurrentSlideIndexProperty, value); }
        }
        public static readonly DependencyProperty IsNavigationLockedProperty =
            DependencyProperty.Register("IsNavigationLocked", typeof (bool), typeof (SlideDisplay), new PropertyMetadata(default(bool)));

        public bool IsNavigationLocked
        {
            get { return (bool) GetValue(IsNavigationLockedProperty); }
            set { SetValue(IsNavigationLockedProperty, value); }
        }
        public int currentSlideId = -1;
        public ObservableCollection<Slide> thumbnailList { get; set; } 
        public static Dictionary<int, PreParser> parsers = new Dictionary<int, PreParser>();
        public static Dictionary<int, PreParser> privateParsers = new Dictionary<int, PreParser>();
        public static SlideIndexConverter SlideIndex;
        public static SlideToThumbConverter SlideToThumb;
        private bool moveTo;
        public SlideDisplay()
        {
            thumbnailList = new ObservableCollection<Slide>();
            SlideIndex = new SlideIndexConverter(thumbnailList);
            SlideToThumb = new SlideToThumbConverter();
            myMaxSlideIndex = -1;
            TeachersCurrentSlideIndex = -1;
            IsNavigationLocked = calculateNavigationLocked();
            InitializeComponent();
            DataContext = this;
            Commands.SyncedMoveRequested.RegisterCommandToDispatcher(new DelegateCommand<int>(MoveToTeacher));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>(MoveTo, slideInConversation));
            Commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<ConversationDetails>(Display));
            Commands.AddSlide.RegisterCommand(new DelegateCommand<object>(addSlide, canAddSlide));
            Commands.MoveToNext.RegisterCommand(new DelegateCommand<object>(moveToNext, isNext));
            Commands.MoveToPrevious.RegisterCommand(new DelegateCommand<object>(moveToPrevious, isPrevious));
            Commands.JoinConversation.RegisterCommandToDispatcher(new DelegateCommand<object>(JoinConversation));
            Commands.ReceiveTeacherStatus.RegisterCommandToDispatcher(new DelegateCommand<TeacherStatus>(receivedStatus, (_unused) => { return StateHelper.mustBeInConversation(); }));
            Commands.EditConversation.RegisterCommandToDispatcher(new DelegateCommand<object>(EditConversation));
            Commands.UpdateNewSlideOrder.RegisterCommandToDispatcher(new DelegateCommand<int>(reorderSlides));
            Commands.LeaveLocation.RegisterCommand(new DelegateCommand<object>(resetLocationLocals));
            Display(Globals.conversationDetails);
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
            var newSlide = MeTLLib.ClientFactory.Connection().AppendSlideAfter(Globals.slide, Globals.conversationDetails.Jid);
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

        private void MoveTo(int slide)
        {
            Dispatcher.adopt(delegate
            {
                if (isSlideInSlideDisplay(slide))
                {
                    myMaxSlideIndex = calculateMaxIndex(myMaxSlideIndex, indexOf(slide));
                    var currentSlide = (Slide)slides.SelectedItem;
                    if (currentSlide == null || currentSlide.id != slide)
                    {
                        currentSlideId = slide;
                        slides.SelectedIndex = myMaxSlideIndex;
                        slides.ScrollIntoView(slides.SelectedItem);
                    }
                }
            });
            Commands.RequerySuggested(Commands.MoveToNext);
            Commands.RequerySuggested(Commands.MoveToPrevious);
        }

        private int calculateMaxIndex(int myIndex, int index)
        {
            return myIndex > index ? myIndex : index;
        }

        private void MoveToTeacher(int where)
        {

            TeachersCurrentSlideIndex = calculateTeacherSlideIndex(myMaxSlideIndex, where.ToString());
            if (Globals.isAuthor) return;
            if (!Globals.synched) return;
            var slide = Globals.slide;
            // don't move if we're already on the slide requested
            if (where == slide) return;

            var action = (Action)(() => Dispatcher.adoptAsync(() => Commands.MoveTo.ExecuteAsync(where)));
            GlobalTimers.SetSyncTimer(action, slide);
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

            var currentIndex = indexOf(Globals.location.currentSlide);
            
            slides.SelectedIndex = currentIndex; 
            if (slides.SelectedIndex == -1)
                slides.SelectedIndex = 0;
            slides.ScrollIntoView(slides.SelectedItem);
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
            if (string.IsNullOrEmpty(details.Jid) || !(Globals.credentials.authorizedGroups.Select(s => s.groupKey).Contains(details.Subject)))
            {
                thumbnailList.Clear();
                return;
            }
            Commands.RequestTeacherStatus.Execute(new TeacherStatus{Conversation = Globals.conversationDetails.Jid, Slide="0", Teacher = Globals.conversationDetails.Author});
            IsNavigationLocked = calculateNavigationLocked();
            if (thumbnailList.Count == 0)
            {
                foreach (var slide in details.Slides.OrderBy(s => s.index).Where(slide => slide.type == Slide.TYPE.SLIDE))
                {
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
            if (moveTo)
            {
                currentSlideIndex++;
                moveTo = false;
            }
            slides.SelectedIndex = currentSlideIndex;
            if (slides.SelectedIndex == -1)
                slides.SelectedIndex = 0;
            slides.ScrollIntoView(slides.SelectedItem);
        }
        private void slides_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var source = (ListBox)sender;
            var removedItems = e.RemovedItems;
            var addedItems = e.AddedItems;
            if (addedItems.Count > 0)
            {
                var selected = (Slide)addedItems[0];
                if (selected.id != currentSlideId)
                {
                    currentSlideId = selected.id;
                    foreach (var slide in removedItems) ((Slide)slide).refresh();
                    Commands.MoveTo.ExecuteAsync(currentSlideId);
                    SendSyncMove(currentSlideId);
                    slides.ScrollIntoView(selected);
                }
            }
        }

        public static void SendSyncMove(int currentSlideId)
        {
            if (Globals.isAuthor && Globals.synched)
                Commands.SendSyncMove.ExecuteAsync(currentSlideId);
        }
    }
}