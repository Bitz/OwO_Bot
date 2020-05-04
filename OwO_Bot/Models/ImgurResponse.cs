namespace OwO_Bot.Models
{
    using System;
    using System.Collections.Generic;
    using J = Newtonsoft.Json.JsonPropertyAttribute;

    public class ImgurResponse
    {
        [J("data")] public Data Data { get; set; }
        [J("success")] public bool Success { get; set; }
        [J("status")] public long? Status { get; set; }
    }

    public class Data
    {
        [J("id")] public string Id { get; set; }
        [J("title")] public object Title { get; set; }
        [J("description")] public object Description { get; set; }
        [J("datetime")] public long? Datetime { get; set; }
        [J("type")] public string Type { get; set; }
        [J("animated")] public bool Animated { get; set; }
        [J("width")] public long? Width { get; set; }
        [J("height")] public long? Height { get; set; }
        [J("size")] public long? Size { get; set; }
        [J("views")] public long? Views { get; set; }
        [J("bandwidth")] public long? Bandwidth { get; set; }
        [J("vote")] public object Vote { get; set; }
        [J("favorite")] public bool Favorite { get; set; }
        [J("nsfw")] public object Nsfw { get; set; }
        [J("section")] public object Section { get; set; }
        [J("account_url")] public object AccountUrl { get; set; }
        [J("account_id")] public long? AccountId { get; set; }
        [J("is_ad")] public bool IsAd { get; set; }
        [J("in_most_viral")] public bool InMostViral { get; set; }
        [J("tags")] public List<object> Tags { get; set; }
        [J("ad_type")] public long? AdType { get; set; }
        [J("ad_url")] public string AdUrl { get; set; }
        [J("in_gallery")] public bool? InGallery { get; set; }
        [J("deletehash")] public string Deletehash { get; set; }
        [J("name")] public string Name { get; set; }
        [J("link")] public Uri Link { get; set; }
    }
}
