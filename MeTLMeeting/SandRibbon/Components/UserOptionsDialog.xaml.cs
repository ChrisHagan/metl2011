using System;
using System.Windows;
using SandRibbon.Providers;
using MeTLLib.DataTypes;
using SandRibbon.Components.Sandpit;
using System.Diagnostics;
using Divelements.SandRibbon;

namespace SandRibbon.Components
{
    public partial class UserOptionsDialog : Window
    {
        protected MeTLLib.MetlConfiguration backend;
        public UserOptionsDialog()
        {
            InitializeComponent();
            DataContext = Globals.UserOptions;
        }
        private void Apply(object sender, RoutedEventArgs e)
        {
            AppCommands.SetUserOptions.Execute(DataContext);
            //this should be wired to a new command - SaveUserOptions, which is commented out in SandRibbonInterop.Commands
            //Commands.SaveUserOptions.Execute(DataContext);
            var level = Pedagogicometer.level((Pedagogicometry.PedagogyCode)((UserOptions)DataContext).pedagogyLevel);
            Trace.TraceInformation("SetPedagogy {0}",level.label);
            AppCommands.SetPedagogyLevel.Execute(level);
            // ChangeLanguage commented out for 182 staging release. Causing a crash.
            //Commands.ChangeLanguage.Execute(System.Windows.Markup.XmlLanguage.GetLanguage(((UserOptions)DataContext).language));

            Close();
        }
        private void Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Reset(object sender, RoutedEventArgs e)
        {
            var defaults = UserOptions.DEFAULT;
            var level = Pedagogicometer.level((Pedagogicometry.PedagogyCode)defaults.pedagogyLevel);

            AppCommands.SetUserOptions.Execute(defaults);
            AppCommands.SetPedagogyLevel.Execute(level);
            Close();
        }
    }
}
