using System;
using System.Linq;
using System.IO;
using System.Net;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MeTLLib;
using SandRibbon.Utils;
using Microsoft.Practices.Composite.Presentation.Commands;
using MeTLLib.DataTypes;
using System.Collections.Generic;
using System.Windows.Media;

namespace SandRibbon.Providers
{
    public class CachedThumbnail
    {
        public ImageSource image;
        public long created;
        public CachedThumbnail(ImageSource i)
        {
            image = i;
            created = DateTime.Now.Ticks;
        }
        public CachedThumbnail(ImageSource i,long creationTime)
        {
            image = i;
            created = creationTime;
        }
    }
    public class ThumbnailProvider
    {
        public static ImageSource emptyImage = new ImageSourceConverter().ConvertFromString("Resources/Slide_Not_Loaded.png") as ImageSource;
        public static CachedThumbnail emptyCachedThumbnail = new CachedThumbnail(emptyImage, 0);
        private static Dictionary<int, CachedThumbnail> cache = new Dictionary<int, CachedThumbnail>();
        private static object cacheLock = new object();
        //acceptableStaleTime is measured in ticks
        public static long acceptableStaleTime = (10 * 1000 * 1000)/* seconds */ * 5;
        private static int maximumCachedBitmaps = 200;
        private static void addToCache(int slideId, CachedThumbnail ct)
        {
            lock (cacheLock)
            {
                if (cache.Keys.Count >= maximumCachedBitmaps)
                {
                    var toRemove = cache.OrderBy(kvp => kvp.Value.created).First();
                    //Console.WriteLine(String.Format("removing item from cache: {0} ({1})",toRemove.Key,toRemove.Value.created));
                    cache.Remove(toRemove.Key);
                }
                //Console.WriteLine(String.Format("adding item to cache: {0} ({1})", slideId, ct.created));
                cache[slideId] = ct;
            }
        }
        private static void paintThumb(Image image)
        {
            image.Dispatcher.adopt(delegate
            {
                try
                {
                    var internalSlide = (Slide)image.DataContext;
                    if (internalSlide != null)
                    {
                        lock (cacheLock)
                        {
                            if (cache.ContainsKey(internalSlide.id))
                            {
                                //Console.WriteLine(String.Format("painting thumbnail: {0}", internalSlide.id));
                                image.Source = cache[internalSlide.id].image;
                            }
                        }
                    }
                    else
                        image.Source = emptyImage;
                }
                catch (Exception e)
                {
                    image.Source = emptyImage;
                }
            });
        }
        public static void thumbnail(Image image, int slideId)
        {
            if (image == null)
                return;
            bool shouldPaintThumb = false;
            lock (cacheLock)
            {
                if (cache.ContainsKey(slideId) && cache[slideId].created > DateTime.Now.Ticks - acceptableStaleTime)
                {
                    shouldPaintThumb = true;
                }
            }
            if (shouldPaintThumb)
            {
                paintThumb(image);
            }
            else
            {
                var server = App.controller.config;
                var host = server.name;
                var url = server.thumbnailUri(slideId.ToString());
                WebThreadPool.QueueUserWorkItem(delegate
                {
                    try
                    {
                        App.auditor.wrapAction(g =>
                        {
                            var client = App.controller.client.resourceProvider;
                            BitmapImage bitmap = null;
                            g(GaugeStatus.InProgress, 10);
                            var bytes = client.secureGetData(url);
                            if (bytes.Length > 0)
                            {
                                using (var stream = new MemoryStream(bytes))
                                {
                                    g(GaugeStatus.InProgress, 20);
                                    bitmap = new BitmapImage();
                                    g(GaugeStatus.InProgress, 30);
                                    try
                                    {
                                        bitmap.BeginInit();
                                        g(GaugeStatus.InProgress, 40);
                                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                        g(GaugeStatus.InProgress, 50);
                                        bitmap.StreamSource = stream;
                                        g(GaugeStatus.InProgress, 60);
                                    }
                                    finally
                                    {
                                        bitmap.EndInit();
                                    }
                                    g(GaugeStatus.InProgress, 70);
                                    bitmap.Freeze();
                                    g(GaugeStatus.InProgress, 80);
                                    stream.Close();
                                    g(GaugeStatus.InProgress, 85);
                                    addToCache(slideId, new CachedThumbnail(bitmap));
                                    g(GaugeStatus.InProgress, 90);
                                }
                            }
                            else
                            {
                                addToCache(slideId, emptyCachedThumbnail);
                                g(GaugeStatus.InProgress, 90);
                            }

                            paintThumb(image);
                        }, "paintThumb", "frontend");
                    }
                    catch (Exception e)
                    {
                        App.Now(string.Format("Error loading thumbnail: {0}", e.Message));
                    }
                });
            }
        }
    }
}