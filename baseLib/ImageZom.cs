using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharp_lib.baseLib
{
    internal class ImageZoom
    {
        public static Image ZoomPicture(Image sourceImage, int targetWidth,int targetHeight)
        {

            ImageFormat format = sourceImage.RawFormat;
            Bitmap saveImg = new Bitmap(targetWidth, targetHeight);

            Graphics g = Graphics.FromImage(saveImg);
            g.Clear(Color.White);

            g.DrawImage(sourceImage, 0, 0, targetWidth, targetHeight);
            sourceImage.Dispose();
            return saveImg;

        }
    }
}