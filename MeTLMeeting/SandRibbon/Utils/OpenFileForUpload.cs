using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Windows;
using SandRibbon.Components.Utility;
using System.ComponentModel;
using MeTLLib.DataTypes;
using SandRibbon.Providers;
using System.Windows.Threading;

namespace SandRibbon.Utils
{
    public class OpenFileForUpload
    {
        private Window _owner;

        public OpenFileForUpload(Window owner)
        {
            _owner = owner;
        }

        public void AddResourceFromDisk()
        {
            AddResourceFromDisk("All files (*.*)|*.*");
        }

        public void AddResourceFromDisk(string filter)
        {
            AddResourceFromDisk(filter, (files) =>
                                    {
                                        foreach (var file in files)
                                        {
                                            UploadFileForUse(file);
                                        }
                                    });
        }
        
        private void AddResourceFromDisk(string filter, Action<IEnumerable<string>> withResources)
        {
            string initialDirectory = "c:\\";
            foreach (var path in new[] { Environment.SpecialFolder.MyPictures, Environment.SpecialFolder.MyDocuments, Environment.SpecialFolder.DesktopDirectory, Environment.SpecialFolder.MyComputer })
                try
                {
                    initialDirectory = Environment.GetFolderPath(path);
                    break;
                }
                catch (Exception)
                {
                }
            var fileBrowser = new OpenFileDialog
                                         {
                                             InitialDirectory = initialDirectory,
                                             Filter = filter,
                                             FilterIndex = 1,
                                             RestoreDirectory = true
                                         };
            var dialogResult = fileBrowser.ShowDialog(_owner);

            if (dialogResult ?? false)
                withResources(fileBrowser.FileNames);
        }

        private const int fileSizeLimit = 50;
        private void UploadFileForUse(string unMangledFilename)
        {
            string filename = unMangledFilename + ".MeTLFileUpload";
            if (filename.Length > 260)
            {
                MeTLMessage.Information("Sorry, your filename is too long, must be less than 260 characters");
                return;
            }
            if (FileHelper.LocalFileLessThanSizeInMegabytes(unMangledFilename, fileSizeLimit))
            {
                var worker = new BackgroundWorker();
                worker.DoWork += (s, e) =>
                 {
                     var target = "presentationSpace"; // looks like this can be "presentationSpace" or "notepad"
                     System.IO.File.Copy(unMangledFilename, filename);
                     MeTLLib.ClientFactory.Connection().UploadAndSendFile(
                         new MeTLStanzas.LocalFileInformation(Globals.slide, Globals.me, target, "public", filename, System.IO.Path.GetFileNameWithoutExtension(filename), false, new System.IO.FileInfo(filename).Length, SandRibbonObjects.DateTimeFactory.Now().Ticks.ToString(), Globals.generateId(filename)));
                     System.IO.File.Delete(filename);
                 };
                worker.RunWorkerCompleted += (s, a) => _owner.Dispatcher.Invoke(DispatcherPriority.Send,
                                                                                   (Action)(() => MeTLMessage.Information(string.Format("Finished uploading {0}.", unMangledFilename))));
                worker.RunWorkerAsync();
            }
            else
            {
                MeTLMessage.Information(String.Format("Sorry your file is too large, it must be less than {0}mb", fileSizeLimit));
                return;
            }
        }
    }
}
