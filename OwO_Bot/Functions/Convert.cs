using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace OwO_Bot.Functions
{
    class Convert
    {
        public static byte[] StreamToByte(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static T NodeToClass<T>(XmlNode node) where T : class
        {
            MemoryStream stm = new MemoryStream();

            StreamWriter stw = new StreamWriter(stm);
            stw.Write(node.OuterXml);
            stw.Flush();

            stm.Position = 0;

            XmlSerializer ser = new XmlSerializer(typeof(T));
            T result = ser.Deserialize(stm) as T;

            return result;
        }

        public static List<T> ReaderToList<T>(IDataReader dr, bool keepOpen = false) where T : new()
        {
            Type businessEntityType = typeof(T);
            List<T> ent = new List<T>();
            Hashtable hashtable = new Hashtable();
            PropertyInfo[] properties = businessEntityType.GetProperties();
            foreach (PropertyInfo info in properties)
            {
                hashtable[info.Name.ToUpper()] = info;
            }
            while (dr.Read())
            {
                T newObject = new T();
                for (int index = 0; index < dr.FieldCount; index++)
                {
                    PropertyInfo info = (PropertyInfo)
                        hashtable[dr.GetName(index).ToUpper()];
                    if (info != null && info.CanWrite)
                    {
                        if (dr[index] != DBNull.Value)
                            //MySQL Cannot properly cast bit to bool, so we have to tell it to get the bit as a boolean.
                            if (info.PropertyType == typeof(bool))
                            {
                                info.SetValue(newObject, (bool) dr[index], null);
                            }
                            else
                            {
                                info.SetValue(newObject, dr[index], null);
                            }
                    }
                }
                ent.Add(newObject);
            }
            if (!keepOpen)
            {
                dr.Close();
            }
            return ent;
        }


        public static string BytesToReadableString(long value, int decimalPlaces = 1)
        {
            string[] sizeSuffixes =
                {"bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"};
            if (decimalPlaces < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(decimalPlaces));
            }
            if (value < 0)
            {
                return "-" + BytesToReadableString(-value);
            }
            if (value == 0)
            {
                return string.Format("{0:n" + decimalPlaces + "} bytes", 0);
            }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int) Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal) value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                sizeSuffixes[mag]);
        }


        #region Logic for resizing images

        public static class ImageSize
        {
            public static Image GetSpecificSize(Image image, long requestedSize, bool tryJpegConvert = false)
            {
                //We will try to convert the image to a jpeg before manually resizing it, if that is enough to get it 
                //Below the requestedSize, we return null, to process the conversion as another path.
                if (tryJpegConvert)
                {
                    if (GetImageSizeAsJpeg(image).Length < requestedSize)
                    {
                        return null;
                    }
                }
                long resizedImageSize = GetImageSize(image);
                Image resizedImage = image;
                float steps = 10;
                double widthReduction = image.Width * (steps / 100);
                double heightReduction =  image.Height * (steps / 100);
                while (resizedImageSize > requestedSize)
                {
                    int width = resizedImage.Width - (int) widthReduction;
                    int height = resizedImage.Height - (int) heightReduction;
                    resizedImage = ResizeImage(resizedImage, width, height);
                    resizedImageSize = GetImageSize(resizedImage);
                }

                return resizedImage;
            }

            private static Image ResizeImage(Image image, int width, int height)
            {
                var destImage = new Bitmap(width, height);
                using (var graphics = Graphics.FromImage(destImage))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.DrawImage(image, new Rectangle(0, 0, width, height));
                }
                return destImage;
            }

            private static long GetImageSize(Image image)
            {
                long maxByteSize;
                using (var ms = new MemoryStream())
                {
                    image.Save(ms, ImageFormat.Png);
                    maxByteSize = ms.Length;
                }
                return maxByteSize;
            }

            private static byte[] GetImageSizeAsJpeg(Image image)
            {
                MemoryStream ms;
                using (ms = new MemoryStream())
                {
                    image.Save(ms, ImageFormat.Jpeg);
                }
                return ms.ToArray();
            }
        }

        #endregion
    }
}