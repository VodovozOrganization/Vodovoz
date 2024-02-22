using System.Xml.Serialization;

namespace FastPaymentsApi.Contracts.Requests
{
	[XmlRoot(ElementName = "cancel_order")]
	public class CancelPaymentRequestDTO : AbstractTicketRequestDTO
	{

	}
}
