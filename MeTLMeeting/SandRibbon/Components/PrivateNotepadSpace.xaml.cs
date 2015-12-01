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

namespace SandRibbon.Components
{
    /// <summary>
    /// Interaction logic for PrivateNotepadSpace.xaml
    /// </summary>
    public partial class PrivateNotepadSpace : UserControl
    {
        public PrivateNotepadSpace()
        {
            InitializeComponent();
            Commands.PreParserAvailable.RegisterCommandToDispatcher(new DelegateCommand<MeTLLib.Providers.Connection.PreParser>(PreParserAvailable));
            Commands.MoveToCollaborationPage.RegisterCommandToDispatcher(new DelegateCommand<int>(MoveTo));
        }

        private void MoveTo(int slide)
        {
            notepadStack.Flush();
        }

        private void PreParserAvailable(MeTLLib.Providers.Connection.PreParser parser)
        {
            BeginInit();
            notepadStack.ReceiveStrokes(parser.ink);
            notepadStack.ReceiveImages(parser.images.Values);
            foreach (var text in parser.text.Values)
                notepadStack.DoText(text);
            foreach (var moveDelta in parser.moveDeltas)
                notepadStack.ReceiveMoveDelta(moveDelta, processHistory: true);

            EndInit();
        }
    }
}
