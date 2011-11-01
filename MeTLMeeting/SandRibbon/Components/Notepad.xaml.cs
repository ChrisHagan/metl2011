using System.Windows.Controls;
using System.Windows.Media;
using MeTLLib.Providers.Connection;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace SandRibbon.Components
{
    public partial class Notepad : UserControl
    {
        public Notepad()
        {
            InitializeComponent();
            stack.canvasStack.Background = Brushes.Transparent;
            Commands.MoveTo.RegisterCommand(new DelegateCommand<object>(moveTo));
            Commands.PreParserAvailable.RegisterCommandToDispatcher(new DelegateCommand<PreParser>(PreParserAvailable));
        }
        private void PreParserAvailable(MeTLLib.Providers.Connection.PreParser parser)
        {
            BeginInit();
            stack.handwriting.ReceiveStrokes(parser.ink);
            stack.images.ReceiveImages(parser.images.Values);
            foreach (var text in parser.text.Values)
                stack.text.doText(text);
            EndInit();
        }
        private void moveTo(object obj)
        {
            stack.Flush();
        }
    }
}
