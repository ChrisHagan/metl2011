using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Pedagogicometry;
using System.Collections.ObjectModel;

namespace SandRibbon.Chrome
{
    public partial class ToolBar
    {
        private ObservableCollection<FrameworkElement> icons = new ObservableCollection<FrameworkElement>();
        public ToolBar()
        {
            InitializeComponent();
            Commands.SetPedagogyLevel.RegisterCommand(new DelegateCommand<PedagogyLevel>(SetPedagogyLevel));
        }
        private void clearUI()
        {
            Dispatcher.adoptAsync(() =>
            {
                foreach (var item in (ItemCollection)Items)
                {
                    var button = (Divelements.SandRibbon.Button)item;
                    button.Visibility = Visibility.Collapsed;
                }
            });
        }
        private void add(string key) {
            Dispatcher.adoptAsync(() =>
            {
                var element = ((Divelements.SandRibbon.Button)this.FindName(key));
                element.Visibility = Visibility.Visible;
            });
        }
        private void SetPedagogyLevel(PedagogyLevel level) 
        { 
            foreach(var i in Enumerable.Range(0, ((int)level.code) + 1))
            {
                switch (i)
                {
                    case 0:
                        clearUI();
                        add("undo");
                        add("redo");
                        break;
                    case 3:
                        //add("friends");
                        //add("notes");
                        add("lectureStyle");
                        add("tutorialStyle");
                        //add("meetingStyle");
                        break; 
                }
            }
        }
    }
}
