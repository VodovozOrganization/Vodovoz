using System.Xml.Serialization;

namespace TaxcomEdo.Api.Dto
{
	[Serializable]
	[XmlRoot(ElementName = "Contacts", Namespace = "http://api-invoice.taxcom.ru/contacts")]
	public class ContactList
	{
		[XmlIgnore]
		public const string XmlNamespace = "http://api-invoice.taxcom.ru/contacts";

		[XmlElement(ElementName = "Contact")]
		public ContactListItem[] Contacts { get; set; }

		[XmlAttribute(AttributeName = "Asof")]
		public DateTime Asof { get; set; }

		[XmlElement(ElementName = "TemplateID")]
		public Guid TemplateId { get; set; }
	}
}
