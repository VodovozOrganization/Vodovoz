namespace DriverApi.Contracts.V6
{
	/// <summary>
	/// Адрес
	/// </summary>
	public class AddressDto
	{
		/// <summary>
		/// Город
		/// </summary>
		public string City { get; set; }

		/// <summary>
		/// Улица
		/// </summary>
		public string Street { get; set; }

		/// <summary>
		/// Номер здания
		/// </summary>
		public string Building { get; set; }

		/// <summary>
		/// Номер квартиры
		/// </summary>
		public string Apartment { get; set; }

		/// <summary>
		/// Этаж
		/// </summary>
		public string Floor { get; set; }

		/// <summary>
		/// Тип входа в здание
		/// </summary>
		public string EntranceType { get; set; }

		/// <summary>
		/// Тип помещения
		/// </summary>
		public string RoomType { get; set; }

		/// <summary>
		/// Категория точки доставки
		/// </summary>
		public string DeliveryPointCategory { get; set; }

		/// <summary>
		/// Вход
		/// </summary>
		public string Entrance { get; set; }

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
