using System.Xml.Serialization;
using System.Collections.Generic;

namespace OwO_Bot.Models
{
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class configuration
    {

        private configurationReddit redditField;

        private configurationImgur imgurField;

        private configurationGfycat gfycatField;

        private configurationSub[] subreddit_configurationsField;

        /// <remarks/>
        public configurationReddit reddit
        {
            get
            {
                return this.redditField;
            }
            set
            {
                this.redditField = value;
            }
        }

        /// <remarks/>
        public configurationImgur imgur
        {
            get
            {
                return this.imgurField;
            }
            set
            {
                this.imgurField = value;
            }
        }

        /// <remarks/>
        public configurationGfycat gfycat
        {
            get
            {
                return this.gfycatField;
            }
            set
            {
                this.gfycatField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("sub", IsNullable = false)]
        public configurationSub[] subreddit_configurations
        {
            get
            {
                return this.subreddit_configurationsField;
            }
            set
            {
                this.subreddit_configurationsField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class configurationReddit
    {

        private string usernameField;

        private string passwordField;

        private string client_idField;

        private string secret_idField;

        private string callback_urlField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string username
        {
            get
            {
                return this.usernameField;
            }
            set
            {
                this.usernameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string password
        {
            get
            {
                return this.passwordField;
            }
            set
            {
                this.passwordField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string client_id
        {
            get
            {
                return this.client_idField;
            }
            set
            {
                this.client_idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string secret_id
        {
            get
            {
                return this.secret_idField;
            }
            set
            {
                this.secret_idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string callback_url
        {
            get
            {
                return this.callback_urlField;
            }
            set
            {
                this.callback_urlField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class configurationImgur
    {

        private string apikeyField;

        private byte useimgurField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string apikey
        {
            get
            {
                return this.apikeyField;
            }
            set
            {
                this.apikeyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte useimgur
        {
            get
            {
                return this.useimgurField;
            }
            set
            {
                this.useimgurField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class configurationGfycat
    {

        private string client_idField;

        private string client_secretField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string client_id
        {
            get
            {
                return this.client_idField;
            }
            set
            {
                this.client_idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string client_secret
        {
            get
            {
                return this.client_secretField;
            }
            set
            {
                this.client_secretField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class configurationSub
    {

        private string subredditField;

        private string tagsField;

        private string hideField;

        private string sensitivetagsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string subreddit
        {
            get
            {
                return this.subredditField;
            }
            set
            {
                this.subredditField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string tags
        {
            get
            {
                return this.tagsField;
            }
            set
            {
                this.tagsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string hide
        {
            get
            {
                return this.hideField;
            }
            set
            {
                this.hideField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("sensitive-tags")]
        public string sensitivetags
        {
            get
            {
                return this.sensitivetagsField;
            }
            set
            {
                this.sensitivetagsField = value;
            }
        }
    }


}
