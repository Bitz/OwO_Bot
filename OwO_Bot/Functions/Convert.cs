
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace OwO_Bot.Functions
{
    class Convert
    {
        public static T ConvertNode<T>(XmlNode node) where T : class
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
    }
}
