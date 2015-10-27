using MeTLLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SandRibbon
{

    public class ServerAwareUserControl : UserControl
    {
        public MetlConfiguration ServerConfig
        {
            get { return (MetlConfiguration)GetValue(serverConfigProperty); }
            protected set { SetValue(serverConfigProperty, value); }
        }

        // Using a DependencyProperty as the backing store for serverConfig.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty serverConfigProperty =
            DependencyProperty.Register("serverConfig", typeof(MetlConfiguration), typeof(ServerAwareUserControl), new PropertyMetadata(MetlConfiguration.empty,(sender,args) => {

            }));
        public MetlContext ServerContext
        {
            get { return (MetlContext)GetValue(ServerContextProperty); }
            protected set { SetValue(ServerContextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ServerContext.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ServerContextProperty =
            DependencyProperty.Register("ServerContext", typeof(MetlContext), typeof(ServerAwareUserControl), new PropertyMetadata(MetlContext.empty));
        public ServerAwareUserControl() : base() {
            this.Resources.Add("serverConfig", ServerConfig);
            this.Resources.Add("serverContext", ServerContext);
        }
        public ServerAwareUserControl(MetlConfiguration backend) : base()
        {
            ServerConfig = backend;
            ServerContext = App.getContextFor(backend);
            this.Resources.Add("serverConfig", ServerConfig);
            this.Resources.Add("serverContext", ServerContext);
        }

    }

    public class ServerAwareWindow : Window
    {
        public MetlConfiguration ServerConfig
        {
            get { return (MetlConfiguration)GetValue(serverConfigProperty); }
            protected set { SetValue(serverConfigProperty, value); }
        }

        // Using a DependencyProperty as the backing store for serverConfig.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty serverConfigProperty =
            DependencyProperty.Register("serverConfig", typeof(MetlConfiguration), typeof(ServerAwareWindow), new PropertyMetadata(MetlConfiguration.empty));
        public MetlContext ServerContext
        {
            get { return (MetlContext)GetValue(ServerContextProperty); }
            protected set { SetValue(ServerContextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ServerContext.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ServerContextProperty =
            DependencyProperty.Register("ServerContext", typeof(MetlContext), typeof(ServerAwareWindow), new PropertyMetadata(MetlContext.empty));
        public ServerAwareWindow() : base() { }
        public ServerAwareWindow(MetlConfiguration backend) : base()
        {
            ServerConfig = backend;
            ServerContext = App.getContextFor(backend);
            this.Resources.Add("serverConfig", ServerConfig);
            this.Resources.Add("serverContext", ServerContext);
        }
    }

    public class ServerAwarePage : Page
    {


        public MetlConfiguration ServerConfig
        {
            get { return (MetlConfiguration)GetValue(serverConfigProperty); }
            protected set { SetValue(serverConfigProperty, value); }
        }

        // Using a DependencyProperty as the backing store for serverConfig.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty serverConfigProperty =
            DependencyProperty.Register("serverConfig", typeof(MetlConfiguration), typeof(ServerAwarePage), new PropertyMetadata(MetlConfiguration.empty));



        public MetlContext ServerContext
        {
            get { return (MetlContext)GetValue(ServerContextProperty); }
            protected set { SetValue(ServerContextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ServerContext.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ServerContextProperty =
            DependencyProperty.Register("ServerContext", typeof(MetlContext), typeof(ServerAwarePage), new PropertyMetadata(MetlContext.empty));

        public ServerAwarePage() : base()
        {
            this.GotFocus += ServerAwarePage_GotFocus;
        }

        private void ServerAwarePage_GotFocus(object sender, RoutedEventArgs e)
        {
            AppCommands.BackendSelected.Execute(ServerConfig);
        }

        public ServerAwarePage(MetlConfiguration config) : base()
        {
            ServerConfig = config;
            ServerContext = App.getContextFor(config);
            this.Resources.Add("serverConfig", ServerConfig);
            this.Resources.Add("serverContext", ServerContext);
            this.Loaded += (sender, args) =>
            {
                AppCommands.BackendSelected.Execute(config);
            };
        }
    }
}
