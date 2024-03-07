namespace DriverApi.Contracts.V5
{
	/// <summary>
	/// Оборудование для доставки
	/// </summary>
	public class OrderDeliveryItemDto
	{
		/// <summary>
		/// Номер строки заказа оборудования для доставки
		/// </summary>
		public int OrderDeliveryItemId { get; set; }

		/// <summary>
		/// Название
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Количество
		/// </summary>
		public int Quantity { get; set; }
	}
}
