using System.Xml.Serialization;

namespace FastPaymentsApi.Contracts.Requests
{
	/// <summary>
	/// Информация об отмене платежа
	/// </summary>
	[XmlRoot(ElementName = "cancel_order")]
	public class CancelPaymentRequestDTO : AbstractTicketRequestDTO
	{

	}
}
