using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Input.StylusPlugIns;
using System.Windows.Media;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Providers;
using SandRibbon.Utils;
using SandRibbonInterop;
//using SandRibbonInterop.MeTLStanzas;
using SandRibbonObjects;
using System.Windows;

namespace SandRibbon.Components.Canvas
{
    public class ViewCanvas : AbstractCanvas
    {
        private enum ViewModes { Moving, Zooming, None };
        private ViewModes currentViewMode = ViewModes.None;
        public ViewCanvas()
        {
            Background = new SolidColorBrush(new Color { A = 1, R = 255, G = 255, B = 255 });
            EditingMode = InkCanvasEditingMode.None;
            this.PreviewMouseDown += setMouseMode;
            this.PreviewMouseMove += mouseDrag;
            this.PreviewMouseUp += clearMouseMode;
            this.MouseLeave += mouseLeave;
            target = "";
            Commands.SetInkCanvasMode.RegisterCommandToDispatcher<object>(new DelegateCommand<object>(setInkCanvasMode));
        }
        private void setInkCanvasMode(object _unused)
        {
            this.EditingMode = InkCanvasEditingMode.None;
        }
        private Point oldPosition;
        private void setMouseMode(object sender, MouseButtonEventArgs e)
        {
            currentViewMode = ViewModes.Moving;
            oldPosition = e.GetPosition(this);
        }
        private void mouseDrag(object sender, MouseEventArgs e)
        {
            switch (currentViewMode)
            {
                case ViewModes.None:
                    return;
                case ViewModes.Moving:
                    if (oldPosition == null) return;
                    if (oldPosition.X == -1 && oldPosition.Y == -1) oldPosition = e.GetPosition(this);
                    else
                    {
                        var newPosition = e.GetPosition(this);
                        var delta = new Point(oldPosition.X - newPosition.X, oldPosition.Y - newPosition.Y);
                        Commands.MoveCanvasByDelta.ExecuteAsync(delta);
                        oldPosition = new Point(-1, -1);
                    }
                    break;
            }
        }
        private void mouseLeave(object sender, MouseEventArgs e)
        {
            currentViewMode = ViewModes.None;
        }
        private void clearMouseMode(object sender, MouseButtonEventArgs e)
        {
            currentViewMode = ViewModes.None;
        }
        public override void showPrivateContent() { }
        public override void hidePrivateContent() { }
        protected override void HandlePaste() { }
        protected override void HandleCopy() { }
        protected override void HandleCut() { }
    }
}