using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using MySql.Data.MySqlClient;

namespace OwO_Bot.Functions.DAL
{
    class DbConnector : IDisposable
    {
        MySqlConnection _sqlConnection;

        private string GetConnectionString()
        {
            return System.Configuration.ConfigurationManager.
                ConnectionStrings["Test"].ConnectionString;
        }

        private MySqlConnection GetConnection()
        {
            if (_sqlConnection == null)
            {
                _sqlConnection = new MySqlConnection(GetConnectionString());
            }
            if (_sqlConnection.State != ConnectionState.Open)
            {
                _sqlConnection.Open();
            }
            return _sqlConnection;
        }

        public void CloseConnection()
        {
            if (_sqlConnection?.State == ConnectionState.Open)
            {
                _sqlConnection.Close();
                _sqlConnection.Dispose();
            }
        }

        public int ExecuteNonQuery(string commandText, ref List<MySqlParameter> parameterCollection,
            bool keepOpen = false)
        {
            int affectedRows;
            MySqlConnection connection = GetConnection();
            MySqlCommand sqlCommand = new MySqlCommand
            {
                Connection = connection,
                CommandType = CommandType.Text,
                CommandText = commandText
            };
            if (parameterCollection != null)
            {
                foreach (MySqlParameter p in parameterCollection.GroupBy(x => x.ParameterName)
                    .Select(y => y.Last()))
                {
                    sqlCommand.Parameters.Add(p);
                }
            }
            using (MySqlTransaction tr = connection.BeginTransaction())
            {
                sqlCommand.Transaction = tr;
                affectedRows = sqlCommand.ExecuteNonQuery();
                tr.Commit();
            }
            if (!keepOpen)
            {
                CloseConnection();
            }
            return affectedRows;
        }


        public MySqlDataReader ExecuteDataReader(string commandText, ref List<MySqlParameter> parameterCollection)
        {
            MySqlConnection connection = GetConnection();

            MySqlCommand sqlCommand = new MySqlCommand
            {
                Connection = connection,
                CommandType = CommandType.Text,
                CommandText = commandText
            };
            if (parameterCollection != null)
            {
                foreach (MySqlParameter p in parameterCollection.GroupBy(x => x.ParameterName)
                    .Select(y => y.Last()))
                {
                    sqlCommand.Parameters.Add(p);
                }
            }
            return sqlCommand.ExecuteReader();
            //CONNECTION WILL NEED TO BE MANUALLY CLOSED!!!
        }

        public void Dispose()
        {
            CloseConnection();
        }

        public static List<T> ConvertToList<T>(IDataReader dr, bool keepOpen = false) where T : new()
        {
            Type businessEntityType = typeof(T);
            List<T> ent = new List<T>();
            Hashtable hashtable = new Hashtable();
            PropertyInfo[] properties = businessEntityType.GetProperties();
            foreach (PropertyInfo info in properties)
            {
                hashtable[info.Name.ToUpper()] = info;
            }
            while (dr.Read())
            {
                T newObject = new T();
                for (int index = 0; index < dr.FieldCount; index++)
                {
                    PropertyInfo info = (PropertyInfo)
                        hashtable[dr.GetName(index).ToUpper()];
                    if (info != null && info.CanWrite)
                    {
                        if (!dr[index].Equals(DBNull.Value))
                            info.SetValue(newObject, dr[index], null);
                    }
                }
                ent.Add(newObject);
            }
            if (!keepOpen)
            {
                dr.Close();
            }
            return ent;
        }
}
}
