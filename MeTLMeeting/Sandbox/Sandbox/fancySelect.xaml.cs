using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace Sandbox
{
    public partial class fancySelect : UserControl
    {
        public fancySelect()
        {
            InitializeComponent();
            SelectionTools.dragEnded.RegisterCommand(new DelegateCommand<Point>(dragEnded));
            SelectionTools.selectEnded.RegisterCommand(new DelegateCommand<object>(selectEnded));
            SelectionTools.resizeItem.RegisterCommand(new DelegateCommand<Point>(resizeItem));
        }

        private void resizeItem(Point Pos)
        {
            foreach(FrameworkElement child in canvas.GetSelectedElements())
            {
                var x = InkCanvas.GetLeft(child);
                var y = InkCanvas.GetTop(child);
                child.Width = Pos.X - x + 10;
                child.Height = Pos.Y - y;
                canvas.UpdateLayout();
                currentTool.SetBounds(canvas.GetSelectionBounds());

            }
        }

        private void selectEnded(object obj)
        {
            clearAdorners();
            canvas.Select(new StrokeCollection());
        }
        private void dragEnded(Point pos)
        {
            var bounds = canvas.GetSelectionBounds();
            var startPos = bounds.TopLeft;
            for (var i = 0; i < canvas.Children.Count; i++ )
            {
                UIElement element = canvas.Children[i];
                var difX = InkCanvas.GetLeft(element) - startPos.X;
                var difY = InkCanvas.GetTop(element) - startPos.Y;
                InkCanvas.SetLeft(element, pos.X + difX - bounds.Width);
                InkCanvas.SetTop(element, pos.Y + difY);
            
            }
            canvas.UpdateLayout();
        }


        private void canvas_SelectionChanging(object sender, InkCanvasSelectionChangingEventArgs e)
        {
        }

        private SelectionToolAdorner currentTool;
        private void InkCanvas_SelectionChanged(object sender, EventArgs e)
        {
           var adornerLayer = AdornerLayer.GetAdornerLayer(this);
           if(adornerLayer == null) return;
           var adorners = adornerLayer.GetAdorners(this);
           if(canvas.GetSelectedElements().Count > 0)
           {
           
               if (adorners != null)
               {
                   foreach (var adorner in adorners)
                       if (adorner is SelectionToolAdorner)
                       {
                           //do not add, instead update selected items. 
                           return;
                       }
               }
               var bounds = canvas.GetSelectionBounds();
               currentTool = new SelectionToolAdorner(this, bounds, canvas.ActualWidth, canvas.ActualHeight);
               adornerLayer.Add(currentTool);
           }
           else
           {
                if(adorners != null)
                    foreach(var adorner in adorners)
                        if(adorner is SelectionToolAdorner)
                            adornerLayer.Remove(adorner);
           }
        }
        public void clearAdorners()
        {
           var adornerLayer = AdornerLayer.GetAdornerLayer(this);
           if(adornerLayer == null) return;
           var adorners = adornerLayer.GetAdorners(this);
            if(adorners != null)
                    foreach(var adorner in adorners)
                        if(adorner is SelectionToolAdorner)
                            adornerLayer.Remove(adorner);
        }

        private void canvas_SelectionMoving(object sender, InkCanvasSelectionEditingEventArgs e)
        {
            updateSelectionToolPosition(canvas.GetSelectionBounds());
        }

        private void updateSelectionToolPosition(Rect bounds)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(this);
            if (adornerLayer == null) return;
            var adorners = adornerLayer.GetAdorners(this);
            SelectionToolAdorner currentToolsAdorner;
            if(adorners != null)
            {
                foreach(var adorner in adorners)
                    if (adorner is SelectionToolAdorner)
                    {
                        ((SelectionToolAdorner)adorner).UpdatePosition(bounds.TopRight);
                    }
            }
            
        }
    }
    public class SelectionToolAdorner: Adorner
    {
        private double x;
        private double y;
        private SelectionTools tools;
        private InkCanvas adornerCanvas;
        private UIElement adornee;

        public SelectionToolAdorner(UIElement adornedElement, Rect bounds, double width, double height) : base(adornedElement)
        {
            x = bounds.TopLeft.X;
            y = bounds.TopLeft.Y;
            adornee = adornedElement;
            tools = new SelectionTools(x, y, bounds, width, height, new string[] { "P", "W" } );
            AddVisualChild(tools);
        }
        public void SetBounds(Rect bounds)
        {
            tools.SetBounds(bounds);
        }
        public void UpdatePosition(Point position)
        {
            x = position.X;
            y = position.Y;
            UpdateLayout();
        }
        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            tools.Arrange(new Rect(new Point(0, 0), tools.DesiredSize));
            return finalSize;
        }
        protected override Visual GetVisualChild(int index)
        {
            return tools;
        }
    }

}
