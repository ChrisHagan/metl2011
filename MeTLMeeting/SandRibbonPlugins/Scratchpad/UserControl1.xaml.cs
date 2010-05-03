using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Ink;
using System.Windows.Interop;
using System.Windows.Media;
using SandRibbon;
using System.Windows.Input;

namespace Scratchpad
{
    public partial class PluginMain : System.Windows.Controls.UserControl
    {
        public static RoutedCommand LoginToScratchPad = new RoutedCommand();
        public static RoutedCommand LogoutFromScratchPad = new RoutedCommand();
        private bool isLoggedIn = false;
       

        public PluginMain()
        {
            InitializeComponent();
        }

        private void canLogin(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isLoggedIn == false;
            e.Handled = true;
        }
        private void LoginExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            isLoggedIn = true;
            e.Handled = true;
            TryLogin();
        }
        private void canLogout(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isLoggedIn == true;
            e.Handled = true;
        }
        private void LogoutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            isLoggedIn = false;
            e.Handled = true;
            TryLogout();
        }

        private FrameworkElement currentScratchPad;

        private void TryLogin()
        {
            var username = scratchpadUsername.Text;
            var password = scratchpadPassword.Password;
            Window ParentWindow = (Window.GetWindow(this));
            Border ScratchPad = new Border();
            currentScratchPad = ScratchPad;
            
            ScratchPad.Name = "ScratchPad";
            ScratchPad.CornerRadius = new CornerRadius(10);
            ScratchPad.Height = 600;
            ScratchPad.Width = 300;
            ScratchPad.BorderThickness = new Thickness(10);
            ScratchPad.BorderBrush = Brushes.BlanchedAlmond;
            ScratchPad.HorizontalAlignment = HorizontalAlignment.Right;
            ScratchPad.VerticalAlignment = VerticalAlignment.Bottom;
            
            UniformGrid ScratchDock = new UniformGrid();
            ScratchDock.Rows = 2;
            ScratchDock.Columns = 1;
            ScratchDock.Width = 280;
            ScratchDock.Height = 580;
            
            InkCanvas ScratchPadInkCanvas = new InkCanvas();
            ScratchPadInkCanvas.StrokeCollected += ScratchPadStrokeCollected;
            ScratchPadInkCanvas.StrokeErased += ScratchPadStrokeErased;
            ScratchPadInkCanvas.StrokesReplaced += ScratchPadStrokesReplaced;
            
            ScrollViewer ScratchPadInkCanvasScrollViewer = new ScrollViewer();
            ScratchPadInkCanvasScrollViewer.Content = ScratchPadInkCanvas;
            ScratchPadInkCanvasScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            ScratchPadInkCanvasScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;

            TextBox ScratchNotes = new TextBox();
            ScratchNotes.Text = "This might be a 'todo' list...";
            ScratchNotes.Background = Brushes.Goldenrod;
            ScratchNotes.AcceptsReturn = true;
            ScratchNotes.AcceptsTab = true;
            ScratchNotes.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            ScratchNotes.TextWrapping = TextWrapping.Wrap;
            ScratchNotes.TextChanged += ScratchPadTextBoxChanged;
            ScratchPad.Child = ScratchDock;
            
            ScratchDock.Children.Add(ScratchPadInkCanvasScrollViewer);
            ScratchDock.Children.Add(ScratchNotes);

            Panel layoutGrid = (Panel)ParentWindow.Content;
            layoutGrid.Children.Add(ScratchPad);
        }
        private void TryLogout()
        {
            Grid layoutGrid = (Grid) ((Window.GetWindow(this)).Content);
            if (currentScratchPad != null)
                layoutGrid.Children.Remove(currentScratchPad);
        }
        private void ScratchPadTextBoxChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            var changedtext = textbox.Text;
        }
        private void ScratchPadStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            
        }
        private void ScratchPadStrokeErased(object sender, RoutedEventArgs e)
        {

        }
        private void ScratchPadStrokesReplaced(object sender, InkCanvasStrokesReplacedEventArgs e)
        {

        }
    }
}