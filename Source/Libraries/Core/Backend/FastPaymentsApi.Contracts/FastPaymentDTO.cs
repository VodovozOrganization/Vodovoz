using System;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;

namespace FastPaymentsApi.Contracts
{
	/// <summary>
	/// Информация о быстром платеже
	/// </summary>
	public class FastPaymentDTO
	{
		/// <summary>
		/// Номер заказа
		/// </summary>
		public int OrderId { get; set; }
		/// <summary>
		/// Дата создания
		/// </summary>
		public DateTime CreationDate { get; set; }
		/// <summary>
		/// Сессия оплаты
		/// </summary>
		public string Ticket { get; set; }
		/// <summary>
		/// Qr зашифрованный в строке Base64
		/// </summary>
		public string QRPngBase64 { get; set; }
		/// <summary>
		/// Внешний номер заказа
		/// </summary>
		public int ExternalId { get; set; }
		/// <summary>
		/// Уникальный номер быстрого платежа, для формирования ссылки на оплату
		/// </summary>
		public Guid FastPaymentGuid { get; set; }
		/// <summary>
		/// Тип оплаты
		/// </summary>
		public FastPaymentPayType FastPaymentPayType { get; set; }
		/// <summary>
		/// Организация
		/// </summary>
		public Organization Organization { get; set; }
		/// <summary>
		/// Телефонный номер, на который идет смс
		/// </summary>
		public string PhoneNumber { get; set; }
		/// <summary>
		/// Откуда оплата
		/// </summary>
		public PaymentFrom PaymentByCardFrom { get; set; }
	}
}
