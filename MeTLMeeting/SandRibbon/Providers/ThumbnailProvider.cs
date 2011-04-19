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
        public static void thumbnail(Image image, int slideId)
        {
            var internalImage = image;
            var internalSlideId = slideId;
            WebThreadPool.QueueUserWorkItem(delegate
            {
                var host = ClientFactory.Connection().server.host.Split('.').First();
                try
                {
                    using (var client = new WebClient())
                    {
                        BitmapImage bitmap = null;
                        var url = string.Format("http://metl.web.monash.edu:9000/application/snapshot?server={0}&slide={1}&width={2}&height={3}", host, internalSlideId, 320, 240);
                        using (var stream = new MemoryStream(client.DownloadData(url)))
                        {
                            bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = stream;
                            bitmap.EndInit();
                            bitmap.Freeze();
                            internalImage.Dispatcher.adopt(delegate { 
                                internalImage.Source = bitmap; 
                            });
                            stream.Close();
                        }
                    }
                }
                catch (Exception e)
                {
                    App.Now(string.Format("Error loading thumbnail: {0}", e.Message)); 
                }
               
            });
        }
    }
}