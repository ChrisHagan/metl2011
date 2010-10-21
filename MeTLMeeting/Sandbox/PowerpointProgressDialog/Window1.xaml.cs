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

namespace PowerpointProgressDialog
{
    public partial class Window1 : Window
    {
        SimplePowerpointLoader loader = new SimplePowerpointLoader();
        public Window1()
        {
            InitializeComponent();
            Loaded += Window1_Loaded;
        }
        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            loader.goingToWorkOn += intention => progress.setItemsToTransfer(Enumerable.Range(0,intention.count).Select(i=>new SlideProgress()));
            loader.workingOn += progressReport => progress.setItemInProgress(progressReport);
            loader.workComplete += nothing => progress.finished();
            loader.Load();
        }
    }
}