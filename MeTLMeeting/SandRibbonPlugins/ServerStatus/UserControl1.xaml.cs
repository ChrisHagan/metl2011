using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Interop;
using System.Windows.Media;
using SandRibbon;
using System.Windows.Input;
using System.Threading;
using System.Net.NetworkInformation;

namespace ServerStatus
{
    public partial class PluginMain : System.Windows.Controls.UserControl
    {

        public static RoutedCommand StartPolling = new RoutedCommand();
        public static RoutedCommand StopPolling = new RoutedCommand();
        private bool isPolling = false;
        private Timer timer;
        private int updateSpeed = 1000;
        public string[] servers = new String[] { 
            "thisisnota.valid.address",
            "aeney.homedns.org",
            "racecar.adm.monash.edu", 
            "spacecaps.adm.monash.edu", 
            "madam.adm.monash.edu", 
            "civic.adm.monash.edu", 
            "reviver.adm.monash.edu", 
            "drawkward.adm.monash.edu", 
            "radar.adm.monash.edu", 
            "abacus.its.monash.edu" };

        public PluginMain()
        {
            InitializeComponent();
            FillServerBox();
        }

        private void FillServerBox()
        {
            foreach (string s in servers)
            {
                var newTextBlock = new TextBlock();
                newTextBlock.Foreground = Brushes.Gray;
                newTextBlock.Text = s;
                ServerBox.Children.Add(newTextBlock);
            }
        }

        private void canStartPolling(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isPolling == false;
            e.Handled = true;
        }
        private void startPollingExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            isPolling = true;
            e.Handled = true;
            beginPolling();
        }
        private void beginPolling()
        {
            if (timer != null)
                timer = null;
            timer = new Timer((strokesObject) =>
            {
                Dispatcher.Invoke((Action)delegate
                                              {
                                                  if (isPolling)
                                                  {
                                                      foreach (TextBlock t in ServerBox.Children)
                                                      {
                                                          try
                                                          {
                                                              Ping p = new Ping();
                                                              var newPing = (p.Send(t.Text.ToString(), 1000));
                                                              switch (newPing.Status)
                                                              {
                                                                  case IPStatus.BadRoute:
                                                                      t.Foreground = Brushes.Black;
                                                                      break;
                                                                  case IPStatus.BadDestination:
                                                                      t.Foreground = Brushes.Black;
                                                                      break;
                                                                  case IPStatus.DestinationNetworkUnreachable:
                                                                      t.Foreground = Brushes.Red;
                                                                      break;
                                                                  case IPStatus.DestinationHostUnreachable:
                                                                      t.Foreground = Brushes.Red;
                                                                      break;
                                                                  case IPStatus.DestinationUnreachable:
                                                                      t.Foreground = Brushes.Red;
                                                                      break;
                                                                  case IPStatus.TimedOut:
                                                                      t.Foreground = Brushes.Red;
                                                                      break;
                                                                  case IPStatus.Success:
                                                                      if (newPing.RoundtripTime > 60)
                                                                          t.Foreground = Brushes.DarkGreen;
                                                                      else if (newPing.RoundtripTime < 30)
                                                                          t.Foreground = Brushes.LightGreen;
                                                                      else if (newPing.RoundtripTime < 1)
                                                                          t.Foreground = Brushes.YellowGreen;
                                                                      else
                                                                          t.Foreground = Brushes.Green;
                                                                      break;
                                                                  default:
                                                                      t.Foreground = Brushes.Gray;
                                                                      break;
                                                              }
                                                          }
                                                          catch (Exception e)
                                                          {
                                                              t.Background = Brushes.Red;
                                                              t.Foreground = Brushes.White;
                                                          }
                                                      }
                                                  }
                                            });
            }, "", 0, updateSpeed);
        }
        private void canStopPolling(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isPolling == true;
            e.Handled = true;
        }
        private void stopPollingExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            isPolling = false;
            e.Handled = true;

            foreach (TextBlock t in ServerBox.Children)
            {
                t.Foreground = Brushes.Gray;
            }

            if (timer != null)
            {
                timer.Change(0, Timeout.Infinite);
                timer = null;
            }
        }
    }
}