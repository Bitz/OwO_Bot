

namespace OwO_Bot.Models
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using J = Newtonsoft.Json.JsonPropertyAttribute;

    public class E621Search
    {
        [J("posts")] public List<Post> Posts { get; set; }
    }

    public class Post
    {
        [J("id")] public long Id { get; set; }

        [J("created_at")] public DateTimeOffset CreatedAt { get; set; }

        [J("updated_at")] public DateTimeOffset UpdatedAt { get; set; }

        [J("file")] public File File { get; set; }

        [J("preview")] public Preview Preview { get; set; }

        [J("sample")] public Preview Sample { get; set; }

        [J("score")] public Score Score { get; set; }

        [J("tags")] public Tags Tags { get; set; }

        [J("locked_tags")] public List<string> LockedTags { get; set; }

        [J("change_seq")] public long ChangeSeq { get; set; }

        [J("flags")] public Flags Flags { get; set; }

        [J("rating")] public string Rating { get; set; }

        [J("fav_count")] public long FavCount { get; set; }

        [J("sources")] public List<Uri> Sources { get; set; }

        [J("pools")] public List<object> Pools { get; set; }

        [J("relationships")] public Relationships Relationships { get; set; }

        [J("approver_id")] public long? ApproverId { get; set; }

        [J("uploader_id")] public long UploaderId { get; set; }

        [J("description")] public string Description { get; set; }

        [J("comment_count")] public long CommentCount { get; set; }

        [J("is_favorited")] public bool IsFavorited { get; set; }
    }

    public class File
    {
        [J("width")] public long Width { get; set; }

        [J("height")] public long Height { get; set; }

        [J("ext")] public string Ext { get; set; }

        [J("size")] public long Size { get; set; }

        [J("md5")] public string Md5 { get; set; }

        [J("url")] public Uri Url { get; set; }
    }

    public class Flags
    {
        [J("pending")] public bool Pending { get; set; }

        [J("flagged")] public bool Flagged { get; set; }

        [J("note_locked")] public bool NoteLocked { get; set; }

        [J("status_locked")] public bool StatusLocked { get; set; }

        [J("rating_locked")] public bool RatingLocked { get; set; }

        [J("deleted")] public bool Deleted { get; set; }
    }

    public class Preview
    {
        [J("width")] public long Width { get; set; }

        [J("height")] public long Height { get; set; }

        [J("url")] public Uri Url { get; set; }

        [J("has", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Has { get; set; }
    }

    public class Relationships
    {
        [J("parent_id")] public long? ParentId { get; set; }

        [J("has_children")] public bool HasChildren { get; set; }

        [J("has_active_children")] public bool HasActiveChildren { get; set; }

        [J("children")] public List<long> Children { get; set; }
    }

    public class Score
    {
        [J("up")] public long Up { get; set; }

        [J("down")] public long Down { get; set; }

        [J("total")] public long Total { get; set; }
    }

    public class Tags
    {
        [J("general")] public List<string> General { get; set; }

        [J("species")] public List<string> Species { get; set; }

        [J("character")] public List<string> Character { get; set; }

        [J("copyright")] public List<string> Copyright { get; set; }

        [J("artist")] public List<string> Artist { get; set; }

        [J("invalid")] public List<string> Invalid { get; set; }

        [J("lore")] public List<string> Lore { get; set; }

        [J("meta")] public List<string> Meta { get; set; }
    }
}