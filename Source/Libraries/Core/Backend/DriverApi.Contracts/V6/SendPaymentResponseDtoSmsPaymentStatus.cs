namespace DriverApi.Contracts.V6
{
	/// <summary>
	/// Статус оплаты по смс
	/// </summary>
	public enum SendPaymentResponseDtoSmsPaymentStatus
	{
		/// <summary>
		/// Ожидает оплаты
		/// </summary>
		WaitingForPayment = 0,
		/// <summary>
		/// Оплачено
		/// </summary>
		Paid = 1,
		/// <summary>
		/// Отменено
		/// </summary>
		Cancelled = 2,
		/// <summary>
		/// Готово к отправке
		/// </summary>
		ReadyToSend = 3,
	}
}
