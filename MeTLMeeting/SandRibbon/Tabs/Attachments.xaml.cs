using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
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
using Button=System.Windows.Controls.Button;

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
            Commands.ReceiveFileResource.RegisterCommand(new DelegateCommand<TargettedFile>(receiveFile));
            Commands.PreParserAvailable.RegisterCommand(new DelegateCommand<PreParser>(preparserAvailable));
            Commands.JoinConversation.RegisterCommand(new DelegateCommand<object>(joinConversation));
        }

        private void joinConversation(object obj)
        {
            files = new ObservableCollection<FileInfo>();
            attachments.ItemsSource = files;
        }

        private void preparserAvailable(PreParser preParser)
        {
            foreach(var file in preParser.files)
                receiveFile(file);
        }
        private void receiveFile(TargettedFile fileInfo)
        {
           Dispatcher.adoptAsync(() => files.Add(new FileInfo
                                                     {
                                                         fileType = getFileType(fileInfo.url),
                                                         filename = fileInfo.name,
                                                         url = fileInfo.url,
                                                         author = fileInfo.author,
                                                         fileImage = getFileImage(fileInfo.url),
                                                         uploadTime =fileInfo.uploadTime, 
                                                         size = fileInfo.size,
                                                         tooltip =  string.Format("Name: {0}\nAuthor: {1}\nUpload Time:{2}\nSize {3:0.00}mb", fileInfo.name, fileInfo.author, fileInfo.uploadTime, fileInfo.size / 1000000.0) 
                                                     }));
        }

        private void saveFile(object sender, RoutedEventArgs e)
        {
            var file = (FileInfo)((Button) sender).DataContext;
            var saveFile = new SaveFileDialog();
            saveFile.Filter = string.Format("{0} (*{1})|*{1}|All Files (*.*)|*.*", file.fileType, System.IO.Path.GetExtension(file.url));
            saveFile.FilterIndex = 1;
            saveFile.RestoreDirectory = true;
            if(saveFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var stream = saveFile.OpenFile();
                var sourceBytes =
                    new WebClient {Credentials = new NetworkCredential("exampleUsername", "examplePassword")}.DownloadData(file.url);
                stream.Write(sourceBytes, 0, sourceBytes.Count());

            }
        }
        //utility methods
        public static string getFileType(string url)
        {
            var extension = System.IO.Path.GetExtension(url).ToLower();
            switch (extension)
            {
                case ".ppt":
                    return "PowerPoint";
                case ".pptx":
                    return "PowerPoint";
                case ".doc":
                    return "Word";
                case ".docx":
                    return "Word";
                case ".txt":
                    return "Text";
                case ".html":
                    return "HTML";
                case ".xls":
                    return "Excel";
                case ".xlsx":
                    return "Excel";
                case ".pdf":
                    return "PDF";
                case ".odt":
                    return "Open Office Document";
                case ".mpg":
                    return "Video";
                case ".mp4":
                    return "Video";
                case ".m4v":
                    return "Video";
                case ".mpz":
                    return "Video";
                case ".mpeg":
                    return "Video";
                case ".divx":
                    return "Video";
                case ".xvid":
                    return "Video";
                case ".avi":
                    return "Video";
                case ".mov":
                    return "QuickTime";
                case ".swf":
                    return "Shockwave";
                case ".wmv":
                    return "Windows Media Video";
                case ".xap":
                    return "Silverlight";
                case ".gif":
                    return "GIF";
                case ".png":
                    return "PNG";
                case ".bmp":
                    return "Bitmap";
                case ".jpeg":
                    return "Jpeg";
                case ".jpg":
                    return "Jpeg";
                case ".mp3":
                    return "Audio";
                case ".wav":
                    return "Audio";
                default :
                    return "Other";
            }
        }
        public static string getFileImage(string url)
        {
            return @"C:\specialMeTL\MeTLMeeting\SandRibbon\Resources\mimetype_readme.png";
        }
    }
}
