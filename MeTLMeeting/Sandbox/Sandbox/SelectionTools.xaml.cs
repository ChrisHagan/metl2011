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

namespace Sandbox
{
    public partial class SelectionTools : UserControl
    {
        private double toolWidth = 20;
        private Point startPoint;
        private Rect myBounds;
        private bool moving = false;
        private bool resizing;
        public static CompositeCommand dragEnded = new CompositeCommand();
        public static CompositeCommand selectEnded = new CompositeCommand();
        public static CompositeCommand resizeItem = new CompositeCommand();
        public SelectionTools(double x, double y, Rect bounds, double width, double height, string[] commands)
        {
            InitializeComponent();
            myBounds = bounds;
            rectangleCanvas.Width = width;
            rectangleCanvas.Height = height;
            move.Width = toolWidth;
            move.Height = toolWidth;
            resize.Width = toolWidth;
            resize.Height = toolWidth;
            Canvas.SetLeft(move, x + bounds.Width + (toolWidth/2));
            Canvas.SetTop(move, Math.Abs(y  - toolWidth));
            Canvas.SetLeft(resize, x + bounds.Width + (toolWidth/2));
            Canvas.SetTop(resize, Math.Abs(y + bounds.Height));
        }
        public void SetBounds(Rect bounds)
        {
            myBounds = bounds;
        }
        private void tools_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(this);
           
            if(pointIsWithin(move))
                moving = true;
            else if(pointIsWithin(resize))
                resizing = true;
            else
            {
               selectEnded.Execute(null);
            }
        }

        private bool pointIsWithin(UIElement tool)
        {
            var x = Canvas.GetLeft(tool);
            var y = Canvas.GetTop(tool);
            return x <= startPoint.X && (x + (1.5 *toolWidth)) >= startPoint.X &&
                   y <= startPoint.Y && (y  + (1.5 *toolWidth)) >= startPoint.Y;
        }

        private void tools_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;
            if (e.LeftButton == MouseButtonState.Pressed &&
                Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                if (moving)
                {
                    var myTools = move;
                    DataObject dragData = new DataObject("myMoveTool", myTools);
                    Console.WriteLine("moving");
                    DragDrop.DoDragDrop(this, dragData, DragDropEffects.All);
                }
                else if(resizing)
                {
                    var myTools = resizing;
                    DataObject dragData = new DataObject("myResizeTool", myTools);
                    Console.WriteLine("resizing");
                    DragDrop.DoDragDrop(this, dragData, DragDropEffects.All);
                }
            }
        }


        private void tools_Drag(object sender, DragEventArgs e)
        {
            var pos= e.GetPosition(this);
            if (e.Data.GetDataPresent("myMoveTool"))
            {
                Canvas.SetLeft(move, pos.X + (toolWidth /2));
                Canvas.SetTop(move, Math.Abs(pos.Y - toolWidth / 2));    
                Canvas.SetLeft(resize, pos.X + (toolWidth/2));
                Canvas.SetTop(resize, Math.Abs(pos.Y + myBounds.Height));
                rectangleCanvas.UpdateLayout();
                dragEnded.Execute(pos);
            }
            else if(e.Data.GetDataPresent("myResizeTool"))
            {
                Canvas.SetLeft(move, pos.X + (toolWidth /2));
                Canvas.SetLeft(resize, pos.X);
                Canvas.SetTop(resize, Math.Abs(pos.Y));
                rectangleCanvas.UpdateLayout();
                resizeItem.Execute(pos);

            }
        }
        private void tools_Drop(object sender, DragEventArgs e)
        {
            tools_Drag(sender, e);
            moving = false;
            resizing = false;
        }
        private void rectangleCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            moving = false;
        }

    }
}
