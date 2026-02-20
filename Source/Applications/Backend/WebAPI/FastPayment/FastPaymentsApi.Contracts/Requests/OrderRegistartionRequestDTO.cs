using System.Xml.Serialization;

namespace FastPaymentsApi.Contracts.Requests
{
	/// <summary>
	/// Инфа для регистрации заказа в системе эквайринга
	/// </summary>
	[XmlRoot(ElementName = "new_order")]
	public class OrderRegistrationRequestDTO : AbstractShopRequestDTO
	{
		/// <summary>
		/// Сумма заказа, указывается в копейках РФ
		/// </summary>
		[XmlElement(ElementName = "amount")]
		public int Amount { get; set; }
		/// <summary>
		/// Нужно подтверждение при оплате
		/// </summary>
		[XmlElement(ElementName = "is_auth_only")]
		public int is_auth_only { get; set; }
		/// <summary>
		/// URL ссылки перехода обратно в магазин со страницы оплаты
		/// </summary>
		[XmlElement(ElementName = "back_url")]
		public string BackUrl { get; set; }
		/// <summary>
		/// URL ссылки перехода обратно в магазин в случае ошибок либо возврата без оплаты
		/// </summary>
		[XmlElement(ElementName = "back_url_fail")]
		public string BackUrlFail { get; set; }
		/// <summary>
		/// URL ссылки перехода обратно в магазин в случае успешной оплаты
		/// </summary>
		[XmlElement(ElementName = "back_url_ok")]
		public string BackUrlOk { get; set; }
		/// <summary>
		/// Номер карты
		/// </summary>
		[XmlElement(ElementName = "card_num")]
		public string CardNum { get; set; }
		/// <summary>
		/// Полный адрес клиента
		/// </summary>
		[XmlElement(ElementName = "client_address")]
		public string ClientAddress { get; set; }
		/// <summary>
		/// Электронка клиента
		/// </summary>
		[XmlElement(ElementName = "client_email")]
		public string ClientEmail { get; set; }
		/// <summary>
		/// Ip адрес клиента
		/// </summary>
		[XmlElement(ElementName = "client_ip")]
		public string ClientIp { get; set; }
		/// <summary>
		/// Полное имя клиента
		/// </summary>
		[XmlElement(ElementName = "client_name")]
		public string ClientName { get; set; }
		/// <summary>
		/// Телефон клиента
		/// </summary>
		[XmlElement(ElementName = "client_phone")]
		public string ClientPhone { get; set; }
		/// <summary>
		/// Месяц экспирации карты
		/// </summary>
		[XmlElement(ElementName = "exp_month")]
		public string ExpMonth { get; set; }
		/// <summary>
		/// Год экспирации карты
		/// </summary>
		[XmlElement(ElementName = "exp_year")]
		public string ExpYear { get; set; }
		/// <summary>
		/// Признак оплаты по qr, 1- оплата по QR-коду, 0- оплата по карте (по умолчанию 0)
		/// </summary>
		[XmlElement(ElementName = "is_qr")]
		public int IsQR { get; set; }
		/// <summary>
		/// Язык
		/// </summary>
		[XmlElement(ElementName = "language")]
		public string Language { get; set; }
		/// <summary>
		/// Описание заказа
		/// </summary>
		[XmlElement(ElementName = "order_description")]
		public string OrderDescription { get; set; }
		/// <summary>
		/// Номер заказа
		/// </summary>
		[XmlElement(ElementName = "order_number")]
		public string OrderNumber { get; set; }
		/// <summary>
		/// Время жизни Qr в минутах
		/// </summary>
		[XmlElement(ElementName = "qr_ttl")]
		public int? QRTtl { get; set; }
		/// <summary>
		/// Признак возврата изображения Qr в ответе, 1- возвращать изображение, 0- не возвращать изображение (по умолчанию 0)
		/// </summary>
		[XmlElement(ElementName = "return_qr_image")]
		public int ReturnQRImage { get; set; }
		/// <summary>
		/// Возвращать ссылку
		/// </summary>
		[XmlElement(ElementName = "return_qr_url")]
		public int ReturnQRUrl { get; set; }
		/// <summary>
		/// Номер транзакции
		/// </summary>
		[XmlElement(ElementName = "txn")]
		public string Transaction { get; set; }
		/// <summary>
		/// Подпись
		/// </summary>
		[XmlElement(ElementName = "signature")]
		public string Signature { get; set; }
		/// <summary>
		/// Условие для сериализации QrTtl
		/// </summary>
		/// <returns>true/false</returns>
		public bool ShouldSerializeQRTtl() => QRTtl.HasValue;
	}
}
