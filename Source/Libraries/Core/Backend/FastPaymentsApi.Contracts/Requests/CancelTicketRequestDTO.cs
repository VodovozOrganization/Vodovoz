namespace FastPaymentsApi.Contracts.Requests
{
	/// <summary>
	/// Инфа для отмены платежа
	/// </summary>
	public class CancelTicketRequestDTO
	{
		/// <summary>
		/// Сессия оплаты
		/// </summary>
		public string Ticket { get; set; }
	}
}
