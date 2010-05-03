using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
//using SandRibbon;
using System.Windows.Forms;
using System.Threading;
using System.Windows.Ink;
using SBSDKComWrapperLib;
using System.Drawing;
using System.Collections;
using MessageBox=System.Windows.MessageBox;

namespace SMARTboard
{
    //plugin
    public partial class PluginMain : System.Windows.Controls.UserControl
    {
        //Smartboard stuff:
        private ISBSDKBaseClass2 Sbsdk;
        private _ISBSDKBaseClass2Events_Event SbsdkEvents;
        private _ISBSDKBaseClass2HoverEvents_Event SbsdkHoverEvents;

        [DllImport("user32.dll")]
        public static extern int RegisterWindowMessageA([MarshalAs(UnmanagedType.LPStr)] string lpString);
        private int SBSDKMessageID = RegisterWindowMessageA("SBSDK_NEW_MESSAGE");
        //end Smartboard stuff

        public static RoutedCommand ConnectToSmartboard = new RoutedCommand();
        public static RoutedCommand DisconnectFromSmartboard = new RoutedCommand();
        private bool isConnected = false;
        public PluginMain()
        {
            InitializeComponent();
        }
        private void canConnectToSmartboard(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isConnected == false && SMARTboardDiagnosticOutput != null;
            e.Handled = true;
        }
        private void ConnectToSmartboardExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            isConnected = true;
            e.Handled = true;
            connectToSmartboard();
        }
        private void connectToSmartboard()
        {
            //Smartboard stuff

            try
            {
                Sbsdk = new SBSDKBaseClass2();
            }
            catch (Exception e)
            {
                // handle exception, we will do nothing
                Console.WriteLine("Exception in Main: " + e.Message);
            }
            if (Sbsdk != null)
            {
                SbsdkEvents = (_ISBSDKBaseClass2Events_Event)Sbsdk;
                SbsdkHoverEvents = (_ISBSDKBaseClass2HoverEvents_Event)Sbsdk;
            }
            if (SbsdkEvents != null)
            {
                SbsdkEvents.OnCircle += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnCircleEventHandler(this.OnCircle);
                SbsdkEvents.OnClear += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnClearEventHandler(this.OnClear);
                SbsdkEvents.OnEraser += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnEraserEventHandler(this.OnEraser);
                SbsdkEvents.OnLine += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnLineEventHandler(this.OnLine);
                SbsdkEvents.OnNext += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnNextEventHandler(this.OnNext);
                SbsdkEvents.OnNoTool += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnNoToolEventHandler(this.OnNoTool);
                SbsdkEvents.OnPen += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnPenEventHandler(this.OnPen);
                SbsdkEvents.OnPrevious += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnPreviousEventHandler(this.OnPrevious);
                SbsdkEvents.OnPrint += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnPrintEventHandler(this.OnPrint);
                SbsdkEvents.OnRectangle += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnRectangleEventHandler(this.OnRectangle);
                SbsdkEvents.OnBoardStatusChange += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnBoardStatusChangeEventHandler(this.OnBoardStatusChange);
                SbsdkEvents.OnXMLAnnotation += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXMLAnnotationEventHandler(this.OnXMLAnnotation);
                SbsdkEvents.OnXYNonProjectedDown += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYNonProjectedDownEventHandler(this.OnXYNonProjectedDown);
                SbsdkEvents.OnXYNonProjectedMove += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYNonProjectedMoveEventHandler(this.OnXYNonProjectedMove);
                SbsdkEvents.OnXYNonProjectedUp += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYNonProjectedUpEventHandler(this.OnXYNonProjectedUp);
                SbsdkEvents.OnXYDown += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYDownEventHandler(this.OnXYDown);
                SbsdkEvents.OnXYMove += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYMoveEventHandler(this.OnXYMove);
                SbsdkEvents.OnXYUp += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYUpEventHandler(this.OnXYUp);
            }
            if (SbsdkHoverEvents != null)
            {
                SbsdkHoverEvents.OnXYHover += new SBSDKComWrapperLib._ISBSDKBaseClass2HoverEvents_OnXYHoverEventHandler(this.OnXYHover);
            }
/*
            if (Sbsdk != null)
            {
                Sbsdk.SBSDKAttachWithMsgWnd(Handle.ToInt32(), false, Handle.ToInt32());
            }
*/
            //endSmartboard stuff

            var newMessage = "Connected to SMARTboard";
            SMARTboardDiagnosticOutput.Items.Add(newMessage);
        }
        private void canDisconnectFromSmartboard(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isConnected == true;
            e.Handled = true;
        }
        private void DisconnectFromSmartboardExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            isConnected = false;
            e.Handled = true;
            disconnectFromSmartboard();
        }
        private void disconnectFromSmartboard()
        {
            //Smartboard stuff

            if (SbsdkEvents != null)
            {
                //SbsdkEvents.OnCircle -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnCircleEventHandler(this.OnCircle);
                //SbsdkEvents.OnClear -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnClearEventHandler(this.OnClear);
                SbsdkEvents.OnEraser -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnEraserEventHandler(this.OnEraser);
                //SbsdkEvents.OnLine -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnLineEventHandler(this.OnLine);
                //SbsdkEvents.OnNext -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnNextEventHandler(this.OnNext);
                SbsdkEvents.OnNoTool -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnNoToolEventHandler(this.OnNoTool);
                SbsdkEvents.OnPen -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnPenEventHandler(this.OnPen);
                //SbsdkEvents.OnPrevious -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnPreviousEventHandler(this.OnPrevious);
                //SbsdkEvents.OnPrint -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnPrintEventHandler(this.OnPrint);
                //SbsdkEvents.OnRectangle -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnRectangleEventHandler(this.OnRectangle);
                SbsdkEvents.OnBoardStatusChange -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnBoardStatusChangeEventHandler(this.OnBoardStatusChange);
                //SbsdkEvents.OnXMLAnnotation -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXMLAnnotationEventHandler(this.OnXMLAnnotation);
                //SbsdkEvents.OnXYNonProjectedDown -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYNonProjectedDownEventHandler(this.OnXYNonProjectedDown);
                //SbsdkEvents.OnXYNonProjectedMove -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYNonProjectedMoveEventHandler(this.OnXYNonProjectedMove);
                //SbsdkEvents.OnXYNonProjectedUp -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYNonProjectedUpEventHandler(this.OnXYNonProjectedUp);
                //SbsdkEvents.OnXYDown -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYDownEventHandler(this.OnXYDown);
                //SbsdkEvents.OnXYMove -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYMoveEventHandler(this.OnXYMove);
                //SbsdkEvents.OnXYUp -= new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYUpEventHandler(this.OnXYUp);
            }
            if (SbsdkHoverEvents != null)
            {
                SbsdkHoverEvents.OnXYHover -= new SBSDKComWrapperLib._ISBSDKBaseClass2HoverEvents_OnXYHoverEventHandler(this.OnXYHover);
            }
            /*
                        if (Sbsdk != null)
                        {
                            Sbsdk.SBSDKAttachWithMsgWnd(Handle.ToInt32(), false, Handle.ToInt32());
                        }
            */
            Sbsdk = null;
            
            //endSmartboard stuff

            var newMessage = "Disconnected from SMARTboard";
            SMARTboardDiagnosticOutput.Items.Add(newMessage);
        }
        //Yet more smartboardstuff:
        // I might need this next bit, but I don't know yet.  We'll have to see.
