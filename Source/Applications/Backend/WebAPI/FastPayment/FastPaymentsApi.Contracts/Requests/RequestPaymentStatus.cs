namespace FastPaymentsApi.Contracts.Requests
{
	/// <summary>
	/// Статус платежа
	/// </summary>
	public enum RequestPaymentStatus
	{
		/// <summary>
		/// Не найден
		/// </summary>
		NotFound = 0,
		/// <summary>
		/// Обрабатывается
		/// </summary>
		Processing,
		/// <summary>
		/// Отбракован
		/// </summary>
		Rejected,
		/// <summary>
		/// Принят
		/// </summary>
		Performed
	}
}
