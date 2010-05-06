using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Windows.Controls.Primitives;

namespace SandRibbon.Components.Sandpit
{
    public partial class S15Boards : UserControl
    {
        public static BoardPlacementConverter BOARD_PLACEMENT_CONVERTER = new BoardPlacementConverter();
        public S15Boards()
        {
            InitializeComponent();
            var boards = BoardManager.boards["S15"].ToList();
            boardDisplay.ItemsSource = boards;
            Commands.SendWakeUp.RegisterCommand(new DelegateCommand<object>(_nothing=>{
                Commands.ToggleFriendsVisibility.Execute(null);
                for (int i = 0; i < BoardManager.DEFAULT_CONVERSATION.Slides.Count;i++)
                {
                    Commands.SendMoveBoardToSlide.Execute(
                        new SandRibbon.Utils.Connection.JabberWire.BoardMove{
                            boardUsername=boards[i].name,
                            roomJid = BoardManager.DEFAULT_CONVERSATION.Slides[i].id
                    });
                }
            }));
            Commands.CloseBoardManager.RegisterCommand(new DelegateCommand<object>(
                _obj => Visibility = Visibility.Collapsed
            ));
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
