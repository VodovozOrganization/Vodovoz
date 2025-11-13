namespace DriverApi.Contracts.V6
{
	/// <summary>
	/// Основная информация о принимаемом переносе
	/// </summary>
	public class RouteListAddressIncomingTransferInfo
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
		/// Идентификатор принимаемого адреса маршрутного листа
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
