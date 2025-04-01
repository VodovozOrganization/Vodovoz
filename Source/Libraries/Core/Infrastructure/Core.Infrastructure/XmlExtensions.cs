using System.Text;
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
	}
}
