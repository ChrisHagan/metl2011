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
        //public string url { get; set; }
        public string author { get; set; }
        public string fileImage { get; set; }
        public string uploadTime { get; set; }
        public long size { get; set; }
        public string tooltip { get; set; }
        public string identity { get; set; }
    }
    public partial class Attachments : RibbonTab
    {
        private ObservableCollection<FileInfo> files;
        public Attachments()
        {
            InitializeComponent();
            files = new ObservableCollection<FileInfo>();
            attachments.ItemsSource = files;
            Commands.ReceiveFileResource.RegisterCommand(new DelegateCommand<MeTLLib.DataTypes.TargettedFile>(receiveFile));
            Commands.PreParserAvailable.RegisterCommand(new DelegateCommand<PreParser>(preparserAvailable));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<object>(clearOutAttachments));
            Commands.UpdateConversationDetails.RegisterCommand(new DelegateCommand<ConversationDetails>(UpdateConversationDetails));
            Commands.FileUpload.RegisterCommand(new DelegateCommand<object>((_unused) => { UploadFile(); }));
        }
        private void UpdateConversationDetails(ConversationDetails details)
        {
            Dispatcher.adopt(delegate
            {
                if (details.IsEmpty) return;
                if (details.IsJidEqual(Globals.location.activeConversation) && details.isDeleted)
                    clearOutAttachments(null);
            });
        }
        private void clearOutAttachments(object obj)
        {
            Dispatcher.adopt(delegate
            {
                files = new ObservableCollection<FileInfo>();
                attachments.ItemsSource = files;
            });
        }

        private void preparserAvailable(PreParser preParser)
        {
            foreach (var file in preParser.files)
                receiveFile(file);
        }
        private void receiveFile(MeTLLib.DataTypes.TargettedFile fileInfo)
        {
            //if (!Globals.conversationDetails.IsJidEqual(fileInfo.conversationJid.ToString())) return;  // it appears that the files aren't holding their jids properly, which is not surprising, given that only C# thinks it's necessary for the element to know it, and I'm not sure what it's using it for, other than this check.
            Dispatcher.adoptAsync(() =>
            {
                var fileInfoFileType = FileHelper.DetermineFileTypeFromExtension(fileInfo.name);
                if (files.Select(f => f.identity).Contains(fileInfo.identity)) return;
                var uploadTime = DateTime.Now;
                try
                {
                    uploadTime = new DateTime(long.Parse(fileInfo.uploadTime));
                }
                catch (Exception ex)
                {
                    try
                    {
                        uploadTime = DateTime.Parse(fileInfo.uploadTime);
                    }
                    catch (Exception exc)
                    {
                    }
                }
                files.Add(new FileInfo
                {
                    fileType = fileInfoFileType,
                    filename = fileInfo.name,
                    author = fileInfo.author,
                    identity = fileInfo.identity,
                    fileImage = FileHelper.GetFileTypeImageResource(fileInfo.name),
                    uploadTime = fileInfo.uploadTime,
                    size = fileInfo.size,
                    tooltip = string.Format("Type: {0}\nAuthor: {1}\nUpload Time: {2}\nSize: {3}",
                    fileInfoFileType,
                    fileInfo.author,
                    uploadTime,
                    string.Format(new FileSizeFormatProvider(), "{0:fs}", fileInfo.size))
                });
            });
        }
        private void saveFile(object sender, RoutedEventArgs e)
        {
            var file = (FileInfo)((Button)sender).DataContext;
            var saveFile = new SaveFileDialog();
            saveFile.FileName = file.filename;
            saveFile.Filter = string.Format("{0} (*{1})|*{1}|All Files (*.*)|*.*", file.fileType, System.IO.Path.GetExtension(file.filename));
            saveFile.FilterIndex = 1;
            saveFile.RestoreDirectory = true;
            if (saveFile.ShowDialog(Window.GetWindow(this)) == true)
            {
                var backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += (s, a) =>
                                               {
                                                   var stream = saveFile.OpenFile();
                                                   var sourceBytes = App.controller.client.downloaderFactory.client().downloadData(App.controller.config.getResource(file.identity));
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
            var upload = new OpenFileForUpload(Window.GetWindow(this));
            upload.AddResourceFromDisk();
        }
    }
}
