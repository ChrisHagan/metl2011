using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbonInterop.Interfaces;
using SandRibbonObjects;
using MeTLLib.DataTypes;

namespace SandRibbon.Components
{
    public partial class SimpleFriendsListing : IFriendsDisplay
    {
        public class FriendToDisplayConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                var friend = (Friend)value;
                return String.Format("{0} @ {1} : {2}", friend.Name, friend.Location, friend.Count);
            }
            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return value;
            }
        }
        private static ColorAnimation pulser;
        public static FriendToDisplayConverter friendToDisplayConverter = new FriendToDisplayConverter();
        public static ObservableCollection<Friend> friendsList = new ObservableCollection<Friend>();
        private const string ANONYMOUS_PIC = @"\Resources\AnonHead.png";
        public SimpleFriendsListing()
        {
            InitializeComponent();
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>((_obj)=>Flush()));
        }
        public void SetPopulation(List<Friend> newPopulation)
        {
            Dispatcher.adoptAsync(delegate
            {
                foreach (var friend in newPopulation)
                    AddFriend(friend);
            });
        }
        private void AddFriend(Friend friend)
        {
            if (friendsList.Where(f => f.Name == friend.Name).FirstOrDefault() == null)
            {
                friendsList.Add(friend);
            }
        }
        public void Join(string who)
        {
            Dispatcher.adoptAsync(
                delegate{
                    SetPopulation(new List<Friend> {new Friend {Name = who}});
                });
        }
        public void Flush()
        {
            foreach (var friend in friendsList)
                friend.Flush();
        }
        private Friend ensure(string who, int where)
        {
            var friend = friendsList.Where(f => f.Name == who).FirstOrDefault();
            if (friend != null)
                return friend;
            var path = @"\Resources\" + who + ".png";
            friend = new Friend { Name = who, Count = 0, Location = where, ImagePath = File.Exists(path) ? path : ANONYMOUS_PIC };
            AddFriend(friend);
            return friend;
        }
        public void Ping(string who, int where)
        {
            Dispatcher.adoptAsync(delegate
            {
                var friend = ensure(who, where);
                friend.Ping();
                friend.Pulse();
                CollectionViewSource.GetDefaultView(friends.ItemsSource).Refresh();
            });
        }
        public void Move(string who, int where)
        {
            Dispatcher.adoptAsync(delegate
            {
                var friend = ensure(who, where);
                friend.MoveTo(where);
            });
        }
        private void HighlightStrokes(object sender, RoutedEventArgs e)
        {
            /*Commented out pending functionality request.
            var user = (Friend)((FrameworkElement)sender).DataContext;
            Commands.HighlightFriend.Execute(user,this);
            Commands.PostHighlightUser.Execute(new List<UserHighlight>(), this);
             */
        }
        private void HighlightUser(object sender, ExecutedRoutedEventArgs e)
        {
            var userHighlights = (List<UserHighlight>)e.Parameter;
            foreach (var friend in friendsList)
                friend.Background = Brushes.White;
            foreach (var userHighlight in userHighlights)
            {
                var friend = friendsList.Where(f => f.Name == userHighlight.user).FirstOrDefault();
                if(friend != null)
                    friend.Background = new SolidColorBrush(userHighlight.color);
            }
        }
        private void cancelAllOverlays(object sender, MouseEventArgs e)
        {
            foreach (var friend in friendsList)
                friend.Background =  Brushes.White;
            Commands.HighlightFriend.Execute(null, this);
        }
    }
}