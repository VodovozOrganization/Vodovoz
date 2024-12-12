using System.Xml.Serialization;

namespace FastPaymentsApi.Contracts.Requests
{
	/// <summary>
	/// Инфа о сессии оплаты
	/// </summary>
	public abstract class AbstractTicketRequestDTO : AbstractShopRequestDTO
	{
		/// <summary>
		/// Сессия оплаты
		/// </summary>
		[XmlElement(ElementName = "ticket")]
		public string Ticket { get; set; }
	}
}
