using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Pages;
using SandRibbon.Pages.Analytics;
using System.Windows.Controls.Ribbon;

namespace SandRibbon.Tabs
{
    public partial class Analytics : RibbonTab
    {
        private SlideAwarePage rootPage;

        public Analytics()
        {
            InitializeComponent();
            if (this.rootPage == null)
            {
                this.rootPage = DataContext as SlideAwarePage;
            }
            var wordCloud = new DelegateCommand<object>(openWordCloud);
            Loaded += delegate
            {
                Commands.WordCloud.RegisterCommand(wordCloud);
            };
            Unloaded += delegate
            {
                Commands.WordCloud.UnregisterCommand(wordCloud);
            };
        }
        public void openWordCloud(object o)
        {
            rootPage.NavigationService.Navigate(new TagCloudPage(rootPage.NetworkController, rootPage.ConversationDetails, rootPage.UserGlobalState, rootPage.UserServerState));
        }
    }
}
