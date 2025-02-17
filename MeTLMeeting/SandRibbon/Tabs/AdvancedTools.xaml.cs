﻿using System;
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
using Divelements.SandRibbon;
using MeTLLib.DataTypes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components;
using SandRibbon.Providers;
using SandRibbon.Utils;

namespace SandRibbon.Tabs
{
    public partial class AdvancedTools: RibbonTab
    {
        public AdvancedTools()
        {
            InitializeComponent();
            Commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
        }

        private void UpdateConversationDetails(ConversationDetails details)
        {
            manageBlackList.Visibility = details.Author == Globals.me ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ManageBlacklist(object sender, RoutedEventArgs e)
        {
            new blacklistController().ShowDialog();
        }
    }
}
