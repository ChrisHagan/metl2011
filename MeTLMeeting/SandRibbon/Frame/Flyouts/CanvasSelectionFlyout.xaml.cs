using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Utility;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;

namespace SandRibbon.Frame.Flyouts
{
    public class SelectionGroup
    {
        public StrokeCollection Strokes { get; set; } = new StrokeCollection();
        public ObservableCollection<Image> Images { get; set; } = new ObservableCollection<Image>();
        public ObservableCollection<MeTLTextBox> Texts { get; set; } = new ObservableCollection<MeTLTextBox>();
        public int StrokeCount { get { return Strokes.Count; } }
        public int TextCount { get { return Texts.Count; } }
        public int ImageCount { get { return Images.Count; } }
        public string Label { get; set; }
        public Privacy AlternatePrivacy { get; set; }
        public string ToggleLabel { get; set; }
        public Visibility Visibility
        {
            get
            {
                return StrokeCount + ImageCount + TextCount > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
    public partial class CanvasSelectionFlyout : FlyoutCard
    {
        public SelectionGroup Public { get; set; } = new SelectionGroup
        {
            Label = "PUBLIC",
            AlternatePrivacy = Privacy.Private,
            ToggleLabel = "Become private"
        };
        public SelectionGroup Private { get; set; } = new SelectionGroup
        {
            Label = "Private",
            AlternatePrivacy = Privacy.Public,
            ToggleLabel = "Become public"
        };
        public ObservableCollection<SelectionGroup> SelectionGroups { get; set; } = new ObservableCollection<SelectionGroup>();
    
        public CanvasSelectionFlyout(StrokeCollection selectedStrokes, ReadOnlyCollection<UIElement> selectedElements)
        {
            Title = "Manage selected elements";
            ShowCloseButton = false;
            foreach (var s in selectedStrokes)
            {
                var t = s.tag();
                switch (t.privacy)
                {
                    case Privacy.Private: Private.Strokes.Add(s); break;
                    default: Public.Strokes.Add(s); break;
                }
            }
            foreach (var e in selectedElements)
            {
                if (e is Image)
                {
                    var i = e as Image;
                    var t = i.tag();
                    switch (t.privacy)
                    {
                        case Privacy.Private: Private.Images.Add(i); break;
                        default: Public.Images.Add(i); break;
                    }
                }
                else if (e is MeTLTextBox)
                {
                    var tb = e as MeTLTextBox;
                    var t = tb.tag();
                    switch (t.privacy)
                    {
                        case Privacy.Private: Private.Texts.Add(tb); break;
                        default: Public.Texts.Add(tb); break;
                    }
                }
            }
            SelectionGroups.Add(Public);
            SelectionGroups.Add(Private);
            InitializeComponent();
            /*There are a number of events which will invalidate this card, which is essentially a dialog.*/
            var expire = new DelegateCommand<object>(o => Commands.CloseFlyoutCard.Execute(this));
            Commands.DeleteSelectedItems.RegisterCommand(expire);
            Commands.SetPrivacyOfItems.RegisterCommand(expire);
            Commands.JoiningConversation.RegisterCommand(expire);
            Commands.RemovePrivacyAdorners.RegisterCommand(expire);
        }
    }
}
