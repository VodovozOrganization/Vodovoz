using System;
using System.Collections.Generic;

namespace DriverAPI.Library.Deprecated2.DTOs
{
	/// <summary>
	/// Дополнительная информация по заказу
	/// </summary>
	[Obsolete("Будет удален с прекращением поддержки API v2")]
	public class OrderAdditionalInfoDto
	{
		/// <summary>
		/// Доступные для смены типы оплаты
		/// </summary>
		public IEnumerable<PaymentDtoType> AvailablePaymentTypes { get; set; }

		/// <summary>
		/// Можно ли отправить смс
		/// </summary>
		public bool CanSendSms { get; set; }

		/// <summary>
		/// Можно ли получить QR-код
		/// </summary>
		public bool CanReceiveQRCode { get; set; }
	}
}
