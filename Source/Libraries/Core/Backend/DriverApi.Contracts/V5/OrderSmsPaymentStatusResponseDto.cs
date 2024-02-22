using System.Collections.Generic;

namespace DriverApi.Contracts.V5
{
	/// <summary>
	/// Ответ сервера о статусе оплаты по смс
	/// </summary>
	public class OrderSmsPaymentStatusResponseDto
	{
		/// <summary>
		/// Доступные типы оплаты для смена
		/// </summary>
		public IEnumerable<PaymentDtoType> AvailablePaymentTypes { get; set; }

		/// <summary>
		/// Можно ли отправить смс
		/// </summary>
		public bool CanSendSms { get; set; }

		/// <summary>
		/// Статус оплаты по смс
		/// </summary>
		public SmsPaymentDtoStatus? SmsPaymentStatus { get; set; }
	}
}
