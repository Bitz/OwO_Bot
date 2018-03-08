using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using OwO_Bot.Models;

namespace OwO_Bot.Functions.DAL
{
    class DbBlackList : IDisposable
    {
        private readonly DbConnector _con;
        //Extracted table name in case we move or rename the table; easier to change this way.
        private string _table = "owo_bot.blacklist";

        public DbBlackList(DbConnector connection)
        {
            _con = connection;
        }

        public DbBlackList()
        {
            _con = new DbConnector();
        }

        public List<Blacklist> GetAllIds(string subreddit)
        {
            string whereString = "";
            var p = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(subreddit))
            {
                p.Add(new MySqlParameter("?SubReddit", subreddit));
                whereString = "WHERE SubReddit = ?SubReddit;";
            }

            string ss = $"SELECT PostID FROM {_table} {whereString}";
            MySqlDataReader r = _con.ExecuteDataReader(ss, ref p);
            using (r)
            {
                return Convert.ReaderToList<Blacklist>(r);
            }
        }

        public int DeleteAllPostsOlderThan(int days = 30)
        {
            string ss = $"DELETE FROM {_table} WHERE CreatedDate < subdate(current_date, ?days);";
            var parameters = new List<MySqlParameter> {new MySqlParameter("?days", days)};
            return _con.ExecuteNonQuery(ss, ref parameters);
        }

        public bool AddToBlacklist(Blacklist imageData)
        {
            string ss = $"INSERT INTO {_table} " +
                        "(PostID, SubReddit, CreatedDate) VALUES " +
                        "(?PostId, ?SubReddit, ?CreatedDate);";
            List<MySqlParameter> parameters = new List<MySqlParameter>
            {
                new MySqlParameter("?PostId", imageData.PostId),
                new MySqlParameter("?SubReddit", imageData.Subreddit),
                new MySqlParameter("?CreatedDate", imageData.CreatedDate),
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
