using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SandRibbon.Pages.Collaboration.Layout
{
    public partial class KunzweilerLens : UserControl
    {
        public ObservableCollection<SignedBounds> bounds = new ObservableCollection<SignedBounds>();
        public KunzweilerLens()
        {
            InitializeComponent();
            Commands.MoveToCollaborationPage.RegisterCommand(new DelegateCommand<object>(OnMove));
            Commands.StrokePlaced.RegisterCommand(new DelegateCommand<PrivateAwareStroke>(StrokePlaced));
            Commands.ToggleLens.RegisterCommand(new DelegateCommand<object>(ToggleLens));
            DataContext = bounds;
            ToggleLens(null);
        }

        bool visible = true;
        private void ToggleLens(object obj)
        {
            visible = !visible;
            this.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void StrokePlaced(PrivateAwareStroke s)
        {
            bounds.Add(s.signedBounds());
        }

        private void OnMove(object obj)
        {
            bounds.Clear();
        }                
    }

    public class SignedBounds {
        public String username {get;set;}
        public Rect bounds { get; set; }
        public int ranking { get; set; }
        public string privacy { get; set; }
    }
}
