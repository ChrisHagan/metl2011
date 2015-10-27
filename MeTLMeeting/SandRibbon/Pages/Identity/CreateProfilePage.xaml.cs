using MeTLLib;
using SandRibbon.Pages.Collaboration;
using SandRibbon.Pages.Collaboration.Palettes;
using SandRibbon.Profiles;
using SandRibbon.Providers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace SandRibbon.Pages.Identity
{
    public partial class CreateProfilePage : ServerAwarePage
    {
        public CreateProfilePage(MetlConfiguration _backend) : base(_backend)
        {
            InitializeComponent();
            DataContext = new Profile
            {
                ownerName = Globals.me,
                logicalName = "",
                castBars = new[] {
                    new Bar(8)
                    {
                        VerticalAlignment = System.Windows.VerticalAlignment.Top,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        Orientation = System.Windows.Controls.Orientation.Horizontal,
                        ScaleFactor = 0.8,
                        Rows = 1,
                        Columns = 8
                    },
                    new Bar(5)
                    {
                        VerticalAlignment = System.Windows.VerticalAlignment.Center,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                        Orientation = System.Windows.Controls.Orientation.Vertical,
                        ScaleFactor = 0.8,
                        Rows = 5,
                        Columns = 1
                    },
                    new Bar(5)
                    {
                        VerticalAlignment = System.Windows.VerticalAlignment.Center,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                        Orientation = System.Windows.Controls.Orientation.Vertical,
                        ScaleFactor = 0.8,
                        Rows = 5,
                        Columns = 1
                    }
                }
            };
        }

        private void ConfigureBars(object sender, RoutedEventArgs e)
        {
            var profile = DataContext as Profile;
            Globals.profiles.Add(profile);
            Globals.currentProfile = profile;
            NavigationService.Navigate(new CommandBarConfigurationPage(ServerConfig,profile));
        }

        private void SkipConfiguringBars(object sender, RoutedEventArgs e)
        {
            var profile = DataContext as Profile;
            Globals.profiles.Add(profile);
            Globals.currentProfile = profile;
            NavigationService.Navigate(new ProfileSelectorPage(ServerConfig,Globals.profiles));
        }
    }
}
