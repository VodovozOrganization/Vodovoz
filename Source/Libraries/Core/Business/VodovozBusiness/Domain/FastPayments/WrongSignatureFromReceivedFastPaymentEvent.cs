using System;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.FastPayments
{
	/// <summary>
	/// Событие отправки уведомления о пришедшем неизвестном колбэке по оплате СБП
	/// </summary>
	public class WrongSignatureFromReceivedFastPaymentEvent : IDomainObject
	{
		public virtual int Id { get; set; }
		/// <summary>
		/// Идентификатор быстрого платежа
		/// </summary>
		public virtual int FastPaymentId { get; set; }
		/// <summary>
		/// Номер заказа
		/// </summary>
		public virtual string OrderNumber { get; set; }
		/// <summary>
		/// Подпись, рассчитанная банком
		/// </summary>
		public virtual string BankSignature { get; set; }
		/// <summary>
		/// Идентификатор магазина
		/// </summary>
		public virtual int ShopId { get; set; }
		/// <summary>
		/// Сгенерированная подпись для проверки
		/// </summary>
		public virtual string GeneratedSignature { get; set; }
		/// <summary>
		/// Дата отправки
		/// </summary>
		public virtual DateTime? SentDate { get; set; }

		public static WrongSignatureFromReceivedFastPaymentEvent Create(string orderNumber, string bankSignature, int shopId, string paymentSignature)
			=> new WrongSignatureFromReceivedFastPaymentEvent
			{
				OrderNumber = orderNumber,
				BankSignature = bankSignature,
				ShopId = shopId,
				GeneratedSignature = paymentSignature,
			};
	}
}
