using System;
using System.Windows;
using System.Windows.Data;
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using SandRibbon.Components.Pedagogicometry;
using SandRibbon.Providers;
using System.ComponentModel;

namespace SandRibbon.Components
{
    [Flags]
    public enum ContentVisibilityEnum
    {
        NoneVisible = 0,
        OwnerVisible = 1 << 0,
        TheirsVisible = 1 << 1,
        MyPrivateVisible = 1 << 2,
        MyPublicVisible = 1 << 3,
        AllVisible = OwnerVisible | TheirsVisible | MyPrivateVisible | MyPublicVisible
    }
    
    public partial class ContentVisibility : INotifyPropertyChanged
    {
        private bool ownerVisible = true;
        private bool theirsVisible = true;
        private bool myPrivateVisible = true; 
        private bool myPublicVisible = true; 

        public event PropertyChangedEventHandler PropertyChanged;
        public static readonly DependencyProperty IsConversationOwnerProperty =
            DependencyProperty.Register("IsConversationOwner", typeof(bool), typeof(ContentVisibility)); 

        public ContentVisibility()
        {
            DataContext = this;

            InitializeComponent();

            Commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<ConversationDetails>((_unused) => { UpdateConversationDetails(); }));
            Commands.UpdateContentVisibility.RegisterCommandToDispatcher(new DelegateCommand<ContentVisibilityEnum>(UpdateContentVisibility));
            Commands.SetContentVisibility.DefaultValue = ContentVisibilityEnum.AllVisible;
        }
    
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public bool OwnerVisible
        {
            get { return ownerVisible; }
            set
            {
                if (value != ownerVisible)
                {
                    ownerVisible = value;
                    NotifyPropertyChanged("OwnerVisible");
                }
            }
        }

        public bool TheirsVisible
        {
            get { return theirsVisible; } 
            set
            {
                if (value != theirsVisible)
                {
                    theirsVisible = value;
                    NotifyPropertyChanged("TheirsVisible");
                }
            }
        }

        public bool MyPrivateVisible
        {
            get { return myPrivateVisible; } 
            set
            {
                if (value != myPrivateVisible)
                {
                    myPrivateVisible = value;
                    NotifyPropertyChanged("MyPrivateVisible");
                }
            }
        }

        public bool MyPublicVisible
        {
            get { return myPublicVisible; } 
            set
            {
                if (value != myPublicVisible)
                {
                    myPublicVisible = value;
                    NotifyPropertyChanged("MyPublicVisible");
                }
            }
        }

        private ContentVisibilityEnum GetCurrentVisibility()
        {
            var flags = OwnerVisible ? ContentVisibilityEnum.OwnerVisible : ContentVisibilityEnum.NoneVisible;
            flags |= TheirsVisible ? ContentVisibilityEnum.TheirsVisible : ContentVisibilityEnum.NoneVisible;
            flags |= MyPrivateVisible ? ContentVisibilityEnum.MyPrivateVisible : ContentVisibilityEnum.NoneVisible;
            flags |= MyPublicVisible ? ContentVisibilityEnum.MyPublicVisible : ContentVisibilityEnum.NoneVisible;

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

        private void UpdateContentVisibility(ContentVisibilityEnum contentVisibility)
        {
            OwnerVisible = IsVisibilityFlagSet(ContentVisibilityEnum.OwnerVisible, contentVisibility);
            TheirsVisible = IsVisibilityFlagSet(ContentVisibilityEnum.TheirsVisible, contentVisibility);
            MyPrivateVisible = IsVisibilityFlagSet(ContentVisibilityEnum.MyPrivateVisible, contentVisibility); 
            MyPublicVisible = IsVisibilityFlagSet(ContentVisibilityEnum.MyPublicVisible, contentVisibility); 
        }

        private bool IsVisibilityFlagSet(ContentVisibilityEnum mask, ContentVisibilityEnum flags)
        {
            return ((flags & mask) == mask);
        }

        public bool IsConversationOwner
        {
            get { return (bool)GetValue(IsConversationOwnerProperty); }
            set { SetValue(IsConversationOwnerProperty, value); }
        }

        private void OnVisibilityChanged(object sender, DataTransferEventArgs args)
        {
            Commands.SetContentVisibility.Execute(GetCurrentVisibility());
        }
    }
}