namespace FastPaymentsApi.Contracts.Requests
{
	/// <summary>
	/// Информация о возврате денежных средств по платежу
	/// </summary>
	public class ReverseTicketRequestDTO
	{
		/// <summary>
		/// Сессия оплаты
		/// </summary>
		public string Ticket { get; set; }

		/// <summary>
		/// Сумма возврата. Если не указана - возвращается полная сумма
		/// </summary>
		public decimal? Amount { get; set; }
	}
}
