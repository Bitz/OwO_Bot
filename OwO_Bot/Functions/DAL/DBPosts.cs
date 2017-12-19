using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using OwO_Bot.Models;

namespace OwO_Bot.Functions.DAL
{
    class DbPosts : IDisposable
    {
        private readonly DbConnector _con;
        //Extracted table name in case we move or rename the table; easier to change this way.
        private string _table = "owo_bot.posts";

        public DbPosts(DbConnector connection)
        {
            _con = connection;
        }

        public DbPosts()
        {
            _con = new DbConnector();
        }

        //public List<ImgHash> GetAllIds()
        //{
        //    string ss = $"SELECT PostID FROM {_table};";
        //    var p = new List<MySqlParameter>();
        //    MySqlDataReader r = _con.ExecuteDataReader(ss, ref p);
        //    using (r)
        //    {
        //        return Convert.ReaderToList<ImgHash>(r);
        //    }
        //}


        //public int DeleteAllPostsOlderThan(int days = 30)
        //{
        //    string ss = $"DELETE FROM {_table} WHERE CreatedDate < UNIX_TIMESTAMP(DATE_SUB(NOW(), INTERVAL ?days DAY));";
        //    var parameters = new List<MySqlParameter> {new MySqlParameter("?days", days)};
        //    return _con.ExecuteNonQuery(ss, ref parameters);
        //}



        public List<Misc.PostRequest> GetPostData(long e621Id)
        {
            string ss = $"SELECT * FROM {_table} WHERE E621Id = ?E621Id;";
            var p = new List<MySqlParameter>
            {
                new MySqlParameter("?E621Id", e621Id)
            };
            MySqlDataReader r = _con.ExecuteDataReader(ss, ref p);
            using (r)
            {
                return Convert.ReaderToList<Misc.PostRequest>(r, true);
            }
        }


        public List<Misc.PostRequest> GetAllPosts()
        {
            string ss = $"SELECT * FROM {_table};";
            var p = new List<MySqlParameter>();
            MySqlDataReader r = _con.ExecuteDataReader(ss, ref p);
            using (r)
            {
                return Convert.ReaderToList<Misc.PostRequest>(r, true);
            }
        }


        public void SetTitle(long e621Id, string title)
        {
            string ss = $"UPDATE {_table} SET Title = ?Title WHERE E621Id = ?E621Id;";
            List<MySqlParameter> parameters = new List<MySqlParameter>
            {
                new MySqlParameter("?E621Id", e621Id),
                new MySqlParameter("?Title", title)
            };
            _con.ExecuteNonQuery(ss, ref parameters, true);
        }

        public string GetTitle(long e621Id)
        {
            string ss = $"SELECT Title FROM {_table} WHERE E621Id = ?E621Id;";
            List<MySqlParameter> parameters = new List<MySqlParameter>
            {
                new MySqlParameter("?E621Id", e621Id.ToString())
            };
            string result = string.Empty;
            using (MySqlDataReader r = _con.ExecuteDataReader(ss, ref parameters))
            {
                while (r.Read())
                {
                    result = r.GetString(r.GetOrdinal("Title"));
                }
            }
            return result;
        }


        public void UpdateColorScheme(long id, string hex)
        {
            string ss = $"UPDATE {_table} SET ColorScheme = ?ColorScheme WHERE Id = ?Id;";
            List<MySqlParameter> parameters = new List<MySqlParameter>
            {
                new MySqlParameter("?Id", id),
                new MySqlParameter("?ColorScheme", hex)
            };
            _con.ExecuteNonQuery(ss, ref parameters, true);
        }

        public List<Misc.PostRequest> GetWithNoColorScheme()
        {
            string ss = $"SELECT Id, ResultUrl FROM {_table} WHERE ColorScheme IS NUlL;";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            using (MySqlDataReader r = _con.ExecuteDataReader(ss, ref parameters))
            {
                return Convert.ReaderToList<Misc.PostRequest>(r, true);
            }
        }

        public bool AddPostToDatabase(Misc.PostRequest postRequest)
        {
            string ss = $"INSERT INTO {_table} " +
                        @"( `E621Id`,
                            `ResultUrl`,
                            `DeleteHash`,
                            `Title`,
                            `RedditPostId`,
                            `Subreddit`,
                            `ColorScheme`,
                            `DatePosted`) VALUES" +
                        "(?E621Id, ?ResultUrl, ?DeleteHash, ?Title, ?RedditPostId, ?Subreddit, ?ColorScheme, ?DatePosted);";
            List<MySqlParameter> parameters = new List<MySqlParameter>
            {
                new MySqlParameter("?E621Id", postRequest.E621Id),
                new MySqlParameter("?ResultUrl", postRequest.ResultUrl),
                new MySqlParameter("?DeleteHash", postRequest.DeleteHash),
                new MySqlParameter("?Title", postRequest.Title),
                new MySqlParameter("?RedditPostId", postRequest.RedditPostId),
                new MySqlParameter("?Subreddit", postRequest.Subreddit),
                new MySqlParameter("?ColorScheme", postRequest.ColorScheme),
                new MySqlParameter("?DatePosted", postRequest.DatePosted)
            };
            var rowsAffected = _con.ExecuteNonQuery(ss, ref parameters);

            return rowsAffected == 1;
        }

        public void Dispose()
        {
            _con?.Dispose();
        }
    }
}
