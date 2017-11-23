using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using MySql.Data.MySqlClient;
using static OwO_Bot.Models.Hashing;

namespace OwO_Bot.Functions.DAL
{
    class DbPosts
    {
        private readonly DbConnector _con = new DbConnector();

        public List<ImgHash> GetAllIds()
        {
            string ss = "SELECT PostID FROM owo_bot.posts;";
            var p = new List<MySqlParameter>();
            MySqlDataReader r = _con.ExecuteDataReader(ss, ref p);
            using (r)
            {
                return DbConnector.ConvertToList<ImgHash>(r);
            }
        }

        public int DeleteAllPostsOlderThan(int days = 30)
        {
            string ss = "DELETE FROM owo_bot.posts WHERE CreatedDate < UNIX_TIMESTAMP(DATE_SUB(NOW(), INTERVAL ?days DAY));";
            var parameters = new List<MySqlParameter> {new MySqlParameter("?days", days)};
            return _con.ExecuteNonQuery(ss, ref parameters);
        }

        public bool AddPostToDatabase(ImgHash imageData)
        {
            string ss = "INSERT INTO owo_bot.posts " +
                        "(PostID, ImageHash, CreatedDate, IsValid) VALUES" +
                        "(?PostId, ?Hash, ?CreatedDate, ?IsValid);";
            List<MySqlParameter> parameters = new List<MySqlParameter>
            {
                new MySqlParameter("?PostId", imageData.PostId),
                new MySqlParameter("?Hash", imageData.ImageHash),
                new MySqlParameter("?CreatedDate", imageData.CreatedDate),
                new MySqlParameter("?IsValid", imageData.IsValid)
            };
            var rowsAffected = _con.ExecuteNonQuery(ss, ref parameters);

            return rowsAffected == 1;
        }

    }
}
