using System;
using System.Security.Permissions;
using System.Windows.Interop;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows;

namespace SandRibbon.Utils
{
  public class CursorHelper
  {
    private static class NativeMethods
    {
      public struct IconInfo
      {
        public bool fIcon;
        public int xHotspot;
        public int yHotspot;
        public IntPtr hbmMask;
        public IntPtr hbmColor;
      }

      [DllImport("user32.dll")]
      public static extern SafeIconHandle CreateIconIndirect(ref IconInfo icon);

      [DllImport("user32.dll")]
      public static extern bool DestroyIcon(IntPtr hIcon);

      [DllImport("user32.dll")]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    private class SafeIconHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
      public SafeIconHandle()
        : base(true)
      {
      }

      override protected bool ReleaseHandle()
      {
        return NativeMethods.DestroyIcon(handle);
      }
    }
    
    private static Cursor InternalCreateCursor(System.Drawing.Bitmap bmp, int xHotSpot, int yHotSpot)
    {
      var iconInfo = new NativeMethods.IconInfo();
      NativeMethods.GetIconInfo(bmp.GetHicon(), ref iconInfo);

      iconInfo.xHotspot = xHotSpot;
      iconInfo.yHotspot = yHotSpot;
      iconInfo.fIcon = false;

      SafeIconHandle cursorHandle = NativeMethods.CreateIconIndirect(ref iconInfo);
      return CursorInteropHelper.Create(cursorHandle);
    }
    public static Cursor CreateCursor(UIElement element, int xHotSpot, 
        int yHotSpot)
    {
      int MIN_DIM = 5;
      var width = Math.Max(MIN_DIM, (int)((FrameworkElement)element).Width);
      var height = Math.Max(MIN_DIM,(int)((FrameworkElement)element).Height);
      element.Measure(new Size(width, height));
      element.Arrange(new Rect(0, 0, width, height));

      RenderTargetBitmap rtb = 
      new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
      rtb.Render(element);

      PngBitmapEncoder encoder = new PngBitmapEncoder();
      encoder.Frames.Add(BitmapFrame.Create(rtb));

      MemoryStream ms = new MemoryStream();
      encoder.Save(ms);

      System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(ms);

      ms.Close();
      ms.Dispose();
      Cursor cur = null;
      var attempts = 0;
      while (cur == null && attempts < 10)
      {
          try
          {
              cur = InternalCreateCursor(bmp, xHotSpot, yHotSpot);
          }
          catch (Exception)
          {

          }

      }
      bmp.Dispose();

      return cur;
    }
  }
}
