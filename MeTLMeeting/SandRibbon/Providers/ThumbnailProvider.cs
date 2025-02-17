﻿using System;
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
        public static ImageSource emptyImage = new ImageSourceConverter().ConvertFromString("Resources/Slide_Not_Loaded.png") as ImageSource;
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
              catch (Exception e) { 
                      image.Source = emptyImage;
              }
          });
        }
        public static void thumbnail(Image image, int slideId)
        {
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
                var server = ClientFactory.Connection().server;
                var host = server.Name;
                var url = string.Format(server.thumbnail + "{0}/{1}",host,internalSlideId);
                //Console.WriteLine(String.Format("Thumbnailing: {0} {1}", internalSlideId, url));
                WebThreadPool.QueueUserWorkItem(delegate
                {
                    try
                    {
                        using (var client = new WebClient())
                        {
                            BitmapImage bitmap = null;
                            using (var stream = new MemoryStream(client.DownloadData(url)))
                            {
                                bitmap = new BitmapImage();
                                bitmap.BeginInit();
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.StreamSource = stream;
                                bitmap.EndInit();
                                bitmap.Freeze();
                                stream.Close();
                                addToCache(slideId, new CachedThumbnail(bitmap));

                            }
                            paintThumb(image);
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
}