using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace FrameworkSupport
{
    public class XmlUtil
    {
        public XmlUtil()
        { }

        public string Serialize(object dataToSerialize, string defaultNamespace = "", string header = "")
        {
            if (dataToSerialize == null)
            {
                return null;
            }

            var serializer = new XmlSerializer(dataToSerialize.GetType(), defaultNamespace);
            var settings = new XmlWriterSettings();

            settings.NewLineHandling = NewLineHandling.None;

            StringBuilder sb = new StringBuilder();

            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                writer.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
                writer.WriteRaw(header);
                serializer.Serialize(writer, dataToSerialize);
                return sb.ToString();
            }
        }

        public T Deserialize<T>(string dataToDeserialize)
        {
            if (string.IsNullOrWhiteSpace(dataToDeserialize))
            {
                return default(T);
            }

            using (StringReader stringReader = new StringReader(dataToDeserialize))
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(stringReader);
            }
        }

        /// <summary>
        /// Convert a string of XML into nicely a indented version for display
        /// </summary>
        public static Func<string, string> PrettyFormat = xml =>
        {
            try
            {
                XDocument doc = XDocument.Parse(xml);
                return doc.ToString();
            }
            catch (Exception)
            {
                return xml;
            }
        };
    }
}