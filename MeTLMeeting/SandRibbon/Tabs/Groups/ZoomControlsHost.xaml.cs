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
using Divelements.SandRibbon;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace SandRibbon.Tabs.Groups
{
    public partial class ZoomControlsHost : RibbonGroup
    {
        public ZoomControlsHost()
        {
            InitializeComponent();
            Commands.SetLayer.RegisterCommandToDispatcher<string>(new DelegateCommand<string>(SetLayer));
            Commands.EndGrabZoom.RegisterCommand(new DelegateCommand<object>(EndGrabZoom));
        }
        private void EndGrabZoom(object _unused)
        {
            GrabZoomButton.IsChecked = false;
        }
        private void SetLayer(string layer)
        {
            switch (layer)
            {
                case "View":
                    View.IsChecked = true;
                    break;
                default:
                    View.IsChecked = false;
                    break;
            }
        }
    }
}
