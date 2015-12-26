using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using MeTLLib.DataTypes;
using MeTLLib.Providers.Connection;
using Microsoft.Practices.Composite.Presentation.Commands;
using Microsoft.Win32;
using Button = System.Windows.Controls.Button;
using SandRibbon.Components.Utility;
using MeTLLib.Providers;
using SandRibbon.Utils;
using System.Windows.Controls.Ribbon;
using SandRibbon.Pages;

namespace SandRibbon.Tabs
{
    public class FileInfo
    {
        public string fileType { get; set; }
        public string filename { get; set; }
        public string url { get; set; }
        public string author { get; set; }
        public string fileImage { get; set; }
        public string uploadTime { get; set; }
        public long size { get; set; }
        public string tooltip { get; set; }
    }
    public partial class Attachments : RibbonTab
    {
        private ObservableCollection<FileInfo> files;
        public SlideAwarePage rootPage { get; protected set; }
        public Attachments()
        {
            InitializeComponent();
            files = new ObservableCollection<FileInfo>();
            attachments.ItemsSource = files;
            var receiveFilesCommand = new DelegateCommand<MeTLLib.DataTypes.TargettedFile>(receiveFile);
            var preParserAvailableCommand = new DelegateCommand<PreParser>(preparserAvailable);
            Loaded += (s, e) =>
            {
                if (rootPage == null)
                    rootPage = DataContext as SlideAwarePage;
                if (!Resources.Contains("ConvertStringToImageSource"))
                {
                    Resources.Add("ConvertStringToImageSource", new ConvertStringToImageSource(rootPage.NetworkController));
                }
                DataContext = this;
                Commands.ReceiveFileResource.RegisterCommand(receiveFilesCommand);
                Commands.PreParserAvailable.RegisterCommand(preParserAvailableCommand);
            };
            Unloaded += (s, e) =>
            {
                Commands.ReceiveFileResource.UnregisterCommand(receiveFilesCommand);
                Commands.PreParserAvailable.UnregisterCommand(preParserAvailableCommand);
            };
        }

        private void preparserAvailable(PreParser preParser)
        {
            foreach (var file in preParser.files)
                receiveFile(file);
        }
        private void receiveFile(MeTLLib.DataTypes.TargettedFile fileInfo)
        {
            if (!rootPage.ConversationDetails.IsJidEqual(fileInfo.conversationJid.ToString())) return;
            Dispatcher.adoptAsync(() =>
            {
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
                                                   var sourceBytes = rootPage.NetworkController.client.resourceProvider.secureGetData(new Uri(file.url,UriKind.RelativeOrAbsolute));
                                                   stream.Write(sourceBytes, 0, sourceBytes.Count());
                                                   stream.Close();
                                               };
                backgroundWorker.RunWorkerCompleted += (s, a) => Dispatcher.Invoke(DispatcherPriority.Send,
                                                                                   (Action)(() => MeTLMessage.Information(string.Format("Finished downloading {0}.", saveFile.FileName))));
                backgroundWorker.RunWorkerAsync();
            }
        }
    }
}
