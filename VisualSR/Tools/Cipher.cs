using System.IO;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace VisualSR.Tools
{
    public class Cipher
    {
        public static void SerializeToFile<T>(string filePath, T objectToWrite, bool append = false, bool JSON = true)
            where T : new()
        {
            if (!JSON)
            {
                TextWriter writer = null;
                try
                {
                    var serializer = new XmlSerializer(typeof(T));
                    writer = new StreamWriter(filePath, append);
                    serializer.Serialize(writer, objectToWrite);
                }
                finally
                {
                    if (writer != null)
                        writer.Close();
                }
                return;
            }
            File.WriteAllText(filePath, SerializeToString(objectToWrite));
        }

        public static string SerializeToString<T>(T toSerialize)
        {
            return new JavaScriptSerializer().Serialize(toSerialize);
        }

        public static T DeSerializeFromString<T>(string data) where T : new()
        {
            return new JavaScriptSerializer().Deserialize<T>(data);
        }

        public static T DeSerializeFromFile<T>(string filePath, bool JSON = true) where T : new()
        {
            if (JSON)
            {
                var data = File.ReadAllText(filePath);
                return new JavaScriptSerializer().Deserialize<T>(data);
            }
            TextReader reader = null;
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                reader = new StreamReader(filePath);
                return (T) serializer.Deserialize(reader);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }
    }
}