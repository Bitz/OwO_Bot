using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using Imgur.API.Endpoints;
using Imgur.API.Authentication;
using Imgur.API.Models;
using Newtonsoft.Json;
using OwO_Bot.Models;
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
            var endpoint = new ImageEndpoint(client, Program.HttpClient);
            C.Write("Uploading to Imgur...");
            //Imgur can only handle Images of 10MB. 
            var tenMb = 10400000; 
            try
            {
                IImage image;
                if (model.RequestSize > tenMb)
                {   //Download the image
                    C.Write("Resizing image...");
                    using (var imageFromUrl = GetImageFromUrl(model.RequestUrl))
                    {
                        using (var imageResized = GetSpecificSize(imageFromUrl, tenMb, true))
                        {
                            Stream result = 
                                imageResized != null ? 
                                    EncodeProperly(imageResized, ImageFormat.Png) :
                                    EncodeProperly(imageFromUrl, ImageFormat.Jpeg);

                            image = endpoint.UploadImageAsync(result, null, model.Title, model.Description)
                                .Result;
                        }
                    }
                }
                else
                {
                    image = endpoint.UploadImageAsync(model.RequestUrl, null, model.Title, model.Description).Result;
                }

                model.ResultUrl = image.Link;
                model.DeleteHash = image.DeleteHash;
            }
            catch (Exception)
            {
                //Fallback!
                model.ResultUrl = model.RequestUrl;
            }
            C.WriteLineNoTime("Done!");
        }

        private static MemoryStream EncodeProperly(System.Drawing.Image image, ImageFormat format)
        {
            var memoryStream = new MemoryStream();
            if (image.PixelFormat != PixelFormat.Format24bppRgb)
            {
                //Some overzealous artists want to have fancy bitformats but imgur does not support them all. (This is you dimwitdog)
                ImageCodecInfo encoder = GetEncoder(format);
                EncoderParameters encParams = new EncoderParameters(1);
                EncoderParameter depth = new EncoderParameter(Encoder.ColorDepth, 24L);
                encParams.Param[0] = depth;
                image.Save(memoryStream, encoder, encParams);
            }
            else
            {
                image.Save(memoryStream, format);
            }
            return memoryStream;
            
        }

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


        public static void PostToGfycat(ref PostRequest model)
        {
            C.Write("Uploading to Gfycat...");
            string token = GetGfycatToken();
            string text = null;
            if (token.Length > 0)
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpWebRequest request = (HttpWebRequest) WebRequest.Create("https://weblogin.redgifs.com/oauth/weblogin");
                    request.Method = WebRequestMethods.Http.Post;
                    request.Accept = "application/json";
                    request.UserAgent = "curl/7.37.0";
                    request.ContentType = "application/json";


                    client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Bearer {token}");
                    Dictionary<string, string> createDictionary = new Dictionary<string, string>
                    {
                        {"noMd5", "true"},
                        {"noResize", "true"},
                        {"nsfw", "3"},
                        {"fetchUrl", model.RequestUrl},
                        {"title", model.Title}
                    };

                    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                    {

                        streamWriter.Write(JsonConvert.SerializeObject(createDictionary, Formatting.Indented));
                    }


                    using (var response = (HttpWebResponse) request.GetResponse())
                    {
                        // ReSharper disable once AssignNullToNotNullAttribute
                        using (var sr = new StreamReader(response.GetResponseStream()))
                        {
                            text = sr.ReadToEnd();
                        }
                    }
                }
            }

            RedGif.Response.Rootobject result = JsonConvert.DeserializeObject<RedGif.Response.Rootobject>(text);

            if (result.isOk)
            {
                bool isDone = false;
                string cUrl = $"https://api.gfycat.com/v1/gfycats/fetch/status/{result.gfyname}";
                while (!isDone)
                {
                    HttpWebRequest r = (HttpWebRequest) WebRequest.Create(cUrl);
                    using (HttpWebResponse k = (HttpWebResponse) r.GetResponse())
                    {
                        if (k.StatusCode == HttpStatusCode.OK && k.ContentLength > 0)
                        {
                            using (var sr = new StreamReader(k.GetResponseStream() ?? throw new InvalidOperationException()))
                            {
                                text = sr.ReadToEnd();
                            }

                            if (text.Contains("complete"))
                            {
                                isDone = true;
                            }
                            else
                            {
                                Thread.Sleep(50);
                            }
                        }
                    }
                }

            }

            model.ResultUrl = $"https://redgifs.com/{result.gfyname}";
            C.WriteLineNoTime("Done!");
        }


        private static string GetGfycatToken()
        {
            string j = "application/json";
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create("https://weblogin.redgifs.com/oauth/weblogin");
            request.Method = WebRequestMethods.Http.Post;
            request.Accept = j;
            request.UserAgent = "curl/7.37.0";
            request.ContentType = j;
            Dictionary<string, string> createDictionary = new Dictionary<string, string>
            {
                {"grant_type", "client_credentials"},
                {"client_id", Constants.Config.gfycat.client_id},
                {"client_secret", Constants.Config.gfycat.client_secret}
            };

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {

                streamWriter.Write(JsonConvert.SerializeObject(createDictionary, Formatting.Indented));
            }

            string text;

            using (var response = (HttpWebResponse) request.GetResponse())
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    text = sr.ReadToEnd();
                }
            }

            return JsonConvert.DeserializeObject<RedGif.Response.Token>(text).access_token;
        }
    }
}