/*        protected override void OnNotifyMessage(Message m)
		{
			if (m.Msg == SBSDKMessageID)
			{
				// process sdk events on our apps main thread
				Sbsdk.SBSDKProcessData();
			}
		}
*/
		/*private void OnMouseDown(object sender, System.Windows.Input.MouseEventArgs e)
		{
            OnXYDown((int)(e.GetPosition(this).X), (int)(e.GetPosition(this).Y), 0, 0);
            var newMessage = "MouseDown";
            SMARTboardDiagnosticOutput.Items.Add(newMessage);
        }

		private void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			OnXYMove(e.X, e.Y, 0, 0);
		}

		private void OnMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			OnXYUp(e.X, e.Y, 0, 0);
            var newMessage = "MouseUp";
            SMARTboardDiagnosticOutput.Items.Add(newMessage);
        }

		private void OnPaint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
/*			int i, j;
			ArrayList Stroke;
			Point Pt, LastPt;
			Graphics g = CreateGraphics();
			Pen pen = new Pen(System.Drawing.Color.Black, 1);

			pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
			for (i = 0; i < StrokeList.Count; i++)
			{
				Stroke = (ArrayList)((Stroke)StrokeList[i]).StrokePoints;
				pen.Color = ((Stroke)StrokeList[i]).StrokeColor;
				pen.Width = (float)((Stroke)StrokeList[i]).StrokeWidth;
				if (Stroke.Count > 0)
				{
					LastPt = (Point)Stroke[0];
					for (j = 1; j < Stroke.Count; j++)
					{
						Pt = (Point)Stroke[j];
						g.DrawLine(pen, LastPt, Pt);
						LastPt = Pt;
					}
				}
			}
*/
        }

		/*private Stroke GetCurrentStroke(int iPointerId)
		{
/*			for (int i = 0; i < CurrentStrokeList.Count; i++)
			{
				if (((Stroke)CurrentStrokeList[i]).iPointerID == iPointerId)
				{
					return (Stroke)CurrentStrokeList[i];
				}
			}
*/
/*			return null;
		}*/
