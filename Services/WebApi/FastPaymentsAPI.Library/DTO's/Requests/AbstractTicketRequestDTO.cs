using System.Xml.Serialization;

namespace FastPaymentsAPI.Library.DTO_s.Requests
{
	public abstract class AbstractTicketRequestDTO : AbstractShopRequestDTO
	{
		[XmlElement(ElementName = "ticket")]
		public string Ticket { get; set; }
	}
}
