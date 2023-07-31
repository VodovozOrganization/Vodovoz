namespace DriverAPI.Library.DTOs
{
	/// <summary>
	/// Товар на продажу
	/// </summary>
	public class OrderSaleItemDto
	{
		/// <summary>
		/// Номер товара на продажу
		/// </summary>
		public int OrderSaleItemId { get; set; }

		/// <summary>
		/// Название
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Количество
		/// </summary>
		public decimal Quantity { get; set; }

		/// <summary>
		/// Цена товара
		/// </summary>
		public decimal OrderItemPrice { get; set; }

		/// <summary>
		/// Полная цена товара
		/// </summary>
		public decimal TotalOrderItemPrice { get; set; }

		/// <summary>
		/// Нужно просканировать код
		/// </summary>
		public bool NeedScanCode { get; set; }

		/// <summary>
		/// Акция бутыль
		/// </summary>
		public bool IsBottleStock { get; set; }

		/// <summary>
		/// Скидка в деньгах
		/// </summary>
		public bool IsDiscountInMoney { get; set; }

		/// <summary>
		/// Причина скидки
		/// </summary>
		public string DiscountReason { get; set; }

		/// <summary>
		/// Скидка
		/// </summary>
		public decimal Discount { get;set; }
	}
}
