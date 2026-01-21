using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
namespace csharp_lib.baseLib
{
    public class ImgToBase64
    {
        static public string toBase64String(string ImageFileName, MyLogger logger)
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
            }
            catch (Exception ex)
            {
                logger?.Error($"toBase64String exception: {ImageFileName}");
                logger?.Error($"toBase64String exception: {ex.ToString()}");
                throw ex;
            }
        }

        static public Image CompressImageWithQuality(string originalJpegPath, int quality)
        {
            using (Image oi = Image.FromFile(originalJpegPath))
            {
                return ImgToBase64.CompressImageWithQuality(oi, quality, ImageFormat.Jpeg);
            }
        }
        static public Image CompressImageWithQuality(Image image, long quality, ImageFormat format)
        {
            // 获取编码器
            ImageCodecInfo encoder = GetEncoder(format);
            if (encoder == null)
                throw new NotSupportedException($"No encoder found for format: {format}");

            // 设置编码参数
            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);

            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, encoder, encoderParams);
                return Image.FromStream(new MemoryStream(ms.ToArray()));
            }
        }
        public static string ConvertImageToBase64WithQuality(string imagePath, long quality, MyLogger logger)
        {
            try
            {
                Image image = Image.FromFile(imagePath);
                using (MemoryStream ms = new MemoryStream())
                {
                    // Create encoder parameters for setting the quality
                    EncoderParameters encoderParameters = new EncoderParameters(1);
                    encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

                    // Get the JPEG codec info
                    ImageCodecInfo jpegCodec = GetEncoder(ImageFormat.Jpeg);

                    // Save the image to the memory stream with specified quality
                    image.Save(ms, jpegCodec, encoderParameters);

                    // Convert the image bytes to a base64 string
                    byte[] imageBytes = ms.ToArray();
                    string base64String = Convert.ToBase64String(imageBytes);

                    return base64String;
                }
            }
            catch (Exception ex)
            {
                logger?.Error($"ConvertImageToBase64WithQuality failed.:{imagePath}");
                logger?.Error($"ConvertImageToBase64WithQuality exception: {ex.ToString()}");
            }
            return null;
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
        public static string Scale_and_Base64String(string ImageFileName, int destWidth, int destHeight)
        {
            try
            {
                Image image = Image.FromFile(ImageFileName);

                Image scaledImage = ImageZoom.ZoomPicture(image, destWidth, destHeight);

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
        public static byte[] CompressImage(string sourcePath, int maxBit = 300 * 1024)
        {
            if (sourcePath == null) throw new ArgumentNullException(nameof(sourcePath));
            if (!File.Exists(sourcePath)) throw new FileNotFoundException("Source image not found", sourcePath);
            // Read original bytes as the initial input for iterative compression
            byte[] currentBytes = File.ReadAllBytes(sourcePath);
            byte[] lastResult = currentBytes;

            // Iteratively compress the last-compressed bytes
            List<int> qualityList = new List<int> { 90,80,70,60,50,40,30,20,10,9,8,7,6,5,4,3,2,1};
            foreach (var quality in qualityList)
            {
                using (var inStream = new MemoryStream(currentBytes))
                using (var img = Image.FromStream(inStream))
                {
                    ImageCodecInfo encoder = GetEncoder(ImageFormat.Jpeg);
                    if (encoder == null)
                        throw new NotSupportedException("No encoder found for JPEG format.");

                    EncoderParameters encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)quality);

                    using (var outStream = new MemoryStream())
                    {
                        img.Save(outStream, encoder, encoderParams);
                        byte[] newBytes = outStream.ToArray();
                        lastResult = newBytes;
                        if (newBytes.Length <= maxBit)
                        {
                            return newBytes;
                        }
                        // continue compressing the last result
                        currentBytes = newBytes;
                        //File.WriteAllBytes(@$"{sourcePath}_{quality}.jpg", newBytes);
                    }
                }
            }

            // Return best-effort result (never null)
            return lastResult ?? File.ReadAllBytes(sourcePath);
        }
        public static string Crop_and_Base64String(string ImageFileName, int cropLeft, int cropTop, int cropRight, int cropBottom)
        {
            try
            {
                Image cropedImage = Crop(ImageFileName, cropLeft, cropTop, cropRight, cropBottom);

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
        public static Image Crop(string orgPicPath, int cropLeft, int cropTop, int cropRight, int cropBottom)
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
