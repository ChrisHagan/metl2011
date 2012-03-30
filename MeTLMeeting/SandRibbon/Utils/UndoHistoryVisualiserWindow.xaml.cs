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
using System.Windows.Shapes;

namespace SandRibbon.Utils
{
    /// <summary>
    /// Interaction logic for UndoHistoryVisualiserWindow.xaml
    /// </summary>
    public partial class UndoHistoryVisualiserWindow : Window
    {
        public UndoHistoryVisualiserWindow()
        {
            InitializeComponent();
        }

        public void UpdateUndoView(IEnumerable<String> items)
        {
            UpdateView(UndoQueueList.Items, items);
        }

        public void UpdateRedoView(IEnumerable<String> items)
        {
            UpdateView(RedoQueueList.Items, items);
        }

        private void UpdateView(ItemCollection itemCollection, IEnumerable<String> items)
        {
            itemCollection.Clear();
            foreach (var item in items)
            {
                var description = String.IsNullOrEmpty(item) ? "Unknown" : item;
                itemCollection.Add(description);
            }
        }

        public void ClearViews()
        {
            UndoQueueList.Items.Clear();
            RedoQueueList.Items.Clear();
        }
    }
}
