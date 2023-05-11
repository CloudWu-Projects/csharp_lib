using System;
using System.Drawing;
using System.IO;
namespace csharp_lib.baseLib
{
    class ImgToBase64
    {
        static public string toBase64String(string ImageFileName)
        {
            try
            {
                Bitmap bmp = new Bitmap(ImageFileName);
                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);
                ms.Close();
                return Convert.ToBase64String(arr);
            }catch(Exception ex)
            {
                //Program.Logger.Error("toBase64String failed.:{0}",ImageFileName);
                //Program.Logger.Error("toBase64String exception: {0}", ex.ToString());
            }
            return null;
        }
        public static string  Scale_and_Base64String(string ImageFileName,int destWidth,int destHeight)
        {
            try
            {
                Image image = Image.FromFile(ImageFileName);

               Image scaledImage =  ImageZoom.ZoomPicture(image,destWidth,destHeight);
               
                MemoryStream ms = new MemoryStream();
                scaledImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);
                ms.Close();
                return Convert.ToBase64String(arr);
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        public static string Crop_and_Base64String(string ImageFileName, int cropLeft, int cropTop, int cropRight, int cropBottom)
        {
            try {
                Image cropedImage = Crop(ImageFileName,  cropLeft,  cropTop,  cropRight,  cropBottom);

                MemoryStream ms = new MemoryStream();
                cropedImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);
                ms.Close();
                return Convert.ToBase64String(arr);
            }
            catch (Exception ex)
            {
            }
            return null;
        }
        public static Image Crop(string orgPicPath,int cropLeft,int cropTop,int cropRight,int cropBottom)
        {

            //从文件加载原图
            Image originImage = Image.FromFile(orgPicPath);
            int left, top, width, height;

            left = originImage.Width * cropLeft / 100;
            top = originImage.Height * cropTop / 100;

            width = originImage.Width * (100 - cropLeft - cropRight) / 100;
            height = originImage.Height * (100 - cropTop - cropBottom) / 100;

            Rectangle cropRegion = new Rectangle(left, top, width, height);
            return PictrueCrop.CropImage(originImage, cropRegion);


        }
    }
}
