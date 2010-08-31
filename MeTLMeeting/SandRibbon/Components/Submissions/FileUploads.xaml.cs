using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using System.Windows.Shapes;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbonInterop.MeTLStanzas;

namespace SandRibbon.Components.Submissions
{
    
    public class FileInfo
    {
        public string filename { get; set;}
        public string url { get; set;}
        public string author { get; set; }
        public string fileType { get; set;}
        public string fileImage { get; set;}
    }
    public partial class FileUploads : Window
    {
        private ObservableCollection<FileInfo> fileList; 
        public FileUploads()
        {
            InitializeComponent();
            fileList = new ObservableCollection<FileInfo>();
            Commands.ReceiveFileResource.RegisterCommand(new DelegateCommand<TargettedFile>(receiveFile));
            files.ItemsSource = fileList;
        }
        public FileUploads(IEnumerable<FileInfo> files): this()
        {
            foreach (var file in files)
                fileList.Add(file);
        }
        private void receiveFile(TargettedFile file)
        {
            Dispatcher.adoptAsync(() => fileList.Add(new FileInfo
                                                         {
                                                             filename = getFileName(file.url),
                                                             fileType = getFileType(file.url),
                                                             url = file.url,
                                                             author = file.author,
                                                             fileImage = getFileImage(file.url)
                                                         }));
        }

        public static string getFileType(string url)
        {
            var extension = System.IO.Path.GetExtension(url).ToLower();
            switch (extension)
            {
                case ".ppt":
                    return "PowerPoint";
                case ".doc":
                    return "Word";
                case ".xls":
                    return "Excel";
                default :
                    return "Other";
            }
        }
        public static string getFileName(string url)
        {
            return System.IO.Path.GetFileName(url);
        }
        public static string getFileImage(string url)
        {
            return @"C:\specialMeTL\MeTLMeeting\SandRibbon\Resources\mimetype_readme.png";
        }
        private void saveFile(object sender, RoutedEventArgs e)
        {
            var saveFile = new SaveFileDialog();
            var file = ((FileInfo) files.SelectedItem);
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
            this.Close();
        }
    }
}
