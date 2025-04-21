using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Core.Infrastructure
{
	public static class XmlExtensions
	{
		public static string ToXmlString<T>(this T data) where T : class
		{
			using(var stringWriter = new StringWriterWithEncoding(Encoding.UTF8))
			{
				var ns = new XmlSerializerNamespaces();
				ns.Add(string.Empty, string.Empty);
				var serializer = new XmlSerializer(typeof(T));
				serializer.Serialize(stringWriter, data, ns);

				return stringWriter.ToString();
			}
		}

		public static T DeserializeXmlString<T>(this string data, bool withoutNamespaces = true) where T : class
		{
			using(var stringReader = new StringReader(data))
			using(var xmlReader = new XmlTextReader(stringReader))
			{
				xmlReader.Namespaces = !withoutNamespaces;
				return (T)new XmlSerializer(typeof(T)).Deserialize(xmlReader);
			}
		}	
	}
}
