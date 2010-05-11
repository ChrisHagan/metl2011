using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Windows.Controls.Primitives;
using SandRibbonObjects;
using System.Threading;
using SandRibbonInterop;
using SandRibbon.Utils;

namespace SandRibbon.Components.Sandpit
{
    public partial class S15Boards : UserControl
    {
        public static BoardPlacementConverter BOARD_PLACEMENT_CONVERTER = new BoardPlacementConverter();
        public static OnlineColorConverter ONLINE_COLOR_CONVERTER = new OnlineColorConverter();
        private static ConversationDetails currentConversation;
        private static ThumbnailInformation draggingSlide;
        public S15Boards()
        {
            InitializeComponent();
            Commands.SendWakeUp.RegisterCommand(new DelegateCommand<object>(_nothing=>{
                Commands.HideConversationSearchBox.Execute(null);
                Commands.ToggleFriendsVisibility.Execute(null);
                BoardManager.ClearBoards("S15");
                var boards = BoardManager.boards["S15"].ToList();
                boardDisplay.ItemsSource = boards;
                for (int i = 0; i < 5; i++)
                {
                    var user = boards[i].name;
                    Commands.SendPing.Execute(user);
                    Commands.SendMoveBoardToSlide.Execute(
                        new SandRibbon.Utils.Connection.JabberWire.BoardMove
                        {
                            boardUsername = user,
                            roomJid = BoardManager.DEFAULT_CONVERSATION.Slides[i].id
                        });
                }
            }));
            Commands.CloseBoardManager.RegisterCommand(new DelegateCommand<object>(
                _obj => Commands.ToggleFriendsVisibility.Execute(null)
            ));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(details =>
            {
                currentConversation = details;
                Display();
            }));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>(MoveTo));
        }
        private void MoveTo(int where) 
        {
            try
            {
                var selectedBoard = ((IEnumerable<Board>)boardDisplay.ItemsSource).Where(b => b.slide == where).FirstOrDefault();
                if (selectedBoard != null && selectedBoard.online)
                    moveTo(selectedBoard);
                else
                {
                    moveTo(100, 170);
                }
            }
            catch (Exception e) {
                Logger.Log(string.Format("Attempted to move avatar on S15 display excepted : {0}", e.Message));
            }
        }
        private void moveTo(Board board)
        {
            moveTo((board.y - BoardManager.AVATAR_HEIGHT / 2) + 40, (board.x - BoardManager.AVATAR_WIDTH / 2) + 60);
        }
        private void moveTo(double x, double y) {
            System.Windows.Controls.Canvas.SetTop(avatar, x);
            System.Windows.Controls.Canvas.SetLeft(avatar, y);
        }
        public void boardClicked(object sender, RoutedEventArgs e) {
            var board = (Board)((FrameworkElement)sender).DataContext;
            if (board.online)
            {
                if (draggingSlide != null)
                {
                    board.slide = draggingSlide.slideId;
                    draggingSlide = null;
                    stopPulsing();
                }
                else
                {
                    moveTo(board);
                    var targetSlide = board.slide;
                    DelegateCommand<int> onConversationJoined = null;
                    onConversationJoined = new DelegateCommand<int>(_nothing =>
                    {
                        Commands.MoveTo.Execute(
                            targetSlide);
                        Commands.JoinConversation.UnregisterCommand(onConversationJoined);
                    });
                    var desiredConversation = Slide.conversationFor(targetSlide).ToString();
                    if (currentConversation == null || currentConversation.Jid != desiredConversation)
                        Commands.JoinConversation.Execute(desiredConversation);
                    else
                        onConversationJoined.Execute(0);
                }
            }
        }
        public void Display()
        {//We only display the details of our current conversation (or the one we're entering)
            var doDisplay = (Action)delegate
            {

                var thumbs = new ObservableCollection<SandRibbonInterop.ThumbnailInformation>();
                foreach (var slide in currentConversation.Slides)
                {
                    if (slide.type == Slide.TYPE.SLIDE)
                    {
                        thumbs.Add(
                            new SandRibbonInterop.ThumbnailInformation
                                {
                                    slideId = slide.id,
                                    slideNumber = currentConversation.Slides.Where(s => s.type == Slide.TYPE.SLIDE).ToList().IndexOf(slide) + 1,
                                });
                    }
                }
                slides.ItemsSource = thumbs;
            };
            if (Thread.CurrentThread != Dispatcher.Thread)
                Dispatcher.BeginInvoke(doDisplay);
            else
                doDisplay();
            Commands.RequerySuggested(Commands.MoveTo);
        }
        private void slideItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var selectedSlide = (ThumbnailInformation)((FrameworkElement)sender).DataContext;
            if (draggingSlide == null || draggingSlide != selectedSlide)
            {
                pulse((FrameworkElement)slides.ItemContainerGenerator.ContainerFromItem(selectedSlide));
                foreach (var board in boardDisplay.ItemsSource)
                    pulse((FrameworkElement)boardDisplay.ItemContainerGenerator.ContainerFromItem(board));
                draggingSlide = selectedSlide;
            }
            else 
            {
                stopPulsing();
            }
        }
        private void pulse(FrameworkElement sender){ 
            var animationPulse = new DoubleAnimation
                                     {
                                         From = .3,
                                         To = 1,
                                         Duration = new Duration(TimeSpan.FromMilliseconds(400)),
                                         AutoReverse = true,
                                         RepeatBehavior = RepeatBehavior.Forever
                                     };
            sender.BeginAnimation(OpacityProperty, animationPulse); 
        }
        private void stopPulsing() 
        {
            foreach (var slide in slides.ItemsSource)
            {
                var container = ((FrameworkElement)slides.ItemContainerGenerator.ContainerFromItem(slide));
                container.BeginAnimation(OpacityProperty, null);
                container.Opacity = 1;
            }
            foreach (var board in boardDisplay.ItemsSource){
                var container = ((FrameworkElement)boardDisplay.ItemContainerGenerator.ContainerFromItem(board));
                container.BeginAnimation(OpacityProperty,null);
                container.Opacity = 1;
            }
        }
    }
    public class OnlineColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new SolidColorBrush((bool)value ? Colors.White : Colors.Black);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    public class BoardPlacementConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)value - BoardManager.DISPLAY_WIDTH / 2;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
