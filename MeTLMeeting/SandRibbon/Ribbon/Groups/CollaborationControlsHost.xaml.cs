using System;
using System.Windows;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Pedagogicometry;
using SandRibbon.Providers;
using MeTLLib.DataTypes;
using SandRibbon.Pages.Collaboration;
using SandRibbon.Pages;

namespace SandRibbon.Tabs.Groups
{
    /// <summary>
    /// Interaction logic for CollaborationControlsHost.xaml
    /// </summary>
    public partial class CollaborationControlsHost 
    {
        public static readonly DependencyProperty NavigationIsLockedProperty = DependencyProperty.Register("NavigationIsLocked", typeof (bool), typeof(CollaborationControlsHost));
        public bool NavigationIsLocked 
        {
            get { return (bool)GetValue(NavigationIsLockedProperty); }
            set { SetValue(NavigationIsLockedProperty, value); }

        }
        public SlideAwarePage rootPage { get; protected set; }
        public CollaborationControlsHost()
        {
            InitializeComponent();            
            Loaded += (s, e) =>
            {
                if (rootPage == null)
                    rootPage = DataContext as SlideAwarePage;                                
            };            
        }        
    }
}
