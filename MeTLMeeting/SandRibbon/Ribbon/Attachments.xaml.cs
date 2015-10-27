using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Threading;
using Divelements.SandRibbon;
using MeTLLib.DataTypes;
using MeTLLib.Providers.Connection;
using Microsoft.Practices.Composite.Presentation.Commands;
using Microsoft.Win32;
using SandRibbon.Providers;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;
using SandRibbon.Components.Utility;
using System.Collections.Generic;
using MeTLLib.Providers;
using SandRibbon.Utils;
using MeTLLib;

namespace SandRibbon.Tabs
{
    public class FileInfo
    {
        public string fileType { get; set; }
        public string filename { get; set; }
        public string url { get; set; }
        public string author {get; set;}
        public string fileImage { get; set; }
        public string uploadTime { get; set; }
        public long size { get; set; }
        public string tooltip { get; set; }
    }
    public partial class Attachments :RibbonTab 
    {
        private ObservableCollection<FileInfo> files; 
        public Attachments()
        {
            InitializeComponent();
            files = new ObservableCollection<FileInfo>();
            attachments.ItemsSource = files;
            App.getContextFor(backend).controller.commands.ReceiveFileResource.RegisterCommand(new DelegateCommand<MeTLLib.DataTypes.TargettedFile>(receiveFile));
            App.getContextFor(backend).controller.commands.PreParserAvailable.RegisterCommand(new DelegateCommand<PreParser>(preparserAvailable));
            App.getContextFor(backend).controller.commands.JoinConversation.RegisterCommandToDispatcher(new DelegateCommand<object>(clearOutAttachments));
            App.getContextFor(backend).controller.commands.UpdateConversationDetails.RegisterCommandToDispatcher(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            AppCommands.FileUpload.RegisterCommand(new DelegateCommand<object>((_unused) => { UploadFile(); }));
        }
        protected MetlConfiguration backend = MetlConfiguration.empty; //This needs to be threaded in, when we get around to making this a flyout, as we'll have to.
        private void UpdateConversationDetails(ConversationDetails details)
        {
            if (details.IsEmpty) return;
            if (details.IsJidEqual(Globals.location.activeConversation) && details.isDeleted)
                clearOutAttachments(null);
        }
        private void clearOutAttachments(object obj)
        {
            files = new ObservableCollection<FileInfo>();
            attachments.ItemsSource = files;
        }

        private void preparserAvailable(PreParser preParser)
        {
            foreach(var file in preParser.files)
                receiveFile(file);
        }
        private void receiveFile(MeTLLib.DataTypes.TargettedFile fileInfo)
        {
            if (!Globals.conversationDetails.IsJidEqual(fileInfo.conversationJid.ToString())) return;
            Dispatcher.adoptAsync(() => {
                                            var fileInfoFileType = FileHelper.DetermineFileTypeFromExtension(fileInfo.name);
                                            if (files.Select(f => f.url).Contains(fileInfo.url)) return;
                                            files.Add(new FileInfo
                                                     {
                                                         fileType = fileInfoFileType,
                                                         filename = fileInfo.name,
                                                         url = fileInfo.url,
                                                         author = fileInfo.author,
                                                         fileImage = FileHelper.GetFileTypeImageResource(fileInfo.name),
                                                         uploadTime = fileInfo.uploadTime, 
                                                         size = fileInfo.size,
                                                         tooltip = string.Format("Type: {0}\nAuthor: {1}\nUpload Time: {2}\nSize: {3}", 
                                                            fileInfoFileType, 
                                                            fileInfo.author, 
                                                            new DateTime(long.Parse(fileInfo.uploadTime)), 
                                                            string.Format(new FileSizeFormatProvider(), "{0:fs}", fileInfo.size))
                                                     }); 
                                            });
        }
        private void saveFile(object sender, RoutedEventArgs e)
        {
            var file = (FileInfo)((Button) sender).DataContext;
            var saveFile = new SaveFileDialog();
            saveFile.FileName = file.filename;
            saveFile.Filter = string.Format("{0} (*{1})|*{1}|All Files (*.*)|*.*", file.fileType, System.IO.Path.GetExtension(file.filename));
            saveFile.FilterIndex = 1;
            saveFile.RestoreDirectory = true;
            if(saveFile.ShowDialog(Window.GetWindow(this)) == true)
            {
                var backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += (s, a) =>
                                               {
                                                   var stream = saveFile.OpenFile();
                                                   var sourceBytes = new WebClient { Credentials = new NetworkCredential( backend.resourceUsername, backend.resourcePassword) }.DownloadData(file.url);
                                                   stream.Write(sourceBytes, 0, sourceBytes.Count());
                                                   stream.Close();
                                               };
                backgroundWorker.RunWorkerCompleted += (s, a) => Dispatcher.Invoke(DispatcherPriority.Send,
                                                                                   (Action)(() => MeTLMessage.Information(string.Format("Finished downloading {0}.", saveFile.FileName))));
                backgroundWorker.RunWorkerAsync();
            }
        }

        private void UploadFile()
        {
            var upload = new OpenFileForUpload(backend,Window.GetWindow(this));
            upload.AddResourceFromDisk();
        }
    }
}
