using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Drawing.Imaging;
using System.Net.Http.Headers;
using System.Threading;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models;
using Newtonsoft.Json;
using OwO_Bot.Models;
using RedditSharp.Things;
using RestSharp;
using static OwO_Bot.Functions.Convert.ImageSize;
using static OwO_Bot.Functions.Html;
using static OwO_Bot.Models.Misc;

namespace OwO_Bot.Functions
{
    class Upload
    {
        public static void PostToImgur(ref PostRequest model)
        {
            ImgurClient client = new ImgurClient(Constants.Config.imgur.apikey);
            ImageEndpoint endpoint = new ImageEndpoint(client);
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
                            image = endpoint.UploadImageBinaryAsync(memoryStream.ToArray(), null, model.Title, model.Description).Result;
                        }
                    }
                }
                C.WriteLineNoTime("Done!");
            }
            else
            {
                image = endpoint.UploadImageUrlAsync(model.RequestUrl, null, model.Title, model.Description).Result;
            }

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
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.gfycat.com/v1/gfycats");
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


                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        // ReSharper disable once AssignNullToNotNullAttribute
                        using (var sr = new StreamReader(response.GetResponseStream()))
                        {
                            text = sr.ReadToEnd();
                        }
                    }
                }
            }
            Gfycat.Response.Rootobject result = JsonConvert.DeserializeObject<Gfycat.Response.Rootobject>(text);

            if (result.isOk)
            {
                bool isDone = false;
                string cUrl = $"https://api.gfycat.com/v1/gfycats/fetch/status/{result.gfyname}";
                while (!isDone)
                {
                    HttpWebRequest r = (HttpWebRequest)WebRequest.Create(cUrl);
                    using (HttpWebResponse k = (HttpWebResponse)r.GetResponse())
                    {
                        if (k.StatusCode == HttpStatusCode.OK && k.GetResponseStream() != null)
                        {
                            using (var sr = new StreamReader(k.GetResponseStream()))
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
            model.ResultUrl = $"https://gfycat.com/{result.gfyname}";
            C.WriteLineNoTime("Done!");
        }


        private static string GetGfycatToken()
        {
            string j = "application/json";
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create("https://api.gfycat.com/v1/oauth/token");
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

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    text = sr.ReadToEnd();
                }
            }
            return JsonConvert.DeserializeObject<Gfycat.Response.Token>(text).access_token;
        }

        public static void PostToImgurAsVideo(ref PostRequest model)
        {
            C.Write("Uploading to Imgur...");

            var bytes = GetArrayFromUrl(model.RequestUrl);

            var client = new RestClient("https://api.imgur.com/3/upload")
            {
                Timeout = 120000
            };
            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", $"Client-ID {Constants.Config.imgur.apikey}");
            request.AlwaysMultipartFormData = true;
            var file = FileParameter.Create("video", bytes, "video");
            request.Files.Add(file);
            request.AddParameter("title", model.Title);
            request.AddParameter("description", model.Description);
            request.AddParameter("disable_audio", "0");
            IRestResponse response = client.Execute(request);
            ImgurResponse r = JsonConvert.DeserializeObject<ImgurResponse>(response.Content);

            model.ResultUrl = $"{r.Data.Link.ToString().TrimEnd('.')}.gifv";
            model.DeleteHash = r.Data.Deletehash;

            C.WriteLineNoTime("Done!");
        }
    }
}
