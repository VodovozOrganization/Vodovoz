namespace FastPaymentsApi.Contracts.Requests
{
	/// <summary>
	/// Статус оплаты для уведомления
	/// </summary>
	public enum PaymentStatusNotification
	{
		/// <summary>
		/// Отменена
		/// </summary>
		canceled,
		/// <summary>
		/// Успешна
		/// </summary>
		succeeded
	}
}
