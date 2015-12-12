using System.Windows.Controls;
using System.Windows.Input;
using SandRibbon.Pages;

namespace SandRibbon.Components
{
    public partial class SlideNavigationControls : UserControl
    {
        public SlideAwarePage rootPage { get; protected set; }
        public SlideNavigationControls()
        {
            InitializeComponent();
            this.PreviewKeyDown += KeyPressed;            
            Loaded += (s, e) =>
            {
                if (rootPage == null)
                {
                    rootPage = DataContext as SlideAwarePage;
                }                
                /*Because the consumers of these commands are only registering after they load, these can't be declared in XAML*/
                moveToPrevious.Command = Commands.MoveToPrevious;
                moveToNext.Command = Commands.MoveToNext;
            };            
        }

        private void KeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.PageUp || e.Key == Key.Up)
            {
                if(Commands.MoveToPrevious.CanExecute(null))
                  Commands.MoveToPrevious.Execute(null);
                e.Handled = true;
            }
            if (e.Key == Key.PageDown || e.Key == Key.Down)
            {
                if(Commands.MoveToNext.CanExecute(null))
                  Commands.MoveToNext.Execute(null);
                e.Handled = true;
            }
        }        
    }
}
