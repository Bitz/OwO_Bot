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
                                info.SetValue(newObject, (bool) dr[index], null);
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


        public static string BytesToReadableString(Int64 value, int decimalPlaces = 1)
        {
            string[] sizeSuffixes =
            { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException(nameof(decimalPlaces)); }
            if (value < 0) { return "-" + BytesToReadableString(-value); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                sizeSuffixes[mag]);
        }
    }
}
