namespace RoboAtsService.Contracts.Responses
{
	/// <summary>
	/// Информация о точке доставки
	/// </summary>
	public class DeliveryPointInfoResponse
	{
		/// <summary>
		/// Номер улицы для Roboats
		/// </summary>
		public int? StreetId { get; set; }

		/// <summary>
		/// Номер дома
		/// </summary>
		public string HouseNumber { get; set; }

		/// <summary>
		/// Корпус
		/// </summary>
		public string BuildingNumber { get; set; }

		/// <summary>
		/// Номер квартиры
		/// </summary>
		public string AppartmentNumber { get; set; }
	}
}
