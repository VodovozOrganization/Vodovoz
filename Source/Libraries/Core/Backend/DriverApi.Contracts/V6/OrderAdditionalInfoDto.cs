using System.Collections.Generic;

namespace DriverApi.Contracts.V6
{
	/// <summary>
	/// Дополнительная информация по заказу
	/// </summary>
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
