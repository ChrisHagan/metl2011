using System;
using System.Linq;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;
using SandRibbonObjects;

namespace SandRibbon.Components
{
    public partial class Drawer : UserControl
    {
        public Drawer()
        {
            InitializeComponent();
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>(id => updateTitleBar(id)));
            adornerScroll.scroll = scroll;
            adornerScroll.scroll.SizeChanged += adornerScroll.scrollChanged;
            adornerScroll.scroll.ScrollChanged += adornerScroll.scroll_ScrollChanged;
        }
        private void updateTitleBar(int id)
        {
            Dispatcher.BeginInvoke((Action)delegate {
                notes.Text = string.Format("Page {0} Notes",
                    Globals.conversationDetails.Slides.Select(s => s.id).ToList().IndexOf(id)+1);
            });
        }
    }
}
