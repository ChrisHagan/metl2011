using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbonInterop.MeTLStanzas;
using SandRibbon.Utils.Connection;
using SandRibbon.Components;
using SandRibbon.Providers;

namespace SandRibbon.Quizzing
{
    public partial class Worm : UserControl
    {
        private List<int> movements = new List<int>();
        private static readonly int POINTS = 39;
        private static readonly int STEP = 5;
        private static readonly int FLOOR = 60;
        private int currentMovement = FLOOR;
        private static readonly int CEILING = 10;
        private static readonly int FALL = 1;
        private static readonly int GRID_COLUMNS = 10;
        private static readonly int HEARTBEAT_FAIL_THRESHOLD = 2;
        private int missedHeartbeats = 0;
        public static DispatcherTimer heart;
        public Worm()
        {
            InitializeComponent();
            Commands.ReceiveWormMove.RegisterCommand(new DelegateCommand<string>(ReceiveWormMove));
            Commands.ReceiveStroke.RegisterCommand(new DelegateCommand<object>(receivedMessage));
            Commands.ReceiveImage.RegisterCommand(new DelegateCommand<object>(receivedMessage));
            Commands.ReceiveTextBox.RegisterCommand(new DelegateCommand<object>(receivedMessage));
            Commands.ReceiveQuiz.RegisterCommand(new DelegateCommand<object>(receivedMessage));
            Commands.ReceiveQuizAnswer.RegisterCommand(new DelegateCommand<object>(receivedMessage));
            Commands.ReceiveDirtyText.RegisterCommand(new DelegateCommand<object>(receivedMessage));
            Commands.ReceiveDirtyStrokes.RegisterCommand(new DelegateCommand<object>(receivedMessage));
            Commands.ReceiveDirtyLiveWindow.RegisterCommand(new DelegateCommand<object>(receivedMessage));
            Commands.ReceiveDirtyImage.RegisterCommand(new DelegateCommand<object>(receivedMessage));
            Commands.ReceiveDirtyConversationDetails.RegisterCommand(new DelegateCommand<object>(receivedMessage));
            Commands.PreParserAvailable.RegisterCommand(new DelegateCommand<PreParser>(receivedMessage));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<string>(
                title=>{
                    if (heart == null)
                        heart = new DispatcherTimer(TimeSpan.FromMilliseconds(2500), DispatcherPriority.ApplicationIdle, Render, Dispatcher);
                }));
        }
        private void receivedMessage(object _obj)
        {
            ResetHeartbeat();
            Inc();
        }
        private object extendLock = new object();
        private void checkForExtendedDesktop()
        {
            lock (extendLock)
            {
                var screenCount = System.Windows.Forms.Screen.AllScreens.Count();
                if (Projector.Window == null && screenCount > 1)
                    Commands.ProxyMirrorPresentationSpace.Execute(null);
                else if (Projector.Window != null && screenCount == 1)
                    Projector.Window.Close();
            }
        }
        private void Render(object _sender, EventArgs _e)
        {
            try
            {
                Commands.SendWormMove.Execute(new WormMove { conversation = Globals.conversationDetails.Jid, direction = "=" });
            }
            catch (NotSetException)
            { 
                //The worm is not intended to function pre conversation join
            }
            checkForExtendedDesktop();
            if (missedHeartbeats >= HEARTBEAT_FAIL_THRESHOLD)
            {
                ResetHeartbeat();
                Commands.Relogin.Execute(null);
            }
            while(movements.Count > POINTS)
                movements = movements.Skip(1).ToList();
            movements.Add(currentMovement);
            currentMovement = bound(currentMovement);
            StreamGeometry sg = new StreamGeometry();
            using (StreamGeometryContext context = sg.Open())
            {
                context.BeginFigure(new Point(0, FLOOR), false, false);
                int x = 0;
                foreach(var movement in movements)
                    context.LineTo(new Point((++x) * STEP, bound(movement)), true, true);
                sg.Freeze();
                System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();
                path.Data = sg;
                path.Stroke = Brushes.Red;
                path.StrokeThickness = 4;
                trail.Children.Clear();
                trail.Children.Add(path);
            }
            Dec();
            missedHeartbeats++;
        }
        private int bound(int y)
        {
            return Math.Max(CEILING, Math.Min(FLOOR, y));
        }
        private void ReceiveWormMove(string direction)
        {
            ResetHeartbeat();
            foreach (var c in direction)
                if (c == '+')
                    Inc();
                else if (c == '-')
                    Dec();
        }
        private void Dec()
        {
            currentMovement++;
        }
        private void Inc()
        {
            currentMovement--;
        }
        private void ResetHeartbeat()
        {
            missedHeartbeats = 0;
        }
    }
    public class WormMove
    {
        public string conversation;
        public string direction;
    }
}
