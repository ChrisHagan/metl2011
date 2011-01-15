using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MeTLLib.Providers.Connection;
using MeTLLib.DataTypes;
using System.Windows.Data;
using MeTLLib;
using Microsoft.Practices.Composite.Presentation.Commands;
using SandRibbon.Utils;
using Size = System.Windows.Size;
using System.ComponentModel;

namespace SandRibbon.Providers
{
    public class ThumbnailProvider
    {
        public class Thumbnail
        {
            public int id;
            public ImageSource thumb { get; set; }

        }
        public static void thumbnail(Image image, int slideId)
        {
            var worker = new BackgroundWorker();
            BitmapImage bitmap = null;//Will build on other thread, freeze and hand back.
            worker.DoWork += delegate
            {
                var host = ClientFactory.Connection().server.host.Split('.').First();
                using (var client = new WebClient())
                {
                    var url = string.Format("http://radar.adm.monash.edu:9000/application/snapshot?server={0}&slide={1}&width={2}&height={3}", host, slideId, 320, 240);
                    var stream = new MemoryStream(client.DownloadData(url));
                    bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    image.Dispatcher.adopt(delegate{image.Source = bitmap;});
                    App.Now("Froze and returned thumbnail {0}", slideId);
                }
            };
            worker.RunWorkerAsync();
        }
    }
}