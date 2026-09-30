namespace CustomerOrdersApi.Library.Default.Dto.Orders
{
	public class OrderItemDto
	{
		private OrderItemDto(int nomenclatureId, decimal count, decimal price, bool isDiscountInMoney, decimal discount)
		{
			NomenclatureId = nomenclatureId;
			Count = count;
			Price = price;
			IsDiscountInMoney = isDiscountInMoney;
			Discount = discount;
		}
		
		/// <summary>
		/// Id номенклатуры
		/// </summary>
		public int NomenclatureId { get; }
		
		/// <summary>
		/// Количество
		/// </summary>
		public decimal Count { get; }
		
		/// <summary>
		/// Цена
		/// </summary>
		public decimal Price { get; }
		
		/// <summary>
		/// Скидка в деньгах?
		/// </summary>
		public bool IsDiscountInMoney { get; }

		/// <summary>
		/// Скидка
		/// </summary>
		public decimal Discount { get; }

		public static OrderItemDto Create(
			int nomenclatureId,
			decimal count,
			decimal price,
			bool isDiscountInMoney,
			decimal discount) =>
			new OrderItemDto(nomenclatureId, count, price, isDiscountInMoney, discount);
	}
}
