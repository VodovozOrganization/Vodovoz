using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Core.Infrastructure
{
	public static class XmlExtensions
	{
		public static readonly Encoding Win1251Encoding = Encoding.GetEncoding("windows-1251");
		public static readonly Encoding Utf8Encoding = Encoding.UTF8;
		
		public static byte[] SerializeObject<T>(this T data, Encoding encoding = null) where T : class
		{
			if(!data.GetType().IsSerializable)
			{
				throw new InvalidOperationException("Переданный тип не сериализуем");
			}

			if(encoding is null)
			{
				encoding = Win1251Encoding;
			}

			return Serialize(data, encoding);
		}

		public static string ToXmlString<T>(this T data, Encoding encoding = null) where T : class
		{
			using(var stringWriter = new StringWriterWithEncoding(encoding ?? Utf8Encoding))
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
		
		public static T DeserializeXmlStream<T>(this Stream data, bool withoutNamespaces = true) where T : class
		{
			using(var xmlReader = new XmlTextReader(data))
			{
				xmlReader.Namespaces = !withoutNamespaces;
				return (T)new XmlSerializer(typeof(T)).Deserialize(xmlReader);
			}
		}
		
		private static byte[] Serialize<T>(T data, Encoding encoding) where T : class
		{
			using (var memoryStream = new MemoryStream())
			{
				using (var streamWriter = new StreamWriter(memoryStream, encoding))
				{
					new XmlSerializer(typeof(T)).Serialize(streamWriter, data);
					return memoryStream.ToArray();
				}
			}
		}
	}
}
