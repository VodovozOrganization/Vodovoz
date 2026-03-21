using System;
using System.Xml.Serialization;

namespace FastPaymentsApi.Contracts
{
	/// <summary>
	/// Класс для преобразования xml об успешной оплате от банка
	/// </summary>
	[XmlRoot(ElementName = "order_info")]
	public class PaidOrderInfoDTO
	{
		/// <summary>
		/// Идентификатор запроса в АБС банка Авангард
		/// </summary>
		[XmlElement(ElementName = "id")]
		public long Id { get; set; }
		/// <summary>
		/// Сессия оплаты
		/// </summary>
		[XmlElement(ElementName = "ticket")]
		public string Ticket { get; set; }
		/// <summary>
		/// Метод подтверждения платежа. Возможные способы – CVV, D3S, SCR, SMS.*
		/// </summary>
		[XmlElement(ElementName = "method_name")]
		public string MethodName { get; set; }
		/// <summary>
		/// Код авторизации
		/// </summary>
		[XmlElement(ElementName = "auth_code")]
		public string AuthCode { get; set; }
		/// <summary>
		/// Статус платежа
		/// </summary>
		[XmlElement(ElementName = "status_code")]
		public FastPaymentDTOStatus Status { get; set; }
		/// <summary>
		/// Описание статуса
		/// </summary>
		[XmlElement(ElementName = "status_desc")]
		public string StatusDescription { get; set; }
		/// <summary>
		/// Дата последнего изменения статуса
		/// </summary>
		[XmlElement(ElementName = "status_date")]
		public DateTime StatusDate { get; set; }
		/// <summary>
		/// Id магазина (Organization.AvangardShopId)/>
		/// </summary>
		[XmlElement(ElementName = "shop_id")]
		public long ShopId { get; set; }
		/// <summary>
		/// Номер заказа
		/// </summary>
		[XmlElement(ElementName = "order_number")]
		public string OrderNumber { get; set; }
		/// <summary>
		/// Сумма оплаты
		/// </summary>
		[XmlElement(ElementName = "amount")]
		public int Amount { get; set; }
		/// <summary>
		/// Сумма возвращенная клиенту
		/// </summary>
		[XmlElement(ElementName = "refund_amount")]
		public int RefundAmount { get; set; }
		/// <summary>
		/// Номер карты
		/// </summary>
		[XmlElement(ElementName = "card_num")]
		public string CardNum { get; set; }
		/// <summary>
		/// Месяц окончания срока действия карты
		/// </summary>
		[XmlElement(ElementName = "exp_mm")]
		public string ExpMM { get; set; }
		/// <summary>
		/// Год окончания срока действия карты
		/// </summary>
		[XmlElement(ElementName = "exp_yy")]
		public string ExpYY { get; set; }
		/// <summary>
		/// Подпись, для проверки валидности запроса
		/// </summary>
		[XmlElement(ElementName = "signature")]
		public string Signature { get; set; }
	}
}
