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

namespace SandRibbon.Providers
{
    public class ThumbnailProvider
    {
        public class Thumbnail
        {
            public int id;
            public ImageBrush thumb { get; set; }

        }
        public static ObservableCollection<Thumbnail> getConversationThumbs(string jid)
        {
            var thumbnails = new ObservableCollection<Thumbnail>();
            var details = ClientFactory.Connection().DetailsOf(jid);
            foreach (var slide in details.Slides)
            {
                thumbnails.Add(new Thumbnail
                                    {   
                                        id = slide.id,
                                        thumb =  getThumbnail(slide.id)
                                    });
            }
            return thumbnails;
        }
        public static ImageBrush updateThumb(int slideId)
        {
           return getThumbnail(slideId);
        }

        private static ImageBrush getThumbnail(int slideId)
        {
            var host = ClientFactory.Connection().server.host.Split('.').First();
            var url = string.Format("http://radar.adm.monash.edu:9000/application/snapshot?server={0}&slide={1}", host, slideId);
            var bitmap = new BitmapImage();
            try
            {
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(url);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.EndInit();
                bitmap.Freeze();
            }
            catch (Exception)
            {
                App.Now("Error in loading a thumbnail. boourns");
            }
            return new ImageBrush(bitmap);
        }

      
        
        
    }
}