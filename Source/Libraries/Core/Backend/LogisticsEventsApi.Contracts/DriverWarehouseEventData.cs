namespace LogisticsEventsApi.Contracts
{
	/// <summary>
	/// Информация о событии и координатами сканирования
	/// </summary>
	public class DriverWarehouseEventData
	{
		/// <summary>
		/// Данные из Qr кода
		/// </summary>
		public string QrData { get; set; }
		/// <summary>
		/// Широта сканирующего
		/// </summary>
		public decimal? Latitude { get; set; }
		/// <summary>
		/// Долгота сканирующего
		/// </summary>
		public decimal? Longitude { get; set; }
	}
}
