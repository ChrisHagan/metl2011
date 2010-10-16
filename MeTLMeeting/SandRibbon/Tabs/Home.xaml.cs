namespace SandRibbon.Tabs
{
    public partial class Home : Divelements.SandRibbon.RibbonTab
    {
        public Home()
        {
            InitializeComponent();
            Commands.SetLayer.ExecuteAsync("Sketch");
        }
    }
}
