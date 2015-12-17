using System;
using System.Linq;
using System.IO;
using System.Net;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MeTLLib;
using SandRibbon.Utils;
using MeTLLib.DataTypes;
using System.Collections.Generic;
using System.Windows.Media;
using SandRibbon.Components;

namespace SandRibbon.Providers
{
    public class CachedThumbnail
    {
        public BitmapImage image;
        public long created;
        public CachedThumbnail(BitmapImage i)
        {
            image = i;
            created = DateTime.Now.Ticks;
        }
    }
    public class ThumbnailProvider
    {
        public NetworkController controller { get; protected set; }
        public ThumbnailProvider(NetworkController _controller)
        {
            controller = _controller;
        }
        public static ImageSource emptyImage = new ImageSourceConverter().ConvertFromString("Resources/Slide_Not_Loaded.png") as ImageSource;
        private Dictionary<int, CachedThumbnail> cache = new Dictionary<int, CachedThumbnail>();
        private object cacheLock = new object();
        //acceptableStaleTime is measured in ticks
        public long acceptableStaleTime = (10 * 1000 * 1000)/* seconds */ * 5;
        private int maximumCachedBitmaps = 200;
        private void addToCache(int slideId, CachedThumbnail ct)
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
        private void paintThumb(Image image)
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
                              image.Source = cache[internalSlide.id].image;
                          }
                      }
                  }
                  else
                      image.Source = emptyImage;
              }
              catch (Exception e) { 
                      image.Source = emptyImage;
              }
          });
        }
        public void thumbnail(Image image, int slideId)
        {
            if (image == null)
                return;
            var slide = (Slide)image.DataContext;
            var internalSlideId = slide.id;
            bool shouldPaintThumb = false;
            lock (cacheLock)
            {
                if (cache.ContainsKey(slideId) && cache[slideId].created > DateTime.Now.Ticks - acceptableStaleTime)
                {
                    shouldPaintThumb = true;
                }
            }
            if (shouldPaintThumb) {
                paintThumb(image);
            } else {
                var server = controller.config;
                var url = server.thumbnailUri(internalSlideId.ToString());
                WebThreadPool.QueueUserWorkItem(delegate
                {
                    try
                    {
                        App.auditor.wrapAction(g => { 
                            using (var client = new WebClient())
                            {
                                BitmapImage bitmap = null;
                                g(GaugeStatus.InProgress, 10);
                                using (var stream = new MemoryStream(client.DownloadData(url)))
                                {
                                    if (stream.Length == 0)
                                    {
                                        //App.Now(string.Format("Error loading thumbnail: zero length image"));
                                        g(GaugeStatus.Failed, 100);
                                    }
                                    else
                                    {
                                        g(GaugeStatus.InProgress, 20);
                                        bitmap = new BitmapImage();
                                        g(GaugeStatus.InProgress, 30);
                                        bitmap.BeginInit();
                                        try
                                        {
                                            g(GaugeStatus.InProgress, 40);
                                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                            g(GaugeStatus.InProgress, 50);
                                            bitmap.StreamSource = stream;
                                            g(GaugeStatus.InProgress, 60);
                                        }
                                        catch { }
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
                                paintThumb(image);
                            }
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