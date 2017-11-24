using System.Xml.Serialization;
using System;
using System.Collections.Generic;

namespace OwO_Bot.Models
{
    /// <remarks/>
    [SerializableAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    [XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class configuration
    {

        private configurationReddit redditField;

        private configurationImgur imgurField;

        private configurationGfycat gfycatField;

        private configurationSub[] subreddit_configurationsField;

        /// <remarks/>
        public configurationReddit reddit
        {
            get => redditField;
            set => redditField = value;
        }

        /// <remarks/>
        public configurationImgur imgur
        {
            get => imgurField;
            set => imgurField = value;
        }

        /// <remarks/>
        public configurationGfycat gfycat
        {
            get => gfycatField;
            set => gfycatField = value;
        }

        /// <remarks/>
        [XmlArrayItemAttribute("sub", IsNullable = false)]
        public configurationSub[] subreddit_configurations
        {
            get => subreddit_configurationsField;
            set => subreddit_configurationsField = value;
        }
    }

    /// <remarks/>
    [SerializableAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    public partial class configurationReddit
    {

        private string usernameField;

        private string passwordField;

        private string client_idField;

        private string secret_idField;

        private string callback_urlField;

        private byte check_Back_X_DaysField;

        /// <remarks/>
        [XmlAttribute]
        public string username
        {
            get => usernameField;
            set => usernameField = value;
        }

        /// <remarks/>
        [XmlAttribute]
        public string password
        {
            get => passwordField;
            set => passwordField = value;
        }

        /// <remarks/>
        [XmlAttribute]
        public string client_id
        {
            get => client_idField;
            set => client_idField = value;
        }

        /// <remarks/>
        [XmlAttribute]
        public string secret_id
        {
            get => secret_idField;
            set => secret_idField = value;
        }

        /// <remarks/>
        [XmlAttribute]
        public string callback_url
        {
            get => callback_urlField;
            set => callback_urlField = value;
        }

        /// <remarks/>
        [XmlAttribute]
        public byte Check_Back_X_Days
        {
            get => check_Back_X_DaysField;
            set => check_Back_X_DaysField = value;
        }
    }

    /// <remarks/>
    [SerializableAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    public partial class configurationImgur
    {

        private string apikeyField;

        /// <remarks/>
        [XmlAttribute]
        public string apikey
        {
            get => apikeyField;
            set => apikeyField = value;
        }
    }

    /// <remarks/>
    [SerializableAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    public partial class configurationGfycat
    {

        private string client_idField;

        private string client_secretField;

        /// <remarks/>
        [XmlAttribute]
        public string client_id
        {
            get => client_idField;
            set => client_idField = value;
        }

        /// <remarks/>
        [XmlAttribute]
        public string client_secret
        {
            get => client_secretField;
            set => client_secretField = value;
        }
    }

    /// <remarks/>
    [SerializableAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    public partial class configurationSub
    {

        private string subredditField;

        private string tagsField;

        private string hideField;

        private string sensitivetagsField;

        /// <remarks/>
        [XmlAttribute]
        public string subreddit
        {
            get => subredditField;
            set => subredditField = value;
        }

        /// <remarks/>
        [XmlAttribute]
        public string tags
        {
            get => tagsField;
            set => tagsField = value;
        }

        /// <remarks/>
        [XmlAttribute]
        public string hide
        {
            get => hideField;
            set => hideField = value;
        }

        /// <remarks/>
        [XmlAttribute("sensitive-tags")]
        public string sensitivetags
        {
            get => sensitivetagsField;
            set => sensitivetagsField = value;
        }
    }



}
