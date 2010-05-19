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
using System.Collections.Generic;
using SandRibbonObjects;

namespace PowerpointJabber
{
    /// <summary>
    /// Interaction logic for MeTLControls.xaml
    /// </summary>
    public partial class MeTLControls : UserControl
    {
        private List<ConversationDetails> conversations;
        public MeTLControls()
        {
            InitializeComponent();
            if (ThisAddIn.instance.wire.isConnected)
            {
                ConnectedBlock.Visibility = Visibility.Visible;
                fillConversations();
            }
            else
                ConnectedBlock.Visibility = Visibility.Collapsed;
        }
        private void fillConversations()
        {
            conversations = new FileConversationDetailsProvider().ListConversations().ToList();
            ConversationList.ItemsSource = conversations;
        }
    }
}
