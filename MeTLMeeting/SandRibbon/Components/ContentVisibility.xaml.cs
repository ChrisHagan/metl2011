using System;
using System.Windows;
using System.Windows.Data;
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using SandRibbon.Components.Pedagogicometry;
using SandRibbon.Providers;

namespace SandRibbon.Components
{
    [Flags]
    public enum ContentVisibilityEnum
    {
        NoneVisible = 0,
        OwnerVisible = 1 << 0,
        TheirsVisible = 1 << 1,
        MineVisible = 1 << 2,
        AllVisible = OwnerVisible | TheirsVisible | MineVisible
    }
    
    public partial class ContentVisibility
    {
        public ContentVisibility()
        {
            DataContext = this;

            InitializeComponent();

            Commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<ConversationDetails>((_unused) => { UpdateConversationDetails(); }));
        }

        private ContentVisibilityEnum GetCurrentVisibility()
        {
            if (ownerContent == null || theirContent == null || myContent == null)
                return ContentVisibilityEnum.NoneVisible;
 
            var flags = ownerContent.IsChecked ?? false ? ContentVisibilityEnum.OwnerVisible : ContentVisibilityEnum.NoneVisible;
            flags |= theirContent.IsChecked ?? false ? ContentVisibilityEnum.TheirsVisible : ContentVisibilityEnum.NoneVisible;
            flags |= myContent.IsChecked ?? false ? ContentVisibilityEnum.MineVisible : ContentVisibilityEnum.NoneVisible;

            // if the owner then ignore owner flag and only use mine flag
            if (Globals.isAuthor)
            {
                flags &= ~ContentVisibilityEnum.OwnerVisible;
            }

            return flags;
        }

        private void UpdateConversationDetails()
        {
            IsConversationOwner = Globals.isAuthor;
        }

        public bool IsConversationOwner
        {
            get { return (bool)GetValue(IsConversationOwnerProperty); }
            set { SetValue(IsConversationOwnerProperty, value); }
        }

        public static readonly DependencyProperty IsConversationOwnerProperty =
            DependencyProperty.Register("IsConversationOwner", typeof(bool), typeof(ContentVisibility)); 

        private bool IsVisibilityFlagSet(ContentVisibilityEnum contentVisible)
        {
            return (GetCurrentVisibility() & contentVisible) != ContentVisibilityEnum.NoneVisible;
        }

        private void contentVisibilityChange(object sender, RoutedEventArgs e)
        {
            Commands.SetContentVisibility.Execute(GetCurrentVisibility());
        }
    }
}