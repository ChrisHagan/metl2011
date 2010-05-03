using System;
using System.Linq;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbonObjects;

namespace SandRibbon.Components
{
    public partial class Drawer : UserControl
    {
        private ConversationDetails currentDetails;
        public Drawer()
        {
            InitializeComponent();
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>(id => updateTitleBar(id)));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            adornerScroll.scroll = scroll;
            adornerScroll.scroll.SizeChanged += adornerScroll.scrollChanged;
            adornerScroll.scroll.ScrollChanged += adornerScroll.scroll_ScrollChanged;
        }
        private void UpdateConversationDetails(ConversationDetails details)
        {
            currentDetails = details;
        }
        private void updateTitleBar(int id)
        {
            Dispatcher.BeginInvoke((Action)delegate {
                notes.Text = string.Format("Page {0} Notes",
                    currentDetails.Slides.Select(s => s.id).ToList().IndexOf(id)+1);
            });
        }
    }
}
