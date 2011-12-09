using System.IO;
using System;

namespace SandRibbon.Utils
{
    public class FileHelper
    {
        private const int KILOBYTE = 1024;
        private const int MEGABYTE = 1024 * KILOBYTE;

        public static long BytesToMegabytes(long size)
        {
            if (size < 0) throw new ArgumentOutOfRangeException("size must be positive number");

            return size / MEGABYTE; 
        }
        /// <summary>
        /// Returns true if the filename specified has a size less than sizeInMegabytes
        /// </summary>
        /// <remarks>
        /// Local is determined by whether the filename starts with http
        /// If the filename is not local, then return true, ie don't care what is the size of the file 
        /// </remarks>
        /// <param name="filename">Locally accessibly filename</param>
        /// <param name="sizeInMegabytes">Maximum (exclusive) size of the file</param>
        /// <returns>True if file is less than sizeInMegabytes, false otherwise</returns>
        public static bool LocalFileLessThanSizeInMegabytes(string filename, int sizeInMegabytes)
        {
            if (filename.StartsWith("http")) return true;

            var info = new FileInfo(filename);
            return info.Length < (sizeInMegabytes * MEGABYTE); // convert sizeInMegabytes to bytes
        }

        public static string DetermineFileTypeFromExtension(string url)
        {
            var extension = Path.GetExtension(url).ToLower();
            switch (extension)
            {
                case ".ppt":
                    return "PowerPoint";
                case ".pptx":
                    return "PowerPoint";
                case ".doc":
                    return "Word";
                case ".docx":
                    return "Word";
                case ".txt":
                    return "Text";
                case ".html":
                    return "HTML";
                case ".xls":
                    return "Excel";
                case ".xlsx":
                    return "Excel";
                case ".pdf":
                    return "PDF";
                case ".odt":
                    return "Open Office Document";
                case ".mpg":
                    return "Video";
                case ".mp4":
                    return "Video";
                case ".m4v":
                    return "Video";
                case ".mpz":
                    return "Video";
                case ".mpeg":
                    return "Video";
                case ".divx":
                    return "Video";
                case ".xvid":
                    return "Video";
                case ".avi":
                    return "Video";
                case ".mov":
                    return "QuickTime";
                case ".swf":
                    return "Shockwave";
                case ".wmv":
                    return "Windows Media Video";
                case ".xap":
                    return "Silverlight";
                case ".gif":
                    return "GIF";
                case ".png":
                    return "PNG";
                case ".bmp":
                    return "Bitmap";
                case ".jpeg":
                    return "Jpeg";
                case ".jpg":
                    return "Jpeg";
                case ".mp3":
                    return "Audio";
                case ".wav":
                    return "Audio";
                default :
                    return "Other";
            }
        }

        public static string GetFileTypeImageResource(string url)
        {
            switch (DetermineFileTypeFromExtension(url))
            {
                case "HTML":
                    return "\\resources\\mimeTypes\\web.png";
                case "Jpeg":
                    return "\\resources\\mimeTypes\\image.png";
                case "Audio":
                    return "\\resources\\mimeTypes\\audio.png";
                case "Other":
                    return "\\resources\\mimeTypes\\unknown.png";
                case "Bitmap":
                    return "\\resources\\mimeTypes\\image.png";
                case "PDF":
                    return "\\resources\\mimeTypes\\publication.png";
                case "Text":
                    return "\\resources\\mimeTypes\\text.png";
                case "Word":
                    return "\\resources\\mimeTypes\\document.png";
                case "PowerPoint":
                    return "\\resources\\mimeTypes\\publication.png";
                case "Excel":
                    return "\\resources\\mimeTypes\\spreadsheet.png";
                case "PNG":
                    return "\\resources\\mimeTypes\\image.png";
                case "JPG":
                    return "\\resources\\mimeTypes\\image.png";
                case "GIF":
                    return "\\resources\\mimeTypes\\image.png";
                case "Windows Media Video":
                    return "\\resources\\mimeTypes\\video.png";
                case "Open Office Document":
                    return "\\resources\\mimeTypes\\document.png";
                case "Silverlight":
                    return "\\resources\\mimeTypes\\gadget.png";
                case "Shockwave":
                    return "\\resources\\mimeTypes\\gadget.png";
                case "Quicktime":
                    return "\\resources\\mimeTypes\\video.png";
                case "Video":
                    return "\\resources\\mimeTypes\\video.png";
                default:
                    return "\\resources\\mimeTypes\\unknown.png";
            }
        }
    }
}
