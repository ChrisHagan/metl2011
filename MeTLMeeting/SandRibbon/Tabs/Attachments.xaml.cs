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
using Divelements.SandRibbon;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Components.Submissions;
using SandRibbon.Utils.Connection;
using SandRibbonInterop.MeTLStanzas;

namespace SandRibbon.Tabs
{
    public partial class Attachments :RibbonTab 
    {
        private List<FileInfo> files; 
        public Attachments()
        {
            InitializeComponent();
            files = new List<FileInfo>();
            Commands.ReceiveFileResource.RegisterCommand(new DelegateCommand<TargettedFile>(receiveFile));
            Commands.PreParserAvailable.RegisterCommand(new DelegateCommand<PreParser>(preparserAvailable));
        }
        private void preparserAvailable(PreParser preParser)
        {
            foreach(var file in preParser.files)
                receiveFile(file);
        }
        private void receiveFile(TargettedFile file)
        {
           Dispatcher.adoptAsync(() => files.Add(new FileInfo
                                                     {
                                                         fileType = FileUploads.getFileType(file.url),
                                                         filename = FileUploads.getFileName(file.url),
                                                         url = file.url,
                                                         author = file.author,
                                                         fileImage = FileUploads.getFileImage(file.url)
                                                     }));
        }

        private void openUploads(object sender, RoutedEventArgs e)
        {
            new FileUploads(files).Show();
        }
    }
}