/*
		private void RemoveCurrentStroke(int iPointerId)
		{
/*			for (int i = 0; i < CurrentStrokeList.Count; i++)
			{
				if (((Stroke)CurrentStrokeList[i]).iPointerID == iPointerId)
				{
					CurrentStrokeList.RemoveAt(i);
					return;
				}
			}
*/		}

		// projected contact events
/*		private void OnXYDown(int x, int y, int z, int iPointerID)
		{
/*			Point Pt = new Point(x, y);
			// create a new stroke and store all the info related to it
			Stroke CurrentStroke = new Stroke();
			CurrentStroke.iPointerID = iPointerID;
			CurrentStroke.StrokePoints.Add(Pt);
			StrokeList.Add(CurrentStroke);
			CurrentStrokeList.Add(CurrentStroke);

			CurrentPen.Width = 3;
			int iRed = 0, iGreen = 0, iBlue = 0;
			if (Sbsdk != null)
			{
				Sbsdk.SBSDKGetToolColor(iPointerID, out iRed, out iGreen, out iBlue);
				CurrentPen.Width = Sbsdk.SBSDKGetToolWidth(iPointerID);
			}
			CurrentPen.Color = Color.FromArgb(iRed, iGreen, iBlue);
			CurrentStroke.StrokeColor = CurrentPen.Color;
			CurrentStroke.StrokeWidth = (int)CurrentPen.Width;
*/
/*            var newMessage = "SMARTboardPen Down";
            SMARTboardDiagnosticOutput.Items.Add(newMessage);
        }

		private void OnXYMove(int x, int y, int z, int iPointerID)
		{
/*			Point Pt = new Point(x, y);
			// check if we have a stroke started for this pointer id
			Stroke CurrentStroke = GetCurrentStroke(iPointerID);
			if (CurrentStroke == null)
			{
				return;
			}

			Point PreviousPt = (Point)CurrentStroke.StrokePoints[CurrentStroke.StrokePoints.Count - 1];
			CurrentStroke.StrokePoints.Add(Pt);
			Graphics g = CreateGraphics();
			CurrentPen.Color = CurrentStroke.StrokeColor;
			CurrentPen.Width = (float)CurrentStroke.StrokeWidth;
			g.DrawLine(CurrentPen, PreviousPt, Pt);
		}

		private void OnXYUp(int x, int y, int z, int iPointerID)
		{
/*			Point Pt = new Point(x, y);
			// check if we have a stroke started for this pointer id
			Stroke CurrentStroke = GetCurrentStroke(iPointerID);
			if (CurrentStroke == null)
			{
				return;
			}

			Point PreviousPt = (Point)CurrentStroke.StrokePoints[CurrentStroke.StrokePoints.Count - 1];
			CurrentStroke.StrokePoints.Add(Pt);
			Graphics g = CreateGraphics();
			CurrentPen.Color = CurrentStroke.StrokeColor;
			CurrentPen.Width = (float)CurrentStroke.StrokeWidth;
			g.DrawLine(CurrentPen, PreviousPt, Pt);
			this.texbox1.AppendText("Time:"+DateTime.Now.ToFileTimeUtc()+", Stroke:"+CurrentStroke.StrokePoints.Count.ToString()+", Color is:"+CurrentPen.Color+", Width is:"+CurrentPen.Width);
			this.texbox1.AppendText("\r\n");
			RemoveCurrentStroke(iPointerID);
*/
/*            var newMessage = "SMARTboardPen Up";
            SMARTboardDiagnosticOutput.Items.Add(newMessage);
        }

		private void OnXYHover(int x, int y, int z, int iPointerID)
		{
		}
*/
		// tool events
		private void OnPen(int iPointerID)
		{
            var newMessage = "SMARTboard pen";
            SMARTboardDiagnosticOutput.Items.Add(newMessage);
        }

		private void OnNoTool(int iPointerID)
		{
            var newMessage = "SMARTboard NoTool";
            SMARTboardDiagnosticOutput.Items.Add(newMessage);

//			CurrentPen.Color = Color.Black;
//			CurrentPen.Width = 1;
		}

		private void OnCircle(int iPointerID)
		{
            var newMessage = "SMARTboardPen Circle";
            SMARTboardDiagnosticOutput.Items.Add(newMessage);
		}

		private void OnEraser(int iPointerID)
		{
            var newMessage = "SMARTboardPen Eraser";
            SMARTboardDiagnosticOutput.Items.Add(newMessage);

		}

		private void OnLine(int iPointerID)
		{
            var newMessage = "SMARTboardPen Line";
            SMARTboardDiagnosticOutput.Items.Add(newMessage);

		}

		private void OnRectangle(int iPointerID)
		{
            var newMessage = "SMARTboardPen Rectangle";
            SMARTboardDiagnosticOutput.Items.Add(newMessage);

		}

		// pen tray button events
		private void OnClear(int iPointerID)
		{
            var newMessage = "SMARTboardPen Clear";
            SMARTboardDiagnosticOutput.Items.Add(newMessage);

		}

		private void OnNext(int iPointerID)
		{
            var newMessage = "SMARTboardPen Next";
            SMARTboardDiagnosticOutput.Items.Add(newMessage);

		}

		private void OnPrevious(int iPointerID)
		{
            var newMessage = "SMARTboardPen Previous";
            SMARTboardDiagnosticOutput.Items.Add(newMessage);

		}

		private void OnPrint(int iPointerID)
		{
            var newMessage = "SMARTboardPen Print";
            SMARTboardDiagnosticOutput.Items.Add(newMessage);

		}

		// triggered when a SMART Board is connected or disconnected
		private void OnBoardStatusChange()
		{
            var newMessage = "SMARTboard boardStatusChange";
            SMARTboardDiagnosticOutput.Items.Add(newMessage);

		}

		// if SBSDKAttach is called with the second parameter set to true
		// this event may be triggered, if you are unsure then
		// don't implement it
		private void OnXMLAnnotation(string xml)
        {
            var newMessage = "SMARTboard XML annotation";
            SMARTboardDiagnosticOutput.Items.Add(newMessage);

		}

		// non-projected contact events
		private void OnXYNonProjectedDown(int x, int y, int z, int iPointerID)
		{
            var newMessage = "SMARTboard XYNonProjectedDown";
            SMARTboardDiagnosticOutput.Items.Add(newMessage);

		}

		private void OnXYNonProjectedMove(int x, int y, int z, int iPointerID)
		{
		}

		private void OnXYNonProjectedUp(int x, int y, int z, int iPointerID)
		{
            var newMessage = "SMARTboard XYNonProjectedUp";
            SMARTboardDiagnosticOutput.Items.Add(newMessage);

		}

        private void SMARTboardDiagnosticOutput_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {

        }
	}
    /*
	public class Stroke
	{
		public ArrayList StrokePoints = new ArrayList();
		public Color StrokeColor;
		public int StrokeWidth = 3;
		public int iPointerID = 0;
	}*/

        //end yet more smartboardstuff

}
/*
    //forms object
    public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TextBox texbox1;
		private System.Windows.Forms.Button button1;
		private ISBSDKBaseClass2 Sbsdk;
		private _ISBSDKBaseClass2Events_Event SbsdkEvents;
		private _ISBSDKBaseClass2HoverEvents_Event SbsdkHoverEvents;
		private System.Drawing.Pen CurrentPen = new System.Drawing.Pen(System.Drawing.Color.Black, 1);
		private ArrayList CurrentStrokeList = new ArrayList();  // List of strokes being drawn by the user.
		private ArrayList StrokeList = new ArrayList();  // List of all strokes.

		[DllImport("user32.dll")]
		public static extern int RegisterWindowMessageA([MarshalAs(UnmanagedType.LPStr)] string lpString);

		private int SBSDKMessageID = RegisterWindowMessageA("SBSDK_NEW_MESSAGE");

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
				
			this.SetStyle(ControlStyles.EnableNotifyMessage, true);

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			try
			{
				Sbsdk = new SBSDKBaseClass2();
			}
			catch (Exception e)
			{
				// handle exception, we will do nothing
				Console.WriteLine("Exception in Main: " + e.Message);
			}
			if (Sbsdk != null)
			{
				SbsdkEvents = (_ISBSDKBaseClass2Events_Event)Sbsdk;
				SbsdkHoverEvents = (_ISBSDKBaseClass2HoverEvents_Event)Sbsdk;
			}
			if (SbsdkEvents != null)
			{
				SbsdkEvents.OnCircle += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnCircleEventHandler(this.OnCircle);
				SbsdkEvents.OnClear += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnClearEventHandler(this.OnClear);
				SbsdkEvents.OnEraser += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnEraserEventHandler(this.OnEraser);
				SbsdkEvents.OnLine += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnLineEventHandler(this.OnLine);
				SbsdkEvents.OnNext += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnNextEventHandler(this.OnNext);
				SbsdkEvents.OnNoTool += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnNoToolEventHandler(this.OnNoTool);
				SbsdkEvents.OnPen += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnPenEventHandler(this.OnPen);
				SbsdkEvents.OnPrevious += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnPreviousEventHandler(this.OnPrevious);
				SbsdkEvents.OnPrint += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnPrintEventHandler(this.OnPrint);
				SbsdkEvents.OnRectangle += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnRectangleEventHandler(this.OnRectangle);
				SbsdkEvents.OnBoardStatusChange += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnBoardStatusChangeEventHandler(this.OnBoardStatusChange);
				SbsdkEvents.OnXMLAnnotation += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXMLAnnotationEventHandler(this.OnXMLAnnotation);
				SbsdkEvents.OnXYNonProjectedDown += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYNonProjectedDownEventHandler(this.OnXYNonProjectedDown);
				SbsdkEvents.OnXYNonProjectedMove += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYNonProjectedMoveEventHandler(this.OnXYNonProjectedMove);
				SbsdkEvents.OnXYNonProjectedUp += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYNonProjectedUpEventHandler(this.OnXYNonProjectedUp);
				SbsdkEvents.OnXYDown += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYDownEventHandler(this.OnXYDown);
				SbsdkEvents.OnXYMove += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYMoveEventHandler(this.OnXYMove);
				SbsdkEvents.OnXYUp += new SBSDKComWrapperLib._ISBSDKBaseClass2Events_OnXYUpEventHandler(this.OnXYUp);
			}
			if (SbsdkHoverEvents != null)
			{
				SbsdkHoverEvents.OnXYHover += new SBSDKComWrapperLib._ISBSDKBaseClass2HoverEvents_OnXYHoverEventHandler(this.OnXYHover);
			}

			if (Sbsdk != null)
			{
				Sbsdk.SBSDKAttachWithMsgWnd(Handle.ToInt32(), false, Handle.ToInt32());
			}

			// always get events through the sdk
			// by default mouse events are sent when no tool is selected
			//Sbsdk.SBSDKSetSendMouseEvents(Handle.ToInt32(), SBCME_NEVER, -1);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			CurrentPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
			this.button1 = new System.Windows.Forms.Button();
			this.texbox1 = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.button1.BackColor = System.Drawing.SystemColors.ActiveBorder;
			this.button1.Location = new System.Drawing.Point(392, 296);
			this.button1.Name = "button1";
			this.button1.TabIndex = 0;
			this.button1.Text = "Close";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			//
			// texbox1
			//
			this.texbox1.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left);
			this.texbox1.BackColor = System.Drawing.SystemColors.AppWorkspace;
			this.texbox1.Height = 100;
			this.texbox1.Width = 800;
			this.texbox1.Location = new System.Drawing.Point(0, 0);
			this.texbox1.Name = "textbox1";
			this.texbox1.TabIndex = 0;
			this.texbox1.Multiline = true;
			this.texbox1.ScrollBars = ScrollBars.Vertical;
			
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(480, 333);
			this.Controls.Add(texbox1);
			//this.Controls.AddRange(new System.Windows.Forms.Control[] {
			//															  this.button1});
			this.Name = "Form1";
			this.Text = "Form1";
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnMouseUp);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			System.Windows.Forms.Application.Run(new Form1());
		}

		protected override void OnNotifyMessage(Message m)
		{
			if (m.Msg == SBSDKMessageID)
			{
				// process sdk events on our apps main thread
				Sbsdk.SBSDKProcessData();
			}
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			OnXYDown(e.X, e.Y, 0, 0);
		}

		private void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			OnXYMove(e.X, e.Y, 0, 0);
		}

		private void OnMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			OnXYUp(e.X, e.Y, 0, 0);
		}

		private void OnPaint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			int i, j;
			ArrayList Stroke;
			Point Pt, LastPt;
			Graphics g = CreateGraphics();
			Pen pen = new Pen(System.Drawing.Color.Black, 1);

			pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
			for (i = 0; i < StrokeList.Count; i++)
			{
				Stroke = (ArrayList)((Stroke)StrokeList[i]).StrokePoints;
				pen.Color = ((Stroke)StrokeList[i]).StrokeColor;
				pen.Width = (float)((Stroke)StrokeList[i]).StrokeWidth;
				if (Stroke.Count > 0)
				{
					LastPt = (Point)Stroke[0];
					for (j = 1; j < Stroke.Count; j++)
					{
						Pt = (Point)Stroke[j];
						g.DrawLine(pen, LastPt, Pt);
						LastPt = Pt;
					}
				}
			}
		}

		private Stroke GetCurrentStroke(int iPointerId)
		{
			for (int i = 0; i < CurrentStrokeList.Count; i++)
			{
				if (((Stroke)CurrentStrokeList[i]).iPointerID == iPointerId)
				{
					return (Stroke)CurrentStrokeList[i];
				}
			}

			return null;
		}

		private void RemoveCurrentStroke(int iPointerId)
		{
			for (int i = 0; i < CurrentStrokeList.Count; i++)
			{
				if (((Stroke)CurrentStrokeList[i]).iPointerID == iPointerId)
				{
					CurrentStrokeList.RemoveAt(i);
					return;
				}
			}
		}

		// projected contact events
		private void OnXYDown(int x, int y, int z, int iPointerID)
		{
			Point Pt = new Point(x, y);
			// create a new stroke and store all the info related to it
			Stroke CurrentStroke = new Stroke();
			CurrentStroke.iPointerID = iPointerID;
			CurrentStroke.StrokePoints.Add(Pt);
			StrokeList.Add(CurrentStroke);
			CurrentStrokeList.Add(CurrentStroke);

			CurrentPen.Width = 3;
			int iRed = 0, iGreen = 0, iBlue = 0;
			if (Sbsdk != null)
			{
				Sbsdk.SBSDKGetToolColor(iPointerID, out iRed, out iGreen, out iBlue);
				CurrentPen.Width = Sbsdk.SBSDKGetToolWidth(iPointerID);
			}
			CurrentPen.Color = Color.FromArgb(iRed, iGreen, iBlue);
			CurrentStroke.StrokeColor = CurrentPen.Color;
			CurrentStroke.StrokeWidth = (int)CurrentPen.Width;
		}

		private void OnXYMove(int x, int y, int z, int iPointerID)
		{
			Point Pt = new Point(x, y);
			// check if we have a stroke started for this pointer id
			Stroke CurrentStroke = GetCurrentStroke(iPointerID);
			if (CurrentStroke == null)
			{
				return;
			}

			Point PreviousPt = (Point)CurrentStroke.StrokePoints[CurrentStroke.StrokePoints.Count - 1];
			CurrentStroke.StrokePoints.Add(Pt);
			Graphics g = CreateGraphics();
			CurrentPen.Color = CurrentStroke.StrokeColor;
			CurrentPen.Width = (float)CurrentStroke.StrokeWidth;
			g.DrawLine(CurrentPen, PreviousPt, Pt);
		}

		private void OnXYUp(int x, int y, int z, int iPointerID)
		{
			Point Pt = new Point(x, y);
			// check if we have a stroke started for this pointer id
			Stroke CurrentStroke = GetCurrentStroke(iPointerID);
			if (CurrentStroke == null)
			{
				return;
			}

			Point PreviousPt = (Point)CurrentStroke.StrokePoints[CurrentStroke.StrokePoints.Count - 1];
			CurrentStroke.StrokePoints.Add(Pt);
			Graphics g = CreateGraphics();
			CurrentPen.Color = CurrentStroke.StrokeColor;
			CurrentPen.Width = (float)CurrentStroke.StrokeWidth;
			g.DrawLine(CurrentPen, PreviousPt, Pt);
			this.texbox1.AppendText("Time:"+DateTime.Now.ToFileTimeUtc()+", Stroke:"+CurrentStroke.StrokePoints.Count.ToString()+", Color is:"+CurrentPen.Color+", Width is:"+CurrentPen.Width);
			this.texbox1.AppendText("\r\n");
			RemoveCurrentStroke(iPointerID);
		}

		private void OnXYHover(int x, int y, int z, int iPointerID)
		{
		}

		// tool events
		private void OnPen(int iPointerID)
		{
		}

		private void OnNoTool(int iPointerID)
		{
			CurrentPen.Color = Color.Black;
			CurrentPen.Width = 1;
		}

		private void OnCircle(int iPointerID)
		{
		}

		private void OnEraser(int iPointerID)
		{
		}

		private void OnLine(int iPointerID)
		{
		}

		private void OnRectangle(int iPointerID)
		{
		}

		// pen tray button events
		private void OnClear(int iPointerID)
		{
		}

		private void OnNext(int iPointerID)
		{
		}

		private void OnPrevious(int iPointerID)
		{
		}

		private void OnPrint(int iPointerID)
		{
		}

		// triggered when a SMART Board is connected or disconnected
		private void OnBoardStatusChange()
		{
		}

		// if SBSDKAttach is called with the second parameter set to true
		// this event may be triggered, if you are unsure then
		// don't implement it
		private void OnXMLAnnotation(string xml)
		{
		}

		// non-projected contact events
		private void OnXYNonProjectedDown(int x, int y, int z, int iPointerID)
		{
		}

		private void OnXYNonProjectedMove(int x, int y, int z, int iPointerID)
		{
		}

		private void OnXYNonProjectedUp(int x, int y, int z, int iPointerID)
		{
		}
	}

	public class Stroke
	{
		public ArrayList StrokePoints = new ArrayList();
		public Color StrokeColor;
		public int StrokeWidth = 3;
		public int iPointerID = 0;
	}
}
*/