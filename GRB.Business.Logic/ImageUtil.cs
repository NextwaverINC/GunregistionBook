using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace GRB.Business.Logic
{
    public class ImageUtil
    {
        public static Boolean SaveImage(String ImagePath, Image ImageData)
        {
            var TempPath = ImagePath.Split('/').ToList<String>();

            var PathLen = 0;
            var TempDir = "";
            var ListCreateDir = new List<String>();

            for (Int32 Index = TempPath.Count - 1; Index >= 1; Index--)
            {
                PathLen += TempPath[Index].Length + 1;
                TempDir = ImagePath.Substring(0, ImagePath.Length - PathLen);
                if (System.IO.Directory.Exists(TempDir))
                    break;
                else
                    ListCreateDir.Add(TempDir);
            }

            foreach (String CreateDirItem in ListCreateDir)
            {
                System.IO.Directory.CreateDirectory(CreateDirItem);
            }

            if (System.IO.File.Exists(ImagePath))
            {
                System.IO.File.Delete(ImagePath);
            }

            ImageData.Save(ImagePath,ImageFormat.Jpeg);

            return true;
        }
        public static Boolean SaveImageWaterMark(String ImagePath, Image ImageData)
        {
            var TempPath = ImagePath.Split('/').ToList<String>();

            var PathLen = 0;
            var TempDir = "";
            var ListCreateDir = new List<String>();

            for (Int32 Index = TempPath.Count - 1; Index >= 1; Index--)
            {
                PathLen += TempPath[Index].Length + 1;
                TempDir = ImagePath.Substring(0, ImagePath.Length - PathLen);
                if (System.IO.Directory.Exists(TempDir))
                    break;
                else
                    ListCreateDir.Add(TempDir);
            }

            foreach (String CreateDirItem in ListCreateDir)
            {
                System.IO.Directory.CreateDirectory(CreateDirItem);
            }

            if (System.IO.File.Exists(ImagePath))
            {
                System.IO.File.Delete(ImagePath);
            }

            ImageData.Save(ImagePath, ImageFormat.Png);

            return true;
        }
    }
}