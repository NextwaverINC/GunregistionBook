using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Diagnostics;

namespace GRB.Console.GenImgToPdf
{
    class GenImgToPdf
    {
        public void GenPdf(string[] args)
        {
            string strHostName = System.Net.Dns.GetHostName();
            string pathImgGun = System.Configuration.ConfigurationSettings.AppSettings["pathImgGun"];
            string pathSavePdf = System.Configuration.ConfigurationSettings.AppSettings["pathSavePdf"];
            // Create a new PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = args[1];

            string dirPath = "\\\\" + strHostName + pathImgGun + args[2];

            DirectoryInfo dir = new DirectoryInfo(dirPath);
            string[] fileList = System.IO.Directory.GetFiles(dirPath, "*.jpg", SearchOption.TopDirectoryOnly);

            foreach (var item in fileList)
            {
                PdfPage page = document.AddPage();
                page.Size = PageSize.A4;
                // Get an XGraphics object for drawing
                XGraphics gfx = XGraphics.FromPdfPage(page);

                var ms = new MemoryStream();

                using (var imageA = Image.FromFile(item))
                using (var newImage = ScaleImage(imageA, 595, 842, 72))
                {
                    newImage.Save(ms, ImageFormat.Jpeg);
                }

                XImage image = XImage.FromStream(ms);

                gfx.DrawImage(image, 0, (842 - image.PixelHeight) / 2);

                System.Console.WriteLine("Add image --> " + item);
            }

            string pathEbook = "\\\\" + strHostName + pathSavePdf + args[2];
            pathEbook = pathEbook.Substring(0, pathEbook.Length - 5);
            if (!System.IO.Directory.Exists(pathEbook))
                System.IO.Directory.CreateDirectory(pathEbook);
            // Save the document...
            string filename = "\\\\" + strHostName + pathSavePdf + args[2] + ".pdf";
            System.Console.WriteLine("Save to --> " + filename);
            document.Save(filename);
            // ...and start a viewer.
            //Process.Start(filename);
            System.Console.WriteLine("Success");
        }

        public static Image ScaleImage(Image image, int maxWidth, int maxHeight, int imgResolution)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            newImage.SetResolution(imgResolution, imgResolution);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);


            return newImage;
        }
    }
}
