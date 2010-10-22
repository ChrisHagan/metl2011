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
using SandRibbon.Providers;
using SandRibbonObjects;
using System.Threading;
using SandRibbonInterop;
using SandRibbon.Utils;
using MeTLLib.DataTypes;

namespace SandRibbon.Components.Sandpit
{
    public partial class S15Boards : UserControl
    {
        public static BoardPlacementConverter BOARD_PLACEMENT_CONVERTER = new BoardPlacementConverter();
        public static OnlineColorConverter ONLINE_COLOR_CONVERTER = new OnlineColorConverter();
        private static ThumbnailInformation draggingSlide;
        private static int currentSlide;
        private static Board NO_BOARD = new Board { 
            name = "S15-0",
            online = true,
            slide = 0,
            x = 80,
            y = 170
        };
        public S15Boards()
        {
            InitializeComponent();
            moveTo(NO_BOARD);
            /*Commands.SendWakeUp.RegisterCommand(new DelegateCommand<object>(arg=>{
                Commands.HideConversationSearchBox.ExecuteAsync(null);
                Commands.ToggleFriendsVisibility.ExecuteAsync(null);
                Commands.ChangeTab.ExecuteAsync("Home");
                BoardManager.ClearBoards("S15");
                var boards = BoardManager.boards["S15"].ToList();
                boardDisplay.ItemsSource = boards;
                for (int i = 0; i < 5; i++)
                {
                    var user = boards[i].name;
                    Commands.SendPing.ExecuteAsync(user);
                    Commands.SendMoveBoardToSlide.ExecuteAsync(
                        new SandRibbon.Utils.Connection.JabberWire.BoardMove
                        {
                            boardUsername = user,
                            roomJid = BoardManager.DEFAULT_CONVERSATION.Slides[i].id
                        });
                }
            }));*/
            Commands.SendMoveBoardToSlide.RegisterCommand(new DelegateCommand<BoardMove>(SendMoveBoardToSlide));
            Commands.CloseBoardManager.RegisterCommand(new DelegateCommand<object>(
                _obj => Commands.ToggleFriendsVisibility.ExecuteAsync(null)
            ));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(details => { if (details != null) Display(); }));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>(MoveTo));
        }
        private void SendMoveBoardToSlide(BoardMove where) 
        {
            if(where.boardUsername == ((Board)avatar.DataContext).name){
                if (currentSlide != where.roomJid)
                    moveTo(NO_BOARD);
            }
            else if (currentSlide == where.roomJid) { 
                var targetBoard = ((IEnumerable<Board>)boardDisplay.ItemsSource).Where(b => b.name == where.boardUsername).FirstOrDefault();
                if (targetBoard != null)
                    moveTo(targetBoard);
            }
        }
        private void MoveTo(int where) 
        {
            try
            {
                currentSlide = where;
                var selectedBoard = ((IEnumerable<Board>)boardDisplay.ItemsSource).Where(b => b.slide == where).FirstOrDefault();
                if (selectedBoard != null && selectedBoard.online)
                    moveTo(selectedBoard);
                else
                    moveTo(NO_BOARD);
            }
            catch (Exception e) {
                Logger.Log(string.Format("Attempted to move avatar on S15 display excepted : {0}", e.Message));
            }
        }
        private void moveTo(Board board)
        {
            moveTo((board.y - BoardManager.AVATAR_HEIGHT / 2) + 40, (board.x - BoardManager.AVATAR_WIDTH / 2) + 60);
            avatar.DataContext = board;
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
                    Commands.SendMoveBoardToSlide.ExecuteAsync(
                        new BoardMove
                        {
                            roomJid = board.slide,
                            boardUsername = board.name
                        });
                }
                else
                {
                    moveTo(board);
                    var targetSlide = board.slide;
                    DelegateCommand<int> onConversationJoined = null;
                    onConversationJoined = new DelegateCommand<int>(_nothing =>
                    {
                        Commands.MoveTo.ExecuteAsync(
                            targetSlide);
                        Commands.JoinConversation.UnregisterCommand(onConversationJoined);
                    });
                    var desiredConversation = MeTLLib.DataTypes.Slide.conversationFor(targetSlide).ToString();
                    try
                    {
                        if (Globals.conversationDetails.Jid != desiredConversation)
                            Commands.JoinConversation.ExecuteAsync(desiredConversation);
                        else
                           onConversationJoined.Execute(0);
                    }
                    catch(NotSetException ex)
                    {
                    }
                }
            }
        }
        public void Display()
        {//We only display the details of our current conversation (or the one we're entering)
           Dispatcher.adoptAsync(delegate
            {

                var thumbs = new ObservableCollection<ThumbnailInformation>();
                foreach (var slide in Globals.slides)
                {
                    if (slide.type == MeTLLib.DataTypes.Slide.TYPE.SLIDE)
                    {
                        thumbs.Add(
                            new ThumbnailInformation
                                {
                                    slideId = slide.id,
                                    slideNumber = Globals.slides.Where(s => s.type == MeTLLib.DataTypes.Slide.TYPE.SLIDE).ToList().IndexOf(slide) + 1,
                                });
                    }
                }
                slides.ItemsSource = thumbs;
            });
            
            Commands.RequerySuggested(Commands.MoveTo);
        }
        private void slideItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var selectedSlide = (ThumbnailInformation)((FrameworkElement)sender).DataContext;
            if (draggingSlide == null || draggingSlide != selectedSlide)
            {
                pulse((FrameworkElement)slides.ItemContainerGenerator.ContainerFromItem(selectedSlide));
                foreach (var board in boardDisplay.ItemsSource)
                {
                    var container = (FrameworkElement)boardDisplay.ItemContainerGenerator.ContainerFromItem(board);
                    if(((Board)container.DataContext).online)
                        pulse(container);
                }
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
