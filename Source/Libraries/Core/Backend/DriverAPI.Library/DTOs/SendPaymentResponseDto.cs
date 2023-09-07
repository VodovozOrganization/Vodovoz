namespace DriverAPI.Library.DTOs
{
	/// <summary>
	/// Ответ на попытку отправить смс для оплаты
	/// </summary>
	public class SendPaymentResponseDto
	{
		/// <summary>
		/// Описание ошибки
		/// </summary>
		public string ErrorDescription { get; set; }

		/// <summary>
		/// Статус отправки смс для оплаты
		/// </summary>
		public SendPaymentResponseDtoMessageStatus Status { get; set; }

		/// <summary>
		/// Статус платежа
		/// </summary>
		public SendPaymentResponseDtoSmsPaymentStatus? PaymentStatus { get; set; }
	}
}
