using System.Text;
using System.Xml.Serialization;
using VodovozInfrastructure;
using VodovozInfrastructure.StringHandlers;

namespace FastPaymentsAPI.Library.Managers
{
	public class DTOManager : IDTOManager
	{
		public string GetXmlStringFromDTO<T>(T dto)
			where T : class
		{
			using var stringwriter = new StringWriterWithEncoding(Encoding.UTF8);
			XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
			ns.Add(string.Empty, string.Empty);
			var serializer = new XmlSerializer(typeof(T));
			serializer.Serialize(stringwriter, dto, ns);

			return stringwriter.ToString();
		}
	}
}
