using System.Xml.Serialization;

namespace TaxcomEdo.Api.Dto
{
	[Serializable]
	public class EmployeeShortInfo
	{
		[XmlElement("Name")]
		public PersonName Name { get; set; }

		[XmlAttribute(AttributeName = "ID")]
		public string ID { get; set; }

		[XmlElement("Position")]
		public string Position { get; set; }
	}
}
