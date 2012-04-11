using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SandRibbon.Utils
{
    public partial class UndoHistory
    {
        #region Debug helpers

        public static void ShowVisualiser(Window parent)
        {
            visualiser.Show(parent);
        }

        public static void HideVisualiser()
        {
            visualiser.Hide();
        }

        #endregion

        public class UndoHistoryVisualiser
        {
            UndoHistoryVisualiserWindow window;
 
            bool IsEnabled
            {
                get { return window != null && window.IsVisible; }
            }

            public UndoHistoryVisualiser()
            {
            }

            public void ClearViews()
            {
                window.ClearViews();
            }

            public void UpdateUndoView(Stack<HistoricalAction> undo)
            {
                if (IsEnabled)
                {
                    window.UpdateUndoView(undo.Select(hist => hist.description));
                }
            }

            public void UpdateRedoView(Stack<HistoricalAction> redo)
            {
                if (IsEnabled)
                {
                    window.UpdateRedoView(redo.Select(hist => hist.description));
                }
            }

            public void Show(Window parent)
            {
                window = new UndoHistoryVisualiserWindow();
                window.Owner = parent;
                window.Show();
            }

            public void Hide()
            {
                if (window != null)
                {
                    window.Hide();
                    window.Close();
                }
            }
        }
    }
}
