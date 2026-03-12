using System.Xml.Serialization;

namespace FastPaymentsApi.Contracts.Requests
{
	/// <summary>
	/// Информация о платеже
	/// </summary>
	[XmlRoot(ElementName = "get_order_info")]
	public class OrderInfoRequestDTO : AbstractTicketRequestDTO
	{

	}
}
