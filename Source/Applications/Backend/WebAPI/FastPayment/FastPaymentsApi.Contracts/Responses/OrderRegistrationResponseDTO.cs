using System.Xml.Serialization;

namespace FastPaymentsApi.Contracts.Responses
{
	/// <summary>
	/// Ответ банка при регистрации заказ в системе эквайринга
	/// </summary>
	[XmlRoot(ElementName = "order_response")]
	public class OrderRegistrationResponseDTO : IAvangardResponseDetails
	{
		/// <summary>
		/// Id
		/// </summary>
		[XmlElement(ElementName = "id")]
		public long Id { get; set; }
		/// <summary>
		/// Сессия оплаты
		/// </summary>
		[XmlElement(ElementName = "ticket")]
		public string Ticket { get; set; }
		/// <summary>
		/// Код, возвращаемый в случае успешной оплаты
		/// </summary>
		[XmlElement(ElementName = "ok_code")]
		public string OkCode { get; set; }
		/// <summary>
		/// Код, возвращаемый в случае каких-либо ошибок в процессе оплаты
		/// </summary>
		[XmlElement(ElementName = "failure_code")]
		public string FailureCode { get; set; }
		/// <summary>
		/// Код ответа
		/// </summary>
		[XmlElement(ElementName = "response_code")]
		public int ResponseCode { get; set; }
		/// <summary>
		/// Детализация ответа
		/// </summary>
		[XmlElement(ElementName = "response_message")]
		public string ResponseMessage { get; set; }
		/// <summary>
		/// Изображение Qr-кода в Base64 png
		/// </summary>
		[XmlElement(ElementName = "qr_png_base64")]
		public string QRPngBase64 { get; set; }
		/// <summary>
		/// Ссылка на qr(не используется)
		/// </summary>
		[XmlElement(ElementName = "qrUrl")]
		public string QRUrl { get; set; }
		/// <summary>
		/// Сообщение об ошибке
		/// </summary>
		[XmlIgnore]
		public string ErrorMessage { get; set; }
	}
}
