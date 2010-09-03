using System.Windows.Controls;
using System.Windows.Media;

namespace SandRibbon.Components
{
    public partial class Notepad : UserControl
    {
        public Notepad()
        {
            InitializeComponent();
            stack.canvasStack.Background = Brushes.Transparent;
        }
    }
}
