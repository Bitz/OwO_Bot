using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using Imgur.API.Endpoints;
using Imgur.API.Authentication;
using Imgur.API.Models;
using static OwO_Bot.Functions.Convert.ImageSize;
using static OwO_Bot.Functions.Html;
using static OwO_Bot.Models.Misc;

namespace OwO_Bot.Functions
{
    class Upload
    {
        public static void PostToImgur(ref PostRequest model)
        {
            var client = new ApiClient(Constants.Config.imgur.apikey);
            ImageEndpoint endpoint = new ImageEndpoint(client, Program.HttpClient);
            C.Write("Uploading to Imgur...");
            IImage image;
            //Imgur can only handle Images of 10MB. 
            int tenMb = 10400000;
            if (model.RequestSize > tenMb)
            {   //Download the image
                C.Write("Resizing image...");
                using (Bitmap imageFromUrl = new Bitmap(GetImageFromUrl(model.RequestUrl)))
                {
                    using (Bitmap resizedBitmap = GetSpecificSize(imageFromUrl, tenMb, true))
                    {
                        using (MemoryStream memoryStream = new MemoryStream(tenMb))
                        {
                            if (resizedBitmap == null)
                            {
                                imageFromUrl.Save(memoryStream, ImageFormat.Jpeg);
                            }
                            else
                            {
                                resizedBitmap.Save(memoryStream, ImageFormat.Png);
                            }
                            image = endpoint.UploadImageAsync(memoryStream, null, model.Title, model.Description).Result;
                        }
                    }
                }
                C.WriteLineNoTime("Done!");
            }
            else
            {
                image = endpoint.UploadImageAsync(model.RequestUrl, null, model.Title, model.Description).Result;
            }

            model.ResultUrl = image.Link;
            model.DeleteHash = image.DeleteHash;
            C.WriteLineNoTime("Done!");
        }

        public static void PostToImgurAsGif(ref PostRequest model)
        {
            C.Write("Uploading to Imgur...");
            var client = new ApiClient(Constants.Config.imgur.apikey);
            ImageEndpoint endpoint = new ImageEndpoint(client, Program.HttpClient);
            var image = endpoint.UploadImageAsync(model.RequestUrl, null, model.Title, model.Description).Result;
            model.ResultUrl = image.Link;
            model.DeleteHash = image.DeleteHash;
            C.WriteLineNoTime("Done!");
        }
    

    public static void PostToImgurAsVideo(ref PostRequest model)
        {
            C.Write("Uploading to Imgur...");
            var client = new ApiClient(Constants.Config.imgur.apikey);
            ImageEndpoint endpoint = new ImageEndpoint(client, Program.HttpClient);

            var bytes = GetStreamFromUrl(model.RequestUrl);
            var image = endpoint.UploadVideoAsync(bytes, null, model.Title, model.Description).Result;
            model.ResultUrl = image.Link;
            model.DeleteHash = image.DeleteHash;

            C.WriteLineNoTime("Done!");
        }
    }
}
