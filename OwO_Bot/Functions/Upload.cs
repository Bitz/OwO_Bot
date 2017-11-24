using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models;
using Newtonsoft.Json;
using OwO_Bot.Models;
using static OwO_Bot.Models.Misc;

namespace OwO_Bot.Functions
{
    class Upload
    {
        public static void PostToImgur(ref PostRequest model)
        {
            ImgurClient client = new ImgurClient(Constants.Config.imgur.apikey);
            ImageEndpoint endpoint = new ImageEndpoint(client);
            IImage image = endpoint.UploadImageUrlAsync(model.RequestUrl, null, model.Title, model.Description).Result;
            model.ResultUrl = image.Link;
            model.DeleteHash = image.DeleteHash;
            //return model;
        }

        public static void PostToGfycat(ref PostRequest model)
        {
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
                                Thread.Sleep(500);
                            }
                        }
                    }
                }

                //request.BeginGetResponse(new AsyncCallback(FinishRequest), request);
            }
            model.ResultUrl = $"https://gfycat.com/{result.gfyname}";
            //return model;
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
    }
}
