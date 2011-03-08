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
using System.Media;
using SandRibbon.Providers;
using MeTLLib.DataTypes;
using SandRibbon.Utils;

namespace SandRibbon.Tabs.Groups
{
    public partial class Live
    {
        public Live()
        {
            InitializeComponent();
        }
        private void Participate_Click(object sender, RoutedEventArgs e)
        {
            if(Globals.location.currentSlide != 0)
                Commands.ListenToAudio.Execute(Slide.ConversationFor(Globals.location.currentSlide));
        }

        private void send_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox && ((CheckBox)sender).IsChecked == true)
            {
                MicrophoneSender.startSending();
            }
            else
            {
                MicrophoneSender.stopSending();
            }
        }
    }
}
