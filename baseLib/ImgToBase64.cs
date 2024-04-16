using System;
using System.Drawing;
using System.Drawing.Imaging;
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
        public static string Compress_and_Base64String(string ImageFileName, int quality ,MyLogger myLogger = null)
        {
            try
            {
                Bitmap bmp = CompressImage(ImageFileName, quality);
                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);
                ms.Close();
                return Convert.ToBase64String(arr);

            }
            catch (Exception ex) { 
                myLogger?.Error($"CompressImage failed.:{ImageFileName}");
                myLogger?.Error($"CompressImage exception: {ex.ToString()}");
                myLogger?.Error($"CompressImage exception: {ex.StackTrace}");
                myLogger?.Error($"CompressImage exception: {ex.Message}");

            }
            return null;
        }
        // 图片压缩方法
        static Bitmap CompressImage(string sourceImagePath, int quality)
        {
            // 加载原始图片
            using (Bitmap sourceImage = new Bitmap(sourceImagePath))
            {
                // 创建保存参数
                EncoderParameters encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, quality); // 设置压缩质量，范围从0到100

                // 获取 JPEG 编码器
                ImageCodecInfo jpegEncoder = GetEncoder(ImageFormat.Jpeg);

                // 创建一个新的 Bitmap 对象，作为压缩后的图片
                Bitmap compressedImage = new Bitmap(sourceImage.Width, sourceImage.Height);

                // 使用 Graphics 对象绘制压缩后的图片
                using (Graphics graphics = Graphics.FromImage(compressedImage))
                {
                    graphics.DrawImage(sourceImage, 0, 0, sourceImage.Width, sourceImage.Height);
                }

                // 将压缩参数应用于压缩后的图片
                MemoryStream memoryStream = new MemoryStream();
                compressedImage.Save(memoryStream, jpegEncoder, encoderParameters);

                // 从内存流中创建最终的压缩图片对象
                Bitmap finalCompressedImage = new Bitmap(memoryStream);

                // 释放内存流资源
                memoryStream.Dispose();

                return finalCompressedImage;
            }
        }

        // 获取指定图像格式的编码器
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
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
