using System.Collections.Generic;
using Newtonsoft.Json;

using J = Newtonsoft.Json.JsonPropertyAttribute;

namespace OwO_Bot.Models
{
    class E621Search
    {
        public class SearchResult
        {
            [J("artist")] public List<string> Artist { get; set; }
            [J("author")] public string Author { get; set; }
            [J("change")] public long Change { get; set; }
            [J("children")] public string Children { get; set; }
            [J("created_at")] public CreatedAt CreatedAt { get; set; }
            [J("creator_id")] public long CreatorId { get; set; }
            [J("description")] public string Description { get; set; }
            [J("fav_count")] public long FavCount { get; set; }
            [J("file_ext")] public string FileExt { get; set; }
            [J("file_size")] public long FileSize { get; set; }
            [J("file_url")] public string FileUrl { get; set; }
            [J("has_children")] public bool HasChildren { get; set; }
            [J("has_comments")] public bool HasComments { get; set; }
            [J("has_notes")] public bool HasNotes { get; set; }
            [J("height")] public long Height { get; set; }
            [J("id")] public long Id { get; set; }
            [J("locked_tags")] public object LockedTags { get; set; }
            [J("md5")] public string Md5 { get; set; }
            [J("parent_id")] public object ParentId { get; set; }
            [J("preview_height")] public long PreviewHeight { get; set; }
            [J("preview_url")] public string PreviewUrl { get; set; }
            [J("preview_width")] public long PreviewWidth { get; set; }
            [J("rating")] public string Rating { get; set; }
            [J("sample_height")] public long SampleHeight { get; set; }
            [J("sample_url")] public string SampleUrl { get; set; }
            [J("sample_width")] public long SampleWidth { get; set; }
            [J("score")] public long Score { get; set; }
            [J("source")] public string Source { get; set; }
            [J("sources")] public List<string> Sources { get; set; }
            [J("status")] public string Status { get; set; }
            [J("tags")] public string Tags { get; set; }
            [J("width")] public long Width { get; set; }
        }

        public class CreatedAt
        {
            [J("json_class")] public string JsonClass { get; set; }
            [J("n")] public long N { get; set; }
            [J("s")] public long S { get; set; }
        }
        

        public class Converter
        {
            public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
            };
        }
    }
}
