namespace DriverApi.Contracts.V5
{
	/// <summary>
	/// Основная информация о передаваемом переносе
	/// </summary>
	public class RouteListAddressOutgoingTransferInfo
	{
		/// <summary>
		/// Статус приема
		/// </summary>
		public AcceptanceStatus AcceptanceStatus { get; set; }

		/// <summary>
		/// Статус передачи
		/// </summary>
		public TransferStatus TransferStatus { get; set; }

		/// <summary>
		/// Идентификатор передаваемого адреса маршрутного листа
		/// </summary>
		public int RouteListAddressId { get; set; }

		/// <summary>
		/// Идентификатор заказа
		/// </summary>
		public int OrderId { get; set; }

		/// <summary>
		/// Идентификатор передающего водителя
		/// </summary>
		public int TransferringDriverId { get; set; }

		/// <summary>
		/// Наименование передающего водителя
		/// </summary>
		public string TransferringDriverTitle { get; set; }

		/// <summary>
		/// Идентификатор принимающего водителя
		/// </summary>
		public int RecievingDriverId { get; set; }

		/// <summary>
		/// Наименование принимающего водителя
		/// </summary>
		public string RecievingDriverTitle { get; set; }
	}
}
