namespace CustomerOrdersApi.Library.V4.Dto.Orders
{
	/// <summary>
	/// Онлайн пакет аренды
	/// </summary>
	public class OnlineRentPackageDto
	{
		/// <summary>
		/// Id пакета аренды
		/// </summary>
		public int RentPackageId { get; set; }

		/// <summary>
		/// Стоимость аренды
		/// </summary>
		public decimal Price { get; set; }
		
		/// <summary>
		/// Количество
		/// </summary>
		public int Count { get; set; }
	}
}
