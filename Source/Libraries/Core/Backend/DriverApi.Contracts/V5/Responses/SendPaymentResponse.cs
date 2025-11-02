namespace DriverApi.Contracts.V5.Responses
{
	/// <summary>
	/// Ответ на попытку отправить смс для оплаты
	/// </summary>
	public class SendPaymentResponse
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
