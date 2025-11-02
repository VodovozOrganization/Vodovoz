using System;

namespace DriverApi.Contracts.V6
{
	/// <summary>
	/// Координата трека
	/// </summary>
	public class TrackCoordinateDto
	{
		/// <summary>
		/// Широта
		/// </summary>
		public decimal Latitude { get; set; }

		/// <summary>
		/// Долгота
		/// </summary>
		public decimal Longitude { get; set; }

		/// <summary>
		/// Время операции на стороне мобильного приложения водителя
		/// </summary>
		public DateTime ActionTimeUtc { get; set; }
	}
}
