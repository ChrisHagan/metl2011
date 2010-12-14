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

namespace Sandbox
{
    public partial class WhatTheFuckin : Window
    {
        public WhatTheFuckin()
        {
            InitializeComponent();
            browser.Navigate(new Uri("http://co1-pptbroadcast.officeapps.live.com/PowerPointBroadcast.aspx?pptbid=b85df351-9f5a-4fdc-b199-6f8b533fc664", UriKind.RelativeOrAbsolute));
        }
    }
}
