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
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;
using SandRibbon.Providers.Structure;
using SandRibbonInterop.MeTLStanzas;
using SandRibbonObjects;

namespace SandRibbon.Components.Sandpit
{
    public partial class ThoughtBubbleLauncher : UserControl
    {
        private string privacy;
        private int currentSlide;
        private ConversationDetails currentDetails;
        public ThoughtBubbleLauncher()
        {
            InitializeComponent();
            Commands.BubbleCurrentSelection.RegisterCommand(new DelegateCommand<object>(BubbleCurrentSelection));
            Commands.SetPrivacy.RegisterCommand(new DelegateCommand<string>(SetPrivacy));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<int>(MoveTo));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(details => currentDetails = details));
        }
      
        private void SetPrivacy(string privacy){
            this.privacy = privacy;
        }
        private void MoveTo(int where) {
            this.currentSlide = where;
        }
        private void BubbleCurrentSelection(object _nothing) 
        {
            string target = null;
            var selection = new List<SelectedIdentity>();
            foreach(var registeredCommand in Commands.DoWithCurrentSelection.RegisteredCommands)
                registeredCommand.Execute((Action<SelectedIdentity>)(id=>{
                    target = id.target;
                    selection.Add(id);
                }));
            if (selection.Count() > 0)
            {
                var details = ConversationDetailsProviderFactory.Provider.AppendSlideAfter(currentSlide, currentDetails.Jid, Slide.TYPE.THOUGHT);
                var slide = details.Slides.Select(s => s.id).Max();
                Commands.SendNewBubble.Execute(new TargettedBubbleContext
                                                   {
                                                       author = Globals.me,
                                                       context = selection,
                                                       privacy = "public",
                                                       slide = currentSlide,
                                                       target = target,
                                                       thoughtSlide =slide 
                                                   });
            }
        } 
    }
}
