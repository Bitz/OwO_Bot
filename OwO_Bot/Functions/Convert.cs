
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace OwO_Bot.Functions
{
    class Convert
    {
        public static T NodeToClass<T>(XmlNode node) where T : class
        {
            MemoryStream stm = new MemoryStream();

            StreamWriter stw = new StreamWriter(stm);
            stw.Write(node.OuterXml);
            stw.Flush();

            stm.Position = 0;

            XmlSerializer ser = new XmlSerializer(typeof(T));
            T result = ser.Deserialize(stm) as T;

            return result;
        }

        public static List<T> ReaderToList<T>(IDataReader dr, bool keepOpen = false) where T : new()
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
                        if (dr[index] != DBNull.Value)
                            //MySQL Cannot properly cast bit to bool, so we have to tell it to get the bit as a boolean.
                            if (info.PropertyType == typeof(bool))
                            {

                                info.SetValue(newObject, dr.GetBoolean(index), null);
                            }
                            else
                            {

                                info.SetValue(newObject, dr[index], null);
                            }
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
