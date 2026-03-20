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

	/// <summary>
	/// Информация о возврате денежных средств по платежу
	/// </summary>
	[XmlRoot(ElementName = "reverse_order")]
	public class ReverseOrderRequestDTO : AbstractTicketRequestDTO
	{
		/// <summary>
		/// Сумма возврата. Если не указана - возвращается полная сумма
		/// </summary>
		[XmlElement(ElementName = "amount")]
		public decimal? Amount { get; set; }
	}
}
