using System.Xml.Serialization;
using System;

namespace OwO_Bot.Models
{


    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class configuration
    {

        private configurationDatabase databaseField;

        private configurationReddit redditField;

        private configurationImgur imgurField;

        private configurationGfycat gfycatField;

        private configurationMail mailField;

        private configurationSub[] subreddit_configurationsField;

        /// <remarks/>
        public configurationDatabase database
        {
            get
            {
                return this.databaseField;
            }
            set
            {
                this.databaseField = value;
            }
        }

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
        public configurationMail mail
        {
            get
            {
                return this.mailField;
            }
            set
            {
                this.mailField = value;
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
    public partial class configurationDatabase
    {

        private string connectionStringField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string connectionString
        {
            get
            {
                return this.connectionStringField;
            }
            set
            {
                this.connectionStringField = value;
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

        private byte check_Back_X_DaysField;

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

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte Check_Back_X_Days
        {
            get
            {
                return this.check_Back_X_DaysField;
            }
            set
            {
                this.check_Back_X_DaysField = value;
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
    public partial class configurationMail
    {

        private string toField;

        private string usernameField;

        private string passwordField;

        private string incoming_serverField;

        private ushort incoming_portField;

        private string outgoing_serverField;

        private ushort outgoing_portField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string to
        {
            get
            {
                return this.toField;
            }
            set
            {
                this.toField = value;
            }
        }

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
        public string incoming_server
        {
            get
            {
                return this.incoming_serverField;
            }
            set
            {
                this.incoming_serverField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort incoming_port
        {
            get
            {
                return this.incoming_portField;
            }
            set
            {
                this.incoming_portField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string outgoing_server
        {
            get
            {
                return this.outgoing_serverField;
            }
            set
            {
                this.outgoing_serverField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort outgoing_port
        {
            get
            {
                return this.outgoing_portField;
            }
            set
            {
                this.outgoing_portField = value;
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
    }




}
