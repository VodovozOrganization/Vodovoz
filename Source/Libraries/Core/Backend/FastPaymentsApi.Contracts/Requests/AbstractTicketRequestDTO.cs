using System.Xml.Serialization;

namespace FastPaymentsApi.Contracts.Requests
{
	public abstract class AbstractTicketRequestDTO : AbstractShopRequestDTO
	{
		[XmlElement(ElementName = "ticket")]
		public string Ticket { get; set; }
	}
}
