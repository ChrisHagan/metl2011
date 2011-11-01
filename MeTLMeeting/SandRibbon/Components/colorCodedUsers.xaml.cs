using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Collections.ObjectModel;

namespace SandRibbon.Components
{
    /// <summary>
    /// Interaction logic for colorCodedUsers.xaml
    /// </summary>
    public partial class colorCodedUsers : Window
    {
        public colorCodedUsers()
        {
            InitializeComponent();
        }
        class UserColor
        {
            public String User {get; set;}
            public SolidColorBrush ColorCode { get; set; }
        }
        public colorCodedUsers(Dictionary<String, Color> userAndColor)
        {
            InitializeComponent();
            var userColors = new ObservableCollection<UserColor>();
            codedUsers.ItemsSource = userColors;
            foreach(var key in userAndColor.Keys)
                userColors.Add(new UserColor { 
                    User = key,
                    ColorCode = new SolidColorBrush(userAndColor[key])
                });
        }
    }
}
