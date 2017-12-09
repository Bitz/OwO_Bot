﻿using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

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

        //public List<ImgHash> GetAllValidPosts()
        //{
        //    string ss = $"SELECT PostID, ImageHash, SubReddit FROM {_table} WHERE IsValid = 1;";
        //    var p = new List<MySqlParameter>();
        //    MySqlDataReader r = _con.ExecuteDataReader(ss, ref p);
        //    using (r)
        //    {
        //        return Convert.ReaderToList<ImgHash>(r, true);
        //    }
        //}

        //public int DeleteAllPostsOlderThan(int days = 30)
        //{
        //    string ss = $"DELETE FROM {_table} WHERE CreatedDate < UNIX_TIMESTAMP(DATE_SUB(NOW(), INTERVAL ?days DAY));";
        //    var parameters = new List<MySqlParameter> {new MySqlParameter("?days", days)};
        //    return _con.ExecuteNonQuery(ss, ref parameters);
        //}

        public bool AddPostToDatabase(Models.Misc.PostRequest postRequest)
        {
            string ss = $"INSERT INTO {_table} " +
                        @"( `E621Id`,
                            `ResultUrl`,
                            `DeleteHash`,
                            `RedditPostId`,
                            `Subreddit`,
                            `DatePosted`) VALUES" +
                        "(?E621Id, ?ResultUrl, ?DeleteHash, ?RedditPostId, ?Subreddit, ?DatePosted);";
            List<MySqlParameter> parameters = new List<MySqlParameter>
            {
                new MySqlParameter("?E621Id", postRequest.E621Id),
                new MySqlParameter("?ResultUrl", postRequest.ResultUrl),
                new MySqlParameter("?DeleteHash", postRequest.DeleteHash),
                new MySqlParameter("?RedditPostId", postRequest.RedditPostId),
                new MySqlParameter("?Subreddit", postRequest.Subreddit),
                new MySqlParameter("?DatePosted", postRequest.DatePosted),
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