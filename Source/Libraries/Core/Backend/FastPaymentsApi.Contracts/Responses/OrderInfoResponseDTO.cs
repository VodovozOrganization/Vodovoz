using FastPaymentsApi.Contracts;
using System;
using System.Xml.Serialization;

namespace FastPaymentsApi.Contracts.Responses
{
	/// <summary>
	/// Информация по оплате от банка
	/// </summary>
	[XmlRoot(ElementName = "order_info")]
	public class OrderInfoResponseDTO : IAvangardResponseDetails
	{
		/// <summary>
		/// Id
		/// </summary>
		[XmlElement(ElementName = "id")]
		public long Id { get; set; }
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
		/// Статус оплаты
		/// </summary>
		[XmlElement(ElementName = "status_code")]
		public FastPaymentDTOStatus Status { get; set; }
		/// <summary>
		/// Описание
		/// </summary>
		[XmlElement(ElementName = "status_desc")]
		public string StatusDescription { get; set; }
		/// <summary>
		/// Дата последнего изменения статуса оплаты
		/// </summary>
		[XmlElement(ElementName = "status_date")]
		public DateTime StatusDate { get; set; }
		/// <summary>
		/// Код ответа
		/// </summary>
		[XmlElement(ElementName = "response_code")]
		public int ResponseCode { get; set; }
		/// <summary>
		/// Детализация
		/// </summary>
		[XmlElement(ElementName = "response_message")]
		public string ResponseMessage { get; set; }
		/// <summary>
		/// Сумма
		/// </summary>
		[XmlElement(ElementName = "amount")]
		public int Amount { get; set; }
		/// <summary>
		/// Сумма, возвращенная клиенту
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
		/// Ссылка rrn
		/// </summary>
		[XmlElement(ElementName = "rrn")]
		public string Rrn { get; set; }
		/// <summary>
		/// Номер транзакции
		/// </summary>
		[XmlElement(ElementName = "txn")]
		public string Transaction { get; set; }
		/// <summary>
		/// Только с подтверждением
		/// </summary>
		[XmlElement(ElementName = "is_auth_only")]
		public string IsAuthOnly { get; set; }
		/// <summary>
		/// Последняя ошибка
		/// </summary>
		[XmlElement(ElementName = "last_error")]
		public string LastError { get; set; }
	}
}
