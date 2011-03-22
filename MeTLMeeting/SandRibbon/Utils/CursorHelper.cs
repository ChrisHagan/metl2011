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
    public static Cursor CreateCursor(int width, int height, System.Drawing.Color color, int xHotSpot, int yHotSpot){
        if (width < 1) width = 1;
        if (height < 1) height = 1;
        var bmp = new System.Drawing.Bitmap(width, height);
        var graphics = System.Drawing.Graphics.FromImage(bmp);
        graphics.FillEllipse(new System.Drawing.SolidBrush(color), 0, 0, width, height);
        return InternalCreateCursor(bmp, xHotSpot, yHotSpot);
    }
  }
}
