using System.Xml.Serialization;

namespace FastPaymentsApi.Contracts.Requests
{
	[XmlRoot(ElementName = "get_order_info")]
	public class OrderInfoRequestDTO : AbstractTicketRequestDTO
	{

	}
}
