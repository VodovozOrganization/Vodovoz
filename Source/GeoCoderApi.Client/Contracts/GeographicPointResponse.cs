namespace GeoCoderApi.Client.Contracts
{
	/// <summary>
	/// Ответ на хапрос координат
	/// </summary>
	public class GeographicPointResponse
	{
		/// <summary>
		/// Широта
		/// </summary>
		public decimal Latitude { get; set; }

		/// <summary>
		/// Долгота
		/// </summary>
		public decimal Longitude { get; set; }
	}
}
