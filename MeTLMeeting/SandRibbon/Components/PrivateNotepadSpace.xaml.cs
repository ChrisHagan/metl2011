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
using MeTLLib.DataTypes;

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
            Commands.PreParserAvailable.RegisterCommand(new DelegateCommand<MeTLLib.Providers.Connection.PreParser>(PreParserAvailable));
            Commands.MoveTo.RegisterCommand(new DelegateCommand<Location>(MoveTo));
        }

        private void MoveTo(Location loc)
        {
            Dispatcher.adopt(delegate
            {
                notepadStack.Flush();
            });
        }

        private void PreParserAvailable(MeTLLib.Providers.Connection.PreParser parser)
        {
            Dispatcher.adopt(delegate
            {
                try
                {
                    BeginInit();
                    notepadStack.ReceiveStrokes(parser.ink);
                    notepadStack.ReceiveImages(parser.images.Values);
                    foreach (var text in parser.text.Values)
                        notepadStack.DoText(text);
                    foreach (var moveDelta in parser.moveDeltas)
                        notepadStack.ReceiveMoveDelta(moveDelta, processHistory: true);

                }
                finally
                {
                    EndInit();
                }
            });
        }
    }
}
