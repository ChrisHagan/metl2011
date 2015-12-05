using SandRibbon.Components;
using SandRibbon.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SandRibbon.Pages.Conversations
{
    public partial class ConversationImportPage : Page, ServerAwarePage
    {
        public NetworkController networkController { get; protected set; }
        public UserGlobalState userGlobal { get; protected set; }
        public UserServerState userServer { get; protected set; }

        public ConversationImportPage(UserGlobalState _userGlobal, UserServerState _userServer, NetworkController _networkController)
        {
            userGlobal = _userGlobal;
            userServer = _userServer;
            networkController = _networkController;
            InitializeComponent();
        }
        protected void loadPowerPoint(PowerpointSpec spec)
        {
            var loader = new PowerPointLoader(networkController);
            //ImportPowerpoint

        }
        public NetworkController getNetworkController()
        {
            return networkController;
        }

        public UserServerState getUserServerState()
        {
            return userServer;
        }

        public UserGlobalState getUserGlobalState()
        {
            return userGlobal;
        }

        public NavigationService getNavigationService()
        {
            return NavigationService;
        }
    }
}
