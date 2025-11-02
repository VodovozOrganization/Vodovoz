using System.Xml.Serialization;

namespace FastPaymentsApi.Contracts.Responses
{
	/// <summary>
	/// Ответ банка при запросе отмены платежа
	/// </summary>
	[XmlRoot(ElementName = "cancel_order_response")]
	public class CancelPaymentResponseDTO
	{
		/// <summary>
		/// Код ответа
		/// </summary>
		[XmlElement(ElementName = "response_code")]
		public int ResponseCode { get; set; }
		/// <summary>
		/// Описание
		/// </summary>
		[XmlElement(ElementName = "response_message")]
		public string ResponseMessage { get; set; }
	}
}
