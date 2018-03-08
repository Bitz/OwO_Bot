using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using static OwO_Bot.Models.Hashing;

namespace OwO_Bot.Functions.DAL
{
    class DbSubredditPosts : IDisposable
    {
        private readonly DbConnector _con;
        //Extracted table name in case we move or rename the table; easier to change this way.
        private string _table = "owo_bot.subreddit_posts";

        public DbSubredditPosts(DbConnector connection)
        {
            _con = connection;
        }

        public DbSubredditPosts()
        {
            _con = new DbConnector();
        }

        public List<ImgHash> GetAllIds()
        {
            string ss = $"SELECT PostID FROM {_table};";
            var p = new List<MySqlParameter>();
            MySqlDataReader r = _con.ExecuteDataReader(ss, ref p);
            using (r)
            {
                return Convert.ReaderToList<ImgHash>(r);
            }
        }

        public List<ImgHash> GetAllValidPosts()
        {
            string ss = $"SELECT PostID, ImageHash, SubReddit FROM {_table} WHERE IsValid = 1;";
            var p = new List<MySqlParameter>();
            MySqlDataReader r = _con.ExecuteDataReader(ss, ref p);
            using (r)
            {
                return Convert.ReaderToList<ImgHash>(r, true);
            }
        }

        public int DeleteAllPostsOlderThan(int days = 30)
        {
            string ss = $"DELETE FROM {_table} WHERE CreatedDate < subdate(current_date, ?days);";
            var parameters = new List<MySqlParameter> {new MySqlParameter("?days", days)};
            return _con.ExecuteNonQuery(ss, ref parameters);
        }

        public bool AddPostToDatabase(ImgHash imageData)
        {
            string ss = $"INSERT INTO {_table} " +
                        "(PostID, ImageHash, SubReddit, CreatedDate, IsValid) VALUES" +
                        "(?PostId, ?Hash, ?SubReddit, ?CreatedDate, ?IsValid);";
            List<MySqlParameter> parameters = new List<MySqlParameter>
            {
                new MySqlParameter("?PostId", imageData.PostId),
                new MySqlParameter("?Hash", imageData.ImageHash),
                new MySqlParameter("?SubReddit", imageData.SubReddit),
                new MySqlParameter("?CreatedDate", imageData.CreatedDate),
                new MySqlParameter("?IsValid", imageData.IsValid),
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
