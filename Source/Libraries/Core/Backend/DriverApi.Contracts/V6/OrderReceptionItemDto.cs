namespace DriverApi.Contracts.V6
{
	/// <summary>
	/// Строка оборудования на возврат
	/// </summary>
	public class OrderReceptionItemDto
	{
		/// <summary>
		/// Номер строки оборудования на возврат в заказе
		/// </summary>
		public int OrderReceptionItemId { get; set; }

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
