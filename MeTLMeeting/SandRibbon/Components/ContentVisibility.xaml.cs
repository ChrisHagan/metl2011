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
    public static class ContentVisibilityUtils
    {
        private static ContentVisibilityEnum setFlag(ContentVisibilityEnum input, ContentVisibilityEnum flag, bool value){
            return input |= value ? flag : ContentVisibilityEnum.NoneVisible;
        }
        private static bool getFlag(ContentVisibilityEnum input, ContentVisibilityEnum flag){
            return (input & flag) == flag; 
        }
       public static bool getMyPublicVisible(ContentVisibilityEnum e){
           return getFlag(e,ContentVisibilityEnum.MyPublicVisible);
        }
        public static ContentVisibilityEnum setMyPublicVisible(ContentVisibilityEnum e, bool b){
            return setFlag(e,ContentVisibilityEnum.MyPublicVisible,b);
        }
        public static bool getMyPrivateVisible(ContentVisibilityEnum e){
            return getFlag(e,ContentVisibilityEnum.MyPrivateVisible);
        }
        public static ContentVisibilityEnum setMyPrivateVisible(ContentVisibilityEnum e, bool b){
            return setFlag(e,ContentVisibilityEnum.MyPrivateVisible,b);
        }
        public static bool getPeersVisible(ContentVisibilityEnum e){
            return getFlag(e,ContentVisibilityEnum.TheirsVisible);
        }
        public static ContentVisibilityEnum setPeersVisible(ContentVisibilityEnum e, bool b){
            return setFlag(e,ContentVisibilityEnum.TheirsVisible,b);
        }
        public static bool getOwnerVisible(ContentVisibilityEnum e){
            return getFlag(e,ContentVisibilityEnum.OwnerVisible);
        }
        public static ContentVisibilityEnum setOwnerVisible(ContentVisibilityEnum e, bool b){
            return setFlag(e,ContentVisibilityEnum.OwnerVisible,b);
        }
        public static bool getAllVisible(ContentVisibilityEnum e){
            return e == ContentVisibilityEnum.AllVisible;
        }
        public static bool getNoneVisible(ContentVisibilityEnum e){
            return e == ContentVisibilityEnum.NoneVisible;
        }
        public static ContentVisibilityEnum toContentVisibilityEnum(bool myPrivateVisible,bool myPublicVisible,bool peersVisible,bool ownerVisible){
            var flags = ownerVisible ? ContentVisibilityEnum.OwnerVisible : ContentVisibilityEnum.NoneVisible;
            flags |= peersVisible ? ContentVisibilityEnum.TheirsVisible : ContentVisibilityEnum.NoneVisible;
            flags |= myPrivateVisible ? ContentVisibilityEnum.MyPrivateVisible : ContentVisibilityEnum.NoneVisible;
            flags |= myPublicVisible ? ContentVisibilityEnum.MyPublicVisible : ContentVisibilityEnum.NoneVisible;
            return flags;
        }
    }


    public static class ContentVisibilityExtensions
    {
        // .NET 4.0 has this method in the framework
        public static bool HasFlag(this ContentVisibilityEnum contentVisibility, ContentVisibilityEnum flag)
        {
            return (contentVisibility & flag) == flag;
        }

        public static ContentVisibilityEnum SetFlag(this ContentVisibilityEnum contentVisibility, ContentVisibilityEnum flag)
        {
            return contentVisibility | flag;
        }

        public static ContentVisibilityEnum ClearFlag(this ContentVisibilityEnum contentVisibility, ContentVisibilityEnum flag)
        {
            return contentVisibility & ~flag;
        }
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
                flags = flags.ClearFlag(ContentVisibilityEnum.OwnerVisible);
            }

            return flags;
        }

        private void UpdateConversationDetails()
        {
            IsConversationOwner = Globals.isAuthor;
        }

        private void UpdateContentVisibility(ContentVisibilityEnum contentVisibility)
        {
            OwnerVisible = contentVisibility.HasFlag(ContentVisibilityEnum.OwnerVisible);
            TheirsVisible = contentVisibility.HasFlag(ContentVisibilityEnum.TheirsVisible);
            MyPrivateVisible = contentVisibility.HasFlag(ContentVisibilityEnum.MyPrivateVisible);
            MyPublicVisible = contentVisibility.HasFlag(ContentVisibilityEnum.MyPublicVisible); 
        }

        public bool IsConversationOwner
        {
            get { return (bool)GetValue(IsConversationOwnerProperty); }
            set 
            { 
                SetValue(IsConversationOwnerProperty, value); 
                NotifyPropertyChanged("IsConversationOwner");
            }
        }

        private void OnVisibilityChanged(object sender, DataTransferEventArgs args)
        {
            Commands.SetContentVisibility.Execute(GetCurrentVisibility());
        }
    }
}